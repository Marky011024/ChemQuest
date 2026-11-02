using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestMainMenuInstaller
{
    [MenuItem("Tools/ChemQuest/Main Menu/Install Navigation and Audio Settings")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");return;
        }
        EditorSceneManager.SaveOpenScenes();
        string menu="Assets/Scenes/MainMenu.unity";
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(menu)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","MainMenu scene missing at "+menu,"OK");return;
        }
        var scene=EditorSceneManager.OpenScene(menu,OpenSceneMode.Single);
        var manager=GameObject.Find("MainMenuManager");
        if(manager==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","MainMenuManager object not found. Nothing modified.","OK");return;
        }
        var extras=manager.GetComponent<ChemQuestMainMenuExtras>();
        if(extras==null)extras=manager.AddComponent<ChemQuestMainMenuExtras>();
        extras.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Register exactly the scenes available in the project's Scenes folder.
        // Preserve original ordering of existing scenes and enable the targets.
        var configured=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] required={"MainMenu","StageSelection","ChemQuestCertificate",
            "Stage1LevelSelection","Stage2LevelSelection","Stage3LevelSelection",
            "Stage4LevelSelection","Stage5LevelSelection"};
        foreach(string name in required)
        {
            string path="Assets/Scenes/"+name+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogWarning("ChemQuest: missing navigation scene "+path);
                continue;
            }
            int index=configured.FindIndex(s=>s.path==path);
            if(index<0)configured.Add(new EditorBuildSettingsScene(path,true));
            else configured[index]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=configured.ToArray();
        EditorUtility.DisplayDialog("ChemQuest",
            "Installed Main Menu shortcuts and Music/SFX settings. Enabled available navigation scenes in Build Settings, including Stage2LevelSelection. Open MainMenu and test all buttons.",
            "OK");
    }
}
