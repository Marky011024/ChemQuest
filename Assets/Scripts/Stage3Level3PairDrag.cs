using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class Stage3Level3PairDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool IsBondPair;
    [HideInInspector] public Stage3Level3Game Game;
    [HideInInspector] public bool Accepted;

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;
    private Vector2 home;
    private bool dragging;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        home = rect.anchoredPosition;
    }

    public void Init(Stage3Level3Game game)
    {
        Game = game;
        if (rect == null) Awake();
        home = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = !Accepted && Game != null && Game.CanInteract;
        if (!dragging) return;
        group.blocksRaycasts = false;
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !Game.CanInteract || canvas == null) return;
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
        if (!dragging) return;
        dragging = false;
        if (!Accepted)
        {
            rect.anchoredPosition = home;
            if (Game != null && Game.CanInteract) Game.WrongPlacement();
        }
    }

    public void Accept()
    {
        Accepted = true;
        group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void Restore()
    {
        Accepted = false;
        if(rect==null)rect=GetComponent<RectTransform>();
        if(group==null)group=GetComponent<CanvasGroup>();
        rect.anchoredPosition=home;
        group.blocksRaycasts=true;
        gameObject.SetActive(true);
    }
}