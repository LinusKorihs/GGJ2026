using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public float NPCTime = 1;

    public Action<float> NPCTimeChanged;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    public void AdjustGameSpeed(float speed)
    {
        Time.timeScale = speed;
    }

        public void AdjustNPCTimeSpeed(float speed)
    {
        NPCTime = speed;
    }

}
