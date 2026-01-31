using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MaskingMinigame : MonoBehaviour
{
    public InputActionAsset _actions;
    [SerializeField] GameObject _minigameCamera;
    [SerializeField] GameObject _maskingMinigameObject;

    [SerializeField] RectMask2D _uimask;
    [SerializeField] Image _timerCircle;

    [Header("Tweak these values for Event Settings")]
    [SerializeField] float _eventDuration;
    [SerializeField] float _percentageGainPerButtonPress = 0.1f;

    float _uiMaskMaxPadding;

    public Action<bool> OnEventFinishedSuccessful;

    private InputAction _useAction;

    //hardcoded offset, so the camera dont just sit on the object
    Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);

    bool _eventActive = false;
    float _minigamePercent = 0f;

    void Awake()
    {
        //emergency find, because minigame camera needs to be outside of char controller, so this connection cannot be saved in char controller prefab. 
        // And this might break for a new scene
        if (_minigameCamera == null)
            GameObject.FindGameObjectWithTag("MiniGameCamera");

        //Rect Mask Right is Z for whatever reason
        _uiMaskMaxPadding = _uimask.padding.z;

        _useAction = _actions.FindActionMap("Player").FindAction("Jump");
        _useAction.performed += OnUseAction;


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

    void OnUseAction(InputAction.CallbackContext context)
    {
        //early out if event is not active
        if (!_eventActive)
            return;

        //if event is active increase progress
        _minigamePercent += _percentageGainPerButtonPress;
    }

    public IEnumerator RunEvent()
    {
        float eventTimer = 0f;
        _minigamePercent = 0f;
        _eventActive = true;
        bool eventSuccess = false;

        while (_eventActive)
        {
            yield return new WaitForSeconds(0.1f);

            eventTimer += 0.1f;

            float timerPercent = eventTimer / _eventDuration;

            SetTimerScale(timerPercent);
            SetProgressBarScale(_minigamePercent);

            if (_minigamePercent >= 1)
            {
                //event win
                _eventActive = false;
                eventSuccess = true;
                break;
            }

            if (eventTimer >= _eventDuration)
            {
                //event failed
                _eventActive = false;
                break;
            }
        }



        EndMinigame(eventSuccess);
    }

    public void EndMinigame(bool eventSuccess)
    {
        _minigameCamera.SetActive(false);
        _maskingMinigameObject.SetActive(false);
        GameManager.Instance.NormalSpeed();

        if (eventSuccess)
            OnEventFinishedSuccessful?.Invoke(true);
        else
            OnEventFinishedSuccessful?.Invoke(false);
    }


    #region  Visual
    void SetProgressBarScale(float percent)
    {
        //maxpadding - maxpadding * percent
        _uimask.padding = new Vector4(0f, 0f, _uiMaskMaxPadding - (_uiMaskMaxPadding * percent), 0f);
    }

    void SetTimerScale(float percent)
    {
        _timerCircle.fillAmount = percent;
    }

    #endregion

}
