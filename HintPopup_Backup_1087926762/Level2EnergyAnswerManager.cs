
using UnityEngine;
using TMPro;

public class Level2EnergyAnswerManager : MonoBehaviour
{
    [Header("Feedback UI")]
    [SerializeField] private TMP_Text energyFeedbackText;

    [Header("Managers")]
    [SerializeField] private Level2ScoreManager scoreManager;
    [SerializeField] private Level2AttemptManager attemptManager;

    private bool answeredCorrectly = false;
    private bool levelFailed = false;
    private int wrongAttempts = 0;

    private void Start()
    {
        answeredCorrectly = false;
        levelFailed = false;
        wrongAttempts = 0;

        if (energyFeedbackText != null)
            energyFeedbackText.text = "";
    }

    public void ChooseExothermic()
    {
        if (answeredCorrectly || levelFailed)
            return;

        answeredCorrectly = true;

        if (energyFeedbackText != null)
        {
            energyFeedbackText.color = Color.green;
            energyFeedbackText.text =
                "CORRECT! The temperature increased, " +
                "indicating an exothermic energy change.";
        }

        // ScoreManager now shows the completion panel.
        if (scoreManager != null)
        {
            scoreManager.CompleteLevel(wrongAttempts);
        }
        else
        {
            Debug.LogError("Level2ScoreManager is not assigned!");
        }

        Debug.Log("Correct answer! Level 2 completed.");
    }

    public void ChooseEndothermic()
    {
        if (answeredCorrectly || levelFailed)
            return;

        wrongAttempts++;

        if (wrongAttempts == 1)
        {
            if (energyFeedbackText != null)
            {
                energyFeedbackText.color = Color.yellow;
                energyFeedbackText.text =
                    "HINT: Compare the temperature before " +
                    "and after the reaction. Did it increase or decrease?";
            }

            Debug.Log("First wrong answer. Hint shown.");
        }
        else if (wrongAttempts == 2)
        {
            levelFailed = true;

            if (energyFeedbackText != null)
            {
                energyFeedbackText.color = Color.red;
                energyFeedbackText.text =
                    "INCORRECT! You have used both attempts.";
            }

            if (attemptManager != null)
            {
                attemptManager.SecondWrongAttempt();
            }
            else
            {
                Debug.LogError("Level2AttemptManager is not assigned!");
            }

            Debug.Log("Second wrong answer. Level failed.");
        }
    }
}
