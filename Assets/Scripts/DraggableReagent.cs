using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableReagent : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Image reagentImage;

    private Vector2 startingPosition;
    private bool accepted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        reagentImage = GetComponent<Image>();

        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (accepted)
            return;

        reagentImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (accepted)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        reagentImage.raycastTarget = true;

        if (!accepted)
        {
            rectTransform.anchoredPosition = startingPosition;
        }
    }

    public void ReagentAccepted()
    {
        accepted = true;
        reagentImage.raycastTarget = true;
        gameObject.SetActive(false);
    }
}