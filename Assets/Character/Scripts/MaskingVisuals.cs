using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MaskingVisuals : MonoBehaviour
{
    [SerializeField] Image _tintImage;

    [SerializeField] Image _UIUseInputHint;
    [SerializeField] Image _UIUseKilHint;

    [SerializeField] Animator _ownAnimator;

    public bool TESTBOOL;


    void Start()
    {
        UpdateVisuals(Color.black, false);
        _ownAnimator.gameObject.SetActive(false);
        _ownAnimator.StopPlayback();
    }

    void Update()
    {
        if (TESTBOOL)
        {
            TESTBOOL = false;
            PlayShapeShiftingAnim();
        }
    }

    public void UpdateVisuals(Color tint, bool state)
    {
        _tintImage.color = tint;
        _tintImage.enabled = state;
    }

    public void SetUseActionHint(bool state)
    {
        _UIUseInputHint.gameObject.SetActive(state);
    }

    public void SetUseKillHint(bool state)
    {
        _UIUseKilHint.gameObject.SetActive(state);
    }

    public void PlayShapeShiftingAnim()
    {
        _ownAnimator.gameObject.SetActive(true);
        _ownAnimator.Play("Transform_animation");
        StartCoroutine(StopAnimDelayed());

    }

    IEnumerator StopAnimDelayed()
    {
        yield return new WaitForSeconds(0.7f);
        StopPlayback();

    }

    public void StopPlayback()
    {
        _ownAnimator.StopPlayback();
        _ownAnimator.gameObject.SetActive(false);
    }

}
