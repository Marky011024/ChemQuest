using UnityEngine;
using TMPro;

public class Level2EnergyAnswerManager : MonoBehaviour
{
    [SerializeField] private TMP_Text energyFeedbackText;
    [SerializeField] private Level2ScoreManager scoreManager;
    [SerializeField] private Level2AttemptManager attemptManager;
    [SerializeField] private GameObject levelCompleteButton;

    private bool answeredCorrectly = false;
    private int wrongAttempts = 0;

    public void ChooseExothermic()
    {
        if (answeredCorrectly)
            return;

        answeredCorrectly = true;

        if (energyFeedbackText != null)
        {
            energyFeedbackText.color = Color.green;
            energyFeedbackText.text =
                "CORRECT! The temperature increased, indicating an exothermic energy change.";
        }

        if (scoreManager != null)
        {
            scoreManager.CompleteLevel(wrongAttempts);
        }
        if (levelCompleteButton != null)
        {
            levelCompleteButton.SetActive(true);
        }

        Debug.Log("Correct energy identification: EXOTHERMIC.");
    }

    public void ChooseEndothermic()
    {
        if (answeredCorrectly)
            return;

        wrongAttempts++;

        if (wrongAttempts == 1)
        {
            if (energyFeedbackText != null)
            {
                energyFeedbackText.color = Color.red;
                energyFeedbackText.text =
                    "HINT: Compare the temperature before and after the reaction.";
            }

            Debug.Log("First incorrect energy identification. Hint shown.");
        }
        else if (wrongAttempts == 2)
        {
            if (energyFeedbackText != null)
            {
                energyFeedbackText.color = Color.red;
                energyFeedbackText.text =
                    "INCORRECT. One energy has been lost.";
            }

            if (attemptManager != null)
            {
                attemptManager.SecondWrongAttempt();
            }

            Debug.Log("Second incorrect energy identification. Level failed.");
        }
    }
}