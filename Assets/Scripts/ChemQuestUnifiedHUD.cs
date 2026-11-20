using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shared, editable HUD used by every StageNLevelM scene.
// Added to saved scenes through ChemQuest > HUD > Apply To All Gameplay Scenes.
// This script only OBSERVES the existing score/energy; it never awards points,
// deducts lives or bypasses a level's existing two-strike retry policy.
public sealed class ChemQuestUnifiedHUD : MonoBehaviour
{
    [SerializeField] private Button menuButton;
    [SerializeField] private TMP_Text energyCounter;
    [SerializeField] private TMP_Text scoreCounter;
    [SerializeField] private Image[] lightningIcons = new Image[5];
    [SerializeField] private Color poweredColor = new Color32(26, 223, 255, 255);
    [SerializeField] private Color depletedColor = new Color32(32, 70, 102, 155);
    private TMP_Text sourceScore;
    private EnergyManager hookedEnergy;
    private float nextRefresh;
    private int stage, level;

    private void OnEnable()
    {
        var match = Regex.Match(gameObject.scene.name, @"^Stage(\d+)Level(\d+)$");
        if (match.Success)
        {
            int.TryParse(match.Groups[1].Value, out stage);
            int.TryParse(match.Groups[2].Value, out level);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OpenExistingPause);
            menuButton.onClick.AddListener(OpenExistingPause);
        }
        // Directly launching any gameplay scene must still get the
        // persistent EnergyManager (and its 15-minute regeneration clock).
        if (Application.isPlaying && EnergyManager.Instance == null)
            new GameObject("SharedEnergyManager",typeof(EnergyManager));
        FindSourceScore();
        HookEnergy();
        Draw();
    }

    private void OnDisable()
    {
        if (menuButton != null) menuButton.onClick.RemoveListener(OpenExistingPause);
        if (hookedEnergy != null) hookedEnergy.OnEnergyChanged -= Draw;
        hookedEnergy = null;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + .3f;
        HookEnergy();
        // Scores may be awarded by different level-specific managers.
        if (sourceScore == null) FindSourceScore();
        Draw();
    }

    private void HookEnergy()
    {
        var current = EnergyManager.Instance;
        if (current == hookedEnergy) return;
        if (hookedEnergy != null) hookedEnergy.OnEnergyChanged -= Draw;
        hookedEnergy = current;
        if (hookedEnergy != null) hookedEnergy.OnEnergyChanged += Draw;
    }

    private void FindSourceScore()
    {
        sourceScore = null;
        var all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (var label in all)
        {
            if (label == null || label == scoreCounter) continue;
            if (label.gameObject.scene != gameObject.scene) continue;
            string name = label.name.ToLowerInvariant();
            if (name == "score" || name == "scoretext" ||
                name == "scorelabel" || name == "referencescore" ||
                name == "referencelivescore")
            {
                sourceScore = label;
                if (name == "referencescore" || name == "referencelivescore")
                    break;
            }
        }
    }

    private void Draw()
    {
        int energy = hookedEnergy != null ? hookedEnergy.CurrentEnergy :
            PlayerPrefs.GetInt("PlayerEnergy", EnergyManager.MaxEnergy);
        energy = Mathf.Clamp(energy, 0, EnergyManager.MaxEnergy);
        if (energyCounter != null)
            energyCounter.text = energy + " / " + EnergyManager.MaxEnergy;
        for (int i = 0; i < lightningIcons.Length; i++)
        {
            if (lightningIcons[i] == null) continue;
            Color tint = i < energy ? poweredColor : depletedColor;
            // The visible glyph lives in a TextMeshPro child. Tint that
            // glyph; a fully transparent backing Image cannot show a fill.
            var glyph = lightningIcons[i].GetComponentInChildren<TMP_Text>(true);
            if (glyph != null) glyph.color = tint;
        }
        int points = 0;
        if (sourceScore != null)
        {
            // Extract the last numeric run from e.g. SCORE\n000100.
            var matches = Regex.Matches(sourceScore.text, @"\d+");
            if (matches.Count > 0)
                int.TryParse(matches[matches.Count - 1].Value, out points);
        }
        else if (stage > 0 && level > 0 &&
                 PlayerPrefs.GetInt("Stage" + stage + "Level" + level + "Completed", 0) == 1)
        {
            points = PlayerPrefs.GetInt("Stage" + stage + "Level" + level + "Score", 0);
        }
        if (scoreCounter != null)
            scoreCounter.text = "SCORE\n" + Mathf.Max(0, points).ToString("D6");
    }

    private void OpenExistingPause()
    {
        // Use the game's existing pause logic so scene transitions and
        // Pause/Resume/Level Selection are not replaced.
        var stage5 = FindFirstObjectByType<Stage5ElementCrucibleGame>();
        if (stage5 != null) { stage5.ReferenceAction(50); return; }

        var pauseManager = FindFirstObjectByType<PauseMenuManager>();
        if (pauseManager != null) { pauseManager.PauseGame(); return; }

        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b == null || b == menuButton || !b.gameObject.activeInHierarchy)
                continue;
            string n = b.name.ToLowerInvariant();
            if (n == "pausebutton" || n == "menubutton" || n == "stage5pausebutton")
            {
                b.onClick.Invoke();
                return;
            }
        }
        Debug.LogWarning("ChemQuest HUD: No existing Pause button/manager found in " +
            gameObject.scene.name + ". Assign the level's Pause handler.");
    }

    // The editor installer wires fields directly; no runtime procedural UI
    // rebuilding occurs. Everything stays editable before Play.
#if UNITY_EDITOR
    public void Configure(Button menu, TMP_Text energy, TMP_Text score, Image[] icons)
    {
        menuButton = menu;
        energyCounter = energy;
        scoreCounter = score;
        lightningIcons = icons;
    }
#endif
}
