using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestReactionEffectsInstaller
{
    [MenuItem("Tools/ChemQuest/Effects/Install Animated Reactions - Stage 2 and 5")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode before installing effects.","OK");
            return;
        }
        if(!EditorUtility.DisplayDialog("ChemQuest Animations",
            "Add non-blocking editable animations to Stage 2 and Stage 5 levels? Existing controllers, energy, scoring, and menus will be preserved.",
            "Install Effects","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        int updated=0;
        foreach(int stage in new[]{2,5})
        {
            for(int level=1;level<=3;level++)
            {
                string path="Assets/Scenes/Stage"+stage+"Level"+level+".unity";
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
                {
                    Debug.LogWarning("Missing scene: "+path);
                    continue;
                }
                Scene scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                GameObject host=GameObject.Find("ChemQuestReactionEffects");
                if(host==null)host=new GameObject("ChemQuestReactionEffects");
                var fx=host.GetComponent<ChemQuestReactionEffects>();
                if(fx==null)fx=host.AddComponent<ChemQuestReactionEffects>();
                fx.Stage=stage;fx.Level=level;
                fx.BuildEditableUI();
                EditorUtility.SetDirty(fx);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene,path);
                updated++;
            }
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Stage2Level1.unity",OpenSceneMode.Single);
        EditorUtility.DisplayDialog("ChemQuest",
            "Animations installed in "+updated+" gameplay scenes. Open Stage 2 Level 1 and test electron drag; then Stage 5 for test feedback. No scenes rebuilt.",
            "OK");
    }
}
