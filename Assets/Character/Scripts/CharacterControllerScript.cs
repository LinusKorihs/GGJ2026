using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllerScript : MonoBehaviour
{
    public enum CharacterDirection { West, East, South}

    [SerializeField]
    CharacterVisual _characterVisual;
    [SerializeField] MaskingSystem _maskingSystem;

    // TODO Set this depending on in which direction our model faces at start
    CharacterDirection _characterDirection = CharacterDirection.East;

    public InputActionAsset _actions;

    [SerializeField] Rigidbody2D _ownRigidbody;

    [Header("Character Parameters")]
    [SerializeField] float _maxSpeed = 10f;
    [SerializeField] float _acceleration = 20f;
    [SerializeField] float _deceleration = 30f;
    [SerializeField] float _inputDeadzone = 0.1f;


    bool _inputsLocked = false;
    int _lockCount =0;


    private InputAction _moveAction;
    private InputAction _useAction;

    public static CharacterControllerScript Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _moveAction = _actions.FindActionMap("Player").FindAction("Move");
        _useAction = _actions.FindActionMap("Player").FindAction("Jump");
        _useAction.performed += OnUseAction;
    }

    void OnEnable()
    {
        _actions.FindActionMap("Player").Enable();
    }
    void OnDisable()
    {
        _actions.FindActionMap("Player").Disable();
    }


    public void LockControls()
    {
        _lockCount++;
        _inputsLocked = true;
    }


    public void UnlockControls()
    {
        _lockCount--;
        if(_lockCount <= 0)
        _inputsLocked = false;
    }

    private void OnUseAction(InputAction.CallbackContext context)
    {
        if (_inputsLocked)
            return;

        LockControls();

        _maskingSystem.TryMaskStealing();
    }


    void FixedUpdate()
    {
        Vector2 currentVelocity = _ownRigidbody.linearVelocity;
        Vector2 moveDirection = GetMoveDirection(_moveAction.ReadValue<Vector2>());

        Vector2 targetVelocity = moveDirection * _maxSpeed;


        //force zero speed when movement frozen
        if (_inputsLocked)
        {
            targetVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
        }



        //if our moveDirection is not zero accelerate, else deccelerate
        float tempAcceleration = moveDirection == Vector2.zero ? _deceleration : _acceleration;
        _ownRigidbody.linearVelocity =
                Vector2.MoveTowards(currentVelocity, targetVelocity, tempAcceleration * Time.fixedDeltaTime);

        SetCharacterDirection(moveDirection);
    }

    Vector2 GetMoveDirection(Vector2 input)
    {
        if (input.magnitude < _inputDeadzone)
            return Vector2.zero;

        return input.normalized;
    }


    void SetCharacterDirection(Vector2 direction)
    {

        int oldDirection = (int)_characterDirection;

        if (direction.x >= _inputDeadzone)
            _characterDirection = CharacterDirection.East;

        else if (direction.x <= -_inputDeadzone)
            _characterDirection = CharacterDirection.West;

        else if(direction.y <= -_inputDeadzone && direction.y < (Mathf.Abs(direction.x)*-1))
            _characterDirection = CharacterDirection.South;

        //only call visual, if we actually switched direction
        if (oldDirection != (int)_characterDirection)
            _characterVisual.SetCharacterDirection(_characterDirection);
    }

    public void SetCharacterSprite(Sprite sprite)
    {
        _characterVisual.UpdateCharacterSprite(sprite);
    }
}



