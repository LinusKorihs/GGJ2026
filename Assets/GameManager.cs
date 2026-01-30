using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void AdjustGameSpeed(float gameSpeed)
    {
        Time.timeScale = gameSpeed;
    }
}
