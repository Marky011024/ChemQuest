
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3RetryManager : MonoBehaviour
{
    [Header("Retry Button")]
    [SerializeField] private GameObject retryButton;

    [Header("Energy Manager")]
    [SerializeField] private Level3EnergyManager energyManager;

    private bool retryShown = false;

    private void Start()
    {
        if (retryButton != null)
            retryButton.SetActive(false);
    }

    private void Update()
    {
        if (retryShown || energyManager == null)
            return;

        if (energyManager.RunFailed)
        {
            retryShown = true;

            if (retryButton != null && !energyManager.IsOutOfEnergy)
                retryButton.SetActive(true);
        }
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
