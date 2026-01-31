using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MaskingMinigame : MonoBehaviour
{
    [SerializeField] GameObject _minigameCamera;
    [SerializeField] GameObject _maskingMinigameObject;

    [SerializeField] RectMask2D _uimask;

    float _uiMaskMaxPadding;

    public Action<bool> OnEventFinishedSuccessful;

    //hardcoded offset, so the camera dont just sit on the object
    Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);

    bool _eventActive = false;

    void Awake()
    {
        //emergency find, because minigame camera needs to be outside of char controller, so this connection cannot be saved in char controller prefab. 
        // And this might break for a new scene
        if (_minigameCamera == null)
            GameObject.FindGameObjectWithTag("MiniGameCamera");

        //Rect Mask Right is Z for whatever reason
        _uiMaskMaxPadding = _uimask.padding.z;


    }
    public void StartStealingMinigame(GameObject target)
    {
        SetupMinigame(target);
        StartCoroutine(RunEvent());
    }


    public void SetupMinigame(GameObject target)
    {
        _minigameCamera.SetActive(true);
        _maskingMinigameObject.SetActive(true);
        _minigameCamera.transform.position = target.transform.position + _cameraOffset;
        GameManager.Instance.SlowSpeed();
    }

    public IEnumerator RunEvent()
    {

        float minigamePercent = 0f;
        _eventActive = true;
        StartCoroutine(DelayedEventFinish());

        while (_eventActive)
        {
            yield return new WaitForSeconds(0.1f);

            minigamePercent += 0.05f;
            // minigame logic


            SetProgressBarScale(minigamePercent);

            if (minigamePercent >= 1)
            {
                _eventActive = false;
            }
        }



        // fail / success fork
        EndMinigame();
    }


    IEnumerator DelayedEventFinish()
    {
        yield return new WaitForSeconds(2.0f);
        _eventActive = false;
    }

    public void EndMinigame()
    {
        _minigameCamera.SetActive(false);
        _maskingMinigameObject.SetActive(false);
        GameManager.Instance.NormalSpeed();

        //fail option
        OnEventFinishedSuccessful?.Invoke(true);
    }


    #region  Visual
    void SetProgressBarScale(float percent)
    {
        //maxpadding - maxpadding * percent
        _uimask.padding = new Vector4(0f, 0f, _uiMaskMaxPadding - (_uiMaskMaxPadding * percent), 0f);
    }

    #endregion

}
