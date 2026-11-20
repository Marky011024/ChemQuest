using TMPro;
using UnityEngine;

// Stage 1 Level 1 only. Observes the existing EnergyManager; never alters
// the two-strike mistakes, deductions, score awards or recovery timer.
public sealed class Stage1Level1HUDCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    private EnergyManager subscribed;

    private void OnEnable()
    {
        EnsureManager();
        Subscribe();
        Render();
    }

    private void OnDisable()
    {
        if (subscribed != null) subscribed.OnEnergyChanged -= Render;
        subscribed = null;
    }

    private void Update()
    {
        if (subscribed != EnergyManager.Instance) Subscribe();
        if (Time.frameCount % 60 == 0) Render();
    }

    private static void EnsureManager()
    {
        // Existing scene already contains EnergyManager, including editor
        // versions where its Awake runs later than this component's OnEnable.
        // No new manager or duplicate EnergyManager is created here.
    }

    private void Subscribe()
    {
        if (subscribed != null) subscribed.OnEnergyChanged -= Render;
        subscribed = EnergyManager.Instance;
        if (subscribed != null) subscribed.OnEnergyChanged += Render;
        Render();
    }

    private void Render()
    {
        if (countText == null) return;
        int value = subscribed != null ? subscribed.CurrentEnergy :
            PlayerPrefs.GetInt("PlayerEnergy", EnergyManager.MaxEnergy);
        countText.text = Mathf.Clamp(value,0,EnergyManager.MaxEnergy) +
            " / " + EnergyManager.MaxEnergy;
    }

#if UNITY_EDITOR
    public void SetCounter(TMP_Text counter) { countText = counter; }
#endif
}
