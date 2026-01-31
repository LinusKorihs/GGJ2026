using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverLoading : MonoBehaviour
{
    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}
