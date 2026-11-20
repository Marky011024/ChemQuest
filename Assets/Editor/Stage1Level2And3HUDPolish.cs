#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Only Stage 1 Levels 2 and 3. Mirrors the already-approved Level 1 HUD.
// Reuses the real Energy1..5 and Level2/3ScoreText bindings; does not create
// a second canvas, modify scene puzzles, or change score/energy rules.
public static class Stage1Level2And3HUDPolish
{
    [MenuItem("ChemQuest/HUD/Fix Stage 1 Levels 2-3 Only")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode","Stop Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 1 Levels 2 and 3",
            "Style ONLY Stage1Level2 and Stage1Level3 with the approved "+
            "Stage1Level1 Pause/Energy/Score design. Scene files will be saved. "+
            "Make a Git commit or backup first.","Apply Fix","Cancel"))return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        string previous=SceneManager.GetActiveScene().path;
        try
        {
            for(int level=2;level<=3;level++)
            {
                string path="Assets/Scenes/Stage1Level"+level+".unity";
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                Fix(scene,level);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 1 HUD completed",
                "Saved Stage 1 Levels 2 and 3. Their HUDs are editable in "+
                "Hierarchy before Play. Test Menu, energy and score.","OK");
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("HUD Installer error",ex.Message,"OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(previous))
                EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
        }
    }

    private static void Fix(Scene scene,int level)
    {
        var canvas=FindScene(scene,"Canvas")?.GetComponent<Canvas>();
        if(canvas==null)throw new Exception("Missing Canvas in "+scene.name);
        var old=FindScene(scene,"ChemQuestUnifiedHUD");
        if(old!=null && old.GetComponent<ChemQuestUnifiedHUD>()!=null)
            UnityEngine.Object.DestroyImmediate(old);

        Color navy=new Color32(3,27,54,236);
        Color gold=new Color32(255,202,76,255);
        Color cyan=new Color32(20,218,255,255);
        var parent=canvas.transform;
        var energy=Ensure(parent,"EnergyPanel",new Vector2(0,1),
            new Vector2(360,-68),new Vector2(435,72));
        var energyImage=energy.GetComponent<Image>();
        if(energyImage==null)energyImage=energy.gameObject.AddComponent<Image>();
        energyImage.sprite=Rounded();
        energyImage.type=Image.Type.Sliced;
        energyImage.color=navy;
        energyImage.raycastTarget=false;

        var title=Label(energy,"EnergyText","ENERGY",
            new Vector2(-160,19),new Vector2(112,24),22,Color.white);
        title.fontStyle=FontStyles.Bold;

        // Reparent already-serialized Energy1..5. Their level managers retain
        // the SAME Image/GameObject references, with no duplicate energy icons.
        for(int i=0;i<5;i++)
        {
            var icon=Find(parent,"Energy"+(i+1))?.GetComponent<Image>();
            if(icon==null)
                throw new Exception("Energy"+(i+1)+" missing from "+scene.name);
            icon.transform.SetParent(energy,false);
            Place(icon.rectTransform,new Vector2(-119+i*53,-6),
                new Vector2(38,49),new Vector2(.5f,.5f));
            icon.color=cyan;
            icon.preserveAspect=true;
            icon.raycastTarget=false;
        }
        var number=Label(energy,"EnergyCountStage1","5 / 5",
            new Vector2(171,-7),new Vector2(78,34),25,Color.white);
        var observer=energy.GetComponent<Stage1Level1HUDCounter>();
        if(observer==null)
            observer=energy.gameObject.AddComponent<Stage1Level1HUDCounter>();
        observer.SetCounter(number);

        var scoreLabel=Find(parent,"Level"+level+"ScoreText")?.GetComponent<TextMeshProUGUI>();
        if(scoreLabel==null)
            throw new Exception("Level"+level+"ScoreText missing from "+scene.name);
        var score=Ensure(parent,"ScorePanel",new Vector2(1,1),
            new Vector2(-112,-68),new Vector2(176,78));
        var scoreImage=score.GetComponent<Image>();
        if(scoreImage==null)scoreImage=score.gameObject.AddComponent<Image>();
        scoreImage.sprite=Rounded();
        scoreImage.type=Image.Type.Sliced;
        scoreImage.color=navy;
        scoreImage.raycastTarget=false;
        scoreLabel.transform.SetParent(score,false);
        Place(scoreLabel.rectTransform,Vector2.zero,
            new Vector2(168,70),new Vector2(.5f,.5f));
        scoreLabel.color=gold;
        scoreLabel.fontSize=25;
        scoreLabel.fontStyle=FontStyles.Bold;
        scoreLabel.alignment=TextAlignmentOptions.Center;
        scoreLabel.raycastTarget=false;
        // Never overwrite its text: the existing ScoreManager owns that.

        // A new *scene-authored* button is needed because the old Pause button
        // is nested inside a prefab. Reparenting prefab children is blocked by
        // Unity, so leave the prefab intact and hide ONLY its old button.
        var manager=FindPauseManager(scene);
        if(manager==null)throw new Exception("PauseMenuManager missing in "+scene.name);
        var menu=Ensure(parent,"Stage1StyledPauseButton",
            new Vector2(0,1),new Vector2(70,-72),new Vector2(94,86));
        var button=menu.GetComponent<Button>();
        if(button==null)button=menu.gameObject.AddComponent<Button>();
        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick,manager.PauseGame);
        foreach(var oldButton in AllButtons(scene))
        {
            if(oldButton==null || oldButton==button)continue;
            string oldName=oldButton.name.ToLowerInvariant();
            if(oldName=="pausebutton" || oldName=="stage1pausebutton")
            {
                // Don't delete/reparent the existing prefab or pause panel.
                // Disabling its click and visual avoids overlapping controls.
                oldButton.interactable=false;
                var oldGraphic=oldButton.targetGraphic as Image;
                if(oldGraphic!=null)oldGraphic.enabled=false;
                foreach(var graphic in oldButton.GetComponentsInChildren<Graphic>(true))
                    graphic.enabled=false;
            }
        }
        var face=menu.GetComponent<Image>();
        if(face==null)face=menu.gameObject.AddComponent<Image>();
        face.sprite=Rounded();
        face.type=Image.Type.Sliced;
        face.color=new Color32(8,49,83,255);
        face.raycastTarget=true;
        button.targetGraphic=face;
        button.interactable=true;
        var outline=face.GetComponent<Outline>();
        if(outline==null)outline=face.gameObject.AddComponent<Outline>();
        outline.effectColor=gold;
        outline.effectDistance=new Vector2(3,-3);
        outline.useGraphicAlpha=false;
        for(int i=0;i<3;i++)
        {
            var line=Ensure(menu,"MenuLine"+(i+1),new Vector2(.5f,.5f),
                new Vector2(0,16-i*16),new Vector2(49,7));
            var img=line.GetComponent<Image>();
            if(img==null)img=line.gameObject.AddComponent<Image>();
            img.color=Color.white;img.raycastTarget=false;
        }
        // Hide any old glyph text rather than render a font-dependent Unicode
        // square. Keep the real Pause button and its listeners intact.
        foreach(var label in menu.GetComponentsInChildren<TMP_Text>(true))
            if(label.name!="MenuLine1"&&label.name!="MenuLine2"&&label.name!="MenuLine3")
                label.enabled=false;
        menu.SetAsLastSibling();
    }

    private static PauseMenuManager FindPauseManager(Scene scene)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var comps=root.GetComponentsInChildren<PauseMenuManager>(true);
            if(comps.Length>0)return comps[0];
        }
        return null;
    }
    private static System.Collections.Generic.IEnumerable<Button> AllButtons(Scene scene)
    {
        foreach(var root in scene.GetRootGameObjects())
            foreach(var b in root.GetComponentsInChildren<Button>(true))
                yield return b;
    }
    private static GameObject FindScene(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var t=Find(root.transform,name);
            if(t!=null)return t.gameObject;
        }
        return null;
    }
    private static Transform Find(Transform root,string name)
    {
        if(root.name==name)return root;
        foreach(Transform child in root)
        {
            var t=Find(child,name);
            if(t!=null)return t;
        }
        return null;
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
        Place(rt,pos,size,anchor);
        return rt;
    }
    private static void Place(RectTransform rt,Vector2 pos,Vector2 size,Vector2 anchor)
    {
        rt.anchorMin=rt.anchorMax=anchor;
        rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition=pos;
        rt.sizeDelta=size;
        rt.localScale=Vector3.one;
    }
    private static TextMeshProUGUI Label(Transform parent,string name,
        string value,Vector2 pos,Vector2 size,int fontSize,Color color)
    {
        var rt=Ensure(parent,name,new Vector2(.5f,.5f),pos,size);
        var text=rt.GetComponent<TextMeshProUGUI>();
        if(text==null)text=rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text=value;
        text.fontSize=fontSize;
        text.color=color;
        text.fontStyle=FontStyles.Bold;
        text.alignment=TextAlignmentOptions.Center;
        text.raycastTarget=false;
        return text;
    }
    private static Sprite Rounded()
    {
        // The approved Level 1 tool created this imported, nine-sliced asset.
        const string path="Assets/ChemQuestSharedHUD/RoundedHUD.png";
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(sprite==null)
            throw new Exception("RoundedHUD.png not found. Run the "+
                "Stage 1 Level 1 HUD fix first to generate it.");
        return sprite;
    }
}
#endif
