
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

    [Header("Reaction Settings")]
    [SerializeField] private float animationDuration = 2.5f;
    [SerializeField] private float colorTransitionDuration = 1f;

    private readonly Color32 initialColor =
        new Color32(189, 239, 255, 160);

    private readonly Color32 precipitateColor =
        new Color32(245, 245, 240, 230);

    private bool reactionStarted = false;
    private bool reactionCompleted = false;

    private Coroutine reactionCoroutine;

    private void Start()
    {
        ResetReaction();
    }

    public void BubbleCO2()
    {
        if (reactionStarted || reactionCompleted)
            return;

        reactionStarted = true;

        if (bubbleCO2Button != null)
            bubbleCO2Button.interactable = false;

        if (reactionFeedbackText != null)
        {
            reactionFeedbackText.color = Color.white;
            reactionFeedbackText.text =
                "CO2 is reacting with limewater...";
        }

        if (bubbleAnimation != null)
            bubbleAnimation.PlayBubbleAnimation();
        else
            Debug.LogWarning(
                "Level3BubbleAnimation is not assigned!"
            );

        reactionCoroutine =
            StartCoroutine(ReactionSequence());
    }

    private IEnumerator ReactionSequence()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, animationDuration)
        );

        if (limewaterLiquid != null)
        {
            Color startColor = limewaterLiquid.color;
            float duration =
                Mathf.Max(0.01f, colorTransitionDuration);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / duration);

                limewaterLiquid.color = Color.Lerp(
                    startColor,
                    precipitateColor,
                    Mathf.SmoothStep(0f, 1f, progress)
                );

                yield return null;
            }

            limewaterLiquid.color = precipitateColor;
        }

        FinishReaction();
        reactionCoroutine = null;
    }

    private void FinishReaction()
    {
        reactionCompleted = true;

        if (reactionFeedbackText != null)
        {
            reactionFeedbackText.color = Color.green;
            reactionFeedbackText.text =
                "The limewater turned milky! " +
                "Calcium carbonate (CaCO3) " +
                "precipitate formed.";
        }

        if (ionPuzzlePanel != null)
        {
            ionPuzzlePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Ion Puzzle Panel is not assigned!"
            );
        }

        Debug.Log(
            "Level 3 reaction completed. " +
            "Calcium carbonate precipitate formed."
        );
    }

    public void ResetReaction()
    {
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
            reactionCoroutine = null;
        }

        reactionStarted = false;
        reactionCompleted = false;

        if (limewaterLiquid != null)
            limewaterLiquid.color = initialColor;

        if (bubbleCO2Button != null)
            bubbleCO2Button.interactable = true;

        if (reactionFeedbackText != null)
            reactionFeedbackText.text = "";

        if (ionPuzzlePanel != null)
            ionPuzzlePanel.SetActive(false);
    }
}
