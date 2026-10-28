
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2AttemptManager : MonoBehaviour
{
    [Header("Level Failed Panel")]
    [SerializeField] private GameObject levelFailedPanel;

    [Header("Out Of Energy Panel")]
    [SerializeField] private GameObject outOfEnergyPanel;

    private bool levelFailed = false;

    private void Start()
    {
        levelFailed = false;

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        if (outOfEnergyPanel != null)
            outOfEnergyPanel.SetActive(false);

        if (EnergyManager.Instance == null)
        {
            Debug.LogError("EnergyManager not found!");
            return;
        }

        EnergyManager.Instance.RefreshEnergy();
        CheckEnergy();
    }

    private void Update()
    {
        // Check energy and regeneration.
        if (EnergyManager.Instance != null)
        {
            CheckEnergy();
        }
    }

    private void CheckEnergy()
    {
        EnergyManager manager = EnergyManager.Instance;

        if (manager == null)
            return;

        manager.RefreshEnergy();

        if (!manager.HasEnergy)
        {
            // FIX: Hide Level Failed Panel
            // when player runs out of energy.
            if (levelFailedPanel != null &&
                levelFailedPanel.activeSelf)
            {
                levelFailedPanel.SetActive(false);
            }

            // Show Out Of Energy Panel.
            if (outOfEnergyPanel != null &&
                !outOfEnergyPanel.activeSelf)
            {
                outOfEnergyPanel.SetActive(true);

                Debug.Log(
                    "OUT OF ENERGY! Wait for regeneration."
                );
            }
        }
        else
        {
            // Hide Out Of Energy Panel
            // when energy becomes available.
            if (outOfEnergyPanel != null &&
                outOfEnergyPanel.activeSelf)
            {
                outOfEnergyPanel.SetActive(false);
            }
        }
    }

    public void SecondWrongAttempt()
    {
        if (levelFailed)
            return;

        EnergyManager manager = EnergyManager.Instance;

        if (manager == null)
        {
            Debug.LogError("EnergyManager not found!");
            return;
        }

        manager.RefreshEnergy();

        if (!manager.HasEnergy)
        {
            CheckEnergy();
            return;
        }

        levelFailed = true;

        // Deduct exactly one shared energy.
        manager.LoseEnergy();

        // Show Level Failed only if
        // player still has energy.
        if (manager.HasEnergy)
        {
            if (levelFailedPanel != null)
                levelFailedPanel.SetActive(true);
        }
        else
        {
            if (levelFailedPanel != null)
                levelFailedPanel.SetActive(false);
        }

        // Update Out Of Energy display.
        CheckEnergy();

        Debug.Log(
            "Second incorrect answer. Energy remaining: "
            + manager.CurrentEnergy
        );
    }

    public void RetryLevel()
    {
        EnergyManager manager = EnergyManager.Instance;

        if (manager == null)
        {
            Debug.LogError("EnergyManager not found!");
            return;
        }

        manager.RefreshEnergy();

        if (!manager.HasEnergy)
        {
            CheckEnergy();

            Debug.Log(
                "Cannot retry. Out of energy!"
            );

            return;
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
