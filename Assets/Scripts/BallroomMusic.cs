using UnityEngine;

public class BallroomMusic : MonoBehaviour
{
    [SerializeField] private string musicName = "Music";

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("[BallroomMusic] Kein AudioManager in der Scene!");
            return;
        }

        AudioManager.instance.PlayAttached(musicName, transform);
    }
}
