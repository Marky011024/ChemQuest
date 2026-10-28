using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Stage5SampleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int SampleIndex;
    [HideInInspector] public Stage5ElementCrucibleGame Game;
    private RectTransform rect;
    private CanvasGroup group;
    private Canvas canvas;
    private Vector2 home;
    private bool dragging;
    private void Awake()
    {
        rect=GetComponent<RectTransform>();
        group=GetComponent<CanvasGroup>();
        canvas=GetComponentInParent<Canvas>();
        home=rect.anchoredPosition;
    }
    public void ResetToken()
    {
        if(rect==null)Awake();
        rect.anchoredPosition=home;
        group.blocksRaycasts=true;
    }
    public void OnBeginDrag(PointerEventData e)
    {
        if(Game==null || !Game.CanInteract)return;
        dragging=true;
        group.blocksRaycasts=false;
        rect.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData e)
    {
        if(!dragging || Game==null || !Game.CanInteract || canvas==null)return;
        rect.anchoredPosition+=e.delta/canvas.scaleFactor;
    }
    public void OnEndDrag(PointerEventData e)
    {
        if(!dragging)return;
        dragging=false;
        group.blocksRaycasts=true;
        rect.anchoredPosition=home;
    }
}
