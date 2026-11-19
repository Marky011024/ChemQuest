#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Stage 5 has approved artwork with the upper HUD already painted in.
// DO NOT add a second HUD or move the game-specific score/energy readouts.
// The material benches and their hit surfaces remain completely untouched.
public static class Stage5LevelsHUDPolish
{
    private static readonly Color Navy = new Color32(8,49,83,255);
    private static readonly Color Gold = new Color32(255,202,76,255);

    [MenuItem("ChemQuest/HUD/Fix Stage 5 Levels 1-3 Only")]
    private static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play","Stop Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Stage 5 HUD — Levels 1-3",
            "Polish the existing Stage 5 Menu button, Energy counter and Score "+
            "using the approved compact design. No second HUD and no game "+
            "mechanics changed. This saves Stage5Level1/2/3 scenes. "+
            "Create a backup or Git commit first.","Apply HUD","Cancel"))return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        string original = SceneManager.GetActiveScene().path;
        int saved=0;
        try
        {
            Sprite rounded=AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/ChemQuestSharedHUD/RoundedHUD.png");
            if(rounded==null)throw new Exception(
                "Missing RoundedHUD.png; run the approved Stage 1 Level 1 HUD setup first.");
            for(int level=1;level<=3;level++)
            {
                string path="Assets/Scenes/Stage5Level"+level+".unity";
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                Fix(scene,level,rounded);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                saved++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 5 HUD",
                "Saved "+saved+" Stage 5 scenes. The upper HUD is editable "+
                "in the Hierarchy in Edit Mode. Test Menu, Energy and Score in Play Mode.",
                "OK");
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("Stage 5 HUD installer",e.Message,"OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(original))
                EditorSceneManager.OpenScene(original,OpenSceneMode.Single);
        }
    }

    private static void Fix(Scene scene,int level,Sprite rounded)
    {
        var canvas=SceneObject(scene,"Stage5Level"+level+"Canvas")?.transform;
        if(canvas==null)throw new Exception(
            scene.name+" has no baked Canvas. Open it and use ChemQuest > "+
            "Stage 5 > Bake Playable UI Into Open Scene first.");
        var menu=Find(canvas,"MenuButton")?.GetComponent<Button>();
        var energy=Find(canvas,"ReferenceLiveEnergy")?.GetComponent<TMP_Text>();
        var score=Find(canvas,"ReferenceLiveScore")?.GetComponent<TMP_Text>();
        // Stage 5 reference-art scenes use one full-screen mapped touch
        // controller to handle the top-left Menu hotspot (action 50 -> Pause).
        // They intentionally do not have a separate MenuButton GameObject.
        var mappedTouch=Find(canvas,"Stage5MappedTouchSurface")
            ?.GetComponent<Stage5ReferenceTouchController>();
        if(energy==null||score==null||(menu==null && mappedTouch==null))
            throw new Exception(scene.name+" needs the saved Stage 5 reference HUD "+
                "and menu input. Use ChemQuest > Stage 5 > Bake Playable UI "+
                "Into Open Scene, then check the scene hierarchy.");

        // Only style an actual Button when the scene contains one.
        // With mapped-touch reference artwork, keep the already painted
        // compact Menu icon. Do not introduce a second raycast hitbox.
        if(menu!=null)
        {
            // The existing MenuButton is already wired to the right pause handler.
            // Its 67 x 65 reference-art hitbox stays in the same location.
            var parent=menu.GetComponent<RectTransform>();
            var face=Ensure(parent,"Stage5CompactMenuFace",
                new Vector2(0,0),new Vector2(63,61));
            var background=face.GetComponent<Image>();
            if(background==null)background=face.gameObject.AddComponent<Image>();
            background.sprite=rounded;
            background.type=Image.Type.Sliced;
            background.color=Navy;
            background.raycastTarget=false;
            var outline=background.GetComponent<Outline>();
            if(outline==null)outline=background.gameObject.AddComponent<Outline>();
            outline.effectColor=Gold;
            outline.effectDistance=new Vector2(2,-2);
            outline.useGraphicAlpha=false;
            outline.enabled=true;
            // No Unicode hamburger character: physical lines won't produce
            // missing-glyph white squares.
            for(int i=0;i<3;i++)
            {
                var rt=Ensure(face,"Stage5MenuLine"+(i+1),
                    new Vector2(0,11-i*11),new Vector2(34,5));
                var img=rt.GetComponent<Image>();
                if(img==null)img=rt.gameObject.AddComponent<Image>();
                img.sprite=null;
                img.type=Image.Type.Simple;
                img.color=Color.white;
                img.raycastTarget=false;
            }
            face.SetAsLastSibling();
            // Keep the actual button clickable even if it uses transparent
            // artwork (Stage5ReferenceLayout manages its hit surface).
            menu.interactable=true;
        }
        else
        {
            Debug.Log("CHEMQUEST Stage 5: "+scene.name+
                " uses the existing mapped Menu hotspot; no duplicate button added.");
        }

        // The five blue lightning icons are painted into the reference PNG.
        // Create editable, non-raycasting dark overlays so spent energy can
        // visibly dim those icons as well as updating the numeric counter.
        // No extra canvas or functional controls are introduced.
        for(int i=0;i<5;i++)
        {
            var rt=Ensure(canvas,"Stage5SpentEnergy"+(i+1),
                new Vector2(-505+i*33,318),new Vector2(27,33));
            var mask=rt.GetComponent<Image>();
            if(mask==null)mask=rt.gameObject.AddComponent<Image>();
            mask.sprite=null;
            mask.type=Image.Type.Simple;
            mask.color=new Color32(5,29,57,225);
            mask.raycastTarget=false;
            rt.SetAsLastSibling();
            // The runtime will show spent slots based on EnergyManager.
            rt.gameObject.SetActive(false);
        }

        // Reference masks cover PRE-PAINTED numerals in the artwork.
        // Their positions are intentional, so restyling text does not
        // duplicate or displace the Stage 5 energy/score readouts.
        energy.color=Color.white;
        energy.fontSize=18;
        energy.fontStyle=FontStyles.Bold;
        energy.alignment=TextAlignmentOptions.Center;
        energy.raycastTarget=false;
        score.color=Gold;
        score.fontSize=18;
        score.fontStyle=FontStyles.Bold;
        score.alignment=TextAlignmentOptions.Center;
        score.raycastTarget=false;
        // Do not replace label text or rebind references: Stage5ReferenceLayout
        // and EnergyManager continue updating both at runtime.
    }

    private static RectTransform Ensure(Transform parent,string name,
        Vector2 position,Vector2 size)
    {
        Transform found=parent.Find(name);
        GameObject go;
        if(found!=null)go=found.gameObject;
        else
        {
            go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
        }
        var rt=go.GetComponent<RectTransform>();
        // Unlike the heating experiment's editable selection rings, the
        // compact menu is created only by this editor command. Do not reset
        // authored positions on subsequent runs.
        if(found==null)
        {
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
            rt.pivot=new Vector2(.5f,.5f);
            rt.anchoredPosition=position;
            rt.sizeDelta=size;
            rt.localScale=Vector3.one;
        }
        return rt;
    }

    private static Transform Find(Transform root,string name)
    {
        if(root.name==name)return root;
        foreach(Transform child in root)
        {
            var found=Find(child,name);
            if(found!=null)return found;
        }
        return null;
    }

    private static GameObject SceneObject(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var found=Find(root.transform,name);
            if(found!=null)return found.gameObject;
        }
        return null;
    }
}
#endif
