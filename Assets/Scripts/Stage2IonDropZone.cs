using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Stage2IonDropZone : MonoBehaviour, IDropHandler
{
    public int RequiredKind;
    public int SlotIndex;
    [HideInInspector] public Stage2IonForgeGame Game;

    private bool occupied;
    private int placedKind = -1;
    public bool IsOccupied => occupied;
    public bool IsCorrect => occupied && placedKind == RequiredKind;

    public void ResetZone()
    {
        occupied = false;
        placedKind = -1;
        var img = GetComponent<Image>();
        if (img != null)
            img.color = RequiredKind == 0
                ? new Color32(63, 197, 228, 38)
                : new Color32(42, 88, 122, 255);

        var text = GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = RequiredKind == 0 ? "" : RequiredKind == 1 ? "DROP +" : "DROP -";
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (occupied || Game == null || !Game.CanInteract) return;
        var drag = eventData.pointerDrag == null
            ? null
            : eventData.pointerDrag.GetComponent<Stage2IonDraggable>();
        if (drag == null || drag.Game != Game || drag.Accepted) return;

        // Levels 1 and 2 keep their strict electron-transfer validation.
        // In Level 3, ALL ions can be placed. Validate the whole lattice
        // only after the player presses FORM BOND.
        if (!Game.IsLatticeLevel && drag.Kind != RequiredKind) return;
        if (RequiredKind == 0 && !Game.CanReceiveElectronAt(SlotIndex)) return;

        occupied = true;
        placedKind = drag.Kind;
        string symbol = drag.GetComponentInChildren<TMP_Text>(true)?.text;
        drag.Consume();

        var label = GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = Game.IsLatticeLevel
                ? (!string.IsNullOrEmpty(symbol) ? symbol : Game.GetAcceptedLabel(placedKind))
                : RequiredKind == 0 ? "" : Game.GetAcceptedLabel(RequiredKind);

        var img = GetComponent<Image>();
        if (img != null)
            img.color = RequiredKind == 0
                ? new Color32(13, 137, 123, 55)
                : new Color32(13, 137, 123, 255);

        Game.AcceptedDrop(placedKind);
    }
}
