using UnityEngine;

public class NPCSearchVisual : MonoBehaviour
{
    [Header("References")]
    public NPCVision vision;
    public NPCPatience patience;
    public Transform maskTransform;


    [Tooltip("Child transform that holds the Base sprite (optional, only for ordering).")]
    public Transform baseObj;

    [Tooltip("Child transform that holds the Patience sprite (optional, only for ordering).")]
    public Transform patienceObj;

    [Tooltip("SpriteMask on SearchVisual (used to reveal Patience sprite).")]
    public SpriteMask mask;

    [Header("Visual Settings")]
    [Tooltip("If true, SearchVisual rotates to match NPCVision facing.")]
    public bool rotateWithFacing = true;

    [Tooltip("Scale multiplier for width (angle) and length (distance).")]
    public float visualScaleMultiplier = 1f;

    [Tooltip("How much the mask grows with patience. 0 = hidden, 1 = full.")]
    public float maskMaxScale = 1f;

    [Tooltip("If your visual cone sprite points UP by default, keep this 0. If it points RIGHT, use -90, etc.")]
    public float spriteFacingAngleOffset = 0f;

    [Header("Debug")]
    public bool debugLogs = false;

    private void Awake()
    {
        if (vision == null) vision = GetComponentInParent<NPCVision>();
        if (patience == null) patience = GetComponentInParent<NPCPatience>();
        if (mask == null) mask = GetComponentInChildren<SpriteMask>();
        if (maskTransform == null && mask != null) maskTransform = mask.transform;

        if (debugLogs) Debug.Log($"[NPCSearchVisual:{name}] Init vision={(vision != null)} patience={(patience != null)} mask={(mask != null)}", this);
    }

    private void LateUpdate()
    {
        if (vision == null || patience == null) return;

        ApplyFovScale();

        if (rotateWithFacing) ApplyRotation();

        ApplyPatienceReveal();
    }

    private void ApplyFovScale()
    {
        float halfRad = (vision.viewAngle * 0.5f) * Mathf.Deg2Rad;

        // "Width at distance 1" factor; grows steeply with angle
        float widthFactor = Mathf.Tan(halfRad);

        // You may need to tweak these multipliers depending on your sprite size.
        float sx = Mathf.Max(0.001f, widthFactor * vision.viewDistance) * visualScaleMultiplier;
        float sy = Mathf.Max(0.001f, vision.viewDistance) * visualScaleMultiplier;

        transform.localScale = new Vector3(sx, sy, 1f);
    }

    private void ApplyRotation()
    {
        Vector2 facing = vision.GetFacing2D();
        if (facing.sqrMagnitude < 0.001f) facing = Vector2.up;

        float ang = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        transform.localRotation = Quaternion.Euler(0f, 0f, ang + spriteFacingAngleOffset);
    }

    private void ApplyPatienceReveal()
    {
        if (maskTransform == null) return;

        float t = patience.Normalized; // 0..1
        float s = Mathf.Lerp(0f, maskMaxScale, t);

        maskTransform.localScale = new Vector3(s, s, 1f);
    }
}
