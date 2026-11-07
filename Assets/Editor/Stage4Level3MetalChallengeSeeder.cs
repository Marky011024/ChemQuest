#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Use once outside Play Mode if you want the two puzzle overlays
// serialized in Stage4Level3.unity. Both remain hidden until unlocked.
public static class Stage4Level3MetalChallengeSeeder
{
    [MenuItem("Tools/ChemQuest/Stage 4/Save Level 3 Malleability and Ductility Puzzles")]
    private static void SavePuzzles()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode ||
            SceneManager.GetActiveScene().name!="Stage4Level3")
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Open Stage4Level3 and exit Play Mode first.","OK");
            return;
        }
        var game=Object.FindFirstObjectByType<Stage4ElectronOceanGame>();
        if(game==null || game.Level!=3)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Stage4Level3 controller not found or Level is not 3.","OK");
            return;
        }
        game.EnsureMetalChallenges();
        game.PolishLevel3Presentation();
        game.FixLevel3ForceSliderLayout();
        var scene=SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        if(EditorSceneManager.SaveScene(scene))
            Debug.Log("ChemQuest: Stage 4 Level 3 puzzle overlays saved to scene.");
    }
}
#endif
