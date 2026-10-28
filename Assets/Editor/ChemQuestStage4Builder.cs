using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestStage4Builder
{
    [MenuItem("Tools/ChemQuest/Build ALL Stage 4 Levels")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before building Stage 4.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Build Stage 4 — Electron Ocean",
            "Create Stage 4 Level Selection and all 3 editable scene hierarchies? Existing scenes will be kept.",
            "Build Stage 4","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        // Copy the Stage 1 artwork/background before building the remaining Stage 4 scenes.
        ChemQuestStage4SelectionDesigner.ApplyDesign();
        var sceneNames=new [] {"Stage4LevelSelection","Stage4Level1","Stage4Level2","Stage4Level3"};
        var energy=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Energy.png");
        var pauseButton=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        var pauseMenu=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        foreach(string sceneName in sceneNames)
        {
            string path="Assets/Scenes/"+sceneName+".unity";
            Scene scene;
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)!=null)
                scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            else
                scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);

            if(sceneName=="Stage4LevelSelection")
            {
                var controller=GameObject.Find("Stage4LevelSelectionController");
                if(controller==null)controller=new GameObject("Stage4LevelSelectionController");
                var manager=controller.GetComponent<Stage4LevelSelectionManager>();
                if(manager==null)manager=controller.AddComponent<Stage4LevelSelectionManager>();
                if(GameObject.Find("StageSelectUI")==null)manager.BuildEditorScene();
                EditorUtility.SetDirty(manager);
            }
            else
            {
                int level=sceneName.EndsWith("1")?1:sceneName.EndsWith("2")?2:3;
                var controller=GameObject.Find("Stage4Level"+level+"Controller");
                if(controller==null)controller=new GameObject("Stage4Level"+level+"Controller");
                var game=controller.GetComponent<Stage4ElectronOceanGame>();
                if(game==null)game=controller.AddComponent<Stage4ElectronOceanGame>();
                game.Level=level;
                game.EnergySprite=energy;
                game.PauseButtonPrefab=pauseButton;
                game.PauseMenuPrefab=pauseMenu;
                game.BuildEditorScene();
                game.PolishEditorScene();
                EditorUtility.SetDirty(game);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,path);
        }
        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var required=new [] {
            "Assets/Scenes/StageSelection.unity",
            "Assets/Scenes/Stage4LevelSelection.unity",
            "Assets/Scenes/Stage4Level1.unity",
            "Assets/Scenes/Stage4Level2.unity",
            "Assets/Scenes/Stage4Level3.unity"
        };
        foreach(var path in required)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogError("Missing Stage 4 scene: "+path);continue;
            }
            int existing=list.FindIndex(x=>x.path==path);
            if(existing<0)list.Add(new EditorBuildSettingsScene(path,true));
            else if(!list[existing].enabled)
                list[existing]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=list.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/Stage4LevelSelection.unity",OpenSceneMode.Single);
        Debug.Log("Stage 4 built: Level Selection, Levels 1, 2, 3, navigation scenes registered.");
        EditorUtility.DisplayDialog("Stage 4 Built",
            "Stage 4 is saved in Assets/Scenes. Open any Stage4Level*.unity and expand Stage4Level*Canvas to edit without Play.","OK");
    }
}