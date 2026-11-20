#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-time, repeatable SCENE EDITOR command for Stage 4 Levels 1, 2 and 3.
// Only reuses the original scene's Pause button, Energy1-5, EnergyCount,
// EnergyTitle and ScoreText. Never introduces a second HUD canvas.
// Original scene scripts keep control of scores, penalties and the pause menu.
public static class Stage4LevelsHUDPolish
{
    private static readonly Color Navy=new Color32(3,27,54,236);
    private static readonly Color MenuBlue=new Color32(8,49,83,255);
    private static readonly Color Cyan=new Color32(20,218,255,255);
    private static readonly Color Gold=new Color32(255,202,76,255);

    [MenuItem("ChemQuest/HUD/Fix Stage 4 Levels 1-3 Only")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play","Stop Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 4 HUD — Levels 1–3",
            "Apply the approved COMPACT Stage 2-style Menu, Energy and Score HUD "+
            "to Stage 4 Level 1, 2 and 3. No second canvas is created. "+
            "Existing puzzle logic and Pause callbacks stay unchanged. "+
            "This SAVES all three scenes. Commit/back up your scene files first.",
            "Apply HUD","Cancel"))return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        string previous=SceneManager.GetActiveScene().path;
        try
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/ChemQuestSharedHUD/RoundedHUD.png");
            if(sprite==null)throw new Exception(
                "Missing RoundedHUD.png. Run the approved Stage 1 Level 1 HUD fix first.");
            int saved=0;
            for(int level=1;level<=3;level++)
            {
                var scene=EditorSceneManager.OpenScene(
                    "Assets/Scenes/Stage4Level"+level+".unity",OpenSceneMode.Single);
                FixScene(scene,level,sprite);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                saved++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 4 HUD complete",
                "Saved "+saved+" Stage 4 scenes. The Menu, Energy and Score "+
                "objects are editable in the Hierarchy before Play. "+
                "Verify interactions and score/energy behavior in Game View.","OK");
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Stage 4 HUD error",ex.Message,"OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(previous))
                EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
        }
    }

    private static void FixScene(Scene scene,int level,Sprite sprite)
    {
        var canvas=SceneObject(scene,"Stage4Level"+level+"Canvas")?.transform;
        if(canvas==null)throw new Exception("Missing Stage4Level"+level+"Canvas.");
        var pause=Find(canvas,"Stage4PauseButton")?.GetComponent<Button>();
        var title=Find(canvas,"EnergyTitle")?.GetComponent<TMP_Text>();
        var count=Find(canvas,"EnergyCount")?.GetComponent<TMP_Text>();
        var score=Find(canvas,"ScoreText")?.GetComponent<TMP_Text>();
        if(pause==null||title==null||count==null||score==null)
            throw new Exception("Missing Stage 4 Pause/Energy/Score object in "+scene.name);
        Image[] bolts=new Image[5];
        for(int i=0;i<5;i++)
        {
            bolts[i]=Find(canvas,"Energy"+(i+1))?.GetComponent<Image>();
            if(bolts[i]==null)throw new Exception("Missing Energy"+(i+1)+" in "+scene.name);
        }

        // Cleanup only the erroneous old overlaid canvas, if any.
        var duplicate=SceneObject(scene,"ChemQuestUnifiedHUD");
        if(duplicate!=null && duplicate.GetComponent<ChemQuestUnifiedHUD>()!=null)
            UnityEngine.Object.DestroyImmediate(duplicate);

        // EXISTING menu Button: preserve its onClick, navigation and pause behavior.
        var menuRT=pause.GetComponent<RectTransform>();
        Place(menuRT,new Vector2(0,1),new Vector2(52,-47),new Vector2(68,64));
        var menuImage=pause.GetComponent<Image>();
        if(menuImage==null)menuImage=pause.gameObject.AddComponent<Image>();
        Skin(menuImage,sprite,MenuBlue,true);
        pause.targetGraphic=menuImage;
        pause.interactable=true;
        var outline=menuImage.GetComponent<Outline>();
        if(outline==null)outline=menuImage.gameObject.AddComponent<Outline>();
        outline.effectColor=Gold;
        outline.effectDistance=new Vector2(2,-2);
        outline.useGraphicAlpha=false;
        outline.enabled=true;
        foreach(var label in pause.GetComponentsInChildren<TMP_Text>(true))
            label.enabled=false;
        for(int i=0;i<3;i++)
        {
            var line=Ensure(menuRT,"Line"+(i+1),new Vector2(.5f,.5f),
                new Vector2(0,11-11*i),new Vector2(34,5));
            var graphic=line.GetComponent<Image>();
            if(graphic==null)graphic=line.gameObject.AddComponent<Image>();
            graphic.sprite=null;
            graphic.type=Image.Type.Simple;
            graphic.color=Color.white;
            graphic.raycastTarget=false;
            graphic.enabled=true;
        }

        // Move (not copy) EXISTING energy sprites: script references survive.
        var energy=Ensure(canvas,"Stage4EnergyPanel",new Vector2(0,1),
            new Vector2(275,-43),new Vector2(346,60));
        var backing=energy.GetComponent<Image>();
        if(backing==null)backing=energy.gameObject.AddComponent<Image>();
        Skin(backing,sprite,Navy,false);

        title.transform.SetParent(energy,false);
        Place(title.rectTransform,new Vector2(.5f,.5f),
            new Vector2(-105,16),new Vector2(83,18));
        title.text="ENERGY";
        title.fontSize=16;
        title.color=Color.white;
        title.fontStyle=FontStyles.Bold;
        title.alignment=TextAlignmentOptions.Center;
        title.raycastTarget=false;
        for(int i=0;i<5;i++)
        {
            bolts[i].transform.SetParent(energy,false);
            Place(bolts[i].rectTransform,new Vector2(.5f,.5f),
                new Vector2(-103+i*36,-6),new Vector2(28,35));
            bolts[i].color=Cyan;
            bolts[i].preserveAspect=true;
            bolts[i].raycastTarget=false;
        }
        count.transform.SetParent(energy,false);
        Place(count.rectTransform,new Vector2(.5f,.5f),
            new Vector2(130,-6),new Vector2(63,29));
        count.fontSize=18;
        count.color=Color.white;
        count.fontStyle=FontStyles.Bold;
        count.alignment=TextAlignmentOptions.Center;
        count.raycastTarget=false;
        // Keep existing 5/5 live update from the Stage 4 level controller.

        var scorePanel=Ensure(canvas,"Stage4ScorePanel",new Vector2(1,1),
            new Vector2(-74,-43),new Vector2(138,65));
        var scoreBacking=scorePanel.GetComponent<Image>();
        if(scoreBacking==null)scoreBacking=scorePanel.gameObject.AddComponent<Image>();
        Skin(scoreBacking,sprite,Navy,false);
        score.transform.SetParent(scorePanel,false);
        Place(score.rectTransform,new Vector2(.5f,.5f),
            Vector2.zero,new Vector2(132,60));
        score.color=Gold;
        score.fontSize=19;
        score.fontStyle=FontStyles.Bold;
        score.alignment=TextAlignmentOptions.Center;
        score.raycastTarget=false;
        // Keep original SCORE\n000000 string + existing scoring logic.

        // HUD stays above artwork but does not cover the puzzle workbench.
        energy.SetAsLastSibling();
        scorePanel.SetAsLastSibling();
        menuRT.SetAsLastSibling();
    }

    private static void Skin(Image graphic,Sprite sprite,Color color,bool raycast)
    {
        graphic.sprite=sprite;
        graphic.type=Image.Type.Sliced;
        graphic.color=color;
        graphic.raycastTarget=raycast;
    }
    private static RectTransform Ensure(Transform parent,string name,
        Vector2 anchor,Vector2 pos,Vector2 size)
    {
        var found=parent.Find(name);
        GameObject go;
        if(found!=null)go=found.gameObject;
        else
        {
            go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
        }
        var rect=go.GetComponent<RectTransform>();
        Place(rect,anchor,pos,size);
        return rect;
    }
    private static void Place(RectTransform rect,Vector2 anchor,Vector2 pos,Vector2 size)
    {
        rect.anchorMin=rect.anchorMax=anchor;
        rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;
        rect.sizeDelta=size;
        rect.localScale=Vector3.one;
    }
    private static Transform Find(Transform parent,string name)
    {
        if(parent.name==name)return parent;
        foreach(Transform child in parent)
        {
            var result=Find(child,name);
            if(result!=null)return result;
        }
        return null;
    }
    private static GameObject SceneObject(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var result=Find(root.transform,name);
            if(result!=null)return result.gameObject;
        }
        return null;
    }
}
#endif
