
using UnityEngine;
using TMPro;

public class H2OWaterPuzzleManager : MonoBehaviour
{
    [Header("Water Puzzle UI")]
    [SerializeField] private TMP_Text successText;

    [Header("Level Completion")]
    [SerializeField] private Stage3Level1CompleteManager completeManager;

    private bool leftBondCompleted = false;
    private bool rightBondCompleted = false;
    private bool puzzleCompleted = false;

    private void Start()
    {
        if (successText != null)
            successText.gameObject.SetActive(false);
    }

    public void BondCompleted(bool isLeftBond)
    {
        if (puzzleCompleted) return;

        if (isLeftBond)
            leftBondCompleted = true;
        else
            rightBondCompleted = true;

        if (leftBondCompleted && rightBondCompleted)
        {
            CompleteWaterPuzzle();
        }
    }

    private void CompleteWaterPuzzle()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;

        if (successText != null)
        {
            successText.text = "H₂O MOLECULE FORMED!";
            successText.gameObject.SetActive(true);
        }

        Debug.Log("H2O WATER PUZZLE COMPLETED!");

        if (completeManager != null)
        {
            completeManager.WaterPuzzleCompleted();
        }
        else
        {
            Debug.LogWarning(
                "Stage3Level1CompleteManager is not assigned to H2O!"
            );
        }
    }
}
