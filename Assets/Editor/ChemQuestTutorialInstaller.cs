using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestTutorialInstaller
{
    [MenuItem("Tools/ChemQuest/Tutorial/Install First-Time Tutorial - ALL 5 Stages")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest First-Time Tutorial",
            "Install the editable first-time tutorial on all existing Stage 1-5 gameplay scenes? Existing puzzles, scores and menus will not be replaced.",
            "Install Tutorial","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        int changed=0;
        for(int stage=1;stage<=5;stage++)
        {
            for(int level=1;level<=3;level++)
            {
                string path="Assets/Scenes/Stage"+stage+"Level"+level+".unity";
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
                {
                    Debug.LogWarning("ChemQuest tutorial: missing scene "+path);
                    continue;
                }
                Scene scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                GameObject host=null;
                foreach(var go in scene.GetRootGameObjects())
                {
                    if(go.name=="ChemQuestFirstTimeTutorial"){host=go;break;}
                }
                if(host==null)host=new GameObject("ChemQuestFirstTimeTutorial");
                var tutorial=host.GetComponent<ChemQuestFirstTimeTutorial>();
                if(tutorial==null)tutorial=host.AddComponent<ChemQuestFirstTimeTutorial>();
                tutorial.Stage=stage;
                tutorial.Level=level;
                tutorial.TutorialVersion="v1";
                tutorial.ShowOnFirstPlay=true;
                tutorial.BuildEditableUI();
                EditorUtility.SetDirty(tutorial);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene,path);
                changed++;
            }
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Stage1Level1.unity",OpenSceneMode.Single);
        EditorUtility.DisplayDialog("ChemQuest",
            "Tutorial installed in "+changed+" gameplay scenes. Open Stage 1 Level 1 and press Play to test. Every tutorial is editable before Play.",
            "OK");
    }

    [MenuItem("Tools/ChemQuest/Tutorial/Reset Tutorial Progress - For Testing")]
    public static void ResetTutorialProgress()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before resetting tutorial test flags.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("Reset Tutorial Status",
            "Reset ONLY the tutorial-seen flags for the Unity Editor's local PlayerPrefs? Level progress, energy, and scores will not be changed.",
            "Reset Tutorials","Cancel"))return;
        for(int stage=1;stage<=5;stage++)
            for(int level=1;level<=3;level++)
                PlayerPrefs.DeleteKey("ChemQuest_Tutorial_S"+stage+"_L"+level+"_v1");
        PlayerPrefs.Save();
        EditorUtility.DisplayDialog("ChemQuest",
            "Tutorial flags reset for the Editor. Re-enter Play Mode to see the introductory guides again.",
            "OK");
    }
}
