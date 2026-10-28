
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage3Level1RetryManager : MonoBehaviour
{
    [Header("Failed Game UI")]
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private Button retryButton;

    [Header("Energy Manager")]
    [SerializeField] private Stage3Level1EnergyManager energyManager;

    private void Start()
    {
        UpdateRetryButton();
    }

    private void OnEnable()
    {
        // Refresh the retry state when this object is enabled.
        UpdateRetryButton();
    }

    private void Update()
    {
        if (failedPanel != null && failedPanel.activeInHierarchy)
            UpdateRetryButton();
    }

    private void UpdateRetryButton()
    {
        if (retryButton == null || energyManager == null)
            return;

        bool canRetry =
            energyManager.RunFailed &&
            !energyManager.IsOutOfEnergy;

        retryButton.interactable = canRetry;
    }

    public void RetryLevel()
    {
        if (energyManager == null ||
            !energyManager.RunFailed ||
            energyManager.IsOutOfEnergy)
        {
            Debug.Log("Retry unavailable: run not failed or no energy.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage3Level1");
    }
}
