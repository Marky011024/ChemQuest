
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class H2OWaterBondManager : MonoBehaviour, IDropHandler
{
    [Header("Bond Configuration")]
    [SerializeField] private Image bondDropZoneImage;
    [SerializeField] private TMP_Text sharedElectronPair;

    [Header("Water Puzzle Manager")]
    [SerializeField] private H2OWaterPuzzleManager puzzleManager;

    [Header("Bond Side")]
    [SerializeField] private bool isLeftBond = true;

    private int electronsAccepted = 0;
    private bool bondCompleted = false;

    private void Start()
    {
        if (sharedElectronPair != null)
            sharedElectronPair.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (bondCompleted || electronsAccepted >= 2)
            return;

        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject == null)
            return;

        H2DraggableElectron electron =
            droppedObject.GetComponent<H2DraggableElectron>();

        // Invalid objects are not accepted.
        // Failed electron drops are tracked by
        // H2DraggableElectron.
        if (electron == null || !electron.enabled)
        {
            Debug.Log("Invalid object dropped on water bond zone!");
            return;
        }

        electronsAccepted++;

        electron.MarkAccepted();
        droppedObject.SetActive(false);

        Debug.Log(
            (isLeftBond ? "Left" : "Right") +
            " water bond: " + electronsAccepted + "/2"
        );

        if (electronsAccepted == 2)
        {
            CompleteBond();
        }
    }

    private void CompleteBond()
    {
        if (bondCompleted)
            return;

        bondCompleted = true;

        if (bondDropZoneImage != null)
        {
            bondDropZoneImage.color =
                new Color32(56, 189, 248, 255);
        }

        if (sharedElectronPair != null)
            sharedElectronPair.gameObject.SetActive(true);

        if (puzzleManager != null)
        {
            puzzleManager.BondCompleted(isLeftBond);
        }
        else
        {
            Debug.LogWarning(
                "H2OWaterPuzzleManager is not assigned!"
            );
        }
    }
}
