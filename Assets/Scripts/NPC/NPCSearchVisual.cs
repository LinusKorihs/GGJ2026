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

    [Tooltip("If true, mask grows from origin forward instead of from center.")]
    public bool anchoredFillFromOrigin = true;

    [Tooltip("Local space offset for the reveal mask origin (0 means mask pivot sits exactly at SearchVisual origin).")]
    public Vector2 maskLocalOriginOffset = Vector2.zero;

    [Header("Debug")]
    public bool debugLogs = false;

    private Vector3 maskBaseLocalPos;

    private void Awake()
    {
        if (vision == null) vision = GetComponentInParent<NPCVision>();
        if (patience == null) patience = GetComponentInParent<NPCPatience>();
        if (mask == null) mask = GetComponentInChildren<SpriteMask>();
        if (maskTransform == null && mask != null) maskTransform = mask.transform;
        if (debugLogs) Debug.Log($"[NPCSearchVisual:{name}] Init vision={(vision != null)} patience={(patience != null)} mask={(mask != null)}", this);
        if (maskTransform != null) maskBaseLocalPos = maskTransform.localPosition;
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
        if (maskTransform == null || mask == null || mask.sprite == null) return;

        float t = patience.Normalized; // 0..1

        // Scale Y grows with patience
        Vector3 s = maskTransform.localScale;
        s.x = maskMaxScale; // keep full width
        s.y = Mathf.Lerp(0f, maskMaxScale, t);
        s.z = 1f;
        maskTransform.localScale = s;

        if (!anchoredFillFromOrigin) return;

        // Use sprite bounds height to offset correctly
        float spriteHeight = mask.sprite.bounds.size.y;  // in local units
        float visibleHeight = spriteHeight * s.y;

        Vector3 pos = maskBaseLocalPos;
        pos.x += maskLocalOriginOffset.x;
        pos.y = maskBaseLocalPos.y + maskLocalOriginOffset.y + (visibleHeight * 0.5f);
        maskTransform.localPosition = pos;
    }

}
