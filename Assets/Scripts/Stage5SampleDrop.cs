using UnityEngine;
using UnityEngine.EventSystems;

public class Stage5SampleDrop : MonoBehaviour, IDropHandler
{
    public int BinIndex=-1; // -1 = testing station, 0/1/2 = ionic/molecular/metallic
    [HideInInspector] public Stage5ElementCrucibleGame Game;
    public void OnDrop(PointerEventData e)
    {
        if(Game==null||!Game.CanInteract||e.pointerDrag==null)return;
        var token=e.pointerDrag.GetComponent<Stage5SampleDrag>();
        if(token==null||token.Game!=Game)return;
        Game.DropSample(token.SampleIndex,BinIndex);
    }
}
