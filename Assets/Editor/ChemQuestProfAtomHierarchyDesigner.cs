using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-click, edit-time installation. All visual objects are serialized into
// Stage1Level1.unity and stay visible in Hierarchy before pressing Play.
// Never writes puzzle objects, scoring scripts, Firebase settings or PlayerPrefs.
public static class ChemQuestProfAtomHierarchyDesigner
{
    private const string ScenePath="Assets/Scenes/Stage1Level1.unity";
    private const string HostName="ProfAtom_Stage1Level1_InteractiveGuide";

    [MenuItem("Tools/ChemQuest/Stage 1/Install Prof Atom Editable Tutorial UI")]
    public static void Install()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest","Please exit Play Mode first.","OK");
            return;
        }
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
        {
            EditorUtility.DisplayDialog("ChemQuest","Stage1Level1 scene not found at "+ScenePath,"OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest Prof Atom UI",
            "Build the tutorial panel, Professor Atom portrait, Step labels, Skip/Next buttons, hand guide, target glow and drag path as SAVED scene objects? Existing gameplay, energy, scoring, and syncing will be preserved.",
            "Install Editable UI","Cancel"))return;

        EditorSceneManager.SaveOpenScenes();
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        GameObject host=null;
        foreach(var root in scene.GetRootGameObjects())
        {
            if(root.name==HostName){host=root;break;}
        }
        if(host==null)
        {
            host=new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host,scene);
        }
        var tutorial=host.GetComponent<Stage1Level1NpcTutorial>();
        if(tutorial==null)tutorial=host.AddComponent<Stage1Level1NpcTutorial>();
        tutorial.BakeEditableHierarchy();
        EditorUtility.SetDirty(host);
        EditorUtility.SetDirty(tutorial);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,ScenePath);

        Selection.activeGameObject=host;
        EditorGUIUtility.PingObject(host);
        EditorUtility.DisplayDialog("ChemQuest",
            "Prof Atom tutorial has been SAVED inside Stage1Level1.unity. Expand '"+
            HostName+"' > ProfAtomGuideCanvas > InGameTutorialLayer in Hierarchy. "+
            "All panel elements exist before Play; runtime binds the existing objects and does NOT duplicate them.",
            "OK");
    }
}
