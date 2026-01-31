using UnityEngine;
using UnityEngine.UI;

public class MaskingVisuals : MonoBehaviour
{
    [SerializeField] Image _tintImage;

    [SerializeField] Image _UIUseInputHint;


    void Start()
    {
        UpdateVisuals(Color.black, false);
    }

    public void UpdateVisuals(Color tint, bool state)
    {
        _tintImage.color = tint;
        _tintImage.enabled = state;
    }

    public void SetUseActionHint(bool state)
    {
        _UIUseInputHint.gameObject.SetActive(state);
    }

}
