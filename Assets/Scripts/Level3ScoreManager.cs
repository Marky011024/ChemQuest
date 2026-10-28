
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

        levelCompleted = true;

        score = wrongAttempts == 0 ? 100 : 75;

        UpdateScoreDisplay();

        PlayerPrefs.SetInt("Stage1Level3Completed", 1);
        PlayerPrefs.SetInt("Stage1Level3Score", score);
        PlayerPrefs.SetInt("Stage2Level1Unlocked", 1);
        PlayerPrefs.Save();

        if (finalScoreText != null)
            finalScoreText.text = score.ToString("D6");

        // Do not display completion panel yet.
        // Wait for the player to click CONTINUE.

        Debug.Log("Stage 1 Level 3 completed! Score: " + score);
    }

    public void ShowCompletionPanel()
    {
        if (!levelCompleted)
            return;

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
