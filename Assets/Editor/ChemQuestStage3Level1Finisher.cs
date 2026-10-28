using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
        // Build visible, persistent canvas hierarchy in Edit Mode.
        var hudCanvasGO=GameObject.Find("Stage3Level1HUDCanvas");
        if(hudCanvasGO==null)
            hudCanvasGO=new GameObject("Stage3Level1HUDCanvas",
                typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=hudCanvasGO.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=80;
        var scaler=hudCanvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1f;

        // Leave existing Stage3Level1Canvas intact. The runtime controller hides
        // its original PauseButton, retaining just this new editable button.
        var pauseButton=FindChild(hudCanvasGO.transform,"Stage3Level1PauseButton");
        if(pauseButton==null && button!=null)
        {
            pauseButton=(GameObject)PrefabUtility.InstantiatePrefab(button,scene);
            pauseButton.name="Stage3Level1PauseButton";
            pauseButton.transform.SetParent(hudCanvasGO.transform,false);
        }
        if(pauseButton!=null)
        {
            var rect=pauseButton.GetComponent<RectTransform>();
            rect.anchorMin=new Vector2(0,1);
            rect.anchorMax=new Vector2(0,1);
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(56,-59);
        }

        var pauseMenu=FindChild(hudCanvasGO.transform,"Stage3Level1PauseMenu");
        if(pauseMenu==null && panel!=null)
        {
            pauseMenu=(GameObject)PrefabUtility.InstantiatePrefab(panel,scene);
            pauseMenu.name="Stage3Level1PauseMenu";
            pauseMenu.transform.SetParent(hudCanvasGO.transform,false);
        }
        if(pauseMenu!=null)
        {
            var rect=pauseMenu.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;
            foreach(var menuButton in pauseMenu.GetComponentsInChildren<Button>(true))
            {
                if(menuButton.name=="StageSelectionButton")
                {
                    var label=menuButton.GetComponentInChildren<TMP_Text>(true);
                    if(label!=null)label.text="STAGE SELECTION";
                }
            }
            pauseMenu.SetActive(false);
        }

        var hintGO=FindChild(hudCanvasGO.transform,"Stage3Level1HintText");
        if(hintGO==null)
        {
            hintGO=new GameObject("Stage3Level1HintText",typeof(RectTransform),
                typeof(TextMeshProUGUI));
            hintGO.transform.SetParent(hudCanvasGO.transform,false);
        }
        var hint=hintGO.GetComponent<TextMeshProUGUI>();
        hint.text="HINT: Share electrons in the correct bonding zones.";
        hint.fontSize=21;
        hint.alignment=TextAlignmentOptions.Center;
        hint.color=new Color32(255,215,92,255);
        hint.raycastTarget=false;
        var hintRect=hintGO.GetComponent<RectTransform>();
        hintRect.anchoredPosition=new Vector2(0,-295);
        hintRect.sizeDelta=new Vector2(850,56);
        hintGO.SetActive(false);

        var outPanel=FindChild(hudCanvasGO.transform,"Stage3Level1OutOfEnergyPanel");
        if(outPanel==null)
        {
            outPanel=CreateImage(hudCanvasGO.transform,"Stage3Level1OutOfEnergyPanel",
                new Vector2(1280,720),Vector2.zero,new Color32(5,14,29,245));
            var card=CreateImage(outPanel.transform,"OutOfEnergyCard",
                new Vector2(700,400),Vector2.zero,new Color32(22,44,76,255));
            CreateText(card.transform,"OutOfEnergyTitle","OUT OF ENERGY!",38,
                new Vector2(0,113),new Vector2(650,80),new Color32(255,206,90,255));
            CreateText(card.transform,"Description","Your shared energy is empty.",22,
                new Vector2(0,45),new Vector2(650,55),Color.white);
            CreateText(card.transform,"EnergyTimerText","Next Energy In: 15:00",25,
                new Vector2(0,-25),new Vector2(640,60),new Color32(255,206,90,255));
            CreateButton(card.transform,"BackToSelectionButton","BACK TO LEVEL SELECTION",
                new Vector2(0,-125),new Vector2(350,65));
        }
        outPanel.SetActive(false);
        var timer=outPanel.GetComponentInChildren<TMP_Text>(true);
        foreach(var t in outPanel.GetComponentsInChildren<TMP_Text>(true))
            if(t.gameObject.name=="EnergyTimerText") timer=t;

        // The failed overlay lives on the top-sorting HUD canvas, not inside
        // the smaller bonding puzzle panel. It remains editable in Edit Mode.
        var failure=FindChild(hudCanvasGO.transform,"Stage3Level1FailedOverlay");
        if(failure==null)
        {
            failure=CreateImage(hudCanvasGO.transform,"Stage3Level1FailedOverlay",
                new Vector2(1280,720),Vector2.zero,new Color32(5,12,27,237));
        }
        var fr=failure.GetComponent<RectTransform>();
        fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;
        fr.offsetMin=Vector2.zero;fr.offsetMax=Vector2.zero;
        // Keep the transparent dim layer blocking puzzle input behind it.
        var background=failure.GetComponent<Image>();
        background.raycastTarget=true;
        var failureCard=FindChild(failure.transform,"FailureCard");
        if(failureCard==null)
            failureCard=CreateImage(failure.transform,"FailureCard",new Vector2(670,400),
                Vector2.zero,new Color32(20,43,75,255));
        var failTitle=FindChild(failureCard.transform,"FailureTitle");
        if(failTitle==null)
            failTitle=CreateText(failureCard.transform,"FailureTitle","STAGE FAILED!",40,
                new Vector2(0,120),new Vector2(625,70),new Color32(255,108,110,255)).gameObject;
        var failDesc=FindChild(failureCard.transform,"FailureMessage");
        if(failDesc==null)
            failDesc=CreateText(failureCard.transform,"FailureMessage",
                "Maximum wrong attempts reached.\\nOne energy has been deducted.",22,
                new Vector2(0,35),new Vector2(620,90),Color.white).gameObject;
        var retry=FindChild(failureCard.transform,"RetryButton");
        if(retry==null)
            retry=CreateButton(failureCard.transform,"RetryButton","RETRY",
                new Vector2(-154,-111),new Vector2(280,66)).gameObject;
        var select=FindChild(failureCard.transform,"LevelSelectionButton");
        if(select==null)
            select=CreateButton(failureCard.transform,"LevelSelectionButton","LEVEL SELECTION",
                new Vector2(154,-111),new Vector2(280,66)).gameObject;
        // The legacy small failed panel is now replaced by this top-level overlay.
        failure.SetActive(false);
        failure.transform.SetAsLastSibling();

        properties.FindProperty("sceneFailedPanel").objectReferenceValue=failure;
        properties.FindProperty("scenePauseButton").objectReferenceValue=
            pauseButton!=null?pauseButton.GetComponent<Button>():null;
        properties.FindProperty("scenePausePanel").objectReferenceValue=pauseMenu;
        properties.FindProperty("sceneOutOfEnergyPanel").objectReferenceValue=outPanel;
        properties.FindProperty("sceneTimerText").objectReferenceValue=timer;
        properties.FindProperty("sceneHintText").objectReferenceValue=hint;
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
    private static GameObject FindChild(Transform parent,string name)
    {
        foreach(Transform child in parent)
            if(child.name==name)return child.gameObject;
        return null;
    }

    private static GameObject CreateImage(Transform parent,string name,
        Vector2 size,Vector2 pos,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.sizeDelta=size;rect.anchoredPosition=pos;
        go.GetComponent<Image>().color=color;
        return go;
    }

    private static TMP_Text CreateText(Transform parent,string name,string text,
        int fontSize,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.anchoredPosition=pos;rect.sizeDelta=size;
        var tmp=go.GetComponent<TextMeshProUGUI>();
        tmp.text=text;tmp.fontSize=fontSize;tmp.alignment=TextAlignmentOptions.Center;
        tmp.color=color;tmp.raycastTarget=false;
        return tmp;
    }

    private static Button CreateButton(Transform parent,string name,
        string label,Vector2 pos,Vector2 size)
    {
        var go=CreateImage(parent,name,size,pos,new Color32(24,93,137,255));
        var b=go.AddComponent<Button>();
        CreateText(go.transform,"Text",label,22,Vector2.zero,size,Color.white);
        return b;
    }

}
