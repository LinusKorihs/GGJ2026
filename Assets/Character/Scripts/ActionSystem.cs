using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : MonoBehaviour, ITriggerReceiver
{

    public MaskData CurrentMask;


    public bool TargetInRange
    {
        get { return _targetsInRange.Count > 0; }
        private set { TargetInRange = value; }
    }



    [SerializeField]
    MaskingVisuals _maskingVisuals;


    List<GameObject> _targetsInRange = new List<GameObject>();
    GameObject _currentTarget;
    GameObject _targetAtMinigameStart;



    [Header("Technical Stuff - Dont touch")]
    [SerializeField] LayerMask _validLayerForCollision;
    [SerializeField]
    MaskingMinigame _maskingMinigame;

    [SerializeField] bool _canShapeshiftVIP = false;


    Coroutine _minigameCoroutine;

    bool _maskingMinigameRunning = false;


    void Update()
    {
        //continously calculate nearest target if we have more than one target
        if (_targetsInRange.Count > 1)
            RecalculateTarget();
    }

    #region Targeting

    public void RemoteTriggerEnter2D(Collider2D collision)
    {
        //only continue if collision is between two objects with valid layers
        // please dont ask me about this bit mask stuff ( 1 << )
        if ((_validLayerForCollision & (1 << collision.gameObject.layer)) == 0)
            return;

        if (!_targetsInRange.Contains(collision.gameObject)) _targetsInRange.Add(collision.gameObject);
        RecalculateTarget();
    }

    public void RemoteTriggerExit2D(Collider2D collision)
    {
        if ((_validLayerForCollision & (1 << collision.gameObject.layer)) == 0)
            return;

        _targetsInRange.Remove(collision.gameObject);
        RecalculateTarget();
    }

    void RecalculateTarget()
    {
        GameObject oldTarget = _currentTarget;
        //early out, if our target list is empty, remove old target highlight
        if (_targetsInRange.Count <= 0)
        {
            _currentTarget = null;
            UpdateTargetVisual(oldTarget.GetComponent<MaskGiver>(), null);
            // no target , disable ui
            _maskingVisuals.SetUseActionHint(false);
            _maskingVisuals.SetUseKillHint(false);
            return;
        }



        // set distance to max
        float distanceToCurrentTarget = float.MaxValue;

        // if we have no target, keep max, else set current distance as baseline
        if (_currentTarget != null)
            distanceToCurrentTarget = Vector2.Distance(this.transform.position, _currentTarget.transform.position);


        //iterate list, check if one of them is closer, then take that as new target
        foreach (var target in _targetsInRange)
        {
            if (Vector2.Distance(this.transform.position, target.transform.position) < distanceToCurrentTarget)
                _currentTarget = target;
        }


        // if we have no old target, dont turn anything off, just turn on, else do both
        if (oldTarget == null)
            UpdateTargetVisual(null, _currentTarget.GetComponent<MaskGiver>());
        else
            UpdateTargetVisual(oldTarget.GetComponent<MaskGiver>(), _currentTarget.GetComponent<MaskGiver>());


        _maskingVisuals.SetUseActionHint(true);
        
        // new target , enable ui
        if (_currentTarget.tag == "VIP" && !_canShapeshiftVIP)
            _maskingVisuals.SetUseActionHint(false);

        if (_currentTarget.tag == "VIP")
            _maskingVisuals.SetUseKillHint(true);
        else
            _maskingVisuals.SetUseKillHint(false);
    }

    void UpdateTargetVisual(MaskGiver oldTarget, MaskGiver newTarget)
    {
        //do nothing if we dont have an old target
        if (oldTarget != null)
            // turn off old target visual 
            oldTarget.UpdateHighlighterVisuals(false);

        //do nothing if we dont have a new target
        if (newTarget != null)
            //else do highlight for new target
            newTarget.UpdateHighlighterVisuals(true);

    }
    #endregion

    #region Killing

    public void TryKill()
    {

        //if noone in range, dont let him try
        if (!TargetInRange)
        {
            CharacterControllerScript.Instance.UnlockControls();
            return;
        }

        //if target isnt killable, early out
        if (!IsKillableTargetInRange())
        {
            CharacterControllerScript.Instance.UnlockControls();
            return;
        }


        if (_currentTarget.GetComponentInParent<KillTarget>().IsKillTarget)
        {
            CharacterControllerScript.Instance.PlayTargetDeathScene(DeathScreen.DeathVersion.CorrectKill, _currentTarget.GetComponent<MaskGiver>().CarriedMask);
            return;
        }
        else
        {
            CharacterControllerScript.Instance.PlayTargetDeathScene(DeathScreen.DeathVersion.WrongKill, _currentTarget.GetComponent<MaskGiver>().CarriedMask);
            return;
        }

    }

    bool IsKillableTargetInRange()
    {
        return _currentTarget.tag == "VIP";
    }
    #endregion

    #region MaskStealing

    public void TryMaskStealing()
    {
        //if noone in range, dont let him try
        if (!TargetInRange)
        {
            CharacterControllerScript.Instance.UnlockControls();
            return;
        }




        //early out if coroutine is already running. Should prevent double usage!
        if (_minigameCoroutine != null)
        {
            CharacterControllerScript.Instance.UnlockControls();
            return;
        }

        _minigameCoroutine = StartCoroutine(StartMinigame());
    }

    IEnumerator StartMinigame()
    {
        //register event finish callback before we start the event // starts listening
        _maskingMinigame.OnEventFinishedSuccessful += MinigameFinishedCallback;
        //assign this, so we dont lose the target, if we get a recalculation during minigame
        _targetAtMinigameStart = _currentTarget;
        _maskingMinigame.StartStealingMinigame(_targetAtMinigameStart);
        _maskingMinigameRunning = true;

        //will run forever until we get the callback. Non blocking though since its coroutine
        while (_maskingMinigameRunning)
        {
            yield return null;
        }

        //register event finish callback before after the event is done // stop listening
        _maskingMinigame.OnEventFinishedSuccessful -= MinigameFinishedCallback;
        _minigameCoroutine = null;
    }

    void MinigameFinishedCallback(bool success)
    {
        if (success)
        {
            _maskingMinigameRunning = false;
            EquipMask(_targetAtMinigameStart.GetComponentInParent<MaskGiver>().CarriedMask);
            CharacterControllerScript.Instance.UnlockControls();
        }
        else
        {
            CharacterControllerScript.Instance.PlayPlayerDeathScene();
        }
    }


    public void EquipMask(MaskData mask)
    {
        CurrentMask = mask;
        ApplyMaskEffects(mask);
    }

    void ApplyMaskEffects(MaskData mask)
    {
        CharacterControllerScript.Instance.SetCharacterSprite(CurrentMask.MaskSprites);
        _maskingVisuals.UpdateVisuals(mask.ScreenTint, mask.ShouldTintScreen);
        CharacterControllerScript.Instance.UpdateAnimator(CurrentMask.WalkingController);
    }



    #endregion
}
