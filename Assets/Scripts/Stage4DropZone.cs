using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Stage4DropZone : MonoBehaviour, IDropHandler
{
    public int Kind;
    [HideInInspector] public Stage4ElectronOceanGame Game;
    private bool occupied;
    public void ResetZone()
    {
        occupied=false;
        var label=GetComponentInChildren<TMP_Text>(true);
        if(label!=null)label.text=Kind==0?"+":Kind==1?".":Kind==2?"-":"+";
        var image=GetComponent<Image>();
        if(image!=null)image.color=new Color32(39,78,108,255);
    }
    public void OnDrop(PointerEventData e)
    {
        if(occupied || Game==null || !Game.CanInteract)return;
        var token=e.pointerDrag==null?null:e.pointerDrag.GetComponent<Stage4Draggable>();
        if(token==null || token.Placed || token.Game!=Game || token.Kind!=Kind)return;
        occupied=true;token.Accept();
        var label=GetComponentInChildren<TMP_Text>(true);
        if(label!=null)label.text=Game.GetPlacedSymbol(Kind);
        var image=GetComponent<Image>();
        if(image!=null)image.color=new Color32(24,129,113,255);
        Game.ZoneFilled(Kind);
    }
}
