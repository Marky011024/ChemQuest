
using UnityEngine;
using UnityEngine.UI;

public class Level3EnergyManager : MonoBehaviour
{
    [Header("Energy Icons")]
    [SerializeField] private Image[] energyIcons;

    private bool runFailed = false;

    public bool IsOutOfEnergy =>
        EnergyManager.Instance == null ||
        !EnergyManager.Instance.HasEnergy;

    public bool RunFailed => runFailed;

    public int CurrentEnergy =>
        EnergyManager.Instance != null
            ? EnergyManager.Instance.CurrentEnergy
            : 0;

    private void Start()
    {
        runFailed = false;

        if (EnergyManager.Instance == null)
        {
            Debug.LogError(
                "Shared EnergyManager not found!"
            );
            UpdateEnergyDisplay();
            return;
        }

        EnergyManager.Instance.OnEnergyChanged +=
            UpdateEnergyDisplay;

        EnergyManager.Instance.RefreshEnergy();
        UpdateEnergyDisplay();
    }

    private void OnDestroy()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged -=
                UpdateEnergyDisplay;
        }
    }

    public void RegisterWrongAttempt()
    {
        // First wrong attempt only shows a hint.
        // No energy deduction.
    }

    public void FailRun()
    {
        if (runFailed || IsOutOfEnergy)
            return;

        runFailed = true;

        EnergyManager.Instance.LoseEnergy();

        UpdateEnergyDisplay();

        Debug.Log(
            "Level 3 run failed. Energy remaining: "
            + CurrentEnergy
        );
    }

    private void UpdateEnergyDisplay()
    {
        if (energyIcons == null)
            return;

        int energy = CurrentEnergy;

        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] != null)
            {
                energyIcons[i].enabled = i < energy;
            }
        }
    }
}
