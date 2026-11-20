using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Copies the Stage 1 Level 1 laboratory background tint to Stage 1 Level 2.
// Visual-only, preserves all original chemistry, bottles, labels and UI.
public static class ChemQuestStage1Level2BackgroundFocus
{
    private const string ScenePath = "Assets/Scenes/Stage1Level2.unity";
    private static readonly Color Level1Tint = new Color(.78f, .81f, .87f, 1f);
    private static readonly Color Level1Overlay = new Color(.018f, .045f, .105f, .40f);

    [MenuItem("Tools/ChemQuest/Stage 1/Level 2 - Match Level 1 Dark Background")]
    public static void Match() { Apply(true); }

    [MenuItem("Tools/ChemQuest/Stage 1/Level 2 - Restore Bright Background")]
    public static void Restore() { Apply(false); }

    private static void Apply(bool dim)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Stop Play Mode first.", "OK");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Stage1Level2.unity not found.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform canvas = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Canvas" && root.GetComponent<Canvas>() != null)
            { canvas = root.transform; break; }

        var bg = canvas != null ? canvas.Find("Background") : null;
        var image = bg != null ? bg.GetComponent<Image>() : null;
        if (image == null)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Main Canvas/Background Image missing. Nothing changed.", "OK");
            return;
        }
        Undo.RecordObject(image, "Match Level 1 background brightness");
        image.color = dim ? Level1Tint : Color.white;
        image.raycastTarget = false;
        EditorUtility.SetDirty(image);

        // The same decorative overlay already exists in both scenes. Match its
        // Level 1 opacity without creating additional overlay UI.
        var overlay = canvas.Find("StageGameplayDimOverlay");
        if (overlay != null)
        {
            var mask = overlay.GetComponent<Image>();
            if (mask != null)
            {
                Undo.RecordObject(mask, "Match Level 1 background overlay");
                mask.color = dim ? Level1Overlay : new Color(.018f,.045f,.105f,.45f);
                mask.raycastTarget = false;
                EditorUtility.SetDirty(mask);
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = image.gameObject;
        EditorGUIUtility.PingObject(image.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            dim ? "Stage 1 Level 2 now matches Level 1's background tint and overlay darkness. Reagent sprites, pouring, beaker, buttons and scripts were not changed."
                : "Stage 1 Level 2 background brightness restored. All gameplay objects preserved.",
            "OK");
    }
}
