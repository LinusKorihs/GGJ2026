using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float NPCTime = 1;

    public Action<float> NPCTimeChanged;
    public bool TESTNPCSPEED = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (TESTNPCSPEED)
        {
            TESTNPCSPEED = false;
            AdjustNPCTimeSpeed(NPCTime == 1 ? 0.2f : 5f);
        }
    }

    public void AdjustGameSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public void AdjustNPCTimeSpeed(float speed)
    {
        NPCTime = speed;
        NPCTimeChanged?.Invoke(NPCTime);
    }

}
