
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1AttemptManager : MonoBehaviour
{
    [Header("Mistake System")]
    [SerializeField] private GameObject mistakeHintText;
    [SerializeField] private GameObject levelFailedPanel;

    [Header("Energy System")]
    [SerializeField] private GameObject[] energyIcons;
    [SerializeField] private GameObject outOfEnergyPanel;

    private int mistakeCount = 0;
    private bool levelFailed = false;

    public int MistakeCount => mistakeCount;
    public bool HasMadeMistake => mistakeCount > 0;

    private void Start()
    {
        if (mistakeHintText != null)
            mistakeHintText.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        if (outOfEnergyPanel != null)
            outOfEnergyPanel.SetActive(false);

        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged +=
                UpdateEnergyDisplay;

            EnergyManager.Instance.RefreshEnergy();

            UpdateEnergyDisplay();

            if (!EnergyManager.Instance.HasEnergy)
            {
                ShowOutOfEnergy();
            }
        }
        else
        {
            Debug.LogError(
                "EnergyManager not found in scene!"
            );
        }
    }

    private void OnDestroy()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged -=
                UpdateEnergyDisplay;
        }
    }

    public void RegisterMistake()
    {
        if (levelFailed)
            return;

        if (EnergyManager.Instance == null)
            return;

        if (!EnergyManager.Instance.HasEnergy)
        {
            ShowOutOfEnergy();
            return;
        }

        mistakeCount++;

        Debug.Log(
            "Level 1 mistake count: " + mistakeCount
        );

        if (mistakeCount == 1)
        {
            if (mistakeHintText != null)
                mistakeHintText.SetActive(true);

            Debug.Log(
                "First mistake: Conceptual hint shown."
            );
        }
        else if (mistakeCount >= 2)
        {
            levelFailed = true;

            if (mistakeHintText != null)
                mistakeHintText.SetActive(false);

            bool energyLost =
                EnergyManager.Instance.LoseEnergy();

            if (energyLost)
            {
                Debug.Log(
                    "Energy lost! Remaining: " +
                    EnergyManager.Instance.CurrentEnergy
                );
            }

            if (!EnergyManager.Instance.HasEnergy)
            {
                ShowOutOfEnergy();
            }
            else
            {
                if (levelFailedPanel != null)
                    levelFailedPanel.SetActive(true);
            }
        }
    }

    public void HideMistakeHint()
    {
        if (mistakeHintText != null)
            mistakeHintText.SetActive(false);
    }

    public void RetryLevel()
    {
        if (EnergyManager.Instance == null)
            return;

        EnergyManager.Instance.RefreshEnergy();

        if (!EnergyManager.Instance.HasEnergy)
        {
            ShowOutOfEnergy();
            return;
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    private void ShowOutOfEnergy()
    {
        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        if (mistakeHintText != null)
            mistakeHintText.SetActive(false);

        if (outOfEnergyPanel != null)
            outOfEnergyPanel.SetActive(true);

        Debug.Log(
            "OUT OF ENERGY! Wait for regeneration."
        );
    }

    private void UpdateEnergyDisplay()
    {
        if (EnergyManager.Instance == null)
            return;

        int currentEnergy =
            EnergyManager.Instance.CurrentEnergy;

        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] != null)
            {
                energyIcons[i].SetActive(
                    i < currentEnergy
                );
            }
        }

        Debug.Log(
            "Energy display updated: " + currentEnergy
        );
    }

    [ContextMenu("Reset Energy To 5")]
    public void ResetEnergyToFive()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.ResetEnergy();
            UpdateEnergyDisplay();
        }
    }
}
