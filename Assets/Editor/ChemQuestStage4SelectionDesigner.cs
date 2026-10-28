using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class ChemQuestStage4SelectionDesigner
{
    private const string Template="Assets/Scenes/Stage1LevelSelection.unity";
    private const string Target="Assets/Scenes/Stage4LevelSelection.unity";
    private const string Backup="Assets/Scenes/Stage4LevelSelection_PreviousDesign.unity";

    [MenuItem("Tools/ChemQuest/Apply Stage 1 Design to Stage 4 Selection")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before applying the design.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 4 Selection Design",
            "Copy the exact Stage 1 Level Selection background and card layout into Stage 4, with corrected text, buttons and locks? A backup of the previous Stage 4 scene is created.",
            "Apply Design","Cancel"))return;
        ApplyDesign();
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 4 Selection now uses the Stage 1 background, cards and energy display. Open Stage4LevelSelection and check its Hierarchy.",
            "OK");
    }

    public static void ApplyDesign()
    {
        EditorSceneManager.SaveOpenScenes();
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Template)==null)
        {
            Debug.LogError("Missing Stage 1 selection template: "+Template);
            return;
        }
        // Idempotent: if a Stage 4 designer already exists, keep user scene edits.
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Target)!=null)
        {
            var existing=EditorSceneManager.OpenScene(Target,OpenSceneMode.Single);
            if(Object.FindObjectOfType<Stage4LevelSelectionManager>(true)!=null &&
               GameObject.Find("StageSelectUI")!=null)
            {
                Debug.Log("Stage 4 already uses the Stage 1 design. Preserving scene edits.");
                return;
            }
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Backup)==null)
                AssetDatabase.CopyAsset(Target,Backup);
            if(!AssetDatabase.DeleteAsset(Target))
            {
                Debug.LogError("Could not replace Stage 4 level selection scene.");
                return;
            }
        }
        if(!AssetDatabase.CopyAsset(Template,Target))
        {
            Debug.LogError("Could not duplicate Stage 1 selection template.");
            return;
        }
        AssetDatabase.Refresh();
        Scene scene=EditorSceneManager.OpenScene(Target,OpenSceneMode.Single);
        foreach(var old in Object.FindObjectsOfType<Stage1LevelSelectionManager>(true))
            Object.DestroyImmediate(old);
        foreach(var old in Object.FindObjectsOfType<Stage3LevelSelectionManager>(true))
            Object.DestroyImmediate(old);
        var controller=GameObject.Find("Stage1LevelSelectionManager");
        if(controller==null)controller=new GameObject("Stage4LevelSelectionController");
        else controller.name="Stage4LevelSelectionController";
        var manager=controller.GetComponent<Stage4LevelSelectionManager>();
        if(manager==null)manager=controller.AddComponent<Stage4LevelSelectionManager>();

        SetText("StageNameText","STAGE 4 — ELECTRON OCEAN");
        SetText("StageSubtitleText","METALLIC BONDING — SEA OF ELECTRONS");
        SetText("Level1Title","LATTICE & ELECTRON CLOUD");
        SetText("Level2Title","VOLTAGE DRIFT & CONDUCTIVITY");
        SetText("Level3Title","MALLEABILITY & STRESS TEST");
        SetText("Level1Description","Arrange Cu or Mg metal cores and free electrons");
        SetText("Level2Description","Connect terminals and observe electron flow");
        SetText("Level3Description","Apply force and observe sliding metal layers");
        var header=GameObject.Find("StageTitle");
        if(header!=null)
        {
            var label=header.GetComponent<TMP_Text>();
            if(label!=null)label.text="SELECT STAGE";
        }

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Stage 4 Selection now has the exact Stage 1 background, navigation cards and visual hierarchy.");
    }

    private static void SetText(string objectName,string value)
    {
        foreach(var text in Object.FindObjectsOfType<TMP_Text>(true))
            if(text.name==objectName)
            {
                text.text=value;
                EditorUtility.SetDirty(text);
                return;
            }
        Debug.LogWarning("Stage 4 selection text not found: "+objectName);
    }
}
