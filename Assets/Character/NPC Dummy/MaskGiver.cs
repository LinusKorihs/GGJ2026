using UnityEngine;

public class MaskGiver : MonoBehaviour
{
    public MaskData CarriedMask;


    [SerializeField] SpriteRenderer _targetHighlighter;


    public void UpdateHighlighterVisuals(bool newState)
    {
        //TODO Need to swap out Sprite according to direction
        _targetHighlighter.gameObject.SetActive(newState);
    }

    public void UpdateHighlighterVisualDirection(Sprite sprite)
    {
        _targetHighlighter.sprite = sprite;
    }
}
