#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Optional one-click Edit Mode setup. Saves the same decoys that Play Mode
// creates automatically, so students/developers can preview them before Play.
public static class Stage4Level1DecoySeeder
{
    [MenuItem("Tools/ChemQuest/Stage 4/Save Level 1 Decoys in Scene")]
    private static void SaveDecoys()
    {
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid() || scene.name!="Stage4Level1")
        {
            EditorUtility.DisplayDialog("ChemQuest Stage 4",
                "Open Assets/Scenes/Stage4Level1.unity before saving the decoys.","OK");
            return;
        }
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest Stage 4",
                "Exit Play Mode before saving decoys into the scene.","OK");
            return;
        }

        var controller=Object.FindFirstObjectByType<Stage4ElectronOceanGame>();
        if(controller==null)
        {
            EditorUtility.DisplayDialog("ChemQuest Stage 4",
                "Stage4ElectronOceanGame controller was not found.","OK");
            return;
        }
        controller.EnsureLevel1Decoys();
        EditorSceneManager.MarkSceneDirty(scene);
        if(EditorSceneManager.SaveScene(scene))
            Debug.Log("ChemQuest: Stage 4 Level 1 decoys saved in Edit Mode.");
    }

    [MenuItem("Tools/ChemQuest/Stage 4/Save Level 1 Decoys in Scene", true)]
    private static bool ValidateSaveDecoys()
        => !EditorApplication.isPlayingOrWillChangePlaymode &&
           SceneManager.GetActiveScene().name=="Stage4Level1";
}
#endif
