using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    GameOverLoading _gameOverLoading;

    public float NPCTime = 1;

    [SerializeField]
    float _slowSpeed = 0.01f;

    [SerializeField]
    float _normalSpeed = 1f;


    public Action<float> NPCTimeChanged;
    public bool TESTNPCSPEED = false;

    public bool IsGameOver;
  

    void Awake()
    {
        if (Instance == null) Instance = this;


        _gameOverLoading = this.GetComponent<GameOverLoading>();
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

    public void SlowSpeed()
    {
        AdjustNPCTimeSpeed(_slowSpeed);
    }

    public void NormalSpeed()
    {
        AdjustNPCTimeSpeed(_normalSpeed);
    }

    public void GameOverPressed()
    {
        _gameOverLoading.LoadGameOver();
    }


    public void AdjustNPCTimeSpeed(float speed)
    {
        NPCTime = speed;
        NPCTimeChanged?.Invoke(NPCTime);
    }

}
