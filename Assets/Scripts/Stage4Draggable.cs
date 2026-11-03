using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform),typeof(CanvasGroup))]
public class Stage4Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Kind; // 0=metal core, 1=delocalized electron, 2=negative terminal, 3=positive terminal
    [HideInInspector] public Stage4ElectronOceanGame Game;
    [HideInInspector] public bool Placed;
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;
    private Vector2 home;
    private bool dragging;

    private void Awake()
    {
        rect=GetComponent<RectTransform>();
        canvas=GetComponentInParent<Canvas>();
        group=GetComponent<CanvasGroup>();
        home=rect.anchoredPosition;
    }

    public void Bind(Stage4ElectronOceanGame game)
    {
        Game=game;
        if(rect==null)Awake();
        home=rect.anchoredPosition;
    }
    public void OnBeginDrag(PointerEventData e)
    {
        dragging=!Placed && Game!=null && Game.CanInteract;
        if(!dragging)return;
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
        group.blocksRaycasts=true;
        if(!dragging)return;
        dragging=false;
        if(!Placed)
        {
            rect.anchoredPosition=home;
            if(Game!=null && Game.CanInteract)Game.RegisterMistake();
        }
    }
    public void Accept()
    {
        Placed=true;
        group.blocksRaycasts=false;
        gameObject.SetActive(false);
    }
    public void ResetToken()
    {
        Placed=false;
        if(rect==null)rect=GetComponent<RectTransform>();
        if(group==null)group=GetComponent<CanvasGroup>();
        rect.anchoredPosition=home;
        group.blocksRaycasts=true;
        gameObject.SetActive(true);
    }
}
