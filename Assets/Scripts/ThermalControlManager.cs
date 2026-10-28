
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThermalControlManager : MonoBehaviour
{
    [Header("Thermal Control UI")]
    [SerializeField] private Slider thermalSlider;
    [SerializeField] private TMP_Text temperatureText;

    [Header("Temperature Settings")]
    [SerializeField] private float initialTemperature = 25f;
    [SerializeField] private float reactionTemperature = 30f;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 3f;

    private Coroutine temperatureAnimation;
    private float currentTemperature;

    private void Awake()
    {
        currentTemperature = initialTemperature;

        if (thermalSlider != null)
        {
            thermalSlider.minValue = 20f;
            thermalSlider.maxValue = 40f;
            thermalSlider.wholeNumbers = false;

            thermalSlider.interactable = false;
            thermalSlider.SetValueWithoutNotify(initialTemperature);
        }

        UpdateTemperature(initialTemperature);
    }

    private void UpdateTemperature(float temperature)
    {
        currentTemperature = temperature;

        if (thermalSlider != null)
        {
            thermalSlider.SetValueWithoutNotify(temperature);
        }

        if (temperatureText != null)
        {
            temperatureText.text =
                "TEMPERATURE: " +
                temperature.ToString("0.0") + "°C";
        }
    }

    public void ShowReactionTemperature()
    {
        if (temperatureAnimation != null)
        {
            StopCoroutine(temperatureAnimation);
        }

        temperatureAnimation = StartCoroutine(
            AnimateTemperature(reactionTemperature)
        );
    }

    private IEnumerator AnimateTemperature(float targetTemperature)
    {
        float startTemperature = currentTemperature;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, animationDuration)
            );

            // Smooth start and finish.
            progress = Mathf.SmoothStep(0f, 1f, progress);

            float temperature = Mathf.Lerp(
                startTemperature,
                targetTemperature,
                progress
            );

            UpdateTemperature(temperature);

            yield return null;
        }

        UpdateTemperature(targetTemperature);

        temperatureAnimation = null;

        Debug.Log(
            "Temperature animation completed: " +
            targetTemperature + "°C."
        );
    }

    public void ResetTemperature()
    {
        if (temperatureAnimation != null)
        {
            StopCoroutine(temperatureAnimation);
            temperatureAnimation = null;
        }

        UpdateTemperature(initialTemperature);
    }
}
