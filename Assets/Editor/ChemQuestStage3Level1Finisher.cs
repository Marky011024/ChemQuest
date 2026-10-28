using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestStage3Level1Finisher
{
    [MenuItem("Tools/ChemQuest/Finish Stage 3 Level 1")]
    public static void Build()
    {
        const string scenePath="Assets/Scenes/Stage3Level1.unity";
        if(!EditorUtility.DisplayDialog("Finish Stage 3 Level 1",
            "Save current scenes and configure the Stage 1 pause menu, visible hint, and shared Out of Energy UI for Stage 3 Level 1?","Finish Level","Cancel"))
            return;

        EditorSceneManager.SaveOpenScenes();
        Scene scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
        var ui=GameObject.Find("Stage3Level1HUD");
        if(ui==null)ui=new GameObject("Stage3Level1HUD");
        var hud=ui.GetComponent<Stage3Level1HUD>();
        if(hud==null)hud=ui.AddComponent<Stage3Level1HUD>();

        var manager=Object.FindObjectOfType<Stage3Level1CompleteManager>();
        if(manager==null)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Stage3Level1CompleteManager not found. Existing scene was not deleted.", "OK");
            return;
        }
        // Repair all H2 and H2O electron references, including H2Electron2,
        // without replacing any existing puzzle GameObjects.
        int repaired=0;
        foreach(var electron in Object.FindObjectsOfType<H2DraggableElectron>(true))
        {
            var serializedElectron=new SerializedObject(electron);
            var field=serializedElectron.FindProperty("completeManager");
            if(field!=null && field.objectReferenceValue==null)
            {
                field.objectReferenceValue=manager;
                serializedElectron.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(electron);
                repaired++;
            }
        }
        Debug.Log("Stage 3 Level 1: repaired "+repaired+" missing electron manager references.");

        var properties=new SerializedObject(hud);
        properties.FindProperty("completeManager").objectReferenceValue=manager;
        var button=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        var panel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        properties.FindProperty("pauseButtonPrefab").objectReferenceValue=button;
        properties.FindProperty("pauseMenuPrefab").objectReferenceValue=panel;
        properties.ApplyModifiedPropertiesWithoutUndo();
        if(button==null||panel==null)
            Debug.LogWarning("Stage 1 pause prefabs are missing; Stage 3 Level 1 uses fallback controls.");

        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var destinations=new [] {
            "Assets/Scenes/StageSelection.unity",
            "Assets/Scenes/Stage3LevelSelection.unity",
            "Assets/Scenes/Stage3Level1.unity",
            "Assets/Scenes/Stage3Level2.unity",
            "Assets/Scenes/Stage3Level3.unity"
        };
        foreach(var dest in destinations)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(dest)==null)
            {
                Debug.LogWarning("Missing scene: "+dest);
                continue;
            }
            int index=list.FindIndex(x=>x.path==dest);
            if(index<0)list.Add(new EditorBuildSettingsScene(dest,true));
            else if(!list[index].enabled)list[index]=new EditorBuildSettingsScene(dest,true);
        }
        EditorBuildSettings.scenes=list.ToArray();

        EditorUtility.SetDirty(hud);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("ChemQuest Stage 3 Level 1 configured. Press Play to test Pause, Retry, Hint, and energy countdown.");
        EditorUtility.DisplayDialog("Stage 3 Level 1", "Level 1 HUD configured. Open the Game tab and press Play.", "OK");
    }
}
