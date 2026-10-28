
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutOfEnergyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text energyTimerText;
    [SerializeField] private GameObject outOfEnergyPanel;

    private void Update()
    {
        if (EnergyManager.Instance == null)
            return;

        EnergyManager energy = EnergyManager.Instance;

        if (energy.HasEnergy)
        {
            if (outOfEnergyPanel != null &&
                outOfEnergyPanel.activeSelf)
            {
                outOfEnergyPanel.SetActive(false);
            }

            return;
        }

        if (outOfEnergyPanel != null &&
            !outOfEnergyPanel.activeSelf)
        {
            outOfEnergyPanel.SetActive(true);
        }

        int seconds = energy.SecondsUntilNextEnergy();

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        if (energyTimerText != null)
        {
            energyTimerText.text =
                "Next Energy In: " +
                minutes.ToString("00") + ":" +
                remainingSeconds.ToString("00");
        }
    }

    public void BackToLevelSelection()
    {
        SceneManager.LoadScene("Stage1LevelSelection");
    }
}
