
using UnityEngine;
using UnityEngine.EventSystems;

public class Level3DraggableIon : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ion Settings")]
    [SerializeField] private string ionType;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private bool wasAccepted = false;

    public string IonType => ionType;

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
        wasAccepted = false;
        canvasGroup.blocksRaycasts = false;
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

        // Always return source ion to original position.
        // The slot will store the selected ion separately.
        ResetPosition();
    }

    public void AcceptDrop()
    {
        wasAccepted = true;
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        wasAccepted = false;
        canvasGroup.blocksRaycasts = true;
    }
}
