using Unity.VisualScripting;
using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    private ITriggerReceiver _receiver;

    void Awake()
    {
        _receiver = GetComponentInParent<ITriggerReceiver>();
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        _receiver?.RemoteTriggerEnter2D(collision);
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        _receiver?.RemoteTriggerExit2D(collision);
    }
}