
using UnityEngine;
using System.Collections;

public class Level3CrystalGrowthAnimation : MonoBehaviour
{
    [SerializeField] private float growthDuration = 1.5f;

    private RectTransform crystalRect;
    private Coroutine growthCoroutine;

    private void Awake()
    {
        crystalRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (crystalRect == null)
            crystalRect = GetComponent<RectTransform>();

        if (growthCoroutine != null)
            StopCoroutine(growthCoroutine);

        growthCoroutine = StartCoroutine(GrowCrystal());
    }

    private IEnumerator GrowCrystal()
    {
        Vector3 targetScale = Vector3.one;
        float elapsed = 0f;

        crystalRect.localScale = Vector3.zero;

        while (elapsed < growthDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / growthDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            crystalRect.localScale = targetScale * smoothProgress;

            yield return null;
        }

        crystalRect.localScale = targetScale;
        growthCoroutine = null;
    }
}
