using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform),typeof(CanvasGroup))]
public class Stage2IonDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Kind; // Level 1/2: 0 = transferable electron; Level 3: 1 = cation, 2 = anion
    [HideInInspector] public Stage2IonForgeGame Game;
    [HideInInspector] public bool Accepted;
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
    public void Initialize(Stage2IonForgeGame game)
    {
        Game=game;
        if(rect==null)Awake();
        home=rect.anchoredPosition;
        Accepted=false;
        gameObject.SetActive(true);
        group.blocksRaycasts=true;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging=!Accepted && Game!=null && Game.CanInteract;
        if(!dragging)return;
        group.blocksRaycasts=false;
        rect.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData eventData)
    {
        if(!dragging || Game==null || !Game.CanInteract || canvas==null)return;
        rect.anchoredPosition+=eventData.delta/canvas.scaleFactor;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts=true;
        if(!dragging)return;
        dragging=false;
        if(Accepted)return;
        rect.anchoredPosition=home;
        if(Game!=null&&Game.CanInteract)Game.WrongAttempt();
    }
    public void Consume()
    {
        Accepted=true;
        group.blocksRaycasts=false;
        gameObject.SetActive(false);
    }
}
