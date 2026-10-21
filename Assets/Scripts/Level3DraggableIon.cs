
using UnityEngine;
using UnityEngine.EventSystems;

public class Level3DraggableIon : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private bool wasAccepted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        wasAccepted = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!wasAccepted)
        {
            ResetPosition();
        }

        wasAccepted = false;
    }

    public void AcceptDrop()
    {
        wasAccepted = true;
    }

    // Return the ion to its original starting position.
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        wasAccepted = false;
        canvasGroup.blocksRaycasts = true;
    }
}
