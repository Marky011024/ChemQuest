
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1ScoreManager : MonoBehaviour
{
    [Header("Existing Score System")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Level1AttemptManager attemptManager;

    [Header("Level Complete Panel")]
    [SerializeField] private GameObject level1CompletePanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Other UI Panels")]
    [SerializeField] private GameObject subMicroscopicPanel;
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject experimentPanel;

    private int score = 0;
    private bool levelCompleted = false;

    private void Start()
    {
        // Hide completion panel when level starts
        if (level1CompletePanel != null)
            level1CompletePanel.SetActive(false);

        UpdateScoreDisplay();
    }

    private bool bowStarted;

    public void CompleteLevel()
    {
        if (levelCompleted || bowStarted)
            return;

        bowStarted = true;
        var bow = gameObject.GetComponent<Stage1BowLearningCheck>();
        if (bow == null) bow = gameObject.AddComponent<Stage1BowLearningCheck>();
        bow.Begin(1, finalScoreText != null ? finalScoreText.font :
            scoreText != null ? scoreText.font : null,
            finalScoreText != null ? finalScoreText.GetComponentInParent<Canvas>() :
            scoreText != null ? scoreText.GetComponentInParent<Canvas>() : null,
            FinishAfterBow);
    }

    private void FinishAfterBow(bool bowHadMistake)
    {
        if (levelCompleted) return;
        levelCompleted = true;

        // Calculate final score
        // No mistakes = 100
        // With mistake = 75
        score = attemptManager != null &&
                (attemptManager.HasMadeMistake || bowHadMistake) ? 75 : 100;

        UpdateScoreDisplay();

        // Save progress and unlock Level 2
        PlayerPrefs.SetInt("Stage1Level1Completed", 1);
        PlayerPrefs.SetInt("Stage1Level1Score", score);
        PlayerPrefs.Save();

        // Update final score display
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "YOUR FINAL SCORE\n" +
                score.ToString("D6");
        }

        // Hide other panels to prevent overlapping
        if (subMicroscopicPanel != null)
            subMicroscopicPanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (experimentPanel != null)
            experimentPanel.SetActive(false);

        // Replace the older tiny inline victory overlay with the
        // Stage 2-style centered completion card.
        if (level1CompletePanel != null)
            level1CompletePanel.SetActive(false);
        ShowUnifiedCompletion();

        Debug.Log(
            "Stage 1 Level 1 completed! Score: " + score +
            ". Level 2 unlocked!"
        );
    }

    private void ShowUnifiedCompletion()
    {
        var popup=GetComponent<Stage1CompletionPopup>();
        if(popup==null)popup=gameObject.AddComponent<Stage1CompletionPopup>();
        popup.Show(1,score,finalScoreText!=null?finalScoreText.font:
            scoreText!=null?scoreText.font:null);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "SCORE\n" + score.ToString("D6");
        }
    }

    public void ReturnToLevelSelection()
    {
        SceneManager.LoadScene("Stage1LevelSelection");
    }

    public void ContinueToLevel2()
    {
        SceneManager.LoadScene("Stage1Level2");
    }
}
