#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Explicit, reversible scene-authoring step (not a runtime spawn).
// Back up or commit scenes before running this command.
public static class ChemQuestHUDInstaller
{
    private const string HudRoot = "ChemQuestUnifiedHUD";
    private static readonly Color Navy = new Color32(3, 24, 49, 242);
    private static readonly Color Gold = new Color32(255, 205, 92, 255);
    private static readonly Color Cyan = new Color32(28, 226, 255, 255);

    [MenuItem("ChemQuest/HUD/Apply Unified HUD To All Gameplay Scenes")]
    private static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode",
                "Stop Play Mode before editing saved scenes.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Apply Shared HUD",
            "This will update and SAVE every StageNLevelM scene in Assets/Scenes. " +
            "Commit or back up your scene files first. Existing level gameplay logic " +
            "will not be modified. Continue?", "Apply to scenes", "Cancel"))
            return;

        string currentPath = SceneManager.GetActiveScene().path;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        int updated = 0;
        var ids = AssetDatabase.FindAssets("t:Scene", new [] {"Assets/Scenes"});
        try
        {
            for (int i=0;i<ids.Length;i++)
            {
                var path=AssetDatabase.GUIDToAssetPath(ids[i]);
                string sceneName=System.IO.Path.GetFileNameWithoutExtension(path);
                if (!Regex.IsMatch(sceneName, @"^Stage[1-9]\d*Level[1-9]\d*$"))
                    continue;
                EditorUtility.DisplayProgressBar("ChemQuest HUD",
                    "Updating " + sceneName, (float)i/Mathf.Max(1,ids.Length));
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                Build(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                updated++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (!string.IsNullOrEmpty(currentPath))
                EditorSceneManager.OpenScene(currentPath,OpenSceneMode.Single);
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Unified HUD",
            "Saved HUD objects in " + updated +
            " gameplay scenes. Test Pause, energy penalties, and score display " +
            "in each stage before committing.", "OK");
    }

    private static void Build(Scene scene)
    {
        GameObject root=null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name==HudRoot) { root=go; break; }
        if (root==null) root=new GameObject(HudRoot,
            typeof(RectTransform),typeof(Canvas),
            typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();
        if(canvas==null)canvas=root.AddComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=1500;
        var scaler=root.GetComponent<CanvasScaler>();
        if(scaler==null)scaler=root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=.5f;
        if(root.GetComponent<GraphicRaycaster>()==null)root.AddComponent<GraphicRaycaster>();

        // Three separate upper HUD clusters reproduce the Stage 5 sample
        // layout while avoiding the game's lower puzzle and notebook space.
        var left=Rect(root.transform,"MenuGroup",new Vector2(0,1),
            new Vector2(56, -47),new Vector2(85,70));
        var menuFace=Panel(left,"MenuFace",new Color32(7,51,79,255),
            Vector2.zero,new Vector2(64,63));
        var edge=menuFace.GetComponent<Outline>();
        if(edge==null)edge=menuFace.gameObject.AddComponent<Outline>();
        edge.effectColor=Gold;edge.effectDistance=new Vector2(2,-2);
        var menu=menuFace.GetComponent<Button>();
        if(menu==null)menu=menuFace.gameObject.AddComponent<Button>();
        menu.targetGraphic=menuFace;
        menu.transition=Selectable.Transition.ColorTint;
        Label(menuFace.transform,"MenuGlyph","☰",34,Color.white,
            Vector2.zero,new Vector2(60,55),TextAlignmentOptions.Center);

        var energy=Rect(root.transform,"EnergyGroup",new Vector2(0,1),
            new Vector2(239,-37),new Vector2(305,72));
        Panel(energy,"EnergyBacking",Navy,Vector2.zero,new Vector2(298,64));
        Label(energy,"EnergyTitle","ENERGY",14,Color.white,
            new Vector2(-107,19),new Vector2(92,22),TextAlignmentOptions.Center);
        Image[] icons=new Image[5];
        for(int i=0;i<5;i++)
        {
            var image=Panel(energy,"EnergyBolt"+(i+1),Color.clear,
                new Vector2(-120+i*34,-8),new Vector2(32,35));
            Label(image.transform,"Bolt","ϟ",34,Cyan,
                Vector2.zero,new Vector2(30,35),TextAlignmentOptions.Center);
            // Tint the bolt's graphic itself for a live fill indicator.
            // The Image stays transparent; it's used as a unified tint target.
            icons[i]=image;
        }
        // The numerals are never decorative: energy is read from EnergyManager.
        var energyCount=Label(energy,"EnergyCount","5 / 5",18,Color.white,
            new Vector2(104,-8),new Vector2(73,34),TextAlignmentOptions.Center);

        var score=Rect(root.transform,"ScoreGroup",new Vector2(1,1),
            new Vector2(-73,-41),new Vector2(145,69));
        Panel(score,"ScoreBacking",Navy,Vector2.zero,new Vector2(137,67));
        var scoreLabel=Label(score,"ScoreValue","SCORE\n000000",21,Gold,
            Vector2.zero,new Vector2(133,63),TextAlignmentOptions.Center);
        scoreLabel.fontStyle=FontStyles.Bold;
        var driver=root.GetComponent<ChemQuestUnifiedHUD>();
        if(driver==null)driver=root.AddComponent<ChemQuestUnifiedHUD>();
        driver.Configure(menu,energyCount,scoreLabel,icons);
    }

    private static RectTransform Rect(Transform parent,string name,
        Vector2 anchor,Vector2 anchored,Vector2 size)
    {
        Transform found=parent.Find(name);
        GameObject obj;
        if(found==null)
        {
            obj=new GameObject(name,typeof(RectTransform));
            obj.transform.SetParent(parent,false);
        }
        else obj=found.gameObject;
        var rt=obj.GetComponent<RectTransform>();
        rt.anchorMin=rt.anchorMax=anchor;
        rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition=anchored;
        rt.sizeDelta=size;
        rt.localScale=Vector3.one;
        return rt;
    }

    private static Image Panel(Transform parent,string name,Color color,
        Vector2 pos,Vector2 size)
    {
        var rt=Rect(parent,name,new Vector2(.5f,.5f),pos,size);
        var img=rt.GetComponent<Image>();
        if(img==null)img=rt.gameObject.AddComponent<Image>();
        img.color=color;
        img.raycastTarget=false;
        return img;
    }

    private static TextMeshProUGUI Label(Transform parent,string name,string value,
        int fontSize,Color color,Vector2 pos,Vector2 size,
        TextAlignmentOptions alignment)
    {
        var rt=Rect(parent,name,new Vector2(.5f,.5f),pos,size);
        var label=rt.GetComponent<TextMeshProUGUI>();
        if(label==null)label=rt.gameObject.AddComponent<TextMeshProUGUI>();
        label.text=value;label.fontSize=fontSize;label.color=color;
        label.alignment=alignment;label.textWrappingMode=TextWrappingModes.NoWrap;
        label.raycastTarget=false;
        return label;
    }
}
#endif
