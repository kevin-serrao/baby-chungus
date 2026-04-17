using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogicManager : MonoBehaviour
{
    public GameObject gameOverScreen;
    public int playerScore;
    public Text scoreText;

    public void restartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        gameOverScreen.SetActive(true);
    }
    [ContextMenu("Increase Score")]
    public void addScore(int score)
    {
        Debug.Log("adding score");
        playerScore += score;
        scoreText.text = playerScore.ToString();
        Debug.Log(scoreText.text);
    }
}
