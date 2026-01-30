using Unity.VisualScripting;
using UnityEngine;

public class MaskGiver : MonoBehaviour
{
    public MaskData carriedMask;

    [SerializeField] GameObject _targetHighlighter;


    public void UpdateHighlighterVisuals(bool newState)
    {
        _targetHighlighter.SetActive(newState);
    }
}
