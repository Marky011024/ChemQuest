using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Level1AttemptManager attemptManager;

    private int score = 0;
    private bool levelCompleted = false;

    private void Start()
    {
        UpdateScoreDisplay();
    }

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        // First correct attempt = 100 points.
        // Correct after one mistake = 75 points.
        if (attemptManager != null && attemptManager.HasMadeMistake)
        {
            score = 75;
        }
        else
        {
            score = 100;
        }

        UpdateScoreDisplay();

        Debug.Log("Stage 1 Level 1 completed! Score: " + score);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE\n" + score.ToString("D6");
        }
    }

    public void ContinueToLevel2()
    {
        // Save Level 1 completion.
        // Stage1LevelSelection will use this to unlock Level 2.
        PlayerPrefs.SetInt("Stage1Level1Completed", 1);

        // Save the Level 1 score for later progress/statistics.
        PlayerPrefs.SetInt("Stage1Level1Score", score);

        PlayerPrefs.Save();

        Debug.Log("Stage 1 Level 1 completion saved. Level 2 unlocked.");

        // Continue to Stage 1 Level 2.
        SceneManager.LoadScene("Stage1Level2");
    }
}