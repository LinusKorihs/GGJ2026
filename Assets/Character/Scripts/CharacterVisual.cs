using UnityEditor.Animations;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{

    CharSprites _charSprites;


    [SerializeField]
    SpriteRenderer _characterStillSpriteRenderer;

    [SerializeField]
    SpriteRenderer _characterAnimationSpriteRenderer;

    [SerializeField] Animator _ownAnimator;

    CharacterControllerScript.CharacterDirection _oldDirection;
    bool _isInWalkingAnim;


    public void AssignCharSprites(CharSprites newCharSprites)
    {
        _charSprites = newCharSprites;
    }

    public void SetCharacterDirection(CharacterControllerScript.CharacterDirection direction)
    {
        switch (direction)
        {
            case CharacterControllerScript.CharacterDirection.West:
                _characterStillSpriteRenderer.sprite = _charSprites.WSprite;
                break;

            case CharacterControllerScript.CharacterDirection.East:
                _characterStillSpriteRenderer.sprite = _charSprites.ESprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                _characterStillSpriteRenderer.sprite = _charSprites.SSprite;
                break;

            case CharacterControllerScript.CharacterDirection.North:
                _characterStillSpriteRenderer.sprite = _charSprites.NSprite;
                break;
        }


    }

    public void SetNewAnimator(AnimatorController animator)
    {
        _ownAnimator.runtimeAnimatorController = animator;
    }

    public void UpdateCharacterSprite(CharSprites newSprites, CharacterControllerScript.CharacterDirection characterDirection)
    {
        _charSprites = newSprites;
        SetCharacterDirection(characterDirection);
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
