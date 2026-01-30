using UnityEngine;

public interface ITriggerReceiver
{
    void RemoteTriggerEnter2D(Collider2D collision);
    void RemoteTriggerExit2D(Collider2D collision);
}