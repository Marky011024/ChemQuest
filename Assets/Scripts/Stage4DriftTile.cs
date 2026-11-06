using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Stage 4 Level 2 puzzle pieces. The drop is deliberately consequence-free:
// validation and attempt counting happen only on CHECK MODEL.
public class Stage4DriftTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string AnswerId;
    public Stage4DriftSlot CurrentSlot { get; private set; }
    private RectTransform rect;
    private RectTransform home;
    private Vector2 homePosition;
    private Canvas canvas;
    private CanvasGroup group;
    private Transform dragLayer;

    public void Configure(string id, RectTransform tray, Transform dragContainer, Canvas parentCanvas)
    {
        AnswerId=id;
        rect=GetComponent<RectTransform>();
        home=tray;homePosition=rect.anchoredPosition;
        dragLayer=dragContainer;canvas=parentCanvas;
        group=GetComponent<CanvasGroup>();
        if(group==null)group=gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if(canvas==null || !gameObject.activeInHierarchy)return;
        if(CurrentSlot!=null){CurrentSlot.Clear(this);CurrentSlot=null;}
        transform.SetParent(dragLayer,true);
        transform.SetAsLastSibling();
        group.blocksRaycasts=false;
    }

    public void OnDrag(PointerEventData data)
    {
        if(canvas==null || rect==null)return;
        rect.position+=new Vector3(data.delta.x,data.delta.y,0f);
    }

    public void OnEndDrag(PointerEventData data)
    {
        if(group!=null)group.blocksRaycasts=true;
        if(CurrentSlot==null)ReturnHome();
    }

    public void Place(Stage4DriftSlot slot)
    {
        if(CurrentSlot!=null && CurrentSlot!=slot)CurrentSlot.Clear(this);
        CurrentSlot=slot;
        transform.SetParent(slot.transform,false);
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=Vector2.zero;
        rect.localScale=Vector3.one;
        group.blocksRaycasts=true;
    }

    public void ReturnHome()
    {
        CurrentSlot=null;
        if(rect==null||home==null)return;
        transform.SetParent(home,false);
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=homePosition;
        rect.localScale=Vector3.one;
        if(group!=null)group.blocksRaycasts=true;
    }
}

public class Stage4DriftSlot : MonoBehaviour, IDropHandler
{
    public string CorrectAnswerId;
    public Stage4DriftTile Occupant { get; private set; }
    public bool IsFilled => Occupant!=null;
    public bool IsCorrect => Occupant!=null && Occupant.AnswerId==CorrectAnswerId;

    public void OnDrop(PointerEventData data)
    {
        var tile=data.pointerDrag==null?null:data.pointerDrag.GetComponent<Stage4DriftTile>();
        if(tile==null)return;
        if(Occupant!=null && Occupant!=tile)Occupant.ReturnHome();
        Occupant=tile;
        tile.Place(this);
    }

    public void Clear(Stage4DriftTile tile)
    {
        if(Occupant==tile)Occupant=null;
    }

    public void ResetSlot()
    {
        if(Occupant!=null)Occupant.ReturnHome();
        Occupant=null;
    }
}
