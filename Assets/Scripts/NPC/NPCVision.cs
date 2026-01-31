using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class NPCVision : MonoBehaviour
{
    [Header("Vision Cone")]
    public float viewDistance = 6f;

    [Range(1f, 179f)]
    public float viewAngle = 90f;

    public Vector3 localOriginOffset = Vector3.zero;
    public float originHeightOffset = 0.0f;

    [Header("Facing / Look Direction")]
    public bool useMovementFacing = true;
    public Vector2 fallbackFacing = Vector2.up;
    public float minMoveSqrMagnitudeToUpdateFacing = 0.01f;
    public float facingSmoothing = 12f;

    [Header("Player Detection")]
    public string playerTag = "Player";
    public LayerMask playerLayerMask;
    public LayerMask occlusionLayerMask;
    public float scanInterval = 0.1f;

    [Header("Events")]
    public UnityEvent onPlayerSeen;
    public UnityEvent onPlayerLost;

    [Header("Debug")]
    public bool debugLogs = false;

    // Runtime
    private Transform player;
    private float nextScanTime;

    private bool isSeeingPlayer;
    private bool lastSeeingPlayer;

    private NavMeshAgent agent;
    private Vector2 currentFacing;

    public bool IsSeeingPlayer => isSeeingPlayer;

    private void Awake()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;

        agent = GetComponentInParent<NavMeshAgent>();
        currentFacing = fallbackFacing.sqrMagnitude > 0.0001f ? fallbackFacing.normalized : Vector2.up;

        if (debugLogs) Log($"Init | Player={(player != null)} | Agent={(agent != null)}");
    }

    private void Update()
    {
        if (Time.time < nextScanTime) return;

        nextScanTime = Time.time + Mathf.Max(0.02f, scanInterval);

        isSeeingPlayer = ScanConeForPlayer2D();

        if (isSeeingPlayer != lastSeeingPlayer)
        {
            lastSeeingPlayer = isSeeingPlayer;

            if (isSeeingPlayer)
            {
                if (debugLogs) Log("Player SEEN -> invoke onPlayerSeen");
                onPlayerSeen?.Invoke();
            }
            else
            {
                if (debugLogs) Log("Player LOST -> invoke onPlayerLost");
                onPlayerLost?.Invoke();
            }
        }
    }

    private bool ScanConeForPlayer2D()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.transform;
            if (player == null) return false;
        }

        Vector2 origin = GetOriginWorld2D();

        // Broadphase
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, viewDistance, playerLayerMask);
        if (hits == null || hits.Length == 0) return false;

        // Find nearest collider with correct tag
        Collider2D best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (!c.CompareTag(playerTag)) continue;

            Vector2 p = c.bounds.center;
            float sqr = (p - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = c;
            }
        }

        if (best == null) return false;

        Vector2 targetPos = best.bounds.center;
        Vector2 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist <= 0.001f) return true;

        Vector2 dir = toTarget / dist;

        // Angle check
        Vector2 facing = GetFacing2D();
        float half = viewAngle * 0.5f;

        float angleToTarget = Vector2.Angle(facing, dir);
        if (angleToTarget > half) return false;

        // LOS check
        if (occlusionLayerMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, occlusionLayerMask);
            if (hit.collider != null) return false;
        }

        return true;
    }

    public Vector2 GetOriginWorld2D()
    {
        Vector3 o = transform.TransformPoint(localOriginOffset);
        o.y += originHeightOffset;
        return new Vector2(o.x, o.y);
    }

    public Vector2 GetFacing2D()
    {
        if (!useMovementFacing || agent == null)
            return currentFacing.sqrMagnitude > 0.0001f ? currentFacing : Vector2.up;

        Vector2 desired = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.y);
        if (desired.sqrMagnitude < minMoveSqrMagnitudeToUpdateFacing)
            desired = new Vector2(agent.velocity.x, agent.velocity.y);

        if (desired.sqrMagnitude >= minMoveSqrMagnitudeToUpdateFacing)
        {
            Vector2 target = desired.normalized;
            currentFacing = facingSmoothing <= 0f
                ? target
                : Vector2.Lerp(currentFacing, target, Time.deltaTime * facingSmoothing);

            if (currentFacing.sqrMagnitude > 0.001f) currentFacing.Normalize();
        }

        return currentFacing;
    }

    private void Log(string msg)
    {
        Debug.Log($"[NPCVision:{transform.parent?.name ?? name}] {msg}", this);
    }

    public static Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}
