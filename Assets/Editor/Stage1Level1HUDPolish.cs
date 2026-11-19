#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Apply once to Stage1Level1 only. Reuses the original scene-bound energy,
// score, and pause managers. NO second Canvas or generic overlay HUD.
public static class Stage1Level1HUDPolish
{
    [MenuItem("ChemQuest/HUD/Fix Stage 1 Level 1 Only")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play","Stop Play Mode first.","OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Stage 1 Level 1 Only",
            "This edits and saves Stage1Level1.unity. The existing score and "+
            "energy scripts remain in control. Back up or commit the scene first.",
            "Apply Fix","Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        const string path="Assets/Scenes/Stage1Level1.unity";
        var previous=SceneManager.GetActiveScene().path;
        try
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            var canvas=FindInScene(scene,"Canvas")?.GetComponent<Canvas>();
            if(canvas==null)throw new System.Exception("Stage 1 Canvas not found.");
            var old=FindInScene(scene,"ChemQuestUnifiedHUD");
            if(old!=null && old.GetComponent<ChemQuestUnifiedHUD>()!=null)
                Object.DestroyImmediate(old);
            var energy=Find(canvas.transform,"EnergyPanel")?.GetComponent<RectTransform>();
            var score=Find(canvas.transform,"ScorePanel")?.GetComponent<RectTransform>();
            var energyLabel=Find(canvas.transform,"EnergyText")?.GetComponent<TextMeshProUGUI>();
            var scoreLabel=Find(canvas.transform,"ScoreText")?.GetComponent<TextMeshProUGUI>();
            if(energy==null||score==null||energyLabel==null||scoreLabel==null)
                throw new System.Exception("Original EnergyPanel / ScorePanel labels missing.");
            Color navy=new Color32(3,27,54,236);
            Color gold=new Color32(255,202,76,255);
            Color cyan=new Color32(20,218,255,255);

            // Edit existing panels: no new overlay and no manager changes.
            Place(energy,new Vector2(322,-68),new Vector2(435,72),
                new Vector2(0,1));
            var energyImage=energy.GetComponent<Image>();
            if(energyImage!=null)
            {
                energyImage.color=navy;energyImage.raycastTarget=false;
                energyImage.sprite=RoundedAsset();energyImage.type=Image.Type.Sliced;
            }
            Place(energyLabel.rectTransform,new Vector2(-160,19),
                new Vector2(112,24),new Vector2(.5f,.5f));
            energyLabel.text="ENERGY";
            energyLabel.fontSize=22;energyLabel.fontStyle=FontStyles.Bold;
            energyLabel.color=Color.white;energyLabel.alignment=TextAlignmentOptions.Center;
            energyLabel.raycastTarget=false;
            for(int i=0;i<5;i++)
            {
                var bolt=Find(energy,"Energy"+(i+1))?.GetComponent<Image>();
                if(bolt==null)continue;
                Place(bolt.rectTransform,new Vector2(-119+i*53,-6),
                    new Vector2(38,49),new Vector2(.5f,.5f));
                bolt.color=cyan;bolt.preserveAspect=true;bolt.raycastTarget=false;
            }
            var counter=Label(energy,"EnergyCountStage1","5 / 5",
                new Vector2(171,-7),new Vector2(78,34),25,Color.white);
            var observer=energy.GetComponent<Stage1Level1HUDCounter>();
            if(observer==null)observer=energy.gameObject.AddComponent<Stage1Level1HUDCounter>();
            observer.SetCounter(counter);

            Place(score,new Vector2(-112,-68),new Vector2(176,78),
                new Vector2(1,1));
            var scoreImage=score.GetComponent<Image>();
            if(scoreImage!=null)
            {
                scoreImage.sprite=RoundedAsset();scoreImage.type=Image.Type.Sliced;
                scoreImage.color=navy;scoreImage.raycastTarget=false;
            }
            Place(scoreLabel.rectTransform,Vector2.zero,new Vector2(168,70),
                new Vector2(.5f,.5f));
            scoreLabel.color=gold;scoreLabel.fontSize=25;
            scoreLabel.fontStyle=FontStyles.Bold;
            scoreLabel.alignment=TextAlignmentOptions.Center;
            scoreLabel.raycastTarget=false;
            // Keep the original score text unchanged: the existing
            // Level1ScoreManager writes SCORE + six-digit value at runtime.

            // Only new visual in this scene: a real button with three
            // drawn bars instead of a missing Unicode hamburger character.
            var menu=Ensure(canvas.transform,"Stage1StyledPauseButton",
                new Vector2(0,1),new Vector2(70,-72),new Vector2(94,86));
            var face=menu.GetComponent<Image>();
            if(face==null)face=menu.gameObject.AddComponent<Image>();
            face.sprite=RoundedAsset();face.type=Image.Type.Sliced;
            face.color=new Color32(8,49,83,255);face.raycastTarget=true;
            var outline=face.GetComponent<Outline>();
            if(outline==null)outline=face.gameObject.AddComponent<Outline>();
            outline.effectColor=gold;outline.effectDistance=new Vector2(3,-3);
            outline.useGraphicAlpha=false;
            for(int i=0;i<3;i++)
            {
                var bar=Ensure(menu,"MenuLine"+(i+1),new Vector2(.5f,.5f),
                    new Vector2(0,16-i*16),new Vector2(49,7));
                var img=bar.GetComponent<Image>();
                if(img==null)img=bar.gameObject.AddComponent<Image>();
                img.color=Color.white;img.raycastTarget=false;
            }
            var button=menu.GetComponent<Button>();
            if(button==null)button=menu.gameObject.AddComponent<Button>();
            button.targetGraphic=face;
            button.onClick.RemoveAllListeners();
            var manager=Object.FindFirstObjectByType<PauseMenuManager>(FindObjectsInactive.Include);
            if(manager==null)throw new System.Exception("Existing PauseMenuManager missing.");
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,
                manager.PauseGame);
            // Hide only old visual pause buttons if a distinct original exists.
            foreach(var b in canvas.GetComponentsInChildren<Button>(true))
            {
                if(b==button)continue;
                var name=b.name.ToLowerInvariant();
                if(name=="pausebutton" || name=="stage1pausebutton")
                {
                    // Hide the older button's render without deleting it or
                    // touching PauseMenuPanel, Resume and navigation buttons.
                    var oldFace=b.targetGraphic as Image;
                    if(oldFace!=null)oldFace.enabled=false;
                    b.interactable=false;
                }
            }
            menu.SetAsLastSibling();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Stage 1 Level 1 HUD saved",
                "Styled original Menu, Energy and Score without duplicate "+
                "canvases. Check Edit Mode Hierarchy and test Pause and energy in Play.","OK");
        }
        finally
        {
            if(!string.IsNullOrEmpty(previous) && previous!=path)
                EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
        }
    }

    private static GameObject FindInScene(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            if(root.name==name)return root;
            var f=Find(root.transform,name);
            if(f!=null)return f.gameObject;
        }
        return null;
    }
    private static Transform Find(Transform root,string name)
    {
        if(root.name==name)return root;
        foreach(Transform child in root)
        {
            var result=Find(child,name);
            if(result!=null)return result;
        }
        return null;
    }
    private static RectTransform Ensure(Transform parent,string name,
        Vector2 anchor,Vector2 pos,Vector2 size)
    {
        var found=parent.Find(name);
        GameObject obj;
        if(found!=null)obj=found.gameObject;
        else
        {
            obj=new GameObject(name,typeof(RectTransform));
            obj.transform.SetParent(parent,false);
        }
        var rect=obj.GetComponent<RectTransform>();
        Place(rect,pos,size,anchor);
        return rect;
    }
    private static void Place(RectTransform r,Vector2 pos,Vector2 size,Vector2 anchor)
    {
        r.anchorMin=r.anchorMax=anchor;
        r.pivot=new Vector2(.5f,.5f);
        r.anchoredPosition=pos;
        r.sizeDelta=size;
        r.localScale=Vector3.one;
    }
    private static TextMeshProUGUI Label(Transform parent,string name,
        string value,Vector2 pos,Vector2 size,int fontSize,Color color)
    {
        var t=Ensure(parent,name,new Vector2(.5f,.5f),pos,size);
        var label=t.GetComponent<TextMeshProUGUI>();
        if(label==null)label=t.gameObject.AddComponent<TextMeshProUGUI>();
        label.text=value;label.fontSize=fontSize;label.color=color;
        label.alignment=TextAlignmentOptions.Center;
        label.raycastTarget=false;
        return label;
    }
    private static Sprite RoundedAsset()
    {
        // Existing baked sprite from earlier HUD tool; reusable in saved scene.
        const string path="Assets/ChemQuestSharedHUD/RoundedHUD.png";
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(sprite!=null)return sprite;
        // Create a real importable sprite instead of runtime-only Texture2D.
        if(!System.IO.Directory.Exists("Assets/ChemQuestSharedHUD"))
            System.IO.Directory.CreateDirectory("Assets/ChemQuestSharedHUD");
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++)
        for(int x=0;x<64;x++)
        {
            float dx=Mathf.Max(Mathf.Abs(x-31.5f)-17f,0);
            float dy=Mathf.Max(Mathf.Abs(y-31.5f)-17f,0);
            float alpha=Mathf.Clamp01(15.5f-Mathf.Sqrt(dx*dx+dy*dy));
            tex.SetPixel(x,y,new Color(1,1,1,alpha));
        }
        tex.Apply();
        System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.spriteBorder=new Vector4(16,16,16,16);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
