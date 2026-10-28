using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Stage2IonDropZone : MonoBehaviour, IDropHandler
{
    public int RequiredKind;
    [HideInInspector] public Stage2IonForgeGame Game;
    private bool occupied;
    public void ResetZone()
    {
        occupied=false;
        var img=GetComponent<Image>();
        if(img!=null)img.color=new Color32(42,88,122,255);
        var text=GetComponentInChildren<TMP_Text>(true);
        if(text!=null)text.text=RequiredKind==0?"DROP e-":RequiredKind==1?"DROP +":"DROP -";
    }
    public void OnDrop(PointerEventData eventData)
    {
        if(occupied||Game==null||!Game.CanInteract)return;
        var drag=eventData.pointerDrag==null?null:eventData.pointerDrag.GetComponent<Stage2IonDraggable>();
        if(drag==null || drag.Game!=Game || drag.Accepted || drag.Kind!=RequiredKind)return;
        occupied=true;
        drag.Consume();
        var text=GetComponentInChildren<TMP_Text>(true);
        if(text!=null)text.text=Game.GetAcceptedLabel(RequiredKind);
        var img=GetComponent<Image>();
        if(img!=null)img.color=new Color32(13,137,123,255);
        Game.AcceptedDrop(RequiredKind);
    }
}
