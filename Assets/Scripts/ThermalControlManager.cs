
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThermalControlManager : MonoBehaviour
{
    [SerializeField] private Slider thermalSlider;
    [SerializeField] private TMP_Text temperatureText;

    private const float InitialTemperature = 25f;
    private const float ReactionTemperature = 30f;

    private void Awake()
    {
        if (thermalSlider != null)
        {
            thermalSlider.minValue = 20f;
            thermalSlider.maxValue = 40f;
            thermalSlider.wholeNumbers = true;

            thermalSlider.value = InitialTemperature;

            // Temperature is observed, not controlled.
            thermalSlider.interactable = false;

            thermalSlider.onValueChanged.AddListener(
                UpdateTemperature
            );
        }

        UpdateTemperature(InitialTemperature);
    }

    private void UpdateTemperature(float temperature)
    {
        if (temperatureText != null)
        {
            temperatureText.text =
                "TEMPERATURE: " +
                temperature.ToString("0") + "°C";
        }
    }

    public void ShowReactionTemperature()
    {
        if (thermalSlider != null)
        {
            thermalSlider.value = ReactionTemperature;
            thermalSlider.interactable = false;
        }

        UpdateTemperature(ReactionTemperature);

        Debug.Log(
            "Simulated temperature increased from " +
            "25°C to 30°C. Identify the energy change."
        );
    }
}
