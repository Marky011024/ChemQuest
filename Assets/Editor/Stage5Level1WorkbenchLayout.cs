#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Save the actual Stage 5 Level 1 serialized UI after layout polishing.
// No scene rebuild, no prefab or gameplay replacement.
public static class Stage5Level1WorkbenchLayout
{
    [MenuItem("Tools/ChemQuest/Stage 5/Save Level 1 Workbench UI Layout")]
    private static void ApplyAndSave()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode ||
            SceneManager.GetActiveScene().name!="Stage5Level1")
        {
            EditorUtility.DisplayDialog("CHEMQUEST",
                "Open Stage5Level1 and exit Play Mode first.","OK");
            return;
        }

        Stage5ElementCrucibleGame game=Object.FindFirstObjectByType<Stage5ElementCrucibleGame>();
        if(game==null || game.Level!=1)
        {
            EditorUtility.DisplayDialog("CHEMQUEST",
                "Stage 5 Level 1 controller not found or Level is not 1.","OK");
            return;
        }
        game.PolishStage5Level1Layout();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        if(EditorSceneManager.SaveOpenScenes())
        {
            Debug.Log("CHEMQUEST: Stage 5 Level 1 workbench UI saved.");
            EditorUtility.DisplayDialog("CHEMQUEST",
                "Stage 5 Level 1 workbench layout saved. Test Sample A/B/C drag, heat slider, observations and Submit in Play Mode.",
                "OK");
        }
    }
}
#endif
