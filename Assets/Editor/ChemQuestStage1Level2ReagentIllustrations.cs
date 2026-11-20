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
        var cu = CreateImage("CopperSulfateBlueCrystals.png", false);
        var na = CreateImage("SodiumHydroxideWhitePellets.png", true);
        if (cu == null || na == null)
        { EditorUtility.DisplayDialog("ChemQuest", "Image import failed; scene was not modified.", "OK"); return; }
        Polish(copper, cu);
        Polish(sodium, na);
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

    static Sprite CreateImage(string file, bool sodium)
    {
        string path = Folder + "/" + file;
        if (!File.Exists(path))
        {
            var tex = new Texture2D(N,N,TextureFormat.RGBA32,false);
            var data = new Color32[N*N];
            for (int y=0; y<N; y++)
            for (int x=0; x<N; x++)
            {
                Color32 p = new Color32(0,0,0,0);
                bool neck = y >= 196 && y <= 224 && x >= 98 && x <= 158;
                bool cap = y >= 214 && y <= 239 && x >= 92 && x <= 164;
                bool body = y >= 45 && y <= 197 && x >= 58 && x <= 198;
                if (body && (x < 65 || x > 191 || y < 52)) p = new Color32(111,207,232,230);
                else if (body) p = new Color32(207,237,240,118);
                if (neck) p = new Color32(140,205,226,230);
                if (cap) p = new Color32(33,56,86,255);
                // Visible contents: blue faceted copper sulfate crystals,
                // or small white sodium hydroxide pellets.
                if (body && x>=69 && x<=187 && y>=62 && y<=150)
                {
                    int u=x-69,v=y-62,cell=23;
                    int ix=u/cell, iy=v/cell, lx=u%cell, ly=v%cell;
                    int skew=((ix*7 + iy*11)%9)-4;
                    int d=(lx-11-skew)*(lx-11-skew)+(ly-11)*(ly-11);
                    bool solid = sodium ? d<67 : (Mathf.Abs(lx-11-skew)+Mathf.Abs(ly-11) < 16);
                    if (solid)
                    {
                        int light=(x+y+ix*3)%19;
                        p = sodium
                            ? new Color32((byte)(225+light),(byte)(231+light),(byte)(236+light),255)
                            : new Color32((byte)(27+light),(byte)(113+light*3),(byte)(199+light*2),255);
                    }
                }
                // Clean solid label strip integrated into the picture.
                if (body && y>=154 && y<=190 && x>=67 && x<=189)
                    p = sodium ? new Color32(45,93,97,249) : new Color32(31,77,140,249);
                // Bright bottle highlight.
                if (body && x>=70 && x<=75 && y>=80 && y<=147)
                    p = new Color32(243,251,255,165);
                data[y*N+x] = p;
            }
            tex.SetPixels32(data);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
        {
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
