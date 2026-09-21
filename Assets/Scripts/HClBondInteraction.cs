using UnityEngine;
using UnityEngine.EventSystems;

public class HClBondInteraction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private BondProgressManager bondProgressManager;
    [SerializeField] private Level1AttemptManager attemptManager;

    private bool bondBroken = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (bondBroken)
            return;

        bondBroken = true;

        Debug.Log(gameObject.name + " bond traced and broken.");

        // The player is now performing the correct action,
        // so hide the previous mistake hint.
        if (attemptManager != null)
        {
            attemptManager.HideMistakeHint();
        }

        if (bondProgressManager != null)
        {
            bondProgressManager.BondBroken();
        }

        gameObject.SetActive(false);
    }
}