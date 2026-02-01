using System.Collections.Generic;
using UnityEngine;

public class KillTargetRandomizer : MonoBehaviour
{

    [SerializeField] List<KillTarget> _killTargets = new List<KillTarget>();

    public string TargetHint;

    int selectedTarget = 0;
    KillTarget _acutalKillTarget;

    public static KillTargetRandomizer Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        TargetHint = "ERROR";

        selectedTarget = Random.Range(0, _killTargets.Count);
        foreach (KillTarget target in _killTargets)
        {
            target.IsKillTarget = false;
        }
        _killTargets[selectedTarget].IsKillTarget = true;
        _acutalKillTarget = _killTargets[selectedTarget];

        TargetHint = _acutalKillTarget.OwnMaskGiver.CarriedMask.MaskId;

    }
}
