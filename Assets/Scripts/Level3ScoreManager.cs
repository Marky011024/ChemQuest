
using UnityEngine;
using TMPro;

public class Level3ScoreManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Complete Panel")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Crystal Formation Effect")]
    [SerializeField] private GameObject caCO3FormationEffectPanel;

    [Header("Puzzle Panel")]
    [SerializeField] private GameObject ionPuzzlePanel;

    private int score = 0;
    private bool levelCompleted = false;
    private bool bowStarted = false;
    private int pendingWrongAttempts = 0;
    private bool puzzleCleared = false;

    private void Start()
    {
        UpdateScoreDisplay();

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    public void CompleteLevel(int wrongAttempts)
    {
        if (levelCompleted)
            return;

        // Store provisional result only. The actual completion and unlock
        // are committed AFTER the BOW Learning Check is completed.
        pendingWrongAttempts = wrongAttempts;
        puzzleCleared = true;
        score = wrongAttempts == 0 ? 100 : 75;
        UpdateScoreDisplay();
        Debug.Log("Stage 1 Level 3 puzzle completed; waiting for Continue and BOW.");
    }

    public void ShowCompletionPanel()
    {
        if (!puzzleCleared || levelCompleted || bowStarted) return;
        bowStarted = true;
        var bow = gameObject.GetComponent<Stage1BowLearningCheck>();
        if (bow == null) bow = gameObject.AddComponent<Stage1BowLearningCheck>();
        bow.Begin(3, finalScoreText != null ? finalScoreText.font :
            scoreText != null ? scoreText.font : null,
            finalScoreText != null ? finalScoreText.GetComponentInParent<Canvas>() :
            scoreText != null ? scoreText.GetComponentInParent<Canvas>() : null,
            FinishAfterBow);
    }

    private void FinishAfterBow(bool bowHadMistake)
    {
        if (levelCompleted) return;
        levelCompleted = true;
        score = pendingWrongAttempts == 0 && !bowHadMistake ? 100 : 75;
        UpdateScoreDisplay();
        PlayerPrefs.SetInt("Stage1Level3Completed", 1);
        PlayerPrefs.SetInt("Stage1Level3Score", score);
        PlayerPrefs.SetInt("Stage2Level1Unlocked", 1);
        PlayerPrefs.Save();

        if (finalScoreText != null)
            finalScoreText.text = score.ToString("D6");

        if (caCO3FormationEffectPanel != null)
            caCO3FormationEffectPanel.SetActive(false);

        if (ionPuzzlePanel != null)
            ionPuzzlePanel.SetActive(false);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.SetAsLastSibling();
        }

        Debug.Log("Level 3 completion panel displayed!");
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "SCORE\n" + score.ToString("D6");
    }
}
