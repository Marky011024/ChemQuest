
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level3BubbleAnimation : MonoBehaviour
{
    [Header("Bubble Settings")]
    [SerializeField] private RectTransform bubbleContainer;
    [SerializeField] private Image limewaterLiquid;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 2.5f;
    [SerializeField] private float bubbleSpawnInterval = 0.15f;
    [SerializeField] private float bubbleTravelDuration = 1f;
    [SerializeField] private float bubbleTravelDistance = 90f;
    [SerializeField] private float bubbleSize = 12f;

    private bool isAnimating = false;
    private Coroutine animationCoroutine;

    private readonly List<GameObject> activeBubbles =
        new List<GameObject>();

    public void PlayBubbleAnimation()
    {
        if (isAnimating || bubbleContainer == null)
            return;

        animationCoroutine =
            StartCoroutine(AnimateBubbles());
    }

    private IEnumerator AnimateBubbles()
    {
        isAnimating = true;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, animationDuration);
        float interval = Mathf.Max(0.01f, bubbleSpawnInterval);

        while (elapsed < duration)
        {
            SpawnBubble();

            yield return new WaitForSeconds(interval);

            elapsed += interval;
        }

        isAnimating = false;
        animationCoroutine = null;
    }

    private void SpawnBubble()
    {
        GameObject bubble = new GameObject(
            "CO2Bubble",
            typeof(RectTransform),
            typeof(Image)
        );

        bubble.transform.SetParent(
            bubbleContainer,
            false
        );

        RectTransform rect =
            bubble.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(bubbleSize, bubbleSize);

        rect.anchoredPosition = new Vector2(
            Random.Range(-40f, 40f),
            -40f
        );

        Image image = bubble.GetComponent<Image>();

        image.color =
            new Color(1f, 1f, 1f, 0.7f);

        image.raycastTarget = false;

        activeBubbles.Add(bubble);

        StartCoroutine(MoveBubble(rect));
    }

    private IEnumerator MoveBubble(RectTransform bubble)
    {
        float elapsed = 0f;
        float duration =
            Mathf.Max(0.01f, bubbleTravelDuration);

        Vector2 start = bubble.anchoredPosition;
        Vector2 end =
            start + Vector2.up * bubbleTravelDistance;

        while (elapsed < duration)
        {
            if (bubble == null)
                yield break;

            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            bubble.anchoredPosition =
                Vector2.Lerp(start, end, progress);

            yield return null;
        }

        if (bubble != null)
        {
            activeBubbles.Remove(bubble.gameObject);
            Destroy(bubble.gameObject);
        }
    }

    public void ResetAnimation()
    {
        StopAllCoroutines();

        animationCoroutine = null;
        isAnimating = false;

        foreach (GameObject bubble in activeBubbles)
        {
            if (bubble != null)
                Destroy(bubble);
        }

        activeBubbles.Clear();
    }

    private void OnDisable()
    {
        ResetAnimation();
    }
}
