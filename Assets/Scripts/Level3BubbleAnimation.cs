
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Level3BubbleAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform bubbleContainer;
    [SerializeField] private Image limewaterLiquid;

    [SerializeField] private float animationDuration = 2.5f;

    private bool isAnimating = false;

    public void PlayBubbleAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateBubbles());
        }
    }

    private IEnumerator AnimateBubbles()
    {
        isAnimating = true;

        float elapsed = 0f;

        Color clearColor = new Color32(189, 239, 255, 160);
        Color milkyColor = new Color32(245, 245, 240, 230);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / animationDuration
            );

            if (limewaterLiquid != null)
            {
                limewaterLiquid.color =
                    Color.Lerp(clearColor, milkyColor, progress);
            }

            if (bubbleContainer != null)
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

                rect.sizeDelta = new Vector2(12, 12);

                rect.anchoredPosition = new Vector2(
                    Random.Range(-40f, 40f),
                    -40f
                );

                Image image = bubble.GetComponent<Image>();

                image.color = new Color(
                    1f, 1f, 1f, 0.7f
                );

                image.raycastTarget = false;

                StartCoroutine(MoveBubble(rect));
            }

            yield return new WaitForSeconds(0.15f);
        }

        if (limewaterLiquid != null)
        {
            limewaterLiquid.color = milkyColor;
        }

        isAnimating = false;
    }

    private IEnumerator MoveBubble(RectTransform bubble)
    {
        float duration = 1f;
        float elapsed = 0f;

        Vector2 start = bubble.anchoredPosition;
        Vector2 end = start + Vector2.up * 90f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (bubble == null)
                yield break;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            bubble.anchoredPosition =
                Vector2.Lerp(start, end, progress);

            yield return null;
        }

        if (bubble != null)
        {
            Destroy(bubble.gameObject);
        }
    }
}
