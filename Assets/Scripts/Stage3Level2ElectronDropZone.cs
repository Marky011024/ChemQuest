using UnityEngine;
using UnityEngine.EventSystems;

public class Stage3Level2ElectronDropZone : MonoBehaviour, IDropHandler
{
    public int ZoneIndex; // 0=left N-H, 1=right N-H, 2=bottom N-H, 3=N lone pair
    [HideInInspector] public Stage3Level2AmmoniaGame Game;

    public void OnDrop(PointerEventData eventData)
    {
        if (Game == null || !Game.CanDragElectron()) return;
        var electron=eventData.pointerDrag==null ? null :
            eventData.pointerDrag.GetComponent<Stage3Level2ElectronDrag>();
        if(electron!=null) Game.TryPlaceElectron(ZoneIndex,electron);
    }
}
