using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Polishes only Stage1Level1 UI. Keeps the imported Stage1 artwork and all gameplay
// GameObjects, scripts, event bindings, PlayerPrefs and puzzle behavior unchanged.
public static class ChemQuestStage1LayoutPolish
{
    [MenuItem("Tools/ChemQuest/Stage Backgrounds/Polish Stage 1 Level 1 UI")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Exit Play Mode before applying UI changes.", "OK");
            return;
        }

        const string path = "Assets/Scenes/Stage1Level1.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Stage1Level1.unity not found.", "OK");
            return;
        }

        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Canvas not found. Scene unchanged.", "OK");
            return;
        }
        var canvas = canvasObject.transform;
        var title = canvas.Find("LevelTitle");
        var instruction = canvas.Find("InstructionPanel");
        var experiment = canvas.Find("ExperimentPanel");
        if (title == null || instruction == null || experiment == null)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Expected Stage 1 UI hierarchy not found. Scene unchanged.", "OK");
            return;
        }

        // The artwork already says CHEMQUEST / STAGE 1. This dynamic title overlaps it.
        // Disable, do not delete; can be restored at any time.
        Undo.RecordObject(title.gameObject, "Hide overlapping Stage 1 title");
        title.gameObject.SetActive(false);

        var panelRect = instruction.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            Undo.RecordObject(panelRect, "Resize Stage 1 instruction panel");
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
            panelRect.pivot = new Vector2(.5f, .5f);
            panelRect.anchoredPosition = new Vector2(0, 160);
            panelRect.sizeDelta = new Vector2(1120, 168);
        }

        var panelImage = instruction.GetComponent<Image>();
        if (panelImage == null) panelImage = Undo.AddComponent<Image>(instruction.gameObject);
        Undo.RecordObject(panelImage, "Improve instruction contrast");
        panelImage.color = new Color(0.018f, 0.065f, 0.13f, .94f);
        panelImage.raycastTarget = false;

        // Keep the instructions readable, centered and away from the flask.
        var instructionText = instruction.Find("InstructionText");
        if (instructionText != null)
        {
            var rect = instructionText.GetComponent<RectTransform>();
            if (rect != null)
            {
                Undo.RecordObject(rect, "Fit Stage 1 instruction text");
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(1030, 134);
            }
            var tmp = instructionText.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                Undo.RecordObject(tmp, "Improve Stage 1 instruction typography");
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 26;
                tmp.fontSizeMax = 34;
                tmp.raycastTarget = false;
            }
        }

        // Reorder only the instruction panel so it stays on top of the background
        // while keeping active pause, energy, dialogs and completion panels in front.
        // Do not move or resize experiment interactables.
        EditorUtility.SetDirty(instruction.gameObject);
        EditorUtility.SetDirty(title.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 1 Level 1 layout updated: removed duplicate title and improved instruction readability. Background, puzzles, score, pause, retry, energy and progress were preserved. Enter Play Mode to inspect.", "OK");
    }
}
