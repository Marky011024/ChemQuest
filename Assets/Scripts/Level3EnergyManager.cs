
using System;
using UnityEngine;
using UnityEngine.UI;

public class Level3EnergyManager : MonoBehaviour
{
    [Header("Energy Icons")]
    [SerializeField] private Image[] energyIcons;

    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 5;
    [SerializeField] private int recoveryMinutes = 15;

    private const string EnergyKey = "ChemQuestEnergy";
    private const string RecoveryTimeKey = "ChemQuestEnergyRecoveryTime";

    private int currentEnergy;
    private bool runFailed = false;

    private double lastRecoveryTime;

    public bool IsOutOfEnergy => currentEnergy <= 0;
    public bool RunFailed => runFailed;
    public int CurrentEnergy => currentEnergy;

    private double CurrentTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private void Start()
    {
        currentEnergy = PlayerPrefs.GetInt(
            EnergyKey, maxEnergy
        );

        currentEnergy = Mathf.Clamp(
            currentEnergy, 0, maxEnergy
        );

        string savedTime = PlayerPrefs.GetString(
            RecoveryTimeKey, ""
        );

        if (!double.TryParse(
            savedTime,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out lastRecoveryTime))
        {
            lastRecoveryTime = CurrentTime();
        }

        runFailed = false;

        RecoverEnergy();
        UpdateEnergyDisplay();
    }

    private void Update()
    {
        if (currentEnergy < maxEnergy)
        {
            RecoverEnergy();
        }
    }

    public void RegisterWrongAttempt()
    {
        // The puzzle script counts incorrect answers.
        // Energy is deducted only when the run fails.
    }

    public void FailRun()
    {
        if (runFailed || IsOutOfEnergy)
            return;

        RecoverEnergy();

        if (currentEnergy <= 0)
            return;

        bool wasFull = currentEnergy == maxEnergy;

        runFailed = true;

        currentEnergy = Mathf.Max(
            0, currentEnergy - 1
        );

        // Start the recovery timer when energy
        // first drops below maximum.
        if (wasFull)
        {
            lastRecoveryTime = CurrentTime();
        }

        SaveEnergy();
        UpdateEnergyDisplay();

        Debug.Log(
            "Level 3 run failed. Energy remaining: "
            + currentEnergy
        );
    }

    private void RecoverEnergy()
    {
        if (currentEnergy >= maxEnergy)
            return;

        double now = CurrentTime();
        double recoverySeconds = recoveryMinutes * 60.0;

        if (recoverySeconds <= 0)
            return;

        double elapsed = Math.Max(
            0, now - lastRecoveryTime
        );

        int recoveredHearts = (int)Math.Floor(
            elapsed / recoverySeconds
        );

        if (recoveredHearts <= 0)
            return;

        currentEnergy = Mathf.Min(
            maxEnergy,
            currentEnergy + recoveredHearts
        );

        lastRecoveryTime +=
            recoveredHearts * recoverySeconds;

        if (currentEnergy >= maxEnergy)
        {
            lastRecoveryTime = now;
        }

        SaveEnergy();
        UpdateEnergyDisplay();

        Debug.Log(
            "Energy recovered! Current energy: "
            + currentEnergy
        );
    }

    private void SaveEnergy()
    {
        PlayerPrefs.SetInt(
            EnergyKey, currentEnergy
        );

        PlayerPrefs.SetString(
            RecoveryTimeKey,
            lastRecoveryTime.ToString(
                System.Globalization.CultureInfo.InvariantCulture
            )
        );

        PlayerPrefs.Save();
    }

    private void UpdateEnergyDisplay()
    {
        if (energyIcons == null)
            return;

        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] != null)
            {
                energyIcons[i].enabled =
                    i < currentEnergy;
            }
        }
    }
}
