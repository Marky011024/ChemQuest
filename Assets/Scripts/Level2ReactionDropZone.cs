using UnityEngine;
using UnityEngine.EventSystems;

public class Level2ReactionDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject reactionLiquid;
    [SerializeField] private GameObject bluePrecipitate;
    [SerializeField] private GameObject energyQuestionPanel;
    [SerializeField] private GameObject thermalControlPanel;
    [SerializeField] private ThermalControlManager thermalControlManager;

    private bool copperSulfateAdded = false;
    private bool sodiumHydroxideAdded = false;
    private bool reactionStarted = false;

    private void Start()
    {
        // Hide reaction results until both reagents are mixed.
        if (reactionLiquid != null)
        {
            reactionLiquid.SetActive(false);
        }

        if (bluePrecipitate != null)
        {
            bluePrecipitate.SetActive(false);
        }

        if (energyQuestionPanel != null)
        {
            energyQuestionPanel.SetActive(false);
        }

        if (thermalControlPanel != null)
        {
            thermalControlPanel.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableReagent reagent =
            eventData.pointerDrag.GetComponent<DraggableReagent>();

        if (reagent == null)
            return;

        string reagentName = eventData.pointerDrag.name;

        // Add Copper Sulfate.
        if (reagentName == "CopperSulfateContainer" &&
            !copperSulfateAdded)
        {
            copperSulfateAdded = true;

            reagent.ReagentAccepted();

            Debug.Log(
                "Copper sulfate added to the reaction beaker."
            );
        }

        // Add Sodium Hydroxide.
        else if (reagentName == "SodiumHydroxideContainer" &&
                 !sodiumHydroxideAdded)
        {
            sodiumHydroxideAdded = true;

            reagent.ReagentAccepted();

            Debug.Log(
                "Sodium hydroxide added to the reaction beaker."
            );
        }

        // Start the reaction only after BOTH reagents are added.
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

        // Show the reaction liquid.
        if (reactionLiquid != null)
        {
            reactionLiquid.SetActive(true);
        }

        // Show the blue Cu(OH)2 precipitate.
        if (bluePrecipitate != null)
        {
            bluePrecipitate.SetActive(true);
        }

        // Show the thermal observation controls.
        if (thermalControlPanel != null)
        {
            thermalControlPanel.SetActive(true);
        }

        if (thermalControlManager != null)
        {
            thermalControlManager.ShowReactionTemperature();
        }

        // Ask the energy-change question only after
        // the learner has observed the reaction.
        if (energyQuestionPanel != null)
        {
            energyQuestionPanel.SetActive(true);
        }

        Debug.Log(
            "REACTION STARTED: Copper sulfate and sodium hydroxide have been mixed."
        );

        Debug.Log(
            "Blue copper(II) hydroxide precipitate has formed."
        );

        Debug.Log(
            "Temperature observation is now available. Investigate the reaction's energy change."
        );
    }
}