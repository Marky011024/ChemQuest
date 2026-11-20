using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Saved scene visual update. Never substitutes or renames the existing draggable
// GameObjects: Level2ReactionDropZone checks their names exactly.
public static class ChemQuestStage1Level2ReagentIllustrations
{
    const string ScenePath = "Assets/Scenes/Stage1Level2.unity";
    const string Folder = "Assets/Art/ChemQuestReagents";
    const int N = 256;

    [MenuItem("Tools/ChemQuest/Stage 1/Level 2 - Install Reagent Bottle Pictures")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorUtility.DisplayDialog("ChemQuest", "Stop Play Mode first.", "OK"); return; }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
        { EditorUtility.DisplayDialog("ChemQuest", "Stage1Level2 scene not found.", "OK"); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var copper = Find(scene, "CopperSulfateContainer");
        var sodium = Find(scene, "SodiumHydroxideContainer");
        if (copper == null || sodium == null)
        { EditorUtility.DisplayDialog("ChemQuest", "The original CopperSulfateContainer and SodiumHydroxideContainer were not found. No changes made.", "OK"); return; }

        if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "ChemQuestReagents");
        var cu = LoadProvidedImage("CopperSulfateBlueCrystals.png");
        var na = LoadProvidedImage("SodiumHydroxideWhitePellets.png");
        if (cu == null || na == null)
        { EditorUtility.DisplayDialog("ChemQuest", "Copy the TWO supplied PNG files into Assets/Art/ChemQuestReagents first. Scene unchanged.", "OK"); return; }
        Polish(copper, cu);
        Polish(sodium, na);
        var beaker = Find(scene, "ReactionBeaker");
        if (beaker == null) Debug.LogWarning("ChemQuest: ReactionBeaker not found. Looking for Level2ReactionDropZone on other scene objects.");
        Level2ReactionDropZone drop = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            drop = root.GetComponentInChildren<Level2ReactionDropZone>(true);
            if (drop != null) break;
        }
        if (drop != null && drop.GetComponent<ChemQuestReagentPourEffect>() == null)
            Undo.AddComponent<ChemQuestReagentPourEffect>(drop.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = copper.gameObject;
        EditorGUIUtility.PingObject(copper.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            "Two illustrated reagent pictures installed in the saved Stage1Level2 scene. The original draggable objects, sizes, positions and drop-zone scripts were preserved. Expand each Container > ReagentIllustration to edit the sprites. Test both drag-and-drop actions in Play Mode.", "OK");
    }

    static RectTransform Find(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<RectTransform>(true))
                if (t.name == name) return t;
        return null;
    }

    static void Polish(RectTransform parent, Sprite sprite)
    {
        var child = parent.Find("ReagentIllustration");
        if (child == null)
        {
            var go = new GameObject("ReagentIllustration", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create reagent picture");
            go.transform.SetParent(parent, false);
            child = go.transform;
        }
        var rt = (RectTransform)child;
        var img = child.GetComponent<Image>();
        Undo.RecordObject(rt, "Fit reagent image");
        Undo.RecordObject(img, "Assign illustrated reagent image");
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
        rt.pivot = new Vector2(.5f, .5f);
        rt.anchoredPosition = new Vector2(0, 14);
        rt.sizeDelta = new Vector2(142, 157);
        rt.localScale = Vector3.one;
        img.sprite = sprite;
        img.color = Color.white;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.raycastTarget = false;
        // The original Image remains the drag hit area. Only its plain fill is hidden.
        var original = parent.GetComponent<Image>();
        if (original != null)
        {
            Undo.RecordObject(original, "Replace flat reagent visual");
            var c = original.color;
            original.color = new Color(c.r,c.g,c.b,0f);
            // Do NOT disable its raycast target or its DraggableReagent component.
        }
        child.SetAsFirstSibling(); // Keep existing chemical-name labels in front of artwork.
        EditorUtility.SetDirty(img);
    }

    static Sprite LoadProvidedImage(string name)
    {
        string path = Folder + "/" + name;
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
