#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Stage 5 Level 3: an editable-scene, repeatable presentation-only layout fix.
/// Keeps the conductivity probe, sample classifications, energy, and scoring intact.
/// </summary>
public static class Stage5Level3LayoutFix
{
    [MenuItem("Tools/ChemQuest/Stage 5/Fix Level 3 UI Layout")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode",
                "Exit Play Mode before updating the scene.", "OK");
            return;
        }
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Stage5Level3")
        {
            EditorUtility.DisplayDialog("Open Stage5Level3",
                "Open the Stage5Level3 scene first.", "OK");
            return;
        }
        var canvas = GameObject.Find("Stage5Level3Canvas");
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Missing canvas",
                "Stage5Level3Canvas was not found. Generate/save the editable scene UI first.", "OK");
            return;
        }
        var root = canvas.transform;
        var board = Find(root, "LabBoard");
        if (board == null)
        {
            EditorUtility.DisplayDialog("Missing workbench",
                "LabBoard was not found. No changes were made.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Fix Stage 5 Level 3",
            "Rearrange Level 3 UI without changing gameplay scripts. Save a backup first. Apply now?",
            "Apply", "Cancel")) return;

        Undo.RegisterFullObjectHierarchyUndo(canvas, "Stage 5 Level 3 layout polish");
        Place(board, 0, -5, 1080, 455);
        Hide(root, "StageText");
        Hide(root, "BoardTitle");

        // Put heading *inside* the board and above the content so the board
        // cannot occlude it (the original screenshot shows title overlap).
        MoveInsideBoard(root, board, "LevelTitle", 0, 190, 1000, 44,
            "SOLUTION & PHASE CONDUCTIVITY", 28);
        MoveInsideBoard(root, board, "Instructions", 0, 152, 1000, 38,
            "Test each sample in SOLID and MOLTEN states. Test AQUEOUS when applicable, then classify.", 16);

        // Larger evidence column and smaller font prevent the long notes from
        // running outside their panel. No notebook content is discarded.
        Place(board, "SampleBank", -377, -35, 225, 260);
        Place(board, "TestStation", -45, -33, 405, 262);
        Place(board, "EvidenceBoard", 347, -30, 305, 282);
        Place(root, "MenuButton", -535, 295, 112, 42);

        var evidence = Find(root, "EvidenceText");
        if (evidence != null)
        {
            Place(evidence, 0, -12, 286, 222);
            SetText(evidence, null, 12);
            var tmp = evidence.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
                tmp.alignment = TextAlignmentOptions.TopLeft;
            }
        }
        Place(root, "EvidenceTitle", 0, 123, 285, 32);
        SetText(Find(root, "EvidenceTitle"), "LAB NOTEBOOK", 17);

        // One row of four experiments, one separate classification row.
        Place(root, "SolidButton", -365, -245, 157, 42);
        Place(root, "MoltenButton", -190, -245, 157, 42);
        Place(root, "AqueousButton", -15, -245, 157, 42);
        Place(root, "ProbeButton", 214, -245, 240, 42);

        Place(root, "Progress", 0, -207, 760, 30);
        SetText(Find(root, "Progress"), null, 16);
        Place(root, "Bin0", -350, -296, 265, 49);
        Place(root, "Bin1", -56, -296, 265, 49);
        Place(root, "Bin2", 237, -296, 265, 49);
        foreach (var name in new[] { "Bin0", "Bin1", "Bin2" })
        {
            var bin = Find(root, name);
            if (bin != null) SetText(Find(bin, "Label"), null, 14);
        }

        Place(root, "SubmitButton", 475, -296, 180, 49);
        SetText(Find(root, "SubmitButton"), "CHECK CLASSIFICATION", 13);

        // Readable bottom feedback strip with non-interactive background.
        var strip = Find(root, "Level3FeedbackStrip");
        if (strip == null)
        {
            var go = new GameObject("Level3FeedbackStrip",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(root, false);
            strip = go.transform;
            var image = go.GetComponent<Image>();
            image.color = new Color32(8, 29, 51, 245);
            image.raycastTarget = false;
        }
        Place(strip, 0, -339, 1060, 29);
        var feedback = Find(root, "Feedback");
        if (feedback != null)
        {
            // Background below text, not on top.
            strip.SetSiblingIndex(feedback.GetSiblingIndex());
            Place(feedback, 0, -339, 1020, 26);
            SetText(feedback, null, 13);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Level 3 layout saved",
            "The scene was updated. Press Play to verify the notebook, classification and conductivity controls.", "OK");
    }

    private static Transform Find(Transform root, string name)
    {
        if (root == null) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static void Hide(Transform root, string name)
    {
        var obj = Find(root, name);
        if (obj != null) obj.gameObject.SetActive(false);
    }

    private static void MoveInsideBoard(Transform root, Transform board, string name,
        float x, float y, float w, float h, string text, int fontSize)
    {
        var item = Find(root, name);
        if (item == null) return;
        if (item.parent != board) item.SetParent(board, false);
        item.gameObject.SetActive(true);
        Place(item, x, y, w, h);
        SetText(item, text, fontSize);
        item.SetAsLastSibling();
    }

    private static void Place(Transform root, string name, float x, float y, float w, float h)
        => Place(Find(root, name), x, y, w, h);

    private static void Place(Transform target, float x, float y, float w, float h)
    {
        if (target == null) return;
        var rect = target.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
    }

    private static void SetText(Transform target, string text, float fontSize)
    {
        if (target == null) return;
        var component = target.GetComponent<TMP_Text>();
        if (component == null) component = target.GetComponentInChildren<TMP_Text>(true);
        if (component == null) return;
        if (text != null) component.text = text;
        component.enableAutoSizing = false;
        component.fontSize = fontSize;
        component.overflowMode = TextOverflowModes.Ellipsis;
    }
}
#endif
