using System.Collections;
using UnityEngine;

public class H2BubbleEffect : MonoBehaviour
{
    [SerializeField] private GameObject[] bubbles;

    public void StartBubbling()
    {
        StopAllCoroutines();
        StartCoroutine(BubbleSequence());
    }

    private IEnumerator BubbleSequence()
    {
        for (int i = 0; i < bubbles.Length; i++)
        {
            if (bubbles[i] != null)
            {
                bubbles[i].SetActive(true);

                RectTransform bubbleRect =
                    bubbles[i].GetComponent<RectTransform>();

                StartCoroutine(RiseBubble(bubbleRect));

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    private IEnumerator RiseBubble(RectTransform bubble)
    {
        Vector2 startPosition = bubble.anchoredPosition;
        Vector2 endPosition =
            startPosition + new Vector2(0f, 120f);

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / duration;

            bubble.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, progress);

            yield return null;
        }

        bubble.gameObject.SetActive(false);

        // Return bubble to its original location
        // so it is ready for future reaction effects.
        bubble.anchoredPosition = startPosition;
    }
}