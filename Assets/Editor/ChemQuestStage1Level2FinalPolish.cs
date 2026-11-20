using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Safe, idempotent edit-time layout for the existing Stage1Level2 scene.
// Keeps the original gameplay objects, event bindings and reagent pour system.
public static class ChemQuestStage1Level2FinalPolish
{
    private const string ScenePath = "Assets/Scenes/Stage1Level2.unity";
    private const string CloudPath = "Assets/Art/ChemQuestReagents/BluePrecipitateCloud.png";

    [MenuItem("Tools/ChemQuest/Stage 1/Level 2 - Polish Assessment, Temperature and Precipitate")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stop Play Mode first.","OK");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stage1Level2 scene not found.","OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        string[] needed={"EnergyQuestionPanel","ThermalControlPanel","EnergyQuestionText",
            "ExothermicButton","EndothermicButton","TemperatureText","ThermalSlider",
            "TemperatureNoteText","BluePrecipitate","CopperSulfateLabel","SodiumHydroxideLabel"};
        foreach(var name in needed)
            if (Find(scene,name)==null)
            {
                EditorUtility.DisplayDialog("ChemQuest","Missing "+name+". No changes applied.","OK");
                return;
            }
        RectTransform questions=Find(scene,"EnergyQuestionPanel");
        RectTransform temp=Find(scene,"ThermalControlPanel");
        Place(questions,new Vector2(-310,-186),new Vector2(352,146));
        Place(temp,new Vector2(310,-186),new Vector2(352,146));
        Tint(questions,new Color(.035f,.14f,.235f,.94f));
        Tint(temp,new Color(.035f,.14f,.235f,.94f));

        // Fit question and two large independent touch targets.
        Place(Find(scene,"EnergyQuestionText"),new Vector2(0,40),new Vector2(331,54));
        StyleText(Find(scene,"EnergyQuestionText"),21,Color.white);
        Place(Find(scene,"ExothermicButton"),new Vector2(-87,-30),new Vector2(163,53));
        Place(Find(scene,"EndothermicButton"),new Vector2(87,-30),new Vector2(163,53));
        StyleButtonText(Find(scene,"ExothermicButton"),16);
        StyleButtonText(Find(scene,"EndothermicButton"),16);

        // Clearly separated temperature, read-only slider and its disclaimer.
        Place(Find(scene,"TemperatureText"),new Vector2(0,41),new Vector2(335,39));
        StyleText(Find(scene,"TemperatureText"),20,new Color(.98f,.9f,.63f,1));
        Place(Find(scene,"ThermalSlider"),new Vector2(0,-2),new Vector2(275,24));
        Place(Find(scene,"TemperatureNoteText"),new Vector2(0,-49),new Vector2(322,34));
        StyleText(Find(scene,"TemperatureNoteText"),13,new Color(.82f,.91f,1,1));

        // Keep labels directly under the bottle artwork, above lower HUD panels.
        Place(Find(scene,"CopperSulfateLabel"),new Vector2(0,-79),new Vector2(190,33));
        Place(Find(scene,"SodiumHydroxideLabel"),new Vector2(0,-79),new Vector2(190,33));
        StyleText(Find(scene,"CopperSulfateLabel"),14,Color.white);
        StyleText(Find(scene,"SodiumHydroxideLabel"),14,Color.white);

        var cloud=CreateCloudSprite();
        if(cloud!=null)
        {
            var precipitate=Find(scene,"BluePrecipitate");
            var solid=precipitate.GetComponent<Image>();
            if(solid!=null)
            {
                Undo.RecordObject(solid,"Soften precipitate color");
                var c=solid.color;
                solid.color=new Color(.10f,.53f,.94f,.48f);
                EditorUtility.SetDirty(solid);
            }
            // Cloud layers are children of the REAL precipitate; they follow its
            // pre-existing reveal/scale animation, and never block UI events.
            for(int i=0;i<3;i++)
            {
                string name="SoftBluePrecipitateCloud_"+(i+1);
                Transform ch=precipitate.Find(name);
                if(ch==null)
                {
                    var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                    Undo.RegisterCreatedObjectUndo(go,"Add blue precipitate cloud");
                    go.transform.SetParent(precipitate,false);
                    ch=go.transform;
                }
                var rt=(RectTransform)ch;
                Undo.RecordObject(rt,"Position precipitate cloud");
                rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
                rt.anchoredPosition=new Vector2((i-1)*25,((i%2)*7)-2);
                rt.sizeDelta=new Vector2(85,39);
                rt.localScale=Vector3.one;
                var image=ch.GetComponent<Image>();
                Undo.RecordObject(image,"Set precipitate cloud art");
                image.sprite=cloud;
                image.color=new Color(.34f,.84f,1f,.88f);
                image.preserveAspect=false;
                image.raycastTarget=false;
                EditorUtility.SetDirty(image);
                // Existing precipitation particle sprites remain above the clouds.
                ch.SetAsFirstSibling();
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=questions.gameObject;
        EditorGUIUtility.PingObject(questions.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            "Saved the Stage 1 Level 2 UI polish. Answer buttons are larger, temperature labels have dedicated spaces, bottle labels are raised, and the precipitate uses editable cloud sprites. The original pouring and chemistry scripts remain unchanged. Test the full experiment in Play Mode.","OK");
    }

    static RectTransform Find(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
            foreach(var t in root.GetComponentsInChildren<RectTransform>(true))
                if(t.name==name)return t;
        return null;
    }
    static void Place(RectTransform rt,Vector2 pos,Vector2 size)
    {
        Undo.RecordObject(rt,"Improve Level 2 layout");
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        EditorUtility.SetDirty(rt);
    }
    static void Tint(RectTransform rt,Color c)
    {
        var im=rt.GetComponent<Image>();if(im==null)return;
        Undo.RecordObject(im,"Improve panel contrast");
        im.color=c;EditorUtility.SetDirty(im);
    }
    static void StyleText(RectTransform rt,int size,Color color)
    {
        var tmp=rt.GetComponent<TMP_Text>();
        if(tmp!=null)
        {
            Undo.RecordObject(tmp,"Improve text readability");
            tmp.fontSize=size;tmp.enableAutoSizing=true;
            tmp.fontSizeMin=Mathf.Max(11,size-4);tmp.fontSizeMax=size;
            tmp.color=color;tmp.alignment=TextAlignmentOptions.Center;
            EditorUtility.SetDirty(tmp);
        }
        var ui=rt.GetComponent<Text>();
        if(ui!=null)
        {
            Undo.RecordObject(ui,"Improve text readability");
            ui.fontSize=size;ui.color=color;ui.alignment=TextAnchor.MiddleCenter;
            EditorUtility.SetDirty(ui);
        }
    }
    static void StyleButtonText(RectTransform b,int size)
    {
        foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))
        {
            Undo.RecordObject(t,"Improve answer readability");
            t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=12;t.fontSizeMax=size;
            t.alignment=TextAlignmentOptions.Center;
            EditorUtility.SetDirty(t);
        }
        foreach(var t in b.GetComponentsInChildren<Text>(true))
        {
            Undo.RecordObject(t,"Improve answer readability");
            t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;
            EditorUtility.SetDirty(t);
        }
    }
    static Sprite CreateCloudSprite()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Art"))
            AssetDatabase.CreateFolder("Assets","Art");
        if(!AssetDatabase.IsValidFolder("Assets/Art/ChemQuestReagents"))
            AssetDatabase.CreateFolder("Assets/Art","ChemQuestReagents");
        if(!File.Exists(CloudPath))
        {
            const int w=160,h=80;
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);
            Color[] pixels=new Color[w*h];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float cloud=0;
                for(int i=0;i<5;i++)
                {
                    float cx=22+i*29, cy=37+(i%2)*8;
                    float dx=(x-cx)/(28f+(i%2)*4),dy=(y-cy)/21f;
                    float a=Mathf.Clamp01(1-dx*dx-dy*dy);
                    cloud=Mathf.Max(cloud,a*a*(3-2*a));
                }
                float feather=Mathf.Clamp01(Mathf.Min(x,w-1-x)/8f)*
                    Mathf.Clamp01(Mathf.Min(y,h-1-y)/8f);
                pixels[y*w+x]=new Color(.24f,.75f,1f,.72f*cloud*feather);
            }
            tex.SetPixels(pixels);tex.Apply();
            File.WriteAllBytes(CloudPath,tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
        AssetDatabase.ImportAsset(CloudPath,ImportAssetOptions.ForceUpdate);
        var imp=AssetImporter.GetAtPath(CloudPath) as TextureImporter;
        if(imp!=null && imp.textureType!=TextureImporterType.Sprite)
        {
            imp.textureType=TextureImporterType.Sprite;
            imp.spriteImportMode=SpriteImportMode.Single;
            imp.alphaIsTransparency=true;
            imp.mipmapEnabled=false;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(CloudPath);
    }
}
