using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Creates the real editable certificate scene in the Hierarchy.
public static class ChemQuestCertificateInstaller
{
    [MenuItem("Tools/ChemQuest/Certificate/Build Chemical Mastery Certificate")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before installation.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest Certificate",
           "Build the certificate scene and register it in Build Settings? Existing levels will NOT be rebuilt or deleted.",
           "Build Certificate","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        string path="Assets/Scenes/ChemQuestCertificate.unity";
        Scene scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null?
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single):
            EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        GameObject host=GameObject.Find("ChemQuestCertificateController");
        if(host==null)host=new GameObject("ChemQuestCertificateController");
        var controller=host.GetComponent<ChemQuestCertificate>();
        if(controller==null)controller=host.AddComponent<ChemQuestCertificate>();
        controller.BuildEditableUI();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,path);
        var list=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int idx=list.FindIndex(s=>s.path==path);
        if(idx<0)list.Add(new EditorBuildSettingsScene(path,true));
        else list[idx]=new EditorBuildSettingsScene(path,true);
        EditorBuildSettings.scenes=list.ToArray();
        EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        EditorUtility.DisplayDialog("ChemQuest",
           "Created ChemQuestCertificate scene. It stays LOCKED until all fifteen levels are completed. Export buttons save PNG or PDF to app-local storage.",
           "OK");
    }
}
