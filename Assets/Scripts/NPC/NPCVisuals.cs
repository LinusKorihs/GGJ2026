using UnityEngine;

public class NPCVisuals : MonoBehaviour
{
    [SerializeField] MaskGiver _ownMaskGiver;

    [SerializeField] SpriteRenderer _characterSpriteRenderer;

    CharacterControllerScript.CharacterDirection _npcDirection;


    void Start()
    {
        _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
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
                _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.WSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.WSprite;
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.ESprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.SSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.SSprite;
                break;

            case CharacterControllerScript.CharacterDirection.North:
                _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.NSprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.NSprite;
                break;

            default:
                _characterSpriteRenderer.sprite = _ownMaskGiver.CarriedMask.MaskSprites.ESprite;
                newHighlightSprite = _ownMaskGiver.CarriedMask.HighlightSprites.ESprite;
                break;
        }

        _ownMaskGiver.UpdateHighlighterVisualDirection(newHighlightSprite);
    }
}
