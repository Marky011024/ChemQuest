using UnityEngine;
using UnityEngine.EventSystems;

public class H2FormationZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BondProgressManager bondProgressManager;

    [Header("H2 Formation Visual")]
    [SerializeField] private RectTransform hydrogen1;
    [SerializeField] private RectTransform hydrogen2;
    [SerializeField] private GameObject h2Bond;
    [SerializeField] private GameObject successText;
    [SerializeField] private Level1ScoreManager scoreManager;
    [SerializeField] private GameObject levelCompleteButton;
    [SerializeField] private Level1AttemptManager attemptManager;
    private bool h2Formed = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (h2Formed)
            return;

        DraggableHydrogen hydrogen =
            eventData.pointerDrag?.GetComponent<DraggableHydrogen>();

        if (hydrogen == null)
            return;

        if (bondProgressManager == null ||
    !bondProgressManager.BothBondsAreBroken)
{
    Debug.Log("Break both H-Cl bonds before forming H2.");

    if (attemptManager != null)
    {
        attemptManager.RegisterMistake();
        
    }
    

    return;
}

        h2Formed = true;

        hydrogen.HydrogenAccepted();

        // Position both hydrogen atoms together.
        if (hydrogen1 != null && hydrogen2 != null)
        {
            hydrogen1.anchoredPosition = new Vector2(-50f, -100f);
            hydrogen2.anchoredPosition = new Vector2(50f, -100f);
        }

        // Reveal the H-H bond.
        if (h2Bond != null)
        {
            h2Bond.SetActive(true);
        }
        // Hide any previous mistake feedback.
        if (attemptManager != null)
        {
            attemptManager.HideMistakeHint();
        }
        // Show successful learning feedback.
        if (successText != null)
        {
            successText.SetActive(true);
        }
        // Complete Level 1 and award the score.
        if (scoreManager != null)
        {
            scoreManager.CompleteLevel();
        }
        // Show the Continue button after Level 1 is completed.
        if (levelCompleteButton != null)
        {
            levelCompleteButton.SetActive(true);
        }
        Debug.Log("H2 molecule successfully formed!");
    }
}