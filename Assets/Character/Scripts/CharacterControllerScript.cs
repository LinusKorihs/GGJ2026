using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllerScript : MonoBehaviour
{

    public InputActionAsset _actions;

    [SerializeField] Rigidbody2D _ownRigidbody;

    [Header("Character Parameters")]
    [SerializeField]
    private float _moveSpeed;


    private InputAction _moveAction;
    private InputAction _useAction;


    void Awake()
    {
        _moveAction = _actions.FindActionMap("Player").FindAction("Move");
        _useAction = _actions.FindActionMap("Player").FindAction("Jump").performed += OnUseAction;
    }

    void OnEnable()
    {
        _actions.FindActionMap("Player").Enable();
    }
    void OnDisable()
    {
        _actions.FindActionMap("Player").Disable();
    }

    private InputAction OnUseAction(InputAction.CallbackContext context)
    {

        Debug.Log("Space pressed");
        return null;
    }

    void FixedUpdate()
    {
        Vector2 moveDirection = _moveAction.ReadValue<Vector2>();
        Debug.Log(moveDirection);
        this.transform.position += new Vector3(moveDirection.x, moveDirection.y, 0) * _moveSpeed * Time.deltaTime;
    }
    
}



