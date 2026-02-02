using System;
using System.Collections;
using Unity.VisualScripting;
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
    [SerializeField] GameObject _cursorGameObject;
    [SerializeField] GameObject _hitMarkerGameObject;

    [SerializeField] GameObject _targetGameObject;

    [Header("Tweak these values for Event Settings")]
    [SerializeField] float _eventDuration;
    //[SerializeField] float _percentageGainPerButtonPress = 0.1f;

    [Header("Cursor Event Parameters")]
    [SerializeField] float _cursorMinPos = -350f;
    [SerializeField] float _cursorSpeed = 10f;
    [SerializeField] float _gainPerHit = 0.1f;
    [SerializeField] float _perfectHitFactor = 5f;

    [SerializeField] float _hitmarkerSpeed = 1;

    [SerializeField] float _hitCooldown = 0.25f;

    float _uiMaskMaxPadding;

    public Action<bool> OnEventFinishedSuccessful;

    private InputAction _useAction;

    //hardcoded offset, so the camera dont just sit on the object
    Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);

    bool _eventActive = false;
    float _minigamePercent = 0f;
    Coroutine _moveCursorCoroutine;
    bool _cursorMoveRight = true;

    bool _onHitCooldown = false;
    float _hitCooldownTimer = 0;

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
        _moveCursorCoroutine = StartCoroutine(MoveCursor());
    }


    void Update()
    {
        if (!_onHitCooldown)
            return;

        _hitCooldownTimer += Time.deltaTime;
        if (_hitCooldownTimer >= _hitCooldown)
        {
            _onHitCooldown = false;
            _hitCooldownTimer = 0;
        }

    }


    public void SetupMinigame(GameObject target)
    {
        _onHitCooldown = false;
        _cursorMoveRight = true;
        _minigameCamera.SetActive(true);
        _maskingMinigameObject.SetActive(true);
        _minigameCamera.transform.position = target.transform.position + _cameraOffset;
        _cursorGameObject.transform.localPosition = new Vector3(_cursorMinPos, _cursorGameObject.transform.localPosition.y, _cursorGameObject.transform.localPosition.z);
        GameManager.Instance.SlowSpeed();
    }

    void OnUseAction(InputAction.CallbackContext context)
    {
        //early out if event is not active
        if (!_eventActive)
            return;

        if (_onHitCooldown)
            return;

        HandleClickInput();

        // OLD MINIGAME
        //if event is active increase progress
        //  _minigamePercent += _percentageGainPerButtonPress;
    }

    void HandleClickInput()
    {
        _onHitCooldown = true;

        float distanceToTarget = Vector2.Distance(_targetGameObject.transform.position, _cursorGameObject.transform.position);
        if (distanceToTarget <= 1)
        {
            //closeness to targe = 0-1f with 1 for perfect hit
            float closenessOfHit = 1 - distanceToTarget;
            _onHitCooldown = true;
            _minigamePercent += _gainPerHit * closenessOfHit * _perfectHitFactor;

            if (closenessOfHit >= 0.5)
                StartCoroutine(DisplayHitMarker());

        }
        else
        {
        }
    }

    IEnumerator MoveCursor()
    {
        while (true)
        {
            int moveRight = _cursorMoveRight ? 1 : -1;
            float dt = Time.unscaledDeltaTime;
            _cursorGameObject.transform.localPosition += new Vector3(_cursorSpeed * moveRight * dt, 0, 0);
            //if we are out right, turn around

            if (_cursorMoveRight && _cursorGameObject.transform.localPosition.x >= (_cursorMinPos * -1))
            {
                _cursorMoveRight = false;
            }
            //else if we are out left, turn around
            else if (!_cursorMoveRight && _cursorGameObject.transform.localPosition.x <= _cursorMinPos)
            {
                _cursorMoveRight = true;
            }

            yield return null;

        }
    }

    IEnumerator DisplayHitMarker()
    {
        _hitMarkerGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        while(_hitMarkerGameObject.transform.localScale.x > 0.1f)
        {
            yield return new WaitForSeconds(0.02f);
            Vector3 currentScale = _hitMarkerGameObject.transform.localScale;
            _hitMarkerGameObject.transform.localScale = new Vector3(currentScale.x - 0.1f * _hitmarkerSpeed,
            currentScale.y - 0.1f * _hitmarkerSpeed,
            currentScale.z - 0.1f * _hitmarkerSpeed);
        }
        _hitMarkerGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
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



    // OLD MINIGAME
    // public IEnumerator RunEvent()
    // {
    //     float eventTimer = 0f;
    //     _minigamePercent = 0f;
    //     _eventActive = true;
    //     bool eventSuccess = false;

    //     while (_eventActive)
    //     {
    //         yield return new WaitForSeconds(0.1f);

    //         eventTimer += 0.1f;

    //         float timerPercent = eventTimer / _eventDuration;

    //         SetTimerScale(timerPercent);
    //         SetProgressBarScale(_minigamePercent);

    //         if (_minigamePercent >= 1)
    //         {
    //             //event win
    //             _eventActive = false;
    //             eventSuccess = true;
    //             break;
    //         }

    //         if (eventTimer >= _eventDuration)
    //         {
    //             //event failed
    //             _eventActive = false;
    //             break;
    //         }
    //     }



    //     EndMinigame(eventSuccess);
    // }

    public void EndMinigame(bool eventSuccess)
    {
        StopCoroutine(_moveCursorCoroutine);

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
