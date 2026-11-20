using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Level 3 visual-only polish: exactly matches Level 1 lab background darkness.
// Preserves existing bubble button, reactions, puzzles, scores and event bindings.
public static class ChemQuestStage1Level3VisualPolish
{
    const string ScenePath = "Assets/Scenes/Stage1Level3.unity";
    static readonly Color Level1Background = new Color(.78f,.81f,.87f,1f);
    static readonly Color Level1Overlay = new Color(.018f,.045f,.105f,.40f);
    [MenuItem("Tools/ChemQuest/Stage 1/Level 3 - Match Dark Lab and Polish CO2 Button")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorUtility.DisplayDialog("ChemQuest","Stop Play Mode before editing the saved Level 3 scene.","OK"); return; }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
        { EditorUtility.DisplayDialog("ChemQuest","Stage1Level3.unity not found.","OK"); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        Transform canvas=null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name=="Canvas" && root.GetComponent<Canvas>()!=null) {canvas=root.transform;break;}
        var bg=canvas!=null?canvas.Find("Background"):null;
        var bgImage=bg!=null?bg.GetComponent<Image>():null;
        if(bgImage==null)
        { EditorUtility.DisplayDialog("ChemQuest","Canvas/Background missing. Nothing changed.","OK");return;}
        Undo.RecordObject(bgImage,"Match Stage 1 dark background");
        bgImage.color=Level1Background;
        bgImage.raycastTarget=false;
        EditorUtility.SetDirty(bgImage);
        var overlay=canvas.Find("StageGameplayDimOverlay");
        if(overlay!=null)
        {
            var im=overlay.GetComponent<Image>();
            if(im!=null)
            {
                Undo.RecordObject(im,"Match Stage 1 overlay brightness");
                im.color=Level1Overlay;
                im.raycastTarget=false;
                EditorUtility.SetDirty(im);
            }
        }
        var bubble=Find(scene,"BubbleCO2Button");
        if(bubble!=null)
        {
            var img=bubble.GetComponent<Image>();
            if(img!=null)
            {
                Undo.RecordObject(img,"Increase CO2 button contrast");
                img.color=new Color(.045f,.40f,.67f,1f);
                EditorUtility.SetDirty(img);
            }
            foreach(var text in bubble.GetComponentsInChildren<TMP_Text>(true))
            {
                Undo.RecordObject(text,"Readable BUBBLE CO2 text");
                text.color=Color.white;
                text.fontSize=24;
                text.enableAutoSizing=true;
                text.fontSizeMin=17;
                text.fontSizeMax=24;
                text.alignment=TextAlignmentOptions.Center;
                text.raycastTarget=false;
                EditorUtility.SetDirty(text);
            }
            foreach(var text in bubble.GetComponentsInChildren<Text>(true))
            {
                Undo.RecordObject(text,"Readable BUBBLE CO2 text");
                text.color=Color.white;
                text.fontSize=22;
                text.alignment=TextAnchor.MiddleCenter;
                text.raycastTarget=false;
                EditorUtility.SetDirty(text);
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Selection.activeGameObject=bg.gameObject;
        EditorGUIUtility.PingObject(bg.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 1 Level 3 saved: laboratory background matches Level 1 darkness, BUBBLE CO2 button text has better contrast. Existing CO2 animation, beaker, reaction and puzzle mechanics were not modified. Test in Play Mode.","OK");
    }
    static RectTransform Find(Scene scene,string name)
    {
        foreach(var root in scene.GetRootGameObjects())
            foreach(var t in root.GetComponentsInChildren<RectTransform>(true))
                if(t.name==name)return t;
        return null;
    }
}
