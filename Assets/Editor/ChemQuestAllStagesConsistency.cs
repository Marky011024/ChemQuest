using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Non-destructive editor-time consistency pass across all existing ChemQuest scenes.
// Does not rebuild gameplay, change student progress, or alter answer validation.
public static class ChemQuestAllStagesConsistency
{
    private static readonly Color32 Dark = new Color32(5, 13, 29, 234);
    private static readonly Color32 Card = new Color32(24, 46, 79, 255);
    private static readonly Color32 Red = new Color32(255, 103, 110, 255);
    private static readonly Color32 Gold = new Color32(255, 213, 105, 255);

    [MenuItem("Tools/ChemQuest/Consistency/1 - Audit All 5 Stages")]
    public static void Audit()
    {
        if(EditorApplication.isPlaying)
        {EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");return;}
        EditorSceneManager.SaveOpenScenes();
        var report=new StringBuilder();
        int checkedScenes=0;
        foreach(var sceneName in ExistingScenes())
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity",OpenSceneMode.Single);
            checkedScenes++;
            report.AppendLine("[" + sceneName + "]");
            var buttons=Object.FindObjectsOfType<Button>(true);
            int active=0;
            foreach(var b in buttons)
            {
                if(!b.gameObject.activeInHierarchy)continue;
                if(!b.interactable)continue;
                active++;
                if(b.targetGraphic==null)
                    report.AppendLine("  WARNING: "+PathOf(b.transform)+" has no target graphic");
            }
            if(sceneName.Contains("Level") && !sceneName.Contains("Selection"))
            {
                if(!Has("PauseButton")&&!Has("Level3PauseButton")&&!Has("Stage4PauseButton")&&
                   !Has("Stage2PauseButton")&&!Has("MenuButton"))
                    report.AppendLine("  WARNING: pause/menu button not found");
                if(!Has("FailedPanel")&&!Has("Level2FailedPanel")&&!Has("Stage3Level1FailedOverlay"))
                    report.AppendLine("  WARNING: failed panel not found (may use legacy panel)");
                if(!Has("CompletePanel")&&!Has("Stage3CompletePanel")&&
                   !Has("LevelCompletePanel")&&!Has("Stage3Level1FinalCompletePanel")&&
                   !Has("Level2CompletePanel"))
                    report.AppendLine("  CHECK: level completion panel naming differs");
            }
            report.AppendLine("  Active interactive buttons: "+active);
        }
        report.AppendLine();
        foreach(var entry in new[]{"StageSelection","Stage2LevelSelection","Stage3LevelSelection",
           "Stage4LevelSelection","Stage5LevelSelection"})
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/"+entry+".unity")==null)
                report.AppendLine("MISSING SCENE: "+entry);
        }
        Debug.Log("CHEMQUEST CONSISTENCY AUDIT ("+checkedScenes+" scenes)\n"+report);
        EditorUtility.DisplayDialog("ChemQuest",
           "Audited "+checkedScenes+" scenes. Open the Console for the full report. This audit changed no gameplay scenes.",
           "OK");
    }

    [MenuItem("Tools/ChemQuest/Consistency/2 - Polish Existing UI and Register Scenes")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");return;}
        if(!EditorUtility.DisplayDialog("ChemQuest - All 5 Stages",
           "Polish EXISTING failed/completed overlays, prevent static labels from blocking UI clicks, and register existing stages in Build Settings? Gameplay mechanics and scores will not be changed.",
           "Apply Safe Fixes","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        int scenes=0,panels=0,labels=0;
        foreach(var sceneName in ExistingScenes())
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity",OpenSceneMode.Single);
            bool changed=false;
            foreach(var text in Object.FindObjectsOfType<TMP_Text>(true))
            {
                // Static labels should never intercept electron drags or button clicks.
                // Avoid disabling the raycast target on editable input fields.
                if(text.GetComponent<TMP_InputField>()!=null)continue;
                if(text.raycastTarget)
                {
                    text.raycastTarget=false;
                    EditorUtility.SetDirty(text);
                    labels++;
                    changed=true;
                }
            }
            // Only style known fullscreen gameplay overlays. Original Stage 1
            // prefabs and the validated Stage 3 Level 1 overlay stay untouched.
            if(sceneName.StartsWith("Stage2")||sceneName.StartsWith("Stage3Level2")||
               sceneName.StartsWith("Stage3Level3")||sceneName.StartsWith("Stage4")||
               sceneName.StartsWith("Stage5"))
            {
                foreach(var name in new[]{"FailedPanel","Level2FailedPanel",
                         "CompletePanel","Level2CompletePanel","Stage3CompletePanel"})
                {
                    var panel=Find(name);
                    if(panel==null || panel.GetComponent<RectTransform>()==null)continue;
                    // Only standardize actual overlays already containing a modal
                    // card or similar recognizable title, not puzzle panels.
                    var card=Child(panel.transform,"ModalCard") ?? Child(panel.transform,"Panel");
                    if(card==null)continue;
                    var rect=panel.GetComponent<RectTransform>();
                    rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
                    rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                    var shade=panel.GetComponent<Image>();
                    if(shade!=null){shade.color=Dark;shade.raycastTarget=true;}
                    var cardRt=card.GetComponent<RectTransform>();
                    if(cardRt!=null)
                    {
                        cardRt.anchorMin=cardRt.anchorMax=new Vector2(.5f,.5f);
                        cardRt.anchoredPosition=Vector2.zero;
                        cardRt.sizeDelta=new Vector2(670,400);
                    }
                    var cardImage=card.GetComponent<Image>();
                    if(cardImage!=null){cardImage.color=Card;cardImage.raycastTarget=false;}
                    var heading=Child(panel.transform,"Title") ?? Child(panel.transform,"Heading");
                    var txt=heading==null?null:heading.GetComponent<TMP_Text>();
                    if(txt!=null)
                    {
                        if(name.Contains("Failed")){txt.text="STAGE FAILED!";txt.color=Red;}
                        else txt.color=Gold;
                        txt.fontStyle=FontStyles.Bold;
                        txt.raycastTarget=false;
                    }
                    foreach(var button in panel.GetComponentsInChildren<Button>(true))
                    {
                        var caption=button.GetComponentInChildren<TMP_Text>(true);
                        if(caption!=null)
                        {
                            caption.fontStyle=FontStyles.Bold;
                            caption.enableAutoSizing=true;
                            caption.fontSizeMin=12;
                            caption.fontSizeMax=21;
                            caption.raycastTarget=false;
                        }
                    }
                    EditorUtility.SetDirty(panel);
                    changed=true;
                    panels++;
                }
            }
            if(changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                scenes++;
            }
        }
        int registered=RegisterExistingScenes();
        EditorUtility.DisplayDialog("ChemQuest",
           "Finished safe UI fixes. Updated "+scenes+" scenes, "+panels+
           " existing modal panels and "+labels+" static labels. Registered "+registered+
           " existing scenes. Review and test in Play Mode before merging.",
           "OK");
    }

    private static int RegisterExistingScenes()
    {
        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int changed=0;
        foreach(var scene in ExistingScenes())
        {
            string path="Assets/Scenes/"+scene+".unity";
            int idx=list.FindIndex(x=>x.path==path);
            if(idx<0){list.Add(new EditorBuildSettingsScene(path,true));changed++;}
            else if(!list[idx].enabled)
            {list[idx]=new EditorBuildSettingsScene(path,true);changed++;}
        }
        EditorBuildSettings.scenes=list.ToArray();
        return changed;
    }
    private static IEnumerable<string> ExistingScenes()
    {
        var names=new List<string>{"StageSelection"};
        for(int stage=1;stage<=5;stage++)
        {
            names.Add("Stage"+stage+"LevelSelection");
            for(int level=1;level<=3;level++)names.Add("Stage"+stage+"Level"+level);
        }
        foreach(var name in names)
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/"+name+".unity")!=null)
                yield return name;
    }
    private static bool Has(string name)=>Find(name)!=null;
    private static GameObject Find(string name)
    {
        foreach(var t in Object.FindObjectsOfType<Transform>(true))
            if(t.name==name)return t.gameObject;
        return null;
    }
    private static Transform Child(Transform root,string name)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
    private static string PathOf(Transform t)
    {
        var result=t.name;
        while(t.parent!=null){t=t.parent;result=t.name+"/"+result;}
        return result;
    }
}
