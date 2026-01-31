using UnityEngine;
using System;

public class NPCVision : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 5f;
    [Range(0f, 360f)] public float fovAngle = 120f;
    public bool useFOV = true;

    [Tooltip("Layers that block line of sight (Walls, Props, etc.)")]
    public LayerMask blockingMask;

    [Header("Patience / Suspicion")]
    [Tooltip("Seconds the player can be in the field of view before being 'caught'.")]
    public float timeToGetCaught = 1.5f;

    [Tooltip("How quickly the meter decays when the player is out of sight (1 = same speed as buildup).")]
    public float decayMultiplier = 0.75f;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Vision Origin (optional)")]
    public Transform visionOrigin; // if empty -> this.transform
    public Vector2 targetOffset = new Vector2(0f, 0.6f); // aim offset on player (2D)

    // 0..1
    public float suspicion01 { get; private set; }
    public bool hasLineOfSight { get; private set; }
    public bool isCaught { get; private set; }

    public event Action<float> OnSuspicionChanged; // float 0..1
    public event Action OnCaught;

    Transform _player;

    void Update()
    {
        if (isCaught) return;

        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) _player = go.transform;
        }

        bool seesPlayer = (_player != null) && CanSee(_player);

        float dt = Time.deltaTime;

        if (seesPlayer)
        {
            hasLineOfSight = true;
            suspicion01 += dt / Mathf.Max(0.01f, timeToGetCaught);
        }
        else
        {
            hasLineOfSight = false;
            suspicion01 -= dt / Mathf.Max(0.01f, timeToGetCaught) * decayMultiplier;
        }

        suspicion01 = Mathf.Clamp01(suspicion01);
        OnSuspicionChanged?.Invoke(suspicion01);

        if (suspicion01 >= 1f)
        {
            isCaught = true;
            OnCaught?.Invoke();
        }
    }

    bool CanSee(Transform target)
    {
        Transform o = visionOrigin != null ? visionOrigin : transform;

        Vector2 origin = o.position;
        Vector2 targetPos = (Vector2)target.position + targetOffset;
        Vector2 toTarget = targetPos - origin;

        float dist = toTarget.magnitude;
        if (dist > detectionRadius) return false;

        Vector2 dir = toTarget / Mathf.Max(0.0001f, dist);

        // Top-Down 2D: Forward is Up
        Vector2 facing = o.up;

        if (useFOV)
        {
            float ang = Vector2.Angle(facing, dir);
            if (ang > fovAngle * 0.5f) return false;
        }

        // LOS check: Raycast from origin to target
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, blockingMask);
        if (hit.collider != null) return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Transform o = visionOrigin != null ? visionOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(o.position, detectionRadius);

        if (!useFOV) return;

        Vector3 facing3 = o.up; // 2D facing
        Vector3 left  = Quaternion.Euler(0, 0, -fovAngle * 0.5f) * facing3;
        Vector3 right = Quaternion.Euler(0, 0,  fovAngle * 0.5f) * facing3;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(o.position, o.position + left * detectionRadius);
        Gizmos.DrawLine(o.position, o.position + right * detectionRadius);
    }
}
