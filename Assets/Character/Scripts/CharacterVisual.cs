using System;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{

    [SerializeField]
    Transform _characterSprite;

    SpriteRenderer _characterSpriteRenderer;

    public void SetCharacterDirection(CharacterControllerScript.CharacterDirection direction)
    {
        switch (direction)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _characterSprite.localScale = new Vector3(-1, 1, 1);
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterSprite.localScale = new Vector3(1, 1, 1);
                break;
        }
    }

}
