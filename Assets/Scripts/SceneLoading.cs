using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverLoading : MonoBehaviour
{
    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void LoadGameWon()
    {
        SceneManager.LoadScene("GameWon");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Trailer");
    }
}
