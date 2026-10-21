
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Level3CrystalSlot : MonoBehaviour, IDropHandler
{
    public enum IonType
    {
        None,
        Calcium,
        Carbonate,
        Sodium
    }

    [Header("Slot Settings")]
    [SerializeField] private IonType expectedIon;

    private IonType placedIon = IonType.None;
    private TMP_Text ionLabel;

    public IonType ExpectedIon => expectedIon;
    public IonType PlacedIon => placedIon;

    public bool IsCorrect =>
        placedIon != IonType.None &&
        placedIon == expectedIon;

    private void Awake()
    {
        GameObject labelObject = new GameObject("PlacedIonLabel");
        labelObject.transform.SetParent(transform, false);

        RectTransform rect =
            labelObject.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        ionLabel = labelObject.AddComponent<TextMeshProUGUI>();

       ionLabel.horizontalAlignment = HorizontalAlignmentOptions.Center;
        ionLabel.verticalAlignment = VerticalAlignmentOptions.Middle;

        // Bigger and bolder ion labels
        ionLabel.fontSize = 18;
        ionLabel.fontStyle = FontStyles.Bold;

        ionLabel.color = Color.black;
        ionLabel.raycastTarget = false;
        ionLabel.textWrappingMode = TextWrappingModes.NoWrap;
        ionLabel.overflowMode = TextOverflowModes.Overflow;
        ionLabel.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject.name == "CalciumIon")
        {
            placedIon = IonType.Calcium;
            ionLabel.text = "Ca<sup>2+</sup>";
        }
        else if (draggedObject.name == "CarbonateIon")
        {
            placedIon = IonType.Carbonate;
            ionLabel.text = "CO<sub>3</sub><sup>2-</sup>";
        }
        else if (draggedObject.name == "SodiumIon")
        {
            placedIon = IonType.Sodium;
            ionLabel.text = "Na<sup>+</sup>";
        }
        else
        {
            return;
        }

        Debug.Log(
            gameObject.name + " received " + placedIon
        );
    }

    public void ResetSlot()
    {
        placedIon = IonType.None;

        if (ionLabel != null)
        {
            ionLabel.text = "";
        }
    }
}
