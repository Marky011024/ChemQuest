using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persists certificate layout adjustments as editable scene objects.
// Safe to reapply outside Play Mode; never modifies certificate scores,
// player preferences, student identity, or export files.
public static class ChemQuestCertificateLayoutFix
{
    [MenuItem("Tools/ChemQuest/Certificate/Fix Certificate UI Layout")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode",
                "Please stop Play Mode before saving the certificate layout.", "OK");
            return;
        }
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "ChemQuestCertificate")
        {
            EditorUtility.DisplayDialog("Open Certificate Scene",
                "Open Assets/Scenes/ChemQuestCertificate.unity first.", "OK");
            return;
        }
        var controller = Object.FindFirstObjectByType<ChemQuestCertificate>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Missing Certificate Controller",
                "Could not find the ChemQuestCertificate component in the scene.", "OK");
            return;
        }
        Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Polish Certificate Layout");
        controller.BuildEditableUI();
        controller.ApplyGameCertificateDesign();
        controller.EnsurePerformanceTable();
        controller.EnsureLockedUI();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CHEMQUEST certificate: transcript, footer and metadata layout saved in scene hierarchy.");
        EditorUtility.DisplayDialog("Certificate Layout Saved",
            "The certificate layout and all 15 editable transcript rows have been saved. Press Play to verify before exporting PNG/PDF.", "OK");
    }
}
