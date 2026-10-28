using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class ChemQuestStage2Builder
{
    private const string Template="Assets/Scenes/Stage1LevelSelection.unity";
    private const string Selection="Assets/Scenes/Stage2LevelSelection.unity";

    [MenuItem("Tools/ChemQuest/Build Complete Stage 2 (All Levels)")]
    public static void BuildAll()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest Stage 2 — ION FORGE",
            "Create all three ionic bonding levels and a Stage 1-themed Level Selection? All UI is editable before Play.",
            "Build Stage 2","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();

        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Template)==null)
        {
            Debug.LogError("Missing template: "+Template);return;
        }
        // Stage 2 selection: clone exact Stage 1 scenery, rather than build a flat page.
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Selection)==null)
        {
            if(!AssetDatabase.CopyAsset(Template,Selection))
            {
                Debug.LogError("Could not create Stage 2 selection from "+Template);return;
            }
            AssetDatabase.Refresh();
        }
        var selectScene=EditorSceneManager.OpenScene(Selection,OpenSceneMode.Single);
        foreach(var old in Object.FindObjectsOfType<Stage1LevelSelectionManager>(true))
            Object.DestroyImmediate(old);
        var controller=GameObject.Find("Stage1LevelSelectionManager");
        if(controller==null)controller=new GameObject("Stage2LevelSelectionController");
        controller.name="Stage2LevelSelectionController";
        if(controller.GetComponent<Stage2LevelSelectionManager>()==null)
            controller.AddComponent<Stage2LevelSelectionManager>();
        SetText("StageNameText","STAGE 2  |  ION FORGE");
        SetText("StageSubtitleText","IONIC BONDING • ELECTRON TRANSFER & LATTICE FORMATION");
        SetText("Level1Title","SINGLE ELECTRON TRANSFER");
        SetText("Level2Title","DIVALENT & MULTI-ANION");
        SetText("Level3Title","CRYSTAL LATTICE");
        SetText("Level1Description","Form BOTH NaCl and KCl using electron transfer");
        SetText("Level2Description","Form BOTH MgO and MgCl2 with charge balance");
        SetText("Level3Description","Arrange a 12-ion alternating NaCl lattice");
        EditorSceneManager.MarkSceneDirty(selectScene);
        EditorSceneManager.SaveScene(selectScene,Selection);

        var energy=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Energy.png");
        var pauseButton=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        var pauseMenu=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        for(int level=1;level<=3;level++)
        {
            string path="Assets/Scenes/Stage2Level"+level+".unity";
            var scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null?
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single):
                EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            var go=GameObject.Find("Stage2Level"+level+"Controller");
            if(go==null)go=new GameObject("Stage2Level"+level+"Controller");
            var game=go.GetComponent<Stage2IonForgeGame>();
            if(game==null)game=go.AddComponent<Stage2IonForgeGame>();
            game.Level=level;
            game.EnergySprite=energy;
            game.PauseButtonPrefab=pauseButton;
            game.PauseMenuPrefab=pauseMenu;
            game.BuildEditableUI();
            game.UpgradeChallengeUI();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,path);
        }

        var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach(var name in new[]{"StageSelection","Stage2LevelSelection",
            "Stage2Level1","Stage2Level2","Stage2Level3"})
        {
            string path="Assets/Scenes/"+name+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogWarning("Missing scene "+path);continue;
            }
            int idx=scenes.FindIndex(x=>x.path==path);
            if(idx<0)scenes.Add(new EditorBuildSettingsScene(path,true));
            else if(!scenes[idx].enabled)scenes[idx]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=scenes.ToArray();
        EditorSceneManager.OpenScene(Selection,OpenSceneMode.Single);
        Debug.Log("ChemQuest Stage 2 generated: Level Selection and Levels 1, 2, 3.");
        EditorUtility.DisplayDialog("Stage 2 — ION FORGE",
            "Saved Stage 2 Level Selection and three gameplay scenes. Open a Stage2Level*.unity scene and expand its Canvas to edit all objects before Play.",
            "OK");
    }

    private static void SetText(string name,string text)
    {
        foreach(var x in Object.FindObjectsOfType<TMP_Text>(true))
        {
            if(x.name!=name)continue;
            x.text=text;x.enableAutoSizing=true;
            EditorUtility.SetDirty(x);
            return;
        }
        Debug.LogWarning("Stage 2 template text not found: "+name);
    }
}
