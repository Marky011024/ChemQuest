using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Edit-time only: moves the REAL Stage1Level1 button and reaction flask.
// Saves everything inside Stage1Level1.unity; no extra duplicate access button.
public static class ChemQuestMicroscopeLayoutFix
{
    private const string ScenePath="Assets/Scenes/Stage1Level1.unity";

    [MenuItem("Tools/ChemQuest/Stage 1/Fix Sub-Microscopic Button and Flask Positions")]
    public static void Fix()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stop Play Mode first.","OK");
            return;
        }
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Missing scene: "+ScenePath,"OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        var button=FindRect(scene,"SubMicroscopicButton");
        var flask=FindRect(scene,"ReactionFlask");
        var tutorial=FindRect(scene,"ProfAtomDialogueBar");

        if(button==null||flask==null)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Could not find the saved "+(button==null?"SubMicroscopicButton ":"")+
                (flask==null?"ReactionFlask ":"")+
                "in Stage1Level1 Hierarchy. No movement applied. "+
                "Check the scene hierarchy or send a screenshot.","OK");
            return;
        }
        var marker=button.GetComponent<ChemQuestMicroscopeLayoutMarker>();
        if(marker==null)marker=Undo.AddComponent<ChemQuestMicroscopeLayoutMarker>(button.gameObject);
        bool first=!marker.applied;
        if(first)
        {
            Undo.RecordObject(marker,"Record original microscope layout");
            marker.originalButtonPosition=button.anchoredPosition;
            marker.originalFlaskPosition=flask.anchoredPosition;
            marker.applied=true;
            Undo.RecordObject(button,"Raise original microscope button");
            Undo.RecordObject(flask,"Raise reaction flask");
            // Both positions are relative to their own RectTransforms.
            // Move the real button and flask without changing anchors or input events.
            button.anchoredPosition+=new Vector2(0,110);
            flask.anchoredPosition+=new Vector2(0,45);
        }

        // Remove the redundant tutorial shortcut created by an earlier version.
        // The tutorial binds to the original SubMicroscopicButton.
        var shortcut=FindRect(scene,"SubMicroscopicAccessButton");
        if(shortcut!=null)
            Undo.DestroyObjectImmediate(shortcut.gameObject);

        EditorUtility.SetDirty(marker);
        EditorUtility.SetDirty(button);
        EditorUtility.SetDirty(flask);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Selection.activeGameObject=button.gameObject;
        EditorGUIUtility.PingObject(button.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            (first?"Moved the ORIGINAL SubMicroscopicButton up by 110 UI units and the ReactionFlask by 45 UI units.":"Existing saved layout retained (no double movement).")+
            "\nDuplicate shortcut removed if present. UI saved in Stage1Level1.unity."+
            "\nCheck 16:9 Game View and ensure the button does not overlap the flask or tutorial."+
            "\nIf artwork still overlaps, adjust these two saved RectTransforms with the Inspector.",
            "OK");
    }

    private static RectTransform FindRect(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            foreach(var rect in root.GetComponentsInChildren<RectTransform>(true))
                if(rect.name==name)return rect;
        }
        return null;
    }
}
