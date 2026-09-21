using TMPro;
using UnityEngine;

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
}