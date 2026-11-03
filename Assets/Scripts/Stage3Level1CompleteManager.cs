
using UnityEngine;
using TMPro;

public class Stage3Level1CompleteManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Complete UI")]
    [SerializeField] private GameObject finalCompletePanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text levelUnlockedText;

    [Header("Failed Game UI")]
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Energy System")]
    [SerializeField] private Stage3Level1EnergyManager energyManager;

    private bool hydrogenCompleted = false;
    private bool waterCompleted = false;
    private bool levelCompleted = false;
    private bool runFailed = false;

    private int wrongAttempts = 0;
    private int score = 0;

    private void Start()
    {
        if (finalCompletePanel != null)
            finalCompletePanel.SetActive(false);

        if (failedPanel != null)
            failedPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        UpdateScoreDisplay();
    }

    public void RegisterWrongAttempt()
    {
        if (levelCompleted || runFailed)
            return;

        wrongAttempts++;

        Debug.Log("Wrong attempts: " + wrongAttempts);

        if (wrongAttempts == 1)
        {
            ShowFeedback(
                "HINT: Share electrons in the correct bonding zones.",
                Color.yellow
            );
        }
        else
        {
            runFailed = true;

            if (energyManager != null)
                energyManager.FailRun();

            // The full-screen failure panel owns the failure message.
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);

            if (failedPanel != null)
            {
                failedPanel.SetActive(true);
                failedPanel.transform.SetAsLastSibling();
            }
        }
    }

    public void HydrogenPuzzleCompleted()
    {
        if (hydrogenCompleted || runFailed)
            return;

        hydrogenCompleted = true;

        Debug.Log("H2 Puzzle Completed!");

        CheckLevelCompletion();
    }

    public void WaterPuzzleCompleted()
    {
        if (waterCompleted || runFailed)
            return;

        waterCompleted = true;

        Debug.Log("H2O Puzzle Completed!");

        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (levelCompleted || runFailed)
            return;

        if (hydrogenCompleted && waterCompleted)
        {
            levelCompleted = true;

            score = wrongAttempts == 0 ? 100 : 75;

            UpdateScoreDisplay();

            PlayerPrefs.SetInt("Stage3Level1Completed", 1);
            PlayerPrefs.SetInt("Stage3Level1Score", score);
            PlayerPrefs.SetInt("Stage3Level2Unlocked", 1);
            PlayerPrefs.Save();

            if (finalScoreText != null)
                finalScoreText.text = score.ToString("D6");

            if (levelUnlockedText != null)
                levelUnlockedText.text = "LEVEL 2 UNLOCKED!";

            if (finalCompletePanel != null)
            {
                finalCompletePanel.SetActive(true);
                finalCompletePanel.transform.SetAsLastSibling();
            }

            Debug.Log(
                "STAGE 3 LEVEL 1 COMPLETED! Score: " + score
            );
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "SCORE\n" + score.ToString("D6");
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
    }

    public void AssignFailurePanel(GameObject panel)
    {
        if(panel == null) return;
        if(failedPanel != null && failedPanel != panel) failedPanel.SetActive(false);
        failedPanel = panel;
        failedPanel.SetActive(runFailed);
        if(runFailed) failedPanel.transform.SetAsLastSibling();
    }

    public void AssignFeedbackText(TMP_Text hint)
    {
        feedbackText=hint;
        if(feedbackText!=null)feedbackText.gameObject.SetActive(false);
    }

    public bool CanInteract()
    {
        return Time.timeScale > 0f && !levelCompleted &&
               !runFailed &&
               (energyManager == null ||
                !energyManager.IsOutOfEnergy);
    }
}
