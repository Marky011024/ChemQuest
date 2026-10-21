
using UnityEngine;
using TMPro;

public class Level2ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private int score = 0;
    private bool levelCompleted = false;

    private void Start()
    {
        UpdateScoreDisplay();
    }

    public void CompleteLevel(int wrongAttempts)
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        // First correct = 100; after one mistake = 75.
        score = wrongAttempts == 0 ? 100 : 75;

        UpdateScoreDisplay();

        // Save completion and unlock Level 3.
        PlayerPrefs.SetInt("Stage1Level2Completed", 1);
        PlayerPrefs.SetInt("Stage1Level2Score", score);
        PlayerPrefs.Save();

        Debug.Log(
            "Stage 1 Level 2 completed! Score: " + score +
            ". Level 3 unlocked."
        );
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE\n" + score.ToString("D6");
        }
    }
}
