
using UnityEngine;
using TMPro;

public class Level2ScoreManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Completion UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text finalScoreText;

    private int score = 0;
    private bool levelCompleted = false;

    private void Start()
    {
        score = 0;
        levelCompleted = false;

        UpdateScoreDisplay();

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    public void CompleteLevel(int wrongAttempts)
    {
        if (levelCompleted)
            return;

        // First correct answer = 100
        // Correct after one wrong answer = 75
        score = wrongAttempts == 0 ? 100 : 75;

        levelCompleted = true;

        UpdateScoreDisplay();

        // Save completion and score
        PlayerPrefs.SetInt("Stage1Level2Completed", 1);
        PlayerPrefs.SetInt("Stage1Level2Score", score);
        PlayerPrefs.Save();

        // Update final score
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "YOUR FINAL SCORE\n" +
                score.ToString("D6");
        }

        // Show completion panel
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Level2CompletePanel is not assigned!"
            );
        }

        Debug.Log(
            "Stage 1 Level 2 completed! Score: " +
            score + ". Level 3 unlocked."
        );
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "SCORE\n" + score.ToString("D6");
        }
    }
}
