
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level3ReactionManager : MonoBehaviour
{
    [Header("Experiment")]
    [SerializeField] private Image limewaterLiquid;
    [SerializeField] private Button bubbleCO2Button;
    [SerializeField] private Level3BubbleAnimation bubbleAnimation;

    [Header("Feedback")]
    [SerializeField] private TMP_Text reactionFeedbackText;

    [Header("Ion Puzzle")]
    [SerializeField] private GameObject ionPuzzlePanel;

    private bool reactionCompleted = false;

    private void Start()
    {
        if (limewaterLiquid != null)
        {
            limewaterLiquid.color =
                new Color32(189, 239, 255, 160);
        }

        if (bubbleCO2Button != null)
        {
            bubbleCO2Button.interactable = true;
        }

        if (reactionFeedbackText != null)
        {
            reactionFeedbackText.text = "";
        }

        if (ionPuzzlePanel != null)
        {
            ionPuzzlePanel.SetActive(false);
        }
    }

    public void BubbleCO2()
    {
        if (reactionCompleted)
            return;

        reactionCompleted = true;

        if (bubbleCO2Button != null)
        {
            bubbleCO2Button.interactable = false;
        }

        if (bubbleAnimation != null)
        {
            bubbleAnimation.PlayBubbleAnimation();
            StartCoroutine(FinishReactionAfterAnimation());
        }
        else
        {
            // Fallback if animation is not assigned.
            FinishReaction();
        }
    }

    private IEnumerator FinishReactionAfterAnimation()
    {
        // Match the animation duration (2.5 seconds).
        yield return new WaitForSeconds(2.5f);

        FinishReaction();
    }

    private void FinishReaction()
    {
        if (limewaterLiquid != null)
        {
            limewaterLiquid.color =
                new Color32(245, 245, 240, 230);
        }

        if (reactionFeedbackText != null)
        {
            reactionFeedbackText.text =
                "The limewater turned milky! " +
                "Calcium carbonate (CaCO3) precipitate formed.";
        }

        if (ionPuzzlePanel != null)
        {
            ionPuzzlePanel.SetActive(true);
        }

        Debug.Log(
            "CO2 reacted with limewater. " +
            "Calcium carbonate precipitate formed."
        );
    }
}
