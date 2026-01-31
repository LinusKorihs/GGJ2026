using UnityEngine;

public class NPCVisionGizmos : MonoBehaviour
{
    [Header("References")]
    public NPCVision vision;
    public NPCPatience patience;

    [Header("Draw Options")]
    public bool drawCone = true;
    public bool drawRadiusCircle = false;

    [Header("Debug")]
    public bool drawOnlyWhenSelected = true;

    private void Awake()
    {
        if (vision == null) vision = GetComponent<NPCVision>();
        if (patience == null) patience = GetComponentInParent<NPCPatience>();
    }

    private void OnDrawGizmosSelected()
    {
        if (drawOnlyWhenSelected)
            Draw();
    }

    private void OnDrawGizmos()
    {
        if (!drawOnlyWhenSelected)
            Draw();
    }

    private void Draw()
    {
        if (vision == null) return;
        if (!drawCone && !drawRadiusCircle) return;

        Vector2 origin = Application.isPlaying ? vision.GetOriginWorld2D() : (Vector2)transform.position;

        if (drawRadiusCircle)
            DrawWireCircle(origin, vision.viewDistance, 32);

        if (drawCone)
        {
            Vector2 facing = Application.isPlaying ? vision.GetFacing2D() : vision.fallbackFacing.normalized;

            float half = vision.viewAngle * 0.5f;

            Vector2 left = NPCVision.Rotate2D(facing, -half);
            Vector2 right = NPCVision.Rotate2D(facing, half);

            Gizmos.DrawLine(origin, origin + left * vision.viewDistance);
            Gizmos.DrawLine(origin, origin + right * vision.viewDistance);
            Gizmos.DrawLine(origin, origin + facing * vision.viewDistance);
        }

        // Optional: draw patience as small line (no colors specified)
        if (Application.isPlaying && patience != null)
        {
            float t = Mathf.Approximately(patience.patienceMax, 0f) ? 0f : Mathf.Clamp01(patience.Patience / patience.patienceMax);
            Vector3 a = new Vector3(origin.x, origin.y, 0f);
            Vector3 b = a + Vector3.up * (0.75f * t);
            Gizmos.DrawLine(a, b);
        }
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
