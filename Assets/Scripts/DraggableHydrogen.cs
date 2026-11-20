using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableHydrogen : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startingPosition;
    private bool accepted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (accepted)
            return;

        canvasGroup.blocksRaycasts = false;

        // Visual tutorial pointer only. Does not modify the drag/drop logic.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Stage1Level1")
        {
            var guide = FindFirstObjectByType<Stage1Level1NpcTutorial>();
            if (guide != null) guide.OnHydrogenDragBegin(rectTransform);
        }
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
        canvasGroup.blocksRaycasts = true;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Stage1Level1")
        {
            var guide = FindFirstObjectByType<Stage1Level1NpcTutorial>();
            if (guide != null) guide.OnHydrogenDragEnd(rectTransform);
        }

        if (!accepted)
        {
            rectTransform.anchoredPosition = startingPosition;
        }
    }

    public void HydrogenAccepted()
    {
        accepted = true;
        canvasGroup.blocksRaycasts = true;
    }
}