using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChemQuestStage3Level2Builder
{
    [MenuItem("Tools/ChemQuest/Build Stage 3 Level 2")]
    public static void Build()
    {
        const string scenePath="Assets/Scenes/Stage3Level2.unity";
        if(!EditorUtility.DisplayDialog("Build ChemQuest Stage 3 Level 2",
           "This will add the NH3 game controller to Stage3Level2. The current scene will be saved first. Continue?","Build","Cancel"))return;
        EditorSceneManager.SaveOpenScenes();
        Scene scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
        var controller=GameObject.Find("Stage3Level2AmmoniaController");
        if(controller==null)controller=new GameObject("Stage3Level2AmmoniaController");
        var game=controller.GetComponent<Stage3Level2AmmoniaGame>();
        if(game==null) game=controller.AddComponent<Stage3Level2AmmoniaGame>();

        var energySprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Energy.png");
        if(energySprite==null)
        {
            Debug.LogError("ChemQuest: Assets/Art/Energy.png was not found or is not imported as Sprite.");
            EditorUtility.DisplayDialog("Missing Stage 1 energy image",
                "Check Assets/Art/Energy.png and its Texture Type (Sprite).", "OK");
            return;
        }
        var serializedGame=new SerializedObject(game);
        serializedGame.FindProperty("energySprite").objectReferenceValue=energySprite;
        serializedGame.FindProperty("stage1PauseButtonPrefab").objectReferenceValue=
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        serializedGame.FindProperty("stage1PauseMenuPrefab").objectReferenceValue=
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        serializedGame.ApplyModifiedPropertiesWithoutUndo();
        // Register navigation destinations without replacing any existing scenes.
        string[] destinations = {
            "Assets/Scenes/StageSelection.unity",
            "Assets/Scenes/Stage3LevelSelection.unity",
            "Assets/Scenes/Stage3Level1.unity",
            "Assets/Scenes/Stage3Level2.unity",
            "Assets/Scenes/Stage3Level3.unity"
        };
        var buildScenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        foreach(string destination in destinations)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(destination)==null)
            {
                Debug.LogError("ChemQuest: Missing scene "+destination);
                continue;
            }
            int index=buildScenes.FindIndex(s=>s.path==destination);
            if(index<0)
                buildScenes.Add(new EditorBuildSettingsScene(destination,true));
            else if(!buildScenes[index].enabled)
                buildScenes[index]=new EditorBuildSettingsScene(destination,true);
        }
        EditorBuildSettings.scenes=buildScenes.ToArray();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("ChemQuest Stage 3 Level 2 ready. Enter Play Mode to generate the interactive NH3 UI.");
        EditorUtility.DisplayDialog("ChemQuest","Stage3Level2 created. Open the scene and press Play. All puzzle UI is generated automatically at runtime.","OK");
    }
}
