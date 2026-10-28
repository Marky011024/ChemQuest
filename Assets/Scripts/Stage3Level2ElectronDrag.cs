using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class Stage3Level2ElectronDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Five electrons from N; three from H. The N-H bonds need one each,
    // while the nitrogen lone pair needs the final two N electrons.
    public bool FromNitrogen;
    [HideInInspector] public Stage3Level2AmmoniaGame Game;
    [HideInInspector] public bool Accepted;

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;
    private Vector2 initialPosition;
    private bool started;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        initialPosition = rect.anchoredPosition;
    }

    public void Configure(Stage3Level2AmmoniaGame game, bool fromNitrogen)
    {
        Game = game;
        FromNitrogen = fromNitrogen;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        started = !Accepted && Game != null && Game.CanDragElectron();
        if(!started) return;
        group.blocksRaycasts = false;
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData data)
    {
        if (!started || Game == null || !Game.CanDragElectron() || canvas == null) return;
        rect.anchoredPosition += data.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData data)
    {
        group.blocksRaycasts = true;
        if (!started) return;
        started = false;
        if (!Accepted)
        {
            rect.anchoredPosition = initialPosition;
            if (Game != null && Game.CanDragElectron()) Game.RegisterWrongDrop();
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
        gameObject.SetActive(true);
        group.blocksRaycasts = true;
        if(rect == null) rect = GetComponent<RectTransform>();
        rect.anchoredPosition = initialPosition;
    }
}
