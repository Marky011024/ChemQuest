
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level3RetryManager : MonoBehaviour
{
    [Header("Retry Button")]
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject levelFailedPanel;
    [SerializeField] private Button failedPanelRetryButton;

    [Header("Failure UI Cleanup")]
    [SerializeField] private GameObject ionPuzzlePanel;

    [Header("Energy Manager")]
    [SerializeField] private Level3EnergyManager energyManager;

    private bool retryShown = false;

    private void Start()
    {
        if (retryButton != null)
            retryButton.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);
    }

    private void Update()
    {
        if (energyManager == null || !energyManager.RunFailed)
            return;

        if (!retryShown)
        {
            retryShown = true;

            // Reuse the same saved Level Failed panel as Stage 1 Level 2.
            if (levelFailedPanel != null)
            {
                // Hide the old puzzle and inline failed-message UI beneath the popup.
                // The saved Level 2-style failed panel lives directly under Canvas.
                if (ionPuzzlePanel != null)
                    ionPuzzlePanel.SetActive(false);
                if (retryButton != null)
                    retryButton.SetActive(false);

                levelFailedPanel.SetActive(true);
                levelFailedPanel.transform.SetAsLastSibling();
            }
            else if (retryButton != null && !energyManager.IsOutOfEnergy)
            {
                // Keep the older inline Retry button as a fallback.
                retryButton.SetActive(true);
            }
        }

        // When energy runs out, prevent retry until it regenerates.
        if (failedPanelRetryButton != null)
            failedPanelRetryButton.interactable = !energyManager.IsOutOfEnergy;
    }

    public void RetryLevel()
    {
        if (energyManager == null ||
            !energyManager.RunFailed ||
            energyManager.IsOutOfEnergy)
            return;

        SceneManager.LoadScene("Stage1Level3");
    }
}
