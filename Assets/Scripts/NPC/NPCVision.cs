using UnityEngine;
using UnityEngine.Events;

public class NPCVision : MonoBehaviour
{
    [Header("Vision Cone")]
    [Tooltip("Max distance the NPC can see.")]
    public float viewDistance = 6f;

    [Tooltip("Full cone angle in degrees (e.g. 90 = 45 left + 45 right).")]
    [Range(1f, 179f)]
    public float viewAngle = 90f;

    [Tooltip("Local space offset from this transform (Eyes) for ray origin / cone origin.")]
    public Vector3 localOriginOffset = Vector3.zero;

    [Tooltip("Extra vertical offset added to the origin (useful if eyes bone is a bit low/high).")]
    public float originHeightOffset = 0.0f;

    [Tooltip("If true, cone direction follows movement direction from NavMeshAgent.")]
    public bool useMovementFacing = true;

    [Tooltip("Fallback direction when no movement is available.")]
    public Vector2 fallbackFacing = Vector2.up;

    [Tooltip("Minimum movement speed to update facing (prevents jitter).")]
    public float minMoveSqrMagnitudeToUpdateFacing = 0.01f;

    [Tooltip("How quickly the facing direction smooths toward movement direction (0 = no smoothing).")]
    public float facingSmoothing = 12f;

    [Header("Player Detection")]
    [Tooltip("Player must have this tag.")]
    public string playerTag = "Player";

    [Tooltip("Only colliders on these layers are considered player candidates (set to Player layer).")]
    public LayerMask playerLayerMask;

    [Tooltip("Obstacles that block vision (e.g. Walls). Leave empty to ignore occlusion.")]
    public LayerMask occlusionLayerMask;

    [Tooltip("How often we scan for the player (seconds). Lower = more responsive, higher = cheaper).")]
    public float scanInterval = 0.1f;

    [Header("Patience")]
    [Tooltip("Current patience value (runtime).")]
    [SerializeField] private float patience = 0f;

    [Tooltip("Patience increases per second while player is seen.")]
    public float patienceGainPerSecond = 35f;

    [Tooltip("Patience decreases per second while player is NOT seen.")]
    public float patienceLossPerSecond = 25f;

    [Tooltip("Clamp for patience value.")]
    public float patienceMax = 100f;

    [Tooltip("When patience reaches this value -> caught.")]
    public float caughtThreshold = 100f;

    [Header("Events")]
    public UnityEvent onCaught;

    [Header("Debug")]
    public bool debugLogs = false;
    public bool debugGizmos = true;
    public bool debugRadiusGizmo = false;

    // Runtime
    private Transform player;
    private float nextScanTime;
    private bool isSeeingPlayer;
    private bool hasCaught;
    private UnityEngine.AI.NavMeshAgent agent;
    private Vector2 currentFacing;

    public float Patience => patience;
    public bool IsSeeingPlayer => isSeeingPlayer;

    private void Awake()
    {
        // No hard dependency: just find by tag once
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;

        if (debugLogs)
        {
            Log($"Initialized. Player found: {(player != null ? player.name : "NO")}");
        }

        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        currentFacing = fallbackFacing.sqrMagnitude > 0.0001f ? fallbackFacing.normalized : Vector2.up;

        if (debugLogs)
        {
            Log($"Agent found: {(agent != null ? agent.name : "NO")}");
        }
    }

    private void Update()
    {
        if (hasCaught) return;

        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + Mathf.Max(0.02f, scanInterval);
            isSeeingPlayer = ScanConeForPlayer2D();
        }

        UpdatePatience(Time.deltaTime);

        if (!hasCaught && patience >= caughtThreshold)
        {
            hasCaught = true;
            patience = caughtThreshold;
            Log("CAUGHT! Patience threshold reached.", true);

            onCaught?.Invoke();
        }
    }

    private void UpdatePatience(float dt)
    {
        float before = patience;

        if (isSeeingPlayer) patience += patienceGainPerSecond * dt;
        else patience -= patienceLossPerSecond * dt;

        patience = Mathf.Clamp(patience, 0f, patienceMax);

        if (debugLogs && Mathf.Abs(patience - before) > 0.01f)
        {
            Log($"Patience {(isSeeingPlayer ? "++" : "--")} -> {patience:0.0}/{patienceMax:0.0}");
        }
    }

    private bool ScanConeForPlayer2D()
    {
        // If player transform wasn't found (scene reload etc.), try again sometimes.
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.transform;
            if (player == null) return false;
        }

        Vector2 origin = GetOriginWorld2D();

        // Broadphase: check if any Player-layer collider is near
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, viewDistance, playerLayerMask);

        if (hits == null || hits.Length == 0) return false;

        // Find the nearest collider belonging to the tagged player (safe if multiple colliders)
        Collider2D best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (!hits[i].CompareTag(playerTag)) continue;

            Vector2 p = hits[i].bounds.center;
            float sqr = (p - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hits[i];
            }
        }

        if (best == null) return false;

        Vector2 targetPos = best.bounds.center;
        Vector2 toTarget = targetPos - origin;

        float dist = toTarget.magnitude;
        if (dist <= 0.0001f) return true;

        Vector2 dir = toTarget / dist;

        // Cone angle check in 2D
        float halfAngle = viewAngle * 0.5f;

        // In 2D, sprites commonly face to the right (local +X)
        Vector2 facing = GetFacing2D();

        float angleToTarget = Vector2.Angle(facing, dir);
        if (angleToTarget > halfAngle) return false;

        // Optional line-of-sight check (occlusion)
        if (occlusionLayerMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, occlusionLayerMask);
            if (hit.collider != null)
            {
                if (debugLogs) Log($"LOS blocked by: {hit.collider.name}");
                return false;
            }
        }

        if (debugLogs) Log("Player seen!");
        return true;
    }

    private Vector2 GetOriginWorld2D()
    {
        Vector3 origin = transform.TransformPoint(localOriginOffset);
        origin.y += originHeightOffset;
        return new Vector2(origin.x, origin.y);
    }

    private void Log(string msg, bool error = false)
    {
        if (!debugLogs && !error) return;

        string prefix = $"[NPCVision:{name}] ";
        if (error) Debug.LogError(prefix + msg, this);
        else Debug.Log(prefix + msg, this);
    }

    private Vector2 GetFacing2D()
    {
        if (!useMovementFacing || agent == null)
            return currentFacing.sqrMagnitude > 0.0001f ? currentFacing : Vector2.up;

        // NavMeshAgent in 2D: desiredVelocity funktioniert meist am besten
        Vector3 dv3 = agent.desiredVelocity;
        Vector2 desired = new Vector2(dv3.x, dv3.y);

        // Falls desiredVelocity mal 0 ist, fallback auf velocity
        if (desired.sqrMagnitude < minMoveSqrMagnitudeToUpdateFacing)
        {
            Vector3 v3 = agent.velocity;
            desired = new Vector2(v3.x, v3.y);
        }

        // Wenn wir wirklich "moving" sind -> update facing
        if (desired.sqrMagnitude >= minMoveSqrMagnitudeToUpdateFacing)
        {
            Vector2 targetFacing = desired.normalized;

            if (facingSmoothing <= 0f)
                currentFacing = targetFacing;
            else
                currentFacing = Vector2.Lerp(currentFacing, targetFacing, Time.deltaTime * facingSmoothing);

            if (currentFacing.sqrMagnitude > 0.0001f)
                currentFacing.Normalize();
        }
        else
        {
            // Steht: facing bleibt wie zuletzt (oder falls ungültig -> fallback)
            if (currentFacing.sqrMagnitude < 0.0001f)
                currentFacing = fallbackFacing.sqrMagnitude > 0.0001f ? fallbackFacing.normalized : Vector2.up;
        }

        return currentFacing;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;

        Vector2 origin;
        if (Application.isPlaying) origin = GetOriginWorld2D();
        else
        {
            Vector3 o3 = transform.TransformPoint(localOriginOffset);
            o3.y += originHeightOffset;
            origin = new Vector2(o3.x, o3.y);
        }

        // Optional radius gizmo as circle
        if (debugRadiusGizmo) DrawWireCircle(origin, viewDistance, 32);

        // Draw cone edges in XY plane
        float half = viewAngle * 0.5f;
        Vector2 facing = Application.isPlaying ? GetFacing2D() : (fallbackFacing.sqrMagnitude > 0.0001f ? fallbackFacing.normalized : Vector2.up);

        Vector2 leftDir = Rotate2D(facing, -half).normalized;
        Vector2 rightDir = Rotate2D(facing, half).normalized;

        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
        Gizmos.DrawLine(origin, origin + facing * viewDistance);
    }

    private static Vector2 Rotate2D(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    private static void DrawWireCircle(Vector2 center, float radius, int segments)
    {
        Vector3 prev = center + Vector2.right * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
