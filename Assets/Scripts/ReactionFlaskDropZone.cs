using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReactionFlaskDropZone : MonoBehaviour, IDropHandler
{
    public bool zincAdded = false;

    [SerializeField] private H2BubbleEffect bubbleEffect;
    [SerializeField] private Button subMicroscopicButton;
    [SerializeField] private GameObject reactionFeedbackText;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableZinc zinc =
            eventData.pointerDrag?.GetComponent<DraggableZinc>();

        if (zinc != null && !zincAdded)
        {
            zincAdded = true;

            Debug.Log("Zinc successfully added to dilute HCl!");

            // Tell the zinc that the drop was successful.
            zinc.ZincAccepted();
            if (subMicroscopicButton != null)
            {
                subMicroscopicButton.interactable = true;
            }
            if (reactionFeedbackText != null)
            {
                reactionFeedbackText.SetActive(true);
            }

            // Start the visible hydrogen gas evolution.
            if (bubbleEffect != null)
            {
                bubbleEffect.StartBubbling();
            }
        }
    }
}