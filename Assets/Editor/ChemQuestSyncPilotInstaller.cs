#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Safe editor-only helper. Adds a pilot to the currently open scene; never edits
// graded pre/post-test scripts or commits to permanent changes without a Save.
public static class ChemQuestSyncPilotInstaller
{
    [MenuItem("ChemQuest/Firebase/Set Up Question Sync Pilot")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("ChemQuest Sync Pilot", "Exit Play Mode before setup.", "OK");
            return;
        }
        var existing = Object.FindFirstObjectByType<ChemQuestQuestionSyncPilot>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            EditorUtility.DisplayDialog("ChemQuest Sync Pilot", "Pilot object already exists. Select it and configure the Web API key in Inspector.", "OK");
            return;
        }
        var go = new GameObject("FirebaseSyncPilot");
        Undo.RegisterCreatedObjectUndo(go, "Add ChemQuest Firebase sync pilot");
        var pilot = Undo.AddComponent<ChemQuestQuestionSyncPilot>(go);
        var serialized = new SerializedObject(pilot);
        var onStart = serialized.FindProperty("runOnStart");
        if (onStart != null) onStart.boolValue = true;
        serialized.ApplyModifiedProperties();
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
        EditorUtility.DisplayDialog("ChemQuest Sync Pilot",
            "FirebaseSyncPilot added to the CURRENT scene. Set the Firebase Web API key in Inspector, save the scene, and enter Play Mode. The pilot only reads syncPilot/demo; it does not change gameplay.", "OK");
    }
}
#endif
