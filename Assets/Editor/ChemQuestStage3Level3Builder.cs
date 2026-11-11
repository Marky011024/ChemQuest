using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestStage3Level3Builder
{
    [MenuItem("Tools/ChemQuest/Stage 3/Apply Level 3 Reference Design")]
    public static void ApplyLevel3ReferenceDesign()
    {
        // Uses the same idempotent, scene-saving operation as the layout fix.
        FixLevel3Canvas();
    }

    [MenuItem("Tools/ChemQuest/Stage 3/Fix Level 3 Canvas and Save Hierarchy")]
    public static void FixLevel3Canvas()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode",
                "Stop Play Mode before editing the saved UI scene.", "OK");
            return;
        }
        const string path="Assets/Scenes/Stage3Level3.unity";
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=path)
        {
            EditorUtility.DisplayDialog("Open Stage3Level3",
                "Open Assets/Scenes/Stage3Level3.unity first.", "OK");
            return;
        }
        var controller=GameObject.Find("Stage3Level3Controller");
        var game=controller==null?null:controller.GetComponent<Stage3Level3Game>();
        if(game==null)
        {
            EditorUtility.DisplayDialog("Missing Controller",
                "Stage3Level3Controller was not found. Use Build Stage 3 Level 3 first.", "OK");
            return;
        }
        // No deletion or recreation: repair the existing serialized canvas.
        game.RepairCanvasLayout();
        EditorUtility.SetDirty(game);
        var canvas=GameObject.Find("Stage3Level3Canvas");
        if(canvas!=null)
        {
            EditorUtility.SetDirty(canvas.transform);
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if(scaler!=null)EditorUtility.SetDirty(scaler);
            var canvasComponent=canvas.GetComponent<Canvas>();
            if(canvasComponent!=null)EditorUtility.SetDirty(canvasComponent);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CHEMQUEST Stage 3 Level 3: Canvas transform repaired and saved in Hierarchy.");
        EditorUtility.DisplayDialog("Layout saved",
            "Stage3Level3Canvas has been reset to normal size and saved. Expand it in Hierarchy before Play.", "OK");
    }

    [MenuItem("Tools/ChemQuest/Build Stage 3 Level 3")]
    public static void Build()
    {
        if(!EditorUtility.DisplayDialog("ChemQuest Stage 3 Level 3",
            "Build editable O2 and CO2 double-bond puzzle UI, menus and shared energy? Existing scene objects will be preserved.",
            "Build Level 3","Cancel"))return;
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before building.","OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        const string path="Assets/Scenes/Stage3Level3.unity";
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Missing "+path,"OK");
            return;
        }
        var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        var go=GameObject.Find("Stage3Level3Controller");
        if(go==null)go=new GameObject("Stage3Level3Controller");
        var game=go.GetComponent<Stage3Level3Game>();
        if(game==null)game=go.AddComponent<Stage3Level3Game>();
        var data=new SerializedObject(game);
        data.FindProperty("energySprite").objectReferenceValue=
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Energy.png");
        data.FindProperty("pauseButtonPrefab").objectReferenceValue=
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        data.FindProperty("pauseMenuPrefab").objectReferenceValue=
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        data.ApplyModifiedPropertiesWithoutUndo();
        game.BuildInEditor();

        var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] required={
            "Assets/Scenes/StageSelection.unity",
            "Assets/Scenes/Stage3LevelSelection.unity",
            "Assets/Scenes/Stage3Level1.unity",
            "Assets/Scenes/Stage3Level2.unity",
            "Assets/Scenes/Stage3Level3.unity"
        };
        foreach(var destination in required)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(destination)==null)
            {
                Debug.LogWarning("ChemQuest: Scene missing: "+destination);
                continue;
            }
            int index=scenes.FindIndex(x=>x.path==destination);
            if(index<0)scenes.Add(new EditorBuildSettingsScene(destination,true));
            else if(!scenes[index].enabled)
                scenes[index]=new EditorBuildSettingsScene(destination,true);
        }
        EditorBuildSettings.scenes=scenes.ToArray();
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("ChemQuest Stage 3 Level 3 saved in Hierarchy (Stage3Level3Canvas). O2 and CO2 puzzles are editable before Play.");
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 3 Level 3 built and saved. Expand Stage3Level3Canvas in Hierarchy. Press Play to test both puzzles.",
            "OK");
    }
}
