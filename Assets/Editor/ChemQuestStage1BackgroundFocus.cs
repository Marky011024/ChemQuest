using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Stage 1 Level 1 only. Edits the ORIGINAL decorative background Image.
// It does not add any overlay or change the flask, zinc, buttons or tutorial.
public static class ChemQuestStage1BackgroundFocus
{
    private const string ScenePath="Assets/Scenes/Stage1Level1.unity";
    private static readonly Color FocusTint=new Color(.78f,.81f,.87f,1f);

    [MenuItem("Tools/ChemQuest/Stage 1/Dim Lab Background 22 Percent")]
    public static void DimBackground() { SetBackground(FocusTint,"22% dimmed"); }

    [MenuItem("Tools/ChemQuest/Stage 1/Restore Original Lab Background")]
    public static void RestoreBackground() { SetBackground(Color.white,"original brightness"); }

    private static void SetBackground(Color target,string description)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before changing the saved scene.","OK");
            return;
        }
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stage 1 Level 1 scene not found.","OK");
            return;
        }
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        Transform gameCanvas=null;
        foreach(var root in scene.GetRootGameObjects())
        {
            if(root.name=="Canvas" && root.GetComponent<Canvas>()!=null)
            {
                gameCanvas=root.transform;
                break;
            }
        }
        if(gameCanvas==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Main Canvas was not found. Nothing changed.","OK");
            return;
        }
        // Only tint the decorative scene artwork. Never tint a full Canvas or
        // add a blocking Image over gameplay, which would also darken controls.
        var bg=gameCanvas.Find("Background");
        Image backgroundImage=bg!=null?bg.GetComponent<Image>():null;
        if(backgroundImage==null)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Canvas/Background Image not found. No objects were changed.","OK");
            return;
        }
        Undo.RecordObject(backgroundImage,"ChemQuest lab background focus");
        backgroundImage.color=target;
        backgroundImage.raycastTarget=false;
        EditorUtility.SetDirty(backgroundImage);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,ScenePath);
        Selection.activeGameObject=backgroundImage.gameObject;
        EditorGUIUtility.PingObject(backgroundImage.gameObject);
        EditorUtility.DisplayDialog("ChemQuest",
            "Stage 1 Level 1 background set to "+description+
            ". Original flask, Sub-Microscopic View button, Prof Atom, energy, HUD and game interactions were not moved."+
            "\nYou can revert from Tools > ChemQuest > Stage 1 > Restore Original Lab Background.",
            "OK");
    }
}
