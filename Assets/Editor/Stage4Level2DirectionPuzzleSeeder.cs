#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Use once in Edit Mode to serialize the voltage-drift direction challenge.
// The puzzle remains hidden until the student completes the experiment.
public static class Stage4Level2DirectionPuzzleSeeder
{
    [MenuItem("Tools/ChemQuest/Stage 4/Save Level 2 Drift Puzzle in Scene")]
    private static void SavePuzzle()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode ||
            SceneManager.GetActiveScene().name!="Stage4Level2")
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Open Stage4Level2 and exit Play Mode before saving.", "OK");
            return;
        }
        var game=Object.FindFirstObjectByType<Stage4ElectronOceanGame>();
        if(game==null || game.Level!=2)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stage 4 Level 2 controller not found.","OK");
            return;
        }
        game.EnsureDriftPuzzle();
        var scene=SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        if(EditorSceneManager.SaveScene(scene))
            Debug.Log("Stage 4 Level 2: drift-direction puzzle saved into scene.");
    }
}
#endif
