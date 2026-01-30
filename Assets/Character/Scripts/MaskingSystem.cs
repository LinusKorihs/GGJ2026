using System;
using System.Collections.Generic;
using UnityEngine;

public class MaskingSystem : MonoBehaviour, ITriggerReceiver
{

    public MaskData CurrentMask;

    //public List<MaskData> StoredMasks;



    [SerializeField]
    MaskingVisuals _maskingVisuals;


    List<GameObject> _targetsInRange = new List<GameObject>();
    GameObject _currentTarget;


    [Header("Technical Stuff - Dont touch")]
    [SerializeField] LayerMask _validLayerForCollision;




    public void EquipMask(MaskData mask)
    {
        CurrentMask = mask;
        ApplyMaskEffects(mask);
    }

    void ApplyMaskEffects(MaskData mask)
    {
        _maskingVisuals.UpdateVisuals(mask.screenTint, mask.shouldTintScreen);
    }

    void Update()
    {
        //continously calculate nearest target if we have more than one target
        if (_targetsInRange.Count > 1)
            RecalculateTarget();
    }

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
}
