
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class H2DraggableElectron : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Level Scoring")]
    [SerializeField] private Stage3Level1CompleteManager completeManager;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;

    private bool accepted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (accepted) return;

        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (accepted || canvas == null) return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!accepted)
        {
            // Record a wrong attempt if the electron
            // was not accepted by the bond zone.
            if (completeManager != null)
            {
                completeManager.RegisterWrongAttempt();
            }

            // Return electron to its original position.
            rectTransform.anchoredPosition = startPosition;

            Debug.Log("H2 electron drop failed.");
        }
    }

    public void MarkAccepted()
    {
        accepted = true;
        canvasGroup.blocksRaycasts = false;
    }
}
