#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Build the EXACT Stage 5 artwork and interactive hierarchy as serialized,
// editable Scene objects. Run once per scene, then save the scene.
// This is not a disposable preview: the gameplay controller reuses this
// hierarchy in Play Mode, so the hierarchy exists both before and after Play.
public static class Stage5BakeHierarchyEditor
{
    private const string MenuPath="ChemQuest/Stage 5/Bake Playable UI Into Open Scene";
    private const string ResourceFolder="Assets/Resources/ChemQuestStage5Reference";
    private const string GeneratedFolder=ResourceFolder+"/GeneratedEditorSprites";

    [MenuItem(MenuPath)]
    private static void BakeCurrentScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stage 5","Stop Play Mode before baking the hierarchy.","OK");
            return;
        }

        Scene scene=SceneManager.GetActiveScene();
        var controller=UnityEngine.Object.FindAnyObjectByType<Stage5ElementCrucibleGame>();
        if(controller==null || controller.Level<1 || controller.Level>3)
        {
            EditorUtility.DisplayDialog("Stage 5",
                "Open Stage5Level1, Stage5Level2 or Stage5Level3 scene first. "+
                "The active scene needs its Stage5ElementCrucibleGame controller.","OK");
            return;
        }

        if(!File.Exists(ResourceFolder+"/Level"+controller.Level+".png"))
        {
            EditorUtility.DisplayDialog("Stage 5","Cannot find "+ResourceFolder+
                "/Level"+controller.Level+".png. Install the reference-art pack first.","OK");
            return;
        }

        EnsureOriginalSprites();
        AssetDatabase.Refresh();
        var canvas=GameObject.Find("Stage5Level"+controller.Level+"Canvas");
        bool alreadyBaked=canvas!=null && FindChild(canvas.transform,
            "Stage5MappedTouchSurface")!=null;
        if(alreadyBaked)
        {
            EditorUtility.DisplayDialog("Stage 5","This scene already contains "+
                "the saved interactive UI. Edit its existing objects in the Hierarchy. "+
                "No duplicates were generated.","OK");
            Selection.activeGameObject=canvas;
            return;
        }

        // Save first so a cancel does not destroy any previous editor changes.
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;

        if(canvas==null)
        {
            controller.BuildEditableUI();
            canvas=GameObject.Find("Stage5Level"+controller.Level+"Canvas");
        }
        if(canvas==null)throw new InvalidOperationException("Stage 5 canvas could not be created.");

        Undo.RegisterCreatedObjectUndo(canvas,"Bake Stage 5 playable UI");
        if(controller.Level==1)controller.PolishStage5Level1Layout();
        controller.PolishStage5ReferenceLayout();

        var presentation=controller.GetComponent<Stage5ReferenceLayout>();
        if(presentation==null)
            presentation=Undo.AddComponent<Stage5ReferenceLayout>(controller.gameObject);

        if(!presentation.Initialize(controller.Level,canvas.transform,null))
        {
            Debug.LogError("CHEMQUEST Stage 5: Artwork or LabBoard was missing.");
            return;
        }

        PersistGeneratedSprites(canvas,controller.Level);

        // All runtime controllers remain serialized and in the scene. Only
        // the sample/result effects begin hidden; they appear after interaction.
        var parent=FindChild(canvas.transform,"ActualSampleOnHeater");
        if(parent!=null)parent.gameObject.SetActive(false);
        var pool=FindChild(canvas.transform,"HeatedSampleLiquidPool");
        if(pool!=null)pool.gameObject.SetActive(false);
        var shine=FindChild(canvas.transform,"HeatedSampleLiquidShine");
        if(shine!=null)shine.gameObject.SetActive(false);
        var preview=FindChild(canvas.transform,"ActiveExperimentCloseup");
        if(preview!=null)preview.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=canvas;
        EditorGUIUtility.PingObject(canvas);
        Debug.Log("CHEMQUEST: Saved Stage5Level"+controller.Level+
            " editable UI in "+scene.path+". Expand Stage5Level"+
            controller.Level+"Canvas in the Hierarchy.");
        EditorUtility.DisplayDialog("Stage 5 UI saved",
            "Level "+controller.Level+" artwork, heater, samples, notebook, "+
            "buttons, and mapped input are now serialized in this scene. "+
            "They stay in the Hierarchy BEFORE Play Mode. "+
            "Repeat for the other two Stage 5 scenes.","OK");
    }

    [MenuItem(MenuPath,true)]
    private static bool ValidateMenu()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void EnsureOriginalSprites()
    {
        for(int i=1;i<=3;i++)MakeSpriteAsset(ResourceFolder+"/Level"+i+".png");
        for(int i=0;i<3;i++)
            MakeSpriteAsset(ResourceFolder+"/DragSample"+i+".png");
        foreach(var file in new[]{"HeatMelt.png","StrikeShatter.png",
            "PressFlatten.png","ConductivityOn.png"})
            MakeSpriteAsset(ResourceFolder+"/"+file);
    }

    private static void MakeSpriteAsset(string assetPath)
    {
        if(!File.Exists(assetPath))return;
        var importer=AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if(importer==null)return;
        if(importer.textureType==TextureImporterType.Sprite)return;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.SaveAndReimport();
    }

    // Runtime-created oval and liquid sprites would otherwise disappear
    // after closing/reopening the editor, because they are not asset-backed.
    // Save them as actual PNG/Sprite assets and reassign their Image.sprite.
    private static void PersistGeneratedSprites(GameObject canvas,int level)
    {
        if(!AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            string current=ResourceFolder;
            AssetDatabase.CreateFolder(current,"GeneratedEditorSprites");
        }
        var converted=new Dictionary<Texture2D,Sprite>();
        int index=0;
        foreach(var image in canvas.GetComponentsInChildren<Image>(true))
        {
            if(image.sprite==null)continue;
            if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(image.sprite)))continue;
            var tex=image.sprite.texture;
            if(tex==null)continue;
            Texture2D key=tex;
            if(!converted.TryGetValue(key,out var persistent))
            {
                var filepath=GeneratedFolder+"/L"+level+"_"+(index++)+"_"+
                    Sanitize(image.gameObject.name)+".png";
                try
                {
                    File.WriteAllBytes(filepath,tex.EncodeToPNG());
                    AssetDatabase.ImportAsset(filepath,ImportAssetOptions.ForceSynchronousImport);
                    MakeSpriteAsset(filepath);
                    persistent=AssetDatabase.LoadAssetAtPath<Sprite>(filepath);
                    if(persistent!=null)converted[key]=persistent;
                }
                catch(Exception e)
                {
                    Debug.LogWarning("Stage 5: Sprite could not be saved: "+
                        filepath+" - "+e.Message);
                }
            }
            if(persistent!=null)image.sprite=persistent;
        }
    }

    private static string Sanitize(string name)
    {
        foreach(char c in Path.GetInvalidFileNameChars())
            name=name.Replace(c,'_');
        return name.Replace(' ','_');
    }

    private static Transform FindChild(Transform root,string name)
    {
        if(root==null)return null;
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
}
#endif
