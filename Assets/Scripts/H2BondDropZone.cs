
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class H2BondDropZone : MonoBehaviour, IDropHandler
{
    [Header("Bond Visuals")]
    [SerializeField] private Image dropZoneImage;
    [SerializeField] private TMP_Text sharedElectronPair;
    [SerializeField] private TMP_Text successText;

    [Header("Level Completion")]
    [SerializeField] private Stage3Level1CompleteManager completeManager;

    private int electronsAccepted = 0;
    private bool puzzleCompleted = false;

    private void Start()
    {
        if (sharedElectronPair != null)
            sharedElectronPair.gameObject.SetActive(false);

        if (successText != null)
            successText.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (puzzleCompleted || electronsAccepted >= 2 ||
            (completeManager != null && !completeManager.CanInteract()) || Time.timeScale <= 0f)
            return;

        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject == null)
            return;

        H2DraggableElectron electron =
            droppedObject.GetComponent<H2DraggableElectron>();

        // INVALID OBJECT DROPPED
        if (electron == null || !electron.enabled || !electron.CanDrag)
        {
            RegisterWrongDrop();
            return;
        }

        // CORRECT ELECTRON ACCEPTED
        electronsAccepted++;

        electron.MarkAccepted();
        droppedObject.SetActive(false);

        Debug.Log(
            "Electron accepted: " + electronsAccepted + "/2"
        );

        if (electronsAccepted == 2)
        {
            CompleteHydrogenPuzzle();
        }
    }

    private void RegisterWrongDrop()
    {
        // Wrong attempts are handled by H2DraggableElectron.
        // Avoid counting the same drop twice.
        Debug.Log("Invalid object dropped on H2 bond zone!");
    }

    private void CompleteHydrogenPuzzle()
    {
        if (puzzleCompleted)
            return;

        puzzleCompleted = true;

        if (dropZoneImage != null)
        {
            dropZoneImage.color =
                new Color32(56, 189, 248, 255);
        }

        if (sharedElectronPair != null)
            sharedElectronPair.gameObject.SetActive(true);

        if (successText != null)
            successText.gameObject.SetActive(true);

        Debug.Log("H2 SINGLE COVALENT BOND FORMED!");

        if (completeManager != null)
        {
            completeManager.HydrogenPuzzleCompleted();
        }
        else
        {
            Debug.LogWarning(
                "Stage3Level1CompleteManager is not assigned!"
            );
        }
    }
}
