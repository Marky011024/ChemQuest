
using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    public const int MaxEnergy = 5;
    private const int RegenSeconds = 15 * 60;

    private const string EnergyKey = "PlayerEnergy";
    private const string TimeKey = "EnergyLastUpdateUTC";

    public int CurrentEnergy { get; private set; }

    public bool HasEnergy => CurrentEnergy > 0;

    public event Action OnEnergyChanged;

    private DateTime lastUpdate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadEnergy();
    }

    private void Update()
    {
        RefreshEnergy();
    }

    private void LoadEnergy()
    {
        CurrentEnergy = Mathf.Clamp(
            PlayerPrefs.GetInt(EnergyKey, MaxEnergy),
            0, MaxEnergy
        );

        string savedTime = PlayerPrefs.GetString(TimeKey, "");

        if (!DateTime.TryParse(
            savedTime,
            null,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out lastUpdate))
        {
            lastUpdate = DateTime.UtcNow;
        }

        RefreshEnergy();
    }

    public void RefreshEnergy()
    {
        if (CurrentEnergy >= MaxEnergy)
            return;

        DateTime now = DateTime.UtcNow;
        double elapsed = (now - lastUpdate).TotalSeconds;

        if (elapsed < RegenSeconds)
            return;

        int recovered = (int)(elapsed / RegenSeconds);

        CurrentEnergy = Mathf.Min(
            MaxEnergy,
            CurrentEnergy + recovered
        );

        lastUpdate = lastUpdate.AddSeconds(
            recovered * RegenSeconds
        );

        if (CurrentEnergy >= MaxEnergy)
            lastUpdate = now;

        SaveEnergy();
        OnEnergyChanged?.Invoke();
    }

    public bool LoseEnergy()
    {
        RefreshEnergy();

        if (CurrentEnergy <= 0)
            return false;

        if (CurrentEnergy == MaxEnergy)
            lastUpdate = DateTime.UtcNow;

        CurrentEnergy--;

        SaveEnergy();
        OnEnergyChanged?.Invoke();

        return true;
    }

    public int SecondsUntilNextEnergy()
    {
        RefreshEnergy();

        if (CurrentEnergy >= MaxEnergy)
            return 0;

        double elapsed =
            (DateTime.UtcNow - lastUpdate).TotalSeconds;

        return Mathf.Clamp(
            RegenSeconds - (int)elapsed,
            0,
            RegenSeconds
        );
    }

    private void SaveEnergy()
    {
        PlayerPrefs.SetInt(EnergyKey, CurrentEnergy);
        PlayerPrefs.SetString(TimeKey, lastUpdate.ToString("o"));
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Energy To 5")]
    public void ResetEnergy()
    {
        CurrentEnergy = MaxEnergy;
        lastUpdate = DateTime.UtcNow;
        SaveEnergy();
        OnEnergyChanged?.Invoke();
    }
}
