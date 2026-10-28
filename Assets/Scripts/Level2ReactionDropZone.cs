
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Level2ReactionDropZone : MonoBehaviour, IDropHandler
{
    [Header("Reaction Objects")]
    [SerializeField] private GameObject reactionLiquid;
    [SerializeField] private GameObject bluePrecipitate;

    [Header("UI Panels")]
    [SerializeField] private GameObject energyQuestionPanel;
    [SerializeField] private GameObject thermalControlPanel;

    [Header("Thermal Control")]
    [SerializeField] private ThermalControlManager thermalControlManager;

    [Header("Reaction Animation")]
    [SerializeField] private float reactionDuration = 3f;
    [SerializeField] private float pulseAmount = 0.04f;

    private bool copperSulfateAdded = false;
    private bool sodiumHydroxideAdded = false;
    private bool reactionStarted = false;

    private CanvasGroup liquidCanvasGroup;
    private CanvasGroup precipitateCanvasGroup;

    private Vector3 originalPrecipitateScale;

    private void Start()
    {
        if (reactionLiquid != null)
        {
            liquidCanvasGroup = GetOrAddCanvasGroup(reactionLiquid);
            reactionLiquid.SetActive(false);
        }

        if (bluePrecipitate != null)
        {
            precipitateCanvasGroup =
                GetOrAddCanvasGroup(bluePrecipitate);

            originalPrecipitateScale =
                bluePrecipitate.transform.localScale;

            bluePrecipitate.SetActive(false);
        }

        if (energyQuestionPanel != null)
            energyQuestionPanel.SetActive(false);

        if (thermalControlPanel != null)
            thermalControlPanel.SetActive(false);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (group == null)
            group = target.AddComponent<CanvasGroup>();

        return group;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (reactionStarted || eventData.pointerDrag == null)
            return;

        DraggableReagent reagent =
            eventData.pointerDrag.GetComponent<DraggableReagent>();

        if (reagent == null)
            return;

        string reagentName = eventData.pointerDrag.name;

        if (reagentName == "CopperSulfateContainer" &&
            !copperSulfateAdded)
        {
            copperSulfateAdded = true;
            reagent.ReagentAccepted();

            Debug.Log("Copper sulfate added.");
        }
        else if (reagentName == "SodiumHydroxideContainer" &&
                 !sodiumHydroxideAdded)
        {
            sodiumHydroxideAdded = true;
            reagent.ReagentAccepted();

            Debug.Log("Sodium hydroxide added.");
        }

        if (copperSulfateAdded &&
            sodiumHydroxideAdded &&
            !reactionStarted)
        {
            StartReaction();
        }
    }

    private void StartReaction()
    {
        reactionStarted = true;
        StartCoroutine(AnimateReaction());
    }

    private IEnumerator AnimateReaction()
    {
        if (reactionLiquid != null)
        {
            reactionLiquid.SetActive(true);
            liquidCanvasGroup.alpha = 0f;
        }

        if (bluePrecipitate != null)
        {
            bluePrecipitate.SetActive(true);
            precipitateCanvasGroup.alpha = 0f;

            bluePrecipitate.transform.localScale =
                originalPrecipitateScale * 0.3f;
        }

        if (thermalControlPanel != null)
            thermalControlPanel.SetActive(true);

        if (thermalControlManager != null)
            thermalControlManager.ShowReactionTemperature();

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, reactionDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, progress);

            // Liquid gradually appears.
            if (liquidCanvasGroup != null)
                liquidCanvasGroup.alpha = smooth;

            // Blue precipitate gradually forms.
            if (precipitateCanvasGroup != null)
                precipitateCanvasGroup.alpha =
                    Mathf.Clamp01((progress - 0.25f) / 0.75f);

            if (bluePrecipitate != null)
            {
                float growth = Mathf.SmoothStep(
                    0.3f, 1f, progress
                );

                float pulse = 1f +
                    Mathf.Sin(elapsed * 10f) *
                    pulseAmount *
                    (1f - progress);

                bluePrecipitate.transform.localScale =
                    originalPrecipitateScale * growth * pulse;
            }

            yield return null;
        }

        if (liquidCanvasGroup != null)
            liquidCanvasGroup.alpha = 1f;

        if (precipitateCanvasGroup != null)
            precipitateCanvasGroup.alpha = 1f;

        if (bluePrecipitate != null)
            bluePrecipitate.transform.localScale =
                originalPrecipitateScale;

        // Show question after reaction animation.
        if (energyQuestionPanel != null)
            energyQuestionPanel.SetActive(true);

        Debug.Log("Reaction animation completed.");
        Debug.Log("Blue Cu(OH)2 precipitate formed.");
    }
}
