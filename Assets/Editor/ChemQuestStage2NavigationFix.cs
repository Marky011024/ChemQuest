using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class ChemQuestStage2NavigationFix
{
    [MenuItem("Tools/ChemQuest/Fix Stage 2 Selection Navigation")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before fixing Stage 2 navigation.","OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        const string template="Assets/Scenes/Stage1LevelSelection.unity";
        const string destination="Assets/Scenes/Stage2LevelSelection.unity";
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(destination)==null)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(template)==null ||
                !AssetDatabase.CopyAsset(template,destination))
            {
                EditorUtility.DisplayDialog("ChemQuest","Could not copy the Stage 1 Level Selection template.","OK");
                return;
            }
            AssetDatabase.Refresh();
        }
        var scene=EditorSceneManager.OpenScene(destination,OpenSceneMode.Single);
        foreach(var x in Object.FindObjectsOfType<Stage1LevelSelectionManager>(true))
            Object.DestroyImmediate(x);
        var controller=Find("Stage2LevelSelectionController");
        if(controller==null)controller=Find("Stage1LevelSelectionManager");
        if(controller==null)controller=new GameObject("Stage2LevelSelectionController");
        controller.name="Stage2LevelSelectionController";
        if(controller.GetComponent<Stage2LevelSelectionManager>()==null)
            controller.AddComponent<Stage2LevelSelectionManager>();

        Set("StageNameText","STAGE 2  |  ION FORGE");
        Set("StageSubtitleText","VALENCE TRANSFER & CRYSTALLINE LATTICE");
        Set("Level1Title","SINGLE ELECTRON TRANSFER");
        Set("Level2Title","DIVALENT TRANSFER");
        Set("Level3Title","CRYSTALLINE LATTICE");
        Set("Level1Description","Transfer valence electrons to complete the octet for NaCl and KCl");
        Set("Level2Description","Complete the octet using MgO and MgCl2");
        Set("Level3Description","Arrange alternating Na+ and Cl- ions in a stable lattice");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,destination);

        var stages=EditorSceneManager.OpenScene("Assets/Scenes/StageSelection.unity",OpenSceneMode.Single);
        var stage2Button=Find("Stage2SelectButton");
        if(stage2Button==null || stage2Button.GetComponent<Button>()==null)
            Debug.LogError("Stage2SelectButton is missing in StageSelection.");
        else
        {
            var b=stage2Button.GetComponent<Button>();
            b.interactable=true;
            b.enabled=true;
            EditorUtility.SetDirty(b);
        }
        var manager=Object.FindObjectOfType<StageSelectionManager>(true);
        if(manager==null)
        {
            var host=Find("StageSelectionManager");
            if(host==null)host=new GameObject("StageSelectionManager");
            host.AddComponent<StageSelectionManager>();
        }
        EditorSceneManager.MarkSceneDirty(stages);
        EditorSceneManager.SaveScene(stages);

        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach(var name in new[]{"StageSelection","Stage2LevelSelection","Stage2Level1","Stage2Level2","Stage2Level3"})
        {
            string path="Assets/Scenes/"+name+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogWarning("Stage 2 missing scene: "+path);
                continue;
            }
            int index=list.FindIndex(x=>x.path==path);
            if(index<0)list.Add(new EditorBuildSettingsScene(path,true));
            else if(!list[index].enabled)list[index]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=list.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/StageSelection.unity",OpenSceneMode.Single);
        Debug.Log("Stage 2 selection repaired, linked via StageSelectionManager.OpenStage2 and registered in Build Settings.");
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 2 Level Selection created/repaired and registered. Play StageSelection and click the Stage 2 SELECT button.",
            "OK");
    }
    private static GameObject Find(string name)
    {
        foreach(var t in Object.FindObjectsOfType<Transform>(true))
            if(t.name==name)return t.gameObject;
        return null;
    }
    private static void Set(string name,string content)
    {
        var go=Find(name);
        var label=go==null?null:go.GetComponent<TMP_Text>();
        if(label==null)return;
        label.text=content;
        EditorUtility.SetDirty(label);
    }
}
