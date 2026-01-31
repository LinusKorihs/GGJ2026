using UnityEditor.Animations;
using UnityEngine;

public class NPCVisuals : MonoBehaviour
{
    [SerializeField] MaskGiver _ownMaskGiver;

    [SerializeField] SpriteRenderer _characterStillSpriteRenderer;

    [SerializeField]
    SpriteRenderer _characterAnimationSpriteRenderer;

    CharacterControllerScript.CharacterDirection _npcDirection;

    [SerializeField] Animator _ownAnimator;

    NPCMovement _ownMovement;

    CharacterControllerScript.CharacterDirection _oldDirection;
    bool _isInWalkingAnim;


    void Awake()
    {
        _ownMovement = GetComponentInParent<NPCMovement>();
        if (_ownMovement == null)
            Debug.LogError("NPC Movement script not found");


        _ownMovement.NPCStateChanged += NPCStateChanged;
    }

    void NPCStateChanged(NPCMovement.NpcState state)
    {
        switch (state)
        {
            case NPCMovement.NpcState.Moving:
                StartWalk(_ownMovement.GetNPCLookDirection());
                break;

            case NPCMovement.NpcState.Stationary:
                StopWalk();
                break;

            default:
                StopWalk();
                break;
        }
    }


    void LateUpdate()
    {
        // probably not the best to do this every frame, but for now i see no other option to listen to a rotation change
        UpdateNPCLookDirection(_ownMovement.GetNPCLookDirection());
    }


    void OnDisable()
    {
        _ownMovement.NPCStateChanged -= NPCStateChanged;
    }

    void Start()
    {
        _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
    }

    public void SetNewAnimator(AnimatorController animator)
    {
        _ownAnimator.runtimeAnimatorController = animator;
    }

    public void UpdateNPCLookDirection(CharacterControllerScript.CharacterDirection newDirection)
    {
        _npcDirection = newDirection;
        ChangeNPCSprite();
    }

    void ChangeNPCSprite()
    {
        Sprite newHighlightSprite;
        switch (_npcDirection)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.WSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.WSprite;
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.ESprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.SSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.SSprite;
                break;

            case CharacterControllerScript.CharacterDirection.North:
                _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.NSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.NSprite;
                break;

            default:
                _characterStillSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.ESprite;
                break;
        }

        _ownMaskGiver.UpdateHighlighterVisualDirection(newHighlightSprite);
    }


    public void StartWalk(CharacterControllerScript.CharacterDirection direction)
    {

        //Early out, if our walking state and our direction are same as before to prevent restarting the anim over and over
        if (_oldDirection == direction && _isInWalkingAnim)
            return;

        _characterStillSpriteRenderer.gameObject.SetActive(false);
        _characterAnimationSpriteRenderer.gameObject.SetActive(true);


        switch (direction)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _ownAnimator.SetTrigger("GoLeft");
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _ownAnimator.SetTrigger("GoRight");
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _ownAnimator.SetTrigger("GoSouth");
                break;

            case CharacterControllerScript.CharacterDirection.North:
                _ownAnimator.SetTrigger("GoNorth");
                break;
        }

        _oldDirection = direction;
        _isInWalkingAnim = true;
    }

    public void StopWalk()
    {
        if (!_isInWalkingAnim)
            return;
            
        _characterStillSpriteRenderer.gameObject.SetActive(true);
        _characterAnimationSpriteRenderer.gameObject.SetActive(false);
        _isInWalkingAnim = false;
        _ownAnimator.SetTrigger("Stop");
    }

}
