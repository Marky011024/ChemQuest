
using UnityEngine;
using TMPro;

public class Level3CrystalGridChecker : MonoBehaviour
{
    [Header("Six Crystal Slots")]
    [SerializeField] private Level3CrystalSlot[] crystalSlots;

    [Header("Existing Level 3 Systems")]
    [SerializeField] private Level3ScoreManager scoreManager;
    [SerializeField] private Level3EnergyManager energyManager;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Crystal Formation Success Effect")]
    [SerializeField] private GameObject caCO3FormationEffectPanel;

    [Header("Puzzle UI")]
    [SerializeField] private GameObject checkAnswerButton;

    [Header("Level Completion UI")]
    [SerializeField] private GameObject continueButton;

    private int wrongAttempts = 0;
    private bool puzzleCompleted = false;
    private bool runFailed = false;

    private void Start()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (caCO3FormationEffectPanel != null)
            caCO3FormationEffectPanel.SetActive(false);

        if (continueButton != null)
            continueButton.SetActive(false);
    }

    public void CheckAnswer()
    {
        if (puzzleCompleted || runFailed)
            return;

        if (energyManager != null && energyManager.IsOutOfEnergy)
            return;

        if (crystalSlots == null || crystalSlots.Length != 6)
        {
            Debug.LogError("Assign exactly 6 crystal slots!");
            return;
        }

        bool allCorrect = true;

        foreach (Level3CrystalSlot slot in crystalSlots)
        {
            if (slot == null)
            {
                Debug.LogError("A crystal slot is missing!");
                return;
            }

            if (!slot.IsCorrect)
                allCorrect = false;
        }

        if (allCorrect)
        {
            puzzleCompleted = true;

            ShowFeedback(
                "EXCELLENT! Ca2+ and CO3 2- formed a CaCO3 crystal!",
                new Color32(117, 255, 170, 255)
            );

            if (scoreManager != null)
                scoreManager.CompleteLevel(wrongAttempts);

            if (checkAnswerButton != null)
                checkAnswerButton.SetActive(false);

            if (caCO3FormationEffectPanel != null)
                caCO3FormationEffectPanel.SetActive(true);

            if (continueButton != null)
            {
                continueButton.SetActive(true);
                continueButton.transform.SetAsLastSibling();
            }

            Debug.Log("CaCO3 crystal formation effect displayed!");
            Debug.Log("Level 3 Continue button displayed!");

            return;
        }

        wrongAttempts++;

        if (wrongAttempts == 1)
        {
            ShowFeedback(
                "HINT: Calcium (Ca2+) and carbonate (CO3 2-) " +
                "must alternate. Sodium (Na+) does not belong!",
                Color.yellow
            );

            ResetAllSlots();
        }
        else
        {
            runFailed = true;

            if (energyManager != null)
                energyManager.FailRun();

            ShowFeedback(
                "STAGE FAILED! Retry the level.",
                Color.red
            );
        }
    }

    private void ResetAllSlots()
    {
        foreach (Level3CrystalSlot slot in crystalSlots)
        {
            if (slot != null)
                slot.ResetSlot();
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
    }
}
