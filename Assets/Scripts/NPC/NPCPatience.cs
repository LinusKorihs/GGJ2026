using UnityEngine;
using UnityEngine.Events;

public class NPCPatience : MonoBehaviour
{
    [Header("References")]
    public NPCVision vision;

    [Header("Patience")]
    [SerializeField] private float patience = 0f;
    public float patienceGainPerSecond = 35f;
    public float patienceLossPerSecond = 25f;
    public float patienceMax = 100f;
    public float caughtThreshold = 100f;

    [Header("Events")]
    public UnityEvent onCaught;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool hasCaught;

    public float Patience => patience;
    public float Normalized => Mathf.Approximately(patienceMax, 0f) ? 0f : Mathf.Clamp01(patience / patienceMax);
    public bool HasCaught => hasCaught;

    private void Awake()
    {
        if (vision == null) vision = GetComponentInChildren<NPCVision>();
        if (debugLogs) Debug.Log($"[NPCPatience:{name}] Init vision={(vision != null)}", this);
    }

    private void Update()
    {
        if (hasCaught) return;
        if (vision == null) return;

        bool seeing = vision.IsSeeingPlayer;
        float dt = Time.deltaTime;

        float before = patience;

        if (seeing) patience += patienceGainPerSecond * dt;
        else if (patience > 0f) patience -= patienceLossPerSecond * dt;

        patience = Mathf.Clamp(patience, 0f, patienceMax);

        if (debugLogs && Mathf.Abs(before - patience) > 0.01f)
            Debug.Log($"[NPCPatience:{name}] {(seeing ? "+" : "-")} -> {patience:0.0}/{patienceMax}", this);

        if (patience >= caughtThreshold)
        {
            hasCaught = true;
            patience = caughtThreshold;

            if (debugLogs) Debug.LogWarning($"[NPCPatience:{name}] CAUGHT!", this);
            onCaught?.Invoke();
        }
    }
}
