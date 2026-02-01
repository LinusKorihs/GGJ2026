using TMPro;
using UnityEngine;

public class KillTargetHint : MonoBehaviour
{

    [SerializeField] TMP_Text _textField;
    string _hintText;
    [SerializeField] GameObject _hintObject;
    string maskName;

    void Start()
    {
        maskName = KillTargetRandomizer.Instance.TargetHint;
        _hintText = _textField.text;
        
        UpdateHintText();
    }


    void UpdateHintText()
    {
        _textField.text = _hintText.Replace("{$MaskName}", maskName);

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            _hintObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            _hintObject.SetActive(false);
    }
}
