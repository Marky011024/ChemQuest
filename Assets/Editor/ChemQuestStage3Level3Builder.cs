using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestStage3Level3Builder
{
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
