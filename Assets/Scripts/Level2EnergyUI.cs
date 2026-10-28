
using UnityEngine;

public class Level2EnergyUI : MonoBehaviour
{
    [Header("Level 2 Energy Icons")]
    [SerializeField] private GameObject[] energyIcons;

    private EnergyManager energyManager;

    private void Awake()
    {
        // Ensure EnergyManager exists even when Level 2
        // is opened directly in the Unity Editor.
        if (EnergyManager.Instance == null)
        {
            GameObject managerObject =
                new GameObject("EnergyManager");

            managerObject.AddComponent<EnergyManager>();
        }
    }

    private void Start()
    {
        energyManager = EnergyManager.Instance;

        if (energyManager == null)
        {
            Debug.LogError(
                "EnergyManager could not be initialized!"
            );
            return;
        }

        energyManager.OnEnergyChanged += UpdateEnergyIcons;

        energyManager.RefreshEnergy();

        UpdateEnergyIcons();
    }

    private void OnDestroy()
    {
        if (energyManager != null)
        {
            energyManager.OnEnergyChanged -= UpdateEnergyIcons;
        }
    }

    private void UpdateEnergyIcons()
    {
        if (energyManager == null)
            return;

        int energy = energyManager.CurrentEnergy;

        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] != null)
            {
                energyIcons[i].SetActive(i < energy);
            }
        }
    }
}
