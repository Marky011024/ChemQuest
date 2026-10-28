using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Applies the real Stage 1 selection scene art/hierarchy—not a re-created imitation.
public static class ChemQuestStage5SelectionDesigner
{
    private const string Template="Assets/Scenes/Stage1LevelSelection.unity";
    private const string Target="Assets/Scenes/Stage5LevelSelection.unity";
    private const string Backup="Assets/Scenes/Stage5LevelSelection_PreviousDesign.unity";

    [MenuItem("Tools/ChemQuest/Apply Stage 1 Design to Stage 5 Selection")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 5 Selection",
            "Apply the exact Stage 1 level selection layout and background to Stage 5? The previous Stage 5 selection scene will be backed up once.",
            "Apply Design","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        if(ApplyDesign())
        {
            EditorSceneManager.OpenScene(Target,OpenSceneMode.Single);
            EditorUtility.DisplayDialog("ChemQuest",
                "Stage 5 selection now uses the exact Stage 1 background and card layout. Test Back and all three Select buttons in Play.",
                "OK");
        }
    }

    public static bool ApplyDesign()
    {
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Template)==null)
        {
            Debug.LogError("Missing Stage 1 selection scene: "+Template);
            return false;
        }
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Target)!=null)
        {
            var scene=EditorSceneManager.OpenScene(Target,OpenSceneMode.Single);
            // Idempotent: do not delete a Stage 5 selection already transformed
            // from Stage 1; preserve any subsequent changes by the user.
            if(Find("StageSelectUI")!=null && Find("Level1SelectButton")!=null)
            {
                ConfigureScene(scene);
                return true;
            }
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Backup)==null &&
               !AssetDatabase.CopyAsset(Target,Backup))
            {
                Debug.LogError("Cannot back up Stage 5 selection. No scene was replaced.");
                return false;
            }
            if(!AssetDatabase.DeleteAsset(Target))
            {
                Debug.LogError("Cannot replace old Stage 5 selection.");
                return false;
            }
        }
        if(!AssetDatabase.CopyAsset(Template,Target))
        {
            Debug.LogError("Cannot copy Stage 1 selection to Stage 5.");
            return false;
        }
        AssetDatabase.Refresh();
        var newScene=EditorSceneManager.OpenScene(Target,OpenSceneMode.Single);
        ConfigureScene(newScene);
        return true;
    }
    private static void ConfigureScene(Scene scene)
    {
        foreach(var x in Object.FindObjectsOfType<Stage1LevelSelectionManager>(true))
            Object.DestroyImmediate(x);

        var old=Find("Stage1LevelSelectionManager");
        var controller=Find("Stage5LevelSelectionController");
        if(controller==null)
            controller=old==null?new GameObject("Stage5LevelSelectionController"):old;
        controller.name="Stage5LevelSelectionController";
        if(controller.GetComponent<Stage5LevelSelectionManager>()==null)
            controller.AddComponent<Stage5LevelSelectionManager>();

        Set("StageNameText","STAGE 5  |  ELEMENT CRUCIBLE");
        Set("StageSubtitleText","MATERIAL CLASSIFICATION & PROPERTY WORKBENCH");
        Set("Level1Number","01");
        Set("Level2Number","02");
        Set("Level3Number","03");
        Set("Level1Title","THERMAL RESISTANCE TRIAL");
        Set("Level2Title","HARDNESS & DUCTILITY ASSAY");
        Set("Level3Title","SOLUTION & PHASE CONDUCTIVITY");
        Set("Level1Description","Heat unknown samples A, B and C and compare melting behavior");
        Set("Level2Description","Strike and press the samples to test brittleness and deformation");
        Set("Level3Description","Test conductivity in different states and classify all samples");

        var header=Find("StageTitle")?.GetComponent<TMP_Text>();
        if(header!=null)header.text="LEVEL SELECTION";
        for(int i=1;i<=3;i++)
        {
            var card=Find("Level"+i+"Card");
            if(card!=null)
            {
                var image=card.GetComponent<Image>();
                if(image!=null && image.sprite==null)
                    image.color=new Color32(10,33,60,226);
                var shadow=card.GetComponent<Shadow>();
                if(shadow==null)shadow=card.AddComponent<Shadow>();
                shadow.effectColor=new Color(0,.02f,.08f,.65f);
                shadow.effectDistance=new Vector2(0,-5);
            }
            var title=Find("Level"+i+"Title")?.GetComponent<TMP_Text>();
            if(title!=null)
            {
                title.enableAutoSizing=true;
                title.fontSizeMin=11;title.fontSizeMax=18;
                title.fontStyle=FontStyles.Bold;
                title.color=new Color32(255,215,109,255);
            }
            var desc=Find("Level"+i+"Description")?.GetComponent<TMP_Text>();
            if(desc!=null)
            {
                desc.enableAutoSizing=true;
                desc.fontSizeMin=10;desc.fontSizeMax=14;
                desc.color=new Color32(235,245,255,255);
            }
            var button=Find("Level"+i+"SelectButton")?.GetComponent<Button>();
            if(button!=null)
            {
                var colors=button.colors;
                colors.disabledColor=new Color32(125,145,160,180);
                colors.highlightedColor=new Color32(221,247,255,255);
                button.colors=colors;
            }
        }
        // Keep original Back button, energy HUD, background art and canvas.
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,Target);
        Debug.Log("Stage 5 selection: exact Stage 1 landscape, card art, HUD and positions retained.");
    }
    private static void Set(string name,string value)
    {
        var text=Find(name)?.GetComponent<TMP_Text>();
        if(text==null){Debug.LogWarning("Stage 5 selection missing label "+name);return;}
        text.text=value;
        EditorUtility.SetDirty(text);
    }
    private static GameObject Find(string name)
    {
        foreach(var t in Object.FindObjectsOfType<Transform>(true))
            if(t.name==name)return t.gameObject;
        return null;
    }
}
