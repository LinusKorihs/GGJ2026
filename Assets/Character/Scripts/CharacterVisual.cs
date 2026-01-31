using System;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{

    CharSprites _charSprites;


    [SerializeField]
    SpriteRenderer _characterSpriteRenderer;


    public void AssignCharSprites(CharSprites newCharSprites)
    {
        _charSprites = newCharSprites;
    }

    public void SetCharacterDirection(CharacterControllerScript.CharacterDirection direction)
    {
        switch (direction)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _characterSpriteRenderer.sprite = _charSprites.WSprite;
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterSpriteRenderer.sprite = _charSprites.ESprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _characterSpriteRenderer.sprite = _charSprites.SSprite;
                break;

            case CharacterControllerScript.CharacterDirection.North:
                _characterSpriteRenderer.sprite = _charSprites.NSprite;
                break;
        }


    }

    public void UpdateCharacterSprite(CharSprites newSprites, CharacterControllerScript.CharacterDirection characterDirection)
    {
        _charSprites = newSprites;
        SetCharacterDirection(characterDirection);
    }

}
