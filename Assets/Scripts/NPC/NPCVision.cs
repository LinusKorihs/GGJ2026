using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[RequireComponent(typeof(PolygonCollider2D))]
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

    [Header("Trigger Cone Collider")]
    public bool useTriggerCone = true;

    [Range(2, 20)]
    public int coneSegments = 2;

    public bool requirePlayerTagInTrigger = true;

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

    private PolygonCollider2D coneCollider;
    private bool playerInsideTrigger;
    private Collider2D lastPlayerColliderInTrigger;

    public bool IsSeeingPlayer => isSeeingPlayer;

    private void Awake()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;

        agent = GetComponentInParent<NavMeshAgent>();
        currentFacing = fallbackFacing.sqrMagnitude > 0.0001f ? fallbackFacing.normalized : Vector2.up;

        coneCollider = GetComponent<PolygonCollider2D>();
        coneCollider.isTrigger = true;
        coneCollider.pathCount = 1;

        UpdateTriggerConeShape();

        if (debugLogs) Log($"Init | Player={(player != null)} | Agent={(agent != null)} | TriggerCone={useTriggerCone}");
    }

    private void Update()
    {
        if (useTriggerCone) UpdateTriggerConeShape();

        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + Mathf.Max(0.02f, scanInterval);

            isSeeingPlayer = ScanConeForPlayer2D();

            // Edge-trigger events
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

        if (!useTriggerCone) return false;

        if (!playerInsideTrigger) return false;

        Vector2 targetPos = lastPlayerColliderInTrigger != null ? (Vector2)lastPlayerColliderInTrigger.bounds.center : (Vector2)player.position;

        Vector2 dir = targetPos - origin;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        dir /= dist;

        // LOS check
        if (occlusionLayerMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, occlusionLayerMask);
            if (hit.collider != null)
            {
                if (debugLogs) Log($"LOS blocked by {hit.collider.name}");
                return false;
            }
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
        if (!useMovementFacing || agent == null) return currentFacing.sqrMagnitude > 0.0001f ? currentFacing : Vector2.up;

        Vector2 desired = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.y);

        if (desired.sqrMagnitude < minMoveSqrMagnitudeToUpdateFacing) desired = new Vector2(agent.velocity.x, agent.velocity.y);

        if (desired.sqrMagnitude >= minMoveSqrMagnitudeToUpdateFacing)
        {
            Vector2 target = desired.normalized;
            currentFacing = facingSmoothing <= 0f ? target : Vector2.Lerp(currentFacing, target, Time.deltaTime * facingSmoothing);

            if (currentFacing.sqrMagnitude > 0.001f) currentFacing.Normalize();
        }

        return currentFacing;
    }

    private void UpdateTriggerConeShape()
    {
        if (!useTriggerCone || coneCollider == null) return;

        Vector2 originLocal = new Vector2(localOriginOffset.x, localOriginOffset.y + originHeightOffset);

        Vector2 facingWorld = GetFacing2D();
        Vector3 localDir3 = transform.InverseTransformDirection(new Vector3(facingWorld.x, facingWorld.y, 0f));
        Vector2 facingLocal = new Vector2(localDir3.x, localDir3.y).normalized;

        if (facingLocal.sqrMagnitude < 0.001f) facingLocal = Vector2.up;

        float half = viewAngle * 0.5f;
        int seg = Mathf.Max(2, coneSegments);

        Vector2[] pts = new Vector2[seg + 2];
        pts[0] = originLocal;

        for (int i = 0; i <= seg; i++)
        {
            float t = i / (float)seg;
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate2D(facingLocal, ang).normalized;
            pts[i + 1] = originLocal + dir * viewDistance;
        }
        coneCollider.SetPath(0, pts);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerCone) return;
        if (requirePlayerTagInTrigger && !other.CompareTag(playerTag)) return;
        if (((1 << other.gameObject.layer) & playerLayerMask) == 0) return;

        playerInsideTrigger = true;
        lastPlayerColliderInTrigger = other;

        if (debugLogs) Log($"Player ENTER cone ({other.name})");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTriggerCone) return;
        if (other != lastPlayerColliderInTrigger) return;

        playerInsideTrigger = false;
        lastPlayerColliderInTrigger = null;

        if (debugLogs) Log("Player EXIT cone");
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
