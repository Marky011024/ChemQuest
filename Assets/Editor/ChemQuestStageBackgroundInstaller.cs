#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Installs the five user-supplied stage artworks as full-scene UI backdrops
/// across the 15 ChemQuest levels without modifying any game controllers.
/// Uses the existing full-screen background Image where possible; otherwise
/// adds a non-interactive Image behind the level UI inside the main Canvas.
/// </summary>
public static class ChemQuestStageBackgroundInstaller
{
    private const string Root = "Assets/ChemQuest_Stage_Backgrounds/StageImages/";

    [MenuItem("Tools/ChemQuest/Stage Backgrounds/Apply to All 15 Levels")]
    public static void InstallAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest", "Exit Play Mode before applying backgrounds.", "OK");
            return;
        }

        // Import images as sprites so the installer works even if files were
        // copied into Assets from a regular ZIP instead of a Unity package.
        Sprite[] sprites = new Sprite[6];
        for (int stage = 1; stage <= 5; stage++)
        {
            string imagePath = Root + "Stage" + stage + "Background.png";
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null)
            {
                EditorUtility.DisplayDialog("ChemQuest", "Missing image: " + imagePath +
                    "\nCopy the entire ChemQuest_Stage_Backgrounds folder into Assets.", "OK");
                return;
            }
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = false;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            sprites[stage] = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (sprites[stage] == null)
            {
                EditorUtility.DisplayDialog("ChemQuest", "Unable to import sprite: " + imagePath, "OK");
                return;
            }
        }

        // Preserve work in current scene. Run on a clean/saved Git branch.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string previousScene = SceneManager.GetActiveScene().path;
        var outcomes = new List<string>();
        int updated = 0;
        int missing = 0;
        try
        {
            for (int stage = 1; stage <= 5; stage++)
            {
                for (int level = 1; level <= 3; level++)
                {
                    string scenePath = "Assets/Scenes/Stage" + stage + "Level" + level + ".unity";
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                    {
                        outcomes.Add("MISSING: " + scenePath);
                        missing++;
                        continue;
                    }
                    try
                    {
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        if (!ApplyToScene(scene, sprites[stage]))
                        {
                            outcomes.Add("NO CANVAS: " + scene.name);
                            continue;
                        }
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        outcomes.Add("UPDATED: " + scene.name + " -> Stage " + stage + " artwork");
                        updated++;
                    }
                    catch (Exception ex)
                    {
                        outcomes.Add("ERROR: " + scenePath + " -> " + ex.Message);
                        Debug.LogException(ex);
                    }
                }
            }
        }
        finally
        {
            // Restore the originally opened scene, rather than leaving the
            // editor on the last stage after the batch operation.
            if (!string.IsNullOrEmpty(previousScene) &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScene) != null)
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }
        Debug.Log("CHEMQUEST STAGE BACKGROUNDS\n" + string.Join("\n", outcomes));
        EditorUtility.DisplayDialog("ChemQuest Stage Backgrounds",
            "Applied artworks to " + updated + " / 15 levels. Missing scenes: " + missing +
            ".\nSee Console for the scene-by-scene report.\n\n" +
            "Test one level from each stage to verify gameplay UI visibility.", "OK");
    }

    private static bool ApplyToScene(Scene scene, Sprite sprite)
    {
        // Match the stage's primary UI canvas (not the HUD overlay canvas).
        Canvas selected = null;
        float largest = -1f;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                // Prefer the main level Canvas over a secondary HUD canvas.
                float size = Mathf.Abs(canvas.GetComponent<RectTransform>()?.rect.width ?? 0f) *
                             Mathf.Abs(canvas.GetComponent<RectTransform>()?.rect.height ?? 0f);
                if (canvas.name.IndexOf("HUD", StringComparison.OrdinalIgnoreCase) >= 0)
                    size -= 100000000f;
                if (selected == null || size > largest)
                {
                    selected = canvas;
                    largest = size;
                }
            }
        }
        if (selected == null) return false;

        // If an existing full-screen background Image is present, simply
        // replace its sprite. This retains original layering and interactables.
        Image existing = FindBackgroundImage(selected);
        if (existing != null)
        {
            existing.sprite = sprite;
            existing.type = Image.Type.Simple;
            existing.preserveAspect = false;
            existing.color = Color.white;
            existing.raycastTarget = false;
            EditorUtility.SetDirty(existing);
            Debug.Log("ChemQuest: replaced existing background Image in " + scene.name);
            return true;
        }

        Transform existingObject = selected.transform.Find("ChemQuestStageArtworkBackground");
        Image image;
        RectTransform rect;
        if (existingObject != null)
        {
            image = existingObject.GetComponent<Image>();
            rect = existingObject.GetComponent<RectTransform>();
        }
        else
        {
            var go = new GameObject("ChemQuestStageArtworkBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(selected.transform, false);
            image = go.GetComponent<Image>();
            rect = go.GetComponent<RectTransform>();
        }
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.SetAsFirstSibling();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = Color.white;
        image.raycastTarget = false; // Never obstruct drag/drop or buttons
        return true;
    }

    private static Image FindBackgroundImage(Canvas canvas)
    {
        Image best = null;
        float bestArea = 0f;
        var canvasRect = canvas.GetComponent<RectTransform>();
        foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
        {
            string name = image.name.ToLowerInvariant();
            if (name != "background" && name != "backgroundpanel" &&
                name != "backgroundimage" && name != "gamebackground" &&
                name != "chemqueststageartworkbackground") continue;
            var rect = image.GetComponent<RectTransform>();
            if (rect == null) continue;
            float area = Mathf.Abs(rect.rect.width * rect.rect.height);
            // A named background is replaced only when it is a large panel,
            // not the tiny background inside a button, title, or puzzle board.
            float canvasArea = canvasRect == null ? 0f :
                Mathf.Abs(canvasRect.rect.width * canvasRect.rect.height);
            if (canvasArea > 0 && area < canvasArea * 0.5f) continue;
            if (area > bestArea) { best = image; bestArea = area; }
        }
        return best;
    }
}
#endif
