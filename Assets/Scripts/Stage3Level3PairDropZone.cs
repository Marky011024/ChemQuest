using UnityEngine;
using UnityEngine.EventSystems;

public class Stage3Level3PairDropZone : MonoBehaviour, IDropHandler
{
    public bool RequiresBondPair;
    [HideInInspector] public Stage3Level3Game Game;
    private bool filled;

    public void ResetZone() { filled=false; }

    public void OnDrop(PointerEventData eventData)
    {
        if (filled || Game==null || !Game.CanInteract) return;
        var token=eventData.pointerDrag==null?null:
            eventData.pointerDrag.GetComponent<Stage3Level3PairDrag>();
        if(token==null || token.Accepted || token.Game!=Game) return;
        if(token.IsBondPair!=RequiresBondPair) return; // token reports one wrong attempt on end drag
        filled=true;
        token.Accept();
        Game.PairPlaced(this);
    }
}