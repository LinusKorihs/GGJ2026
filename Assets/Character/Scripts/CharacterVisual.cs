using System;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{

    [SerializeField]
    Transform _characterSprite;

    [SerializeField]
    Sprite _eastWestSprite;

    [SerializeField]
    Sprite _southSprite;

    SpriteRenderer _characterSpriteRenderer;

    void Awake()
    {
        _characterSpriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void SetCharacterDirection(CharacterControllerScript.CharacterDirection direction)
    {
        switch (direction)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _characterSprite.localScale = new Vector3(-1, 1, 1);
                 _characterSpriteRenderer.sprite = _eastWestSprite;
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterSprite.localScale = new Vector3(1, 1, 1);
                  _characterSpriteRenderer.sprite = _eastWestSprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _characterSprite.localScale = new Vector3(1, 1, 1);
                _characterSpriteRenderer.sprite = _southSprite;
                break;
        }
    }

    public void UpdateCharacterSprite(Sprite sprite)
    {
        _characterSpriteRenderer.sprite = sprite;
    }

}
