using System.Collections;
using UnityEngine;

public class StageCarouselManager : MonoBehaviour
{
    [Header("Stage Cards")]
    [SerializeField] private RectTransform stage1Card;
    [SerializeField] private RectTransform stage2Card;
    [SerializeField] private RectTransform stage3Card;
    [SerializeField] private RectTransform stage4Card;
    [SerializeField] private RectTransform stage5Card;

    [Header("Carousel Settings")]
    [SerializeField] private float cardSpacing = 480f;
    [SerializeField] private float slideDuration = 0.35f;

    private RectTransform[] stageCards;

    private int currentPage = 0;
    private bool isSliding = false;

    private void Start()
    {
        stageCards = new RectTransform[]
        {
            stage1Card,
            stage2Card,
            stage3Card,
            stage4Card,
            stage5Card
        };

        SetCardPositionsImmediately();
    }

    public void NextStage()
    {
        if (isSliding)
            return;

        if (currentPage < 2)
        {
            currentPage++;
            StartCoroutine(AnimateCards());
        }
    }

    public void PreviousStage()
    {
        if (isSliding)
            return;

        if (currentPage > 0)
        {
            currentPage--;
            StartCoroutine(AnimateCards());
        }
    }

    private void SetCardPositionsImmediately()
    {
        float offset = currentPage * cardSpacing;

        for (int i = 0; i < stageCards.Length; i++)
        {
            if (stageCards[i] == null)
                continue;

            Vector2 position = stageCards[i].anchoredPosition;
            position.x = (-480f + (i * cardSpacing)) - offset;

            stageCards[i].anchoredPosition = position;
        }
    }

    private IEnumerator AnimateCards()
    {
        isSliding = true;

        Vector2[] startPositions = new Vector2[stageCards.Length];
        Vector2[] targetPositions = new Vector2[stageCards.Length];

        float offset = currentPage * cardSpacing;

        for (int i = 0; i < stageCards.Length; i++)
        {
            if (stageCards[i] == null)
                continue;

            startPositions[i] = stageCards[i].anchoredPosition;

            targetPositions[i] = new Vector2(
                (-480f + (i * cardSpacing)) - offset,
                stageCards[i].anchoredPosition.y
            );
        }

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / slideDuration);

            // Smooth ease-in/ease-out movement
            t = t * t * (3f - 2f * t);

            for (int i = 0; i < stageCards.Length; i++)
            {
                if (stageCards[i] == null)
                    continue;

                stageCards[i].anchoredPosition =
                    Vector2.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < stageCards.Length; i++)
        {
            if (stageCards[i] != null)
                stageCards[i].anchoredPosition = targetPositions[i];
        }

        isSliding = false;
    }
}