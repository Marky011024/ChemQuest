#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Reskins EXISTING HUD elements, never overlays a second HUD canvas.
// Stage 5's original reference HUD is deliberately left unchanged.
public static class ChemQuestExistingHUDStyle
{
    private static readonly Color DeepBlue = new Color32(4, 29, 60, 255);
    private static readonly Color ElectricBlue = new Color32(45, 217, 255, 255);
    private static readonly Color ScoreGold = new Color32(255, 204, 85, 255);
    private static readonly Color BorderGold = new Color32(255, 201, 90, 255);

    [MenuItem("ChemQuest/HUD/Style Existing HUDs (Stages 1-4, No Duplicates)")]
    private static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play","Stop Play Mode before modifying saved UI.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Style Existing ChemQuest HUDs",
            "Update the existing Pause, Energy and Score visuals in Stages 1-4. "+
            "Stage 5 is not touched. No duplicate HUDs or new gameplay scripts "+
            "will be added. This saves the scenes; back up or commit your scenes first.",
            "Apply style","Cancel")) return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var original=SceneManager.GetActiveScene().path;
        int styled=0, skipped=0;
        var ids=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"});
        try
        {
            for(int i=0;i<ids.Length;i++)
            {
                var path=AssetDatabase.GUIDToAssetPath(ids[i]);
                var name=System.IO.Path.GetFileNameWithoutExtension(path);
                var match=Regex.Match(name,@"^Stage([1-4])Level([1-9]\d*)$");
                if(!match.Success) continue;
                EditorUtility.DisplayProgressBar("Style ChemQuest HUD",name,
                    (float)i/Mathf.Max(1,ids.Length));
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                if(StyleScene(scene))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    styled++;
                }
                else skipped++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if(!string.IsNullOrEmpty(original))
                EditorSceneManager.OpenScene(original,OpenSceneMode.Single);
        }
        EditorUtility.DisplayDialog("ChemQuest HUD styling",
            "Styled "+styled+" scenes; "+skipped+" scenes had no compatible "+
            "HUD objects. Existing gameplay, score and energy systems were not changed. "+
            "Inspect Scene and Game view before committing.","OK");
    }

    private static bool StyleScene(Scene scene)
    {
        bool changed=false;
        foreach(var root in scene.GetRootGameObjects())
        {
            // Never alter duplicate UI as a workaround. The previous
            // ChemQuestUnifiedHUD should be removed with Restore Original HUD.
            if(root.name=="ChemQuestUnifiedHUD") continue;
            foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if(graphic==null) continue;
                string name=graphic.name.ToLowerInvariant();
                if(graphic is TMP_Text label)
                {
                    bool score=(name=="scoretext" || name=="score" ||
                        name=="level3scoretext" || name=="scorevalue");
                    bool energy=(name=="energytext" || name=="energycount" ||
                        name=="energytitle" || name=="energylabel");
                    if(score)
                    {
                        label.color=ScoreGold;
                        label.fontStyle=FontStyles.Bold;
                        changed=true;
                    }
                    if(energy)
                    {
                        label.color=Color.white;
                        label.fontStyle=FontStyles.Bold;
                        changed=true;
                    }
                }
                else if(graphic is Image image)
                {
                    var boltMatch=Regex.Match(name,@"^energy([1-5])$");
                    if(boltMatch.Success)
                    {
                        // Keep each level's original lightning Sprite and
                        // animation, but use the same electric-blue palette.
                        image.color=ElectricBlue;
                        image.preserveAspect=true;
                        changed=true;
                    }
                    else if(name=="energypanel" || name=="scorepanel")
                    {
                        image.color=DeepBlue;
                        changed=true;
                    }
                }
            }

            foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                if(button==null)continue;
                string n=button.name.ToLowerInvariant();
                if(n!="pausebutton" && n!="menubutton" &&
                    n!="stage2pausebutton" && n!="stage3pausebutton" &&
                    n!="stage4pausebutton") continue;
                var face=button.targetGraphic as Image;
                if(face==null)face=button.GetComponent<Image>();
                if(face==null)continue;
                // Only change appearance. Keep Button.onClick and transform.
                face.color=DeepBlue;
                var outline=face.GetComponent<Outline>();
                if(outline==null)outline=face.gameObject.AddComponent<Outline>();
                outline.effectColor=BorderGold;
                outline.effectDistance=new Vector2(2f,-2f);
                outline.useGraphicAlpha=false;
                outline.enabled=true;
                changed=true;
            }
        }
        return changed;
    }
}
#endif
