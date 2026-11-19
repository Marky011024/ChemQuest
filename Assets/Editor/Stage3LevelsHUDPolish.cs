#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Stage 3 only. Reuses existing HUD objects, sprite assets and controllers.
// No duplicate canvas. Matches compact, approved Stage 2 HUD proportions.
public static class Stage3LevelsHUDPolish
{
    private static readonly Color Navy=new Color32(3,27,54,236);
    private static readonly Color Gold=new Color32(255,202,76,255);
    private static readonly Color Cyan=new Color32(20,218,255,255);

    [MenuItem("ChemQuest/HUD/Fix Stage 3 Levels 1-3 Only")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play","Stop Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 3 HUD Only",
            "Restyle only Stage 3 Levels 1-3 to the approved compact menu, "+
            "energy, and score design. This saves three scenes. "+
            "Commit/backup the scenes first.","Apply Fix","Cancel"))return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        string previous=SceneManager.GetActiveScene().path;
        int saved=0;
        try
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/ChemQuestSharedHUD/RoundedHUD.png");
            if(sprite==null)throw new Exception(
                "RoundedHUD.png is missing. Run Stage 1 Level 1 HUD setup first.");
            for(int level=1;level<=3;level++)
            {
                var scene=EditorSceneManager.OpenScene(
                    "Assets/Scenes/Stage3Level"+level+".unity",OpenSceneMode.Single);
                Fix(scene,level,sprite);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                saved++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 3 HUD",
                "Saved "+saved+" scenes. Check Menu, 5/5 Energy and Score in Game View.",
                "OK");
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("Stage 3 HUD error",e.Message,"OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(previous))
                EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
        }
    }

    private static void Fix(Scene scene,int level,Sprite sprite)
    {
        string canvasName=level==1?"Stage3Level1Canvas":
            level==2?"Stage3Level2GeneratedCanvas":"Stage3Level3Canvas";
        var canvas=SceneObject(scene,canvasName)?.transform;
        if(canvas==null)throw new Exception("Missing canvas "+canvasName);
        var duplicate=SceneObject(scene,"ChemQuestUnifiedHUD");
        if(duplicate!=null && duplicate.GetComponent<ChemQuestUnifiedHUD>()!=null)
            UnityEngine.Object.DestroyImmediate(duplicate);

        Image[] icons=new Image[5];
        for(int i=0;i<5;i++)
        {
            icons[i]=Find(canvas,"Energy"+(i+1))?.GetComponent<Image>();
            if(icons[i]==null)throw new Exception("Missing Energy"+(i+1)+" in "+scene.name);
        }
        string scoreName=level==1?"Level3ScoreText":level==2?"Score":"ScoreText";
        var scoreLabel=Find(canvas,scoreName)?.GetComponent<TMP_Text>();
        if(scoreLabel==null)throw new Exception("Missing "+scoreName+" in "+scene.name);
        var energyTitle=Find(canvas,level==1?"EnergyTitle":
            level==2?"EnergyLabel":"EnergyHeading")?.GetComponent<TMP_Text>();
        var energyCount=Find(canvas,level==1?"EnergyText":
            level==2?"EnergyStatus":"EnergyCount")?.GetComponent<TMP_Text>();
        // Level 1 does not have a persistent energy label: the existing
        // five energy images are present, so add editable decorative labels.
        var energyPanel=Ensure(canvas,"Stage3EnergyPanel",new Vector2(0,1),
            new Vector2(275,-43),new Vector2(346,60));
        var energyImage=energyPanel.GetComponent<Image>();
        if(energyImage==null)energyImage=energyPanel.gameObject.AddComponent<Image>();
        Skin(energyImage,sprite,Navy,false);
        if(energyTitle==null)
            energyTitle=NewLabel(energyPanel,"Stage3EnergyTitle","ENERGY");
        else energyTitle.transform.SetParent(energyPanel,false);
        Place(energyTitle.rectTransform,new Vector2(.5f,.5f),
            new Vector2(-105,16),new Vector2(83,18));
        energyTitle.color=Color.white;energyTitle.fontSize=16;
        energyTitle.fontStyle=FontStyles.Bold;
        energyTitle.alignment=TextAlignmentOptions.Center;
        energyTitle.raycastTarget=false;
        for(int i=0;i<5;i++)
        {
            icons[i].transform.SetParent(energyPanel,false);
            Place(icons[i].rectTransform,new Vector2(.5f,.5f),
                new Vector2(-103+i*36,-6),new Vector2(28,35));
            icons[i].color=Cyan;icons[i].preserveAspect=true;
            icons[i].raycastTarget=false;
        }
        if(energyCount==null)
        {
            energyCount=NewLabel(energyPanel,"Stage3EnergyCount","5 / 5");
            var counter=energyPanel.GetComponent<Stage1Level1HUDCounter>();
            if(counter==null)counter=energyPanel.gameObject.AddComponent<Stage1Level1HUDCounter>();
            counter.SetCounter(energyCount);
        }
        else energyCount.transform.SetParent(energyPanel,false);
        Place(energyCount.rectTransform,new Vector2(.5f,.5f),
            new Vector2(130,-6),new Vector2(63,29));
        energyCount.fontSize=18;energyCount.fontStyle=FontStyles.Bold;
        energyCount.color=Color.white;energyCount.alignment=TextAlignmentOptions.Center;
        energyCount.raycastTarget=false;

        var scorePanel=Ensure(canvas,"Stage3ScorePanel",new Vector2(1,1),
            new Vector2(-74,-43),new Vector2(138,65));
        var scoreImage=scorePanel.GetComponent<Image>();
        if(scoreImage==null)scoreImage=scorePanel.gameObject.AddComponent<Image>();
        Skin(scoreImage,sprite,Navy,false);
        scoreLabel.transform.SetParent(scorePanel,false);
        Place(scoreLabel.rectTransform,new Vector2(.5f,.5f),
            Vector2.zero,new Vector2(132,60));
        scoreLabel.color=Gold;scoreLabel.fontSize=19;
        scoreLabel.fontStyle=FontStyles.Bold;
        scoreLabel.alignment=TextAlignmentOptions.Center;
        scoreLabel.raycastTarget=false;
        // Do not change text, listeners or score manager references.

        Button menu=Find(canvas,level==2?"PauseButton":
            level==3?"Level3PauseButton":"Stage3Level1PauseButton")?.GetComponent<Button>();
        if(level==1)
        {
            // Stage 3 Level 1 has an ALREADY serialized pause button under
            // Stage3Level1HUDCanvas. Use that exact button: no duplicate.
            var controller=SceneObject(scene,"Stage3Level1HUD")?.GetComponent<Stage3Level1HUD>();
            if(controller==null)throw new Exception("Stage 3 Level 1 pause controller missing.");
            var serial=new SerializedObject(controller);
            var field=serial.FindProperty("scenePauseButton");
            var configured=field!=null?field.objectReferenceValue as Button:null;
            if(configured!=null)menu=configured;
            if(menu==null)
                throw new Exception("Stage 3 Level 1 scenePauseButton is not assigned.");
        }
        if(menu==null)throw new Exception("Pause Button missing in "+scene.name);
        var menuRT=menu.GetComponent<RectTransform>();
        Place(menuRT,new Vector2(0,1),new Vector2(52,-47),new Vector2(68,64));
        var menuImage=menu.GetComponent<Image>();
        if(menuImage==null)menuImage=menu.gameObject.AddComponent<Image>();
        Skin(menuImage,sprite,new Color32(8,49,83,255),true);
        menu.targetGraphic=menuImage;
        menu.interactable=true;
        var outline=menuImage.GetComponent<Outline>();
        if(outline==null)outline=menuImage.gameObject.AddComponent<Outline>();
        outline.effectColor=Gold;outline.effectDistance=new Vector2(2,-2);
        outline.useGraphicAlpha=false;outline.enabled=true;
        foreach(var label in menu.GetComponentsInChildren<TMP_Text>(true))
            label.enabled=false;
        for(int i=0;i<3;i++)
        {
            var line=Ensure(menuRT,"Line"+(i+1),new Vector2(.5f,.5f),
                new Vector2(0,11-i*11),new Vector2(34,5));
            var img=line.GetComponent<Image>();
            if(img==null)img=line.gameObject.AddComponent<Image>();
            img.sprite=null;img.type=Image.Type.Simple;
            img.color=Color.white;img.raycastTarget=false;
        }
        energyPanel.SetAsLastSibling();
        scorePanel.SetAsLastSibling();
        menuRT.SetAsLastSibling();
    }

    private static TMP_Text NewLabel(Transform parent,string name,string value)
    {
        var t=Ensure(parent,name,new Vector2(.5f,.5f),
            Vector2.zero,new Vector2(80,26));
        var label=t.GetComponent<TextMeshProUGUI>();
        if(label==null)label=t.gameObject.AddComponent<TextMeshProUGUI>();
        label.text=value;
        return label;
    }
    private static void Skin(Image i,Sprite sprite,Color color,bool raycast)
    {
        i.sprite=sprite;i.type=Image.Type.Sliced;
        i.color=color;i.raycastTarget=raycast;
    }
    private static RectTransform Ensure(Transform parent,string name,
        Vector2 anchor,Vector2 pos,Vector2 size)
    {
        var t=parent.Find(name);
        GameObject go;
        if(t==null)
        {
            go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
        }
        else go=t.gameObject;
        var rt=go.GetComponent<RectTransform>();
        Place(rt,anchor,pos,size);
        return rt;
    }
    private static void Place(RectTransform rt,Vector2 anchor,Vector2 pos,Vector2 size)
    {
        rt.anchorMin=rt.anchorMax=anchor;rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        rt.localScale=Vector3.one;
    }
    private static Transform Find(Transform root,string name)
    {
        if(root.name==name)return root;
        foreach(Transform c in root)
        {
            var result=Find(c,name);
            if(result!=null)return result;
        }
        return null;
    }
    private static GameObject SceneObject(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var child=Find(root.transform,name);
            if(child!=null)return child.gameObject;
        }
        return null;
    }
}
#endif
