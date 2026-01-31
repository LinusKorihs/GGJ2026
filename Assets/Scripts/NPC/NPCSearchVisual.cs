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

    [Tooltip("If your visual cone sprite points UP by default, keep this 0. If it points RIGHT, use -90, etc.")]
    public float spriteFacingAngleOffset = 0f;

    [Tooltip("If true, mask grows from origin forward instead of from center.")]
    public bool anchoredFillFromOrigin = true;

    public Vector3 scaleObj = new Vector3(0.45f, 0.45f, 1f);

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
        transform.localScale = scaleObj;
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
        mask.alphaCutoff = Mathf.Lerp(1f, 0f, t);
    }

}
