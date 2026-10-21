using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2AttemptManager : MonoBehaviour
{
    [SerializeField] private GameObject[] energyIcons;
    [SerializeField] private GameObject levelFailedPanel;

    private const string EnergyKey = "PlayerEnergy";
    private const int MaxEnergy = 5;

    private int currentEnergy;

    private void Start()
    {
        // Create the saved energy value if this is the first time.
        if (!PlayerPrefs.HasKey(EnergyKey))
        {
            PlayerPrefs.SetInt(EnergyKey, MaxEnergy);
            PlayerPrefs.Save();
        }

        currentEnergy = PlayerPrefs.GetInt(EnergyKey, MaxEnergy);

        UpdateEnergyDisplay();

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(false);
        }
    }

    public void SecondWrongAttempt()
    {
        if (currentEnergy > 0)
        {
            currentEnergy--;

            PlayerPrefs.SetInt(EnergyKey, currentEnergy);
            PlayerPrefs.Save();
        }

        UpdateEnergyDisplay();

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }

        Debug.Log(
            "Second incorrect answer. Energy remaining: " + currentEnergy
        );
    }

    private void UpdateEnergyDisplay()
    {
        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] != null)
            {
                energyIcons[i].SetActive(i < currentEnergy);
            }
        }
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}