using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class ChemQuestStage5Builder
{
    private const string Folder="Assets/Scenes/";
    [MenuItem("Tools/ChemQuest/Build Complete Stage 5 (All Levels)")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)
        {EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before building Stage 5.","OK");return;}
        if(!EditorUtility.DisplayDialog("ChemQuest | Stage 5",
            "Create/refresh editable Stage 5 scenes and all buttons without deleting existing Stage 1-4 scenes?",
            "Build Stage 5","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        var background=FindStage1Background();
        if(!ChemQuestStage5SelectionDesigner.ApplyDesign())return;
        for(int level=1;level<=3;level++)BuildLevel(level,background);
        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach(string name in new[]{"StageSelection","Stage5LevelSelection",
            "Stage5Level1","Stage5Level2","Stage5Level3"})
        {
            var path=Folder+name+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)continue;
            int position=list.FindIndex(x=>x.path==path);
            if(position<0)list.Add(new EditorBuildSettingsScene(path,true));
            else if(!list[position].enabled)list[position]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=list.ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Folder+"Stage5LevelSelection.unity");
        EditorUtility.DisplayDialog("Stage 5 Ready",
            "Created Stage5LevelSelection and Stage5Level1/2/3 with editable GameObjects. Check Console and test Play for buttons and gameplay.",
            "OK");
    }
    private static Sprite FindStage1Background()
    {
        string template=Folder+"Stage1LevelSelection.unity";
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(template)==null)return null;
        EditorSceneManager.OpenScene(template,OpenSceneMode.Single);
        Sprite best=null;
        float bestArea=0;
        foreach(var image in Object.FindObjectsOfType<Image>(true))
        {
            if(image.sprite==null)continue;
            var rt=image.GetComponent<RectTransform>();
            float area=rt!=null?Mathf.Abs(rt.rect.width*rt.rect.height):0;
            if(area>bestArea)
            {best=image.sprite;bestArea=area;}
        }
        return best;
    }
    private static Scene OpenOrCreate(string path)
    {
        return AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null?
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single):
            EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
    }
    private static void BuildLevel(int level,Sprite background)
    {
        string path=Folder+"Stage5Level"+level+".unity";
        var scene=OpenOrCreate(path);
        var controller=GameObject.Find("Stage5Level"+level+"Controller");
        if(controller==null)controller=new GameObject("Stage5Level"+level+"Controller");
        var script=controller.GetComponent<Stage5ElementCrucibleGame>();
        if(script==null)script=controller.AddComponent<Stage5ElementCrucibleGame>();
        script.Level=level;
        script.EnergySprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Energy.png");
        script.BackgroundSprite=background;
        script.BuildEditableUI();
        EditorUtility.SetDirty(script);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,path);
    }
    private static void BuildSelection(Sprite background)
    {
        var path=Folder+"Stage5LevelSelection.unity";
        var scene=OpenOrCreate(path);
        var obj=GameObject.Find("Stage5LevelSelectionController");
        if(obj==null)obj=new GameObject("Stage5LevelSelectionController");
        if(obj.GetComponent<Stage5LevelSelectionManager>()==null)
            obj.AddComponent<Stage5LevelSelectionManager>();
        if(GameObject.Find("Stage5LevelSelectionCanvas")!=null)
        {
            EditorSceneManager.SaveScene(scene,path);return;
        }
        var go=new GameObject("Stage5LevelSelectionCanvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=go.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.matchWidthOrHeight=1f;
        var bg=Image(go.transform,"Stage1StyleBackground",Vector2.zero,
            new Vector2(1280,720),new Color32(6,26,44,255));
        bg.sprite=background;bg.raycastTarget=false;
        bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;
        bg.rectTransform.offsetMin=Vector2.zero;bg.rectTransform.offsetMax=Vector2.zero;
        var shade=Image(go.transform,"Overlay",Vector2.zero,new Vector2(1280,720),
            new Color32(7,19,37,190));shade.raycastTarget=false;
        shade.rectTransform.anchorMin=Vector2.zero;shade.rectTransform.anchorMax=Vector2.one;
        shade.rectTransform.offsetMin=Vector2.zero;shade.rectTransform.offsetMax=Vector2.zero;
        Txt(go.transform,"Title","STAGE 5  |  ELEMENT CRUCIBLE",34,new Vector2(0,265),
            new Vector2(1100,70),Color.white);
        Txt(go.transform,"Subtitle","MATERIAL CLASSIFICATION & PROPERTY WORKBENCH",20,
            new Vector2(0,216),new Vector2(1010,45),new Color32(111,218,249,255));
        var titles=new[]{"THERMAL RESISTANCE","HARDNESS & DUCTILITY","PHASE CONDUCTIVITY"};
        var subtitles=new[]{
            "Drag samples A/B/C to the heating bench. Compare thermal behavior.",
            "Use strike and press to observe brittleness and malleability.",
            "Test conductivity in relevant states and classify by combined evidence."};
        for(int level=1;level<=3;level++)
        {
            float x=-355+(level-1)*355;
            var card=Image(go.transform,"Level"+level+"Card",new Vector2(x,-34),
                new Vector2(322,364),new Color32(20,46,78,242));
            var outline=card.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color32(63,171,209,180);
            outline.effectDistance=new Vector2(2,2);
            Txt(card.transform,"LevelText","LEVEL "+level,22,new Vector2(0,119),
                new Vector2(275,45),new Color32(255,213,102,255));
            Txt(card.transform,"Heading",titles[level-1],23,new Vector2(0,55),
                new Vector2(300,67),Color.white);
            Txt(card.transform,"Details",subtitles[level-1],17,new Vector2(0,-33),
                new Vector2(288,103),Color.white);
            Button(card.transform,"Stage5Level"+level+"Button","SELECT",
                new Vector2(0,-137),new Vector2(258,55),new Color32(12,131,143,255));
        }
        Button(go.transform,"Stage5BackButton","BACK TO STAGES",new Vector2(-480,310),
            new Vector2(200,46),new Color32(54,82,122,255));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,path);
    }
    private static Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.sizeDelta=size;rt.anchoredPosition=pos;
        var img=go.GetComponent<Image>();img.color=color;return img;
    }
    private static void Txt(Transform parent,string name,string value,int font,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchoredPosition=pos;rt.sizeDelta=size;
        var txt=go.GetComponent<TextMeshProUGUI>();
        txt.text=value;txt.fontSize=font;txt.color=color;
        txt.alignment=TextAlignmentOptions.Center;txt.raycastTarget=false;
    }
    private static void Button(Transform parent,string name,string caption,Vector2 pos,Vector2 size,Color color)
    {
        var img=Image(parent,name,pos,size,color);
        img.gameObject.AddComponent<Button>().targetGraphic=img;
        Txt(img.transform,"Caption",caption,18,Vector2.zero,size,Color.white);
    }
}
