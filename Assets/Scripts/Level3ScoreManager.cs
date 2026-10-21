
using UnityEngine;
using TMPro;

public class Level3ScoreManager : MonoBehaviour
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

        score = wrongAttempts == 0 ? 100 : 75;

        UpdateScoreDisplay();

        PlayerPrefs.SetInt("Stage1Level3Completed", 1);
        PlayerPrefs.SetInt("Stage1Level3Score", score);
        PlayerPrefs.Save();

        Debug.Log(
            "Stage 1 Level 3 completed! Score: " + score
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
