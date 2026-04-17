using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages game logic such as score, game over, and scene restarts.
/// </summary>
public class LogicManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Text scoreText;

    public int PlayerScore { get; private set; }

    /// <summary>
    /// Restarts the current scene.
    /// </summary>
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Shows the game over screen.
    /// </summary>
    public void GameOver()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Adds to the player's score and updates the UI.
    /// </summary>
    [ContextMenu("Increase Score")]
    public void AddScore(int score)
    {
        Debug.Log("adding score");
        PlayerScore += score;
        if (scoreText != null)
            scoreText.text = PlayerScore.ToString();
        Debug.Log(scoreText != null ? scoreText.text : "scoreText is null");
    }
}
