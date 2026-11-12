
using UnityEngine;
using TMPro;

public class Level2ScoreManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Completion UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject experimentPanel;
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

    private bool bowStarted;

    public void CompleteLevel(int wrongAttempts)
    {
        if (levelCompleted || bowStarted)
            return;

        bowStarted = true;
        var bow = gameObject.GetComponent<Stage1BowLearningCheck>();
        if (bow == null) bow = gameObject.AddComponent<Stage1BowLearningCheck>();
        bow.Begin(2, finalScoreText != null ? finalScoreText.font :
            scoreText != null ? scoreText.font : null,
            finalScoreText != null ? finalScoreText.GetComponentInParent<Canvas>() :
            scoreText != null ? scoreText.GetComponentInParent<Canvas>() : null,
            bowHadMistake => FinishAfterBow(wrongAttempts, bowHadMistake));
    }

    private void FinishAfterBow(int wrongAttempts, bool bowHadMistake)
    {
        if (levelCompleted) return;
        // Correct on first attempt throughout the challenge = 100;
        // any incorrect answer (experiment or BOW) = 75.
        score = (wrongAttempts == 0 && !bowHadMistake) ? 100 : 75;
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

        // Hide the active experiment and answer controls before showing completion.
        if (experimentPanel != null)
            experimentPanel.SetActive(false);

        // Show completion panel above all other gameplay UI.
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.SetAsLastSibling();
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
