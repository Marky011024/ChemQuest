
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
    private bool dragStarted;
    public bool IsAccepted => accepted;
    public bool CanDrag => !accepted && completeManager != null && completeManager.CanInteract() && Time.timeScale > 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        // Some Stage 3 Level 1 electron objects were missing this Inspector reference.
        // Resolve it safely so a single unassigned electron cannot become undraggable.
        EnsureCompleteManager();

        startPosition = rectTransform.anchoredPosition;
    }

    private void EnsureCompleteManager()
    {
        if (completeManager == null)
            completeManager = FindObjectOfType<Stage3Level1CompleteManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        EnsureCompleteManager();
        dragStarted = CanDrag;
        if (!dragStarted) return;

        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragStarted || !CanDrag || canvas == null) return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        bool countWrong = dragStarted && CanDrag;
        dragStarted = false;

        if (!accepted)
        {
            // Record a wrong attempt if the electron
            // was not accepted by the bond zone.
            if (countWrong)
                completeManager.RegisterWrongAttempt();

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
