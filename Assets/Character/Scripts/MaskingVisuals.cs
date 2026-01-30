using UnityEngine;
using UnityEngine.UI;

public class MaskingVisuals : MonoBehaviour
{
    [SerializeField] Image _tintImage;

    void Start()
    {
        UpdateVisuals(Color.black, false);
    }

    public void UpdateVisuals(Color tint, bool enabled)
    {
        _tintImage.color= tint; 
        _tintImage.enabled = enabled;
    }
}
