#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Stage 2 only. Reskins the existing Level 1-3 HUD objects;
// preserves the original Pause Button onClick, energy icon references,
// score text bindings, bonding experiments, and notebook layout.
// No extra canvas or runtime-generated UI.
public static class Stage2LevelsHUDPolish
{
    private static readonly Color Navy = new Color32(3,27,54,236);
    private static readonly Color Gold = new Color32(255,202,76,255);
    private static readonly Color Cyan = new Color32(20,218,255,255);

    [MenuItem("ChemQuest/HUD/Fix Stage 2 Levels 1-3 Only")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode","Stop Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Style Stage 2 Levels 1-3",
            "Update only Stage2Level1, Stage2Level2 and Stage2Level3. " +
            "Reuse their existing Pause Button, Energy1-5, EnergyText and ScoreText. " +
            "This saves these three scenes. Create a Git commit or backup first.",
            "Apply Fix","Cancel")) return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string previous = SceneManager.GetActiveScene().path;
        int completed=0;
        try
        {
            var rounded=AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/ChemQuestSharedHUD/RoundedHUD.png");
            if(rounded==null)throw new Exception(
                "Missing rounded HUD sprite. Run the approved Stage 1 Level 1 HUD fix first.");
            // Each scene is modified only after its required objects pass validation.
            for(int level=1;level<=3;level++)
            {
                string path="Assets/Scenes/Stage2Level"+level+".unity";
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                FixScene(scene,level,rounded);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                completed++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 2 HUD Saved",
                "Updated "+completed+" Stage 2 levels. Existing buttons and gameplay " +
                "logic were retained. Inspect the Hierarchy, then test each scene in Game View.",
                "OK");
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("Stage 2 HUD",e.Message,"OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(previous))
                EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
        }
    }

    private static void FixScene(Scene scene,int level,Sprite rounded)
    {
        var canvas=SceneObject(scene,"Stage2Level"+level+"Canvas")?.transform;
        if(canvas==null)throw new Exception("Missing Stage 2 Canvas in "+scene.name);
        var originalMenu=Child(canvas,"Stage2PauseButton")?.GetComponent<Button>();
        var title=Child(canvas,"EnergyTitle")?.GetComponent<TMP_Text>();
        var energyText=Child(canvas,"EnergyText")?.GetComponent<TMP_Text>();
        var scoreText=Child(canvas,"ScoreText")?.GetComponent<TMP_Text>();
        if(originalMenu==null || title==null || energyText==null || scoreText==null)
            throw new Exception("Stage 2 HUD references missing from "+scene.name);
        Image[] bolts=new Image[5];
        for(int i=0;i<5;i++)
        {
            bolts[i]=Child(canvas,"Energy"+(i+1))?.GetComponent<Image>();
            if(bolts[i]==null)
                throw new Exception("Energy"+(i+1)+" missing in "+scene.name);
        }

        // Remove only the old erroneous duplicate overlay if present.
        var duplicate=SceneObject(scene,"ChemQuestUnifiedHUD");
        if(duplicate!=null && duplicate.GetComponent<ChemQuestUnifiedHUD>()!=null)
            UnityEngine.Object.DestroyImmediate(duplicate);

        // Stage 5 reference proportions: compact Menu 68x64, Energy 346x60,
        // Score 138x65 on the 1280x720 reference canvas.
        // Reapply command safely to previously edited scenes (idempotent).
        // Menu: existing Pause button and Unity onClick callback preserved.
        var menuRT=originalMenu.GetComponent<RectTransform>();
        Place(menuRT,new Vector2(0,1),new Vector2(52,-47),new Vector2(68,64));
        var menuFace=originalMenu.GetComponent<Image>();
        if(menuFace==null)menuFace=originalMenu.gameObject.AddComponent<Image>();
        Skin(menuFace,rounded,new Color32(8,49,83,255),true);
        originalMenu.targetGraphic=menuFace;
        originalMenu.interactable=true;
        var border=menuFace.GetComponent<Outline>();
        if(border==null)border=menuFace.gameObject.AddComponent<Outline>();
        border.effectColor=Gold;
        border.effectDistance=new Vector2(2,-2);
        border.useGraphicAlpha=false;
        border.enabled=true;

        // Three simple Image bars instead of a missing Unicode glyph.
        foreach(var label in originalMenu.GetComponentsInChildren<TMP_Text>(true))
            label.enabled=false;
        for(int i=0;i<3;i++)
        {
            var line=Ensure(menuRT,"Line"+(i+1),new Vector2(.5f,.5f),
                new Vector2(0,11-11*i),new Vector2(34,5));
            var img=line.GetComponent<Image>();
            if(img==null)img=line.gameObject.AddComponent<Image>();
            img.sprite=null;
            img.type=Image.Type.Simple;
            img.color=Color.white;
            img.raycastTarget=false;
        }

        // Existing energy icons/labels are reparented, not cloned. References
        // stored in Stage 2 level scripts remain valid.
        var energy=Ensure(canvas,"Stage2EnergyPanel",new Vector2(0,1),
            new Vector2(275,-43),new Vector2(346,60));
        var backing=energy.GetComponent<Image>();
        if(backing==null)backing=energy.gameObject.AddComponent<Image>();
        Skin(backing,rounded,Navy,false);
        title.transform.SetParent(energy,false);
        Place(title.rectTransform,new Vector2(.5f,.5f),
            new Vector2(-105,16),new Vector2(83,18));
        title.text="ENERGY";
        title.color=Color.white;
        title.fontSize=16;
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
        energyText.transform.SetParent(energy,false);
        Place(energyText.rectTransform,new Vector2(.5f,.5f),
            new Vector2(130,-6),new Vector2(63,29));
        energyText.color=Color.white;
        energyText.fontSize=18;
        energyText.fontStyle=FontStyles.Bold;
        energyText.alignment=TextAlignmentOptions.Center;
        energyText.raycastTarget=false;
        // Existing stage 2 code remains responsible for '5 / 5' updates.

        var score=Ensure(canvas,"Stage2ScorePanel",new Vector2(1,1),
            new Vector2(-74,-43),new Vector2(138,65));
        var scoreBacking=score.GetComponent<Image>();
        if(scoreBacking==null)scoreBacking=score.gameObject.AddComponent<Image>();
        Skin(scoreBacking,rounded,Navy,false);
        scoreText.transform.SetParent(score,false);
        Place(scoreText.rectTransform,new Vector2(.5f,.5f),
            Vector2.zero,new Vector2(132,60));
        scoreText.fontSize=19;
        scoreText.fontStyle=FontStyles.Bold;
        scoreText.color=Gold;
        scoreText.alignment=TextAlignmentOptions.Center;
        scoreText.raycastTarget=false;
        // Existing score manager owns SCORE and the six-digit number.
        energy.SetAsLastSibling();
        score.SetAsLastSibling();
        menuRT.SetAsLastSibling();
    }

    private static void Skin(Image image,Sprite rounded,Color color,bool hit)
    {
        image.sprite=rounded;
        image.type=Image.Type.Sliced;
        image.color=color;
        image.raycastTarget=hit;
    }

    private static RectTransform Ensure(Transform parent,string name,
        Vector2 anchor,Vector2 position,Vector2 size)
    {
        var child=parent.Find(name);
        GameObject go;
        if(child==null)
        {
            go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
        }
        else go=child.gameObject;
        var rt=go.GetComponent<RectTransform>();
        Place(rt,anchor,position,size);
        return rt;
    }

    private static void Place(RectTransform rt,Vector2 anchor,
        Vector2 position,Vector2 size)
    {
        rt.anchorMin=rt.anchorMax=anchor;
        rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition=position;
        rt.sizeDelta=size;
        rt.localScale=Vector3.one;
    }

    private static Transform Child(Transform root,string name)
    {
        if(root.name==name)return root;
        foreach(Transform ch in root)
        {
            var found=Child(ch,name);
            if(found!=null)return found;
        }
        return null;
    }
    private static GameObject SceneObject(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var hit=Child(root.transform,name);
            if(hit!=null)return hit.gameObject;
        }
        return null;
    }
}
#endif
