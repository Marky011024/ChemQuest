using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestTutorialInstaller
{
    [MenuItem("Tools/ChemQuest/Tutorial/Install Tutorials - Level 1 of Each Stage Only")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest First-Time Tutorial",
            "Install tutorials ONLY on Stage 1-5 Level 1? Tutorials already installed in Levels 2 and 3 will be removed. Puzzles, scores, and menus are preserved.",
            "Install Tutorial","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        int changed=0;
        int removed=0;
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
                if(level!=1)
                {
                    if(host!=null)
                    {
                        Object.DestroyImmediate(host);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene,path);
                        removed++;
                    }
                    continue;
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
            "Tutorial installed in "+changed+" Level 1 scenes. Removed "+removed+" old tutorials from Levels 2 and 3. Open Stage 1 Level 1 and press Play to test.",
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
