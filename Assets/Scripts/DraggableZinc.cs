using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableZinc : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Image zincImage;

    private Vector2 startingPosition;
    private bool accepted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        zincImage = GetComponent<Image>();

        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        accepted = false;

        // Allow the flask underneath to receive the drop event.
        zincImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Turn zinc raycasting back on.
        zincImage.raycastTarget = true;

        // Wrong drop = return to zinc station.
        if (!accepted)
        {
            rectTransform.anchoredPosition = startingPosition;
        }
    }

    public void ZincAccepted()
    {
        accepted = true;

        // Hide the zinc dish after it has been
        // successfully added to the reaction flask.
        gameObject.SetActive(false);
    }
}