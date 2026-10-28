using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
                PolishExistingScene();
                EditorSceneManager.MarkSceneDirty(existing);
                EditorSceneManager.SaveScene(existing);
                Debug.Log("Stage 4 selection polished in place; custom scene changes preserved.");
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

        PolishExistingScene();
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
    // Enhance the original Stage 1 art rather than replacing it with a plain background.
    // All tweaks are stable if this menu command is used more than once.
    private static void PolishExistingScene()
    {
        SetText("StageNameText","STAGE 4  |  ELECTRON OCEAN");
        SetText("StageSubtitleText","METALLIC BONDING • SEA OF DELOCALIZED ELECTRONS");
        SetText("Level1Title","LATTICE & ELECTRON CLOUD");
        SetText("Level2Title","VOLTAGE & CONDUCTIVITY");
        SetText("Level3Title","MALLEABILITY & STRESS");
        SetText("Level1Description","Build a Cu or Mg lattice with free electrons");
        SetText("Level2Description","Connect terminals and explore electron drift");
        SetText("Level3Description","Apply force and shift the metal layers");
        SetText("Level1Number","01");
        SetText("Level2Number","02");
        SetText("Level3Number","03");

        var title=FindText("StageNameText");
        if(title!=null)
        {
            title.enableAutoSizing=true;
            title.fontSizeMin=18;
            title.fontSizeMax=30;
            title.fontStyle=FontStyles.Bold;
            title.color=new Color32(255,209,98,255);
        }
        var subtitle=FindText("StageSubtitleText");
        if(subtitle!=null)
        {
            subtitle.enableAutoSizing=true;
            subtitle.fontSizeMin=11;
            subtitle.fontSizeMax=19;
            subtitle.color=new Color32(237,244,255,255);
        }
        for(int i=1;i<=3;i++)
        {
            string num=i.ToString();
            var card=FindObject("Level"+num+"Card");
            if(card!=null)
            {
                // Keep the source scene's art and card positions. Improve contrast
                // so text remains readable on the detailed landscape artwork.
                var image=card.GetComponent<Image>();
                if(image!=null && image.sprite==null)
                    image.color=new Color32(10,33,60,226);
                var shadow=card.GetComponent<Shadow>();
                if(shadow==null)shadow=card.AddComponent<Shadow>();
                shadow.effectColor=new Color(0f,0.04f,0.11f,.65f);
                shadow.effectDistance=new Vector2(0,-4);
                shadow.useGraphicAlpha=true;
            }
            var heading=FindText("Level"+num+"Title");
            if(heading!=null)
            {
                heading.fontStyle=FontStyles.Bold;
                heading.enableAutoSizing=true;
                heading.fontSizeMin=11;
                heading.fontSizeMax=19;
                heading.color=new Color32(255,216,119,255);
            }
            var description=FindText("Level"+num+"Description");
            if(description!=null)
            {
                description.enableAutoSizing=true;
                description.fontSizeMin=10;
                description.fontSizeMax=14;
                description.color=new Color32(240,246,255,255);
            }
            var buttonGO=FindObject("Level"+num+"SelectButton");
            if(buttonGO!=null)
            {
                var button=buttonGO.GetComponent<Button>();
                if(button!=null)
                {
                    var colors=button.colors;
                    colors.normalColor=Color.white;
                    colors.highlightedColor=new Color32(235,247,255,255);
                    colors.pressedColor=new Color32(184,219,240,255);
                    colors.disabledColor=new Color32(135,150,168,175);
                    colors.colorMultiplier=1f;
                    colors.fadeDuration=.12f;
                    button.colors=colors;
                }
            }
        }
        // Keep original Back and Energy controls untouched, preserving the
        // established Stage 1 style and pre-existing UI layout.
    }

    private static GameObject FindObject(string objectName)
    {
        foreach(var x in Object.FindObjectsOfType<Transform>(true))
            if(x.name==objectName)return x.gameObject;
        return null;
    }
    private static TMP_Text FindText(string objectName)
    {
        foreach(var x in Object.FindObjectsOfType<TMP_Text>(true))
            if(x.name==objectName)return x;
        return null;
    }

}
