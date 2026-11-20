using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Non-destructive, repeatable visual polish for the EXISTING Stage 1 Level 1
// Sub-Microscopic View. Does not relocate atoms or modify chemistry scripts.
public static class ChemQuestStage1AtomViewPolish
{
    private const string Path="Assets/Scenes/Stage1Level1.unity";
    private static readonly Color PanelColor=new Color(.065f,.155f,.26f,.93f);
    private static readonly Color GlowColor=new Color(1f,.78f,.25f,.35f);

    [MenuItem("Tools/ChemQuest/Stage 1/Polish Sub-Microscopic Atom and Bond UI")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stop Play Mode before polishing the saved scene.","OK");
            return;
        }
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Path)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stage1Level1.unity not found. No changes made.","OK");
            return;
        }
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene=EditorSceneManager.OpenScene(Path,OpenSceneMode.Single);
        RectTransform panel=Find(scene,"SubMicroscopicPanel");
        RectTransform bond1=Find(scene,"HClBond1");
        RectTransform bond2=Find(scene,"HClBond2");
        if(panel==null || bond1==null || bond2==null)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Could not find SubMicroscopicPanel, HClBond1 and HClBond2. No layout changes made. Check the scene hierarchy.","OK");
            return;
        }
        Undo.IncrementCurrentGroup();
        int group=Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("ChemQuest Atom View Visual Polish");
        int improved=0;
        // A moderately brighter panel keeps bonds legible over the dim lab.
        var bg=panel.GetComponent<Image>();
        if(bg!=null)
        {
            Undo.RecordObject(bg,"Brighten sub-microscopic panel");
            bg.color=PanelColor;
            // Keep the existing raycast setting: the panel may contain UI.
            EditorUtility.SetDirty(bg);
            improved++;
        }
        improved+=PolishBond(bond1);
        improved+=PolishBond(bond2);
        // Adjust ONLY labels inside the microscopic panel. Never resize the
        // draggable Hydrogen transforms or move the existing flask / button.
        foreach(var text in panel.GetComponentsInChildren<Text>(true))
        {
            if(text==null || text.name=="Label")continue;
            if(text.fontSize>=9 && text.fontSize<21)
            {
                Undo.RecordObject(text,"Improve atom-view text legibility");
                text.fontSize=Mathf.Min(24,text.fontSize+3);
                EditorUtility.SetDirty(text);improved++;
            }
        }
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if(text==null)continue;
            if(text.fontSize>=9 && text.fontSize<21)
            {
                Undo.RecordObject(text,"Improve microscopic UI text legibility");
                text.fontSize=Mathf.Min(24,text.fontSize+3);
                EditorUtility.SetDirty(text);improved++;
            }
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,Path);
        Selection.activeGameObject=panel.gameObject;
        EditorGUIUtility.PingObject(panel.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            "Sub-Microscopic View polished and saved to Stage1Level1.unity. "+
            "Updated "+improved+" visual elements. Expand Canvas > SubMicroscopicPanel to see HClBond1 and HClBond2 glow components in the Inspector. "+
            "Flask, Sub-Microscopic button, tutorial, scoring, and atom interactions were not moved or replaced. "+
            "Test H-Cl taps and hydrogen dragging in Play Mode.","OK");
    }

    private static int PolishBond(RectTransform bond)
    {
        // Outline the ORIGINAL interactive bond graphic, rather than creating
        // a clickable overlay that could swallow pointer events.
        var graphic=bond.GetComponent<Graphic>();
        if(graphic==null)graphic=bond.GetComponentInChildren<Graphic>(true);
        if(graphic==null)return 0;
        var outline=graphic.GetComponent<Outline>();
        if(outline==null)outline=Undo.AddComponent<Outline>(graphic.gameObject);
        Undo.RecordObject(outline,"Highlight the H-Cl bond");
        outline.effectColor=GlowColor;
        outline.effectDistance=new Vector2(3f,-3f);
        outline.useGraphicAlpha=true;
        EditorUtility.SetDirty(outline);
        // The Outline is serialized in the saved Hierarchy and never raycasts.
        return 1;
    }

    private static RectTransform Find(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
            foreach(var r in root.GetComponentsInChildren<RectTransform>(true))
                if(r.name==name)return r;
        return null;
    }
}
