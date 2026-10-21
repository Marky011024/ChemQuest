
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Level3IonDropZone : MonoBehaviour, IDropHandler
{
    [Header("Puzzle Feedback")]
    [SerializeField] private TMP_Text ionPuzzleFeedbackText;

    [Header("Scoring")]
    [SerializeField] private Level3ScoreManager scoreManager;

    [Header("Energy")]
    [SerializeField] private Level3EnergyManager energyManager;

    [Header("Draggable Ions")]
    [SerializeField] private Level3DraggableIon calciumIon;
    [SerializeField] private Level3DraggableIon carbonateIon;
    [SerializeField] private Level3DraggableIon sodiumIon;

    private bool calciumAdded = false;
    private bool carbonateAdded = false;
    private bool sodiumAdded = false;

    private bool puzzleCompleted = false;
    private bool runFailed = false;

    private int wrongAttempts = 0;

    private void Start()
    {
        if (ionPuzzleFeedbackText != null)
            ionPuzzleFeedbackText.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (puzzleCompleted || runFailed ||
            eventData.pointerDrag == null)
            return;

        if (energyManager != null &&
            energyManager.IsOutOfEnergy)
            return;

        GameObject draggedObject = eventData.pointerDrag;

        Level3DraggableIon draggable =
            draggedObject.GetComponent<Level3DraggableIon>();

        if (draggable == null)
            return;

        if (draggedObject.name == "CalciumIon" && !calciumAdded)
        {
            calciumAdded = true;
            draggable.AcceptDrop();

            Debug.Log("Calcium ion selected.");
        }
        else if (draggedObject.name == "CarbonateIon" && !carbonateAdded)
        {
            carbonateAdded = true;
            draggable.AcceptDrop();

            Debug.Log("Carbonate ion selected.");
        }
        else if (draggedObject.name == "SodiumIon" && !sodiumAdded)
        {
            sodiumAdded = true;
            draggable.AcceptDrop();

            Debug.Log("Sodium ion selected.");
        }
    }

    // Called by the CHECK ANSWER button.
    public void CheckAnswer()
    {
        if (puzzleCompleted || runFailed)
            return;

        if (energyManager != null &&
            energyManager.IsOutOfEnergy)
            return;

        bool correctAnswer =
            calciumAdded &&
            carbonateAdded &&
            !sodiumAdded;

        if (correctAnswer)
        {
            CompletePuzzle();
        }
        else
        {
            wrongAttempts++;

            if (wrongAttempts == 1)
            {
                ShowFeedback(
                    "CONCEPT HINT: CaCO3 forms from Ca2+ and CO3 2-. Try again!",
                    Color.yellow
                );

                ResetSelection();

                Debug.Log(
                    "First incorrect submission. No energy lost."
                );
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

                Debug.Log(
                    "Second incorrect submission. One energy lost."
                );
            }
        }
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        ShowFeedback(
            "EXCELLENT! Calcium carbonate (CaCO3) precipitate formed!",
            new Color32(117, 255, 170, 255)
        );

        if (scoreManager != null)
            scoreManager.CompleteLevel(wrongAttempts);

        Debug.Log("Stage 1 Level 3 completed!");
    }

    private void ResetSelection()
    {
        calciumAdded = false;
        carbonateAdded = false;
        sodiumAdded = false;

        if (calciumIon != null)
            calciumIon.ResetPosition();

        if (carbonateIon != null)
            carbonateIon.ResetPosition();

        if (sodiumIon != null)
            sodiumIon.ResetPosition();
    }

    private void ShowFeedback(string message, Color color)
    {
        if (ionPuzzleFeedbackText == null)
            return;

        ionPuzzleFeedbackText.gameObject.SetActive(true);
        ionPuzzleFeedbackText.color = color;
        ionPuzzleFeedbackText.text = message;
    }
}
