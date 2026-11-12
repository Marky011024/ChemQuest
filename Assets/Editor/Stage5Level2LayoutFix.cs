#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>One-time, non-gameplay UI repair for the editable Stage5Level2 scene.</summary>
public static class Stage5Level2LayoutFix
{
    [MenuItem("Tools/ChemQuest/Stage 5/Fix Level 2 UI Layout")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode before applying this layout fix.", "OK");
            return;
        }
        if (SceneManager.GetActiveScene().name != "Stage5Level2")
        {
            EditorUtility.DisplayDialog("Open Stage 5 Level 2", "Open Stage5Level2 before applying the fix.", "OK");
            return;
        }
        var canvas = GameObject.Find("Stage5Level2Canvas");
        if (canvas == null || Find(canvas.transform, "LabBoard") == null)
        {
            EditorUtility.DisplayDialog("Editable UI missing", "Stage5Level2Canvas / LabBoard is missing. Save or generate the editable UI first.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Fix Stage 5 Level 2", "Reposition UI objects only. Back up or commit local scene changes before continuing.", "Apply", "Cancel")) return;

        Undo.RegisterFullObjectHierarchyUndo(canvas, "Fix Stage 5 Level 2 layout");
        var root = canvas.transform;
        var board = Find(root, "LabBoard");
        Place(board, 0, -12, 1080, 462);
        Hide(root, "StageText");
        Hide(root, "BoardTitle");
        // Render the heading and instructions as topmost children of the board.
        // Previously they were behind the board Image, so they disappeared.
        var title = Find(root, "LevelTitle");
        var instructions = Find(root, "Instructions");
        if (title != null)
        {
            title.SetParent(board, false);
            title.gameObject.SetActive(true);
            Place(title, 0, 191, 980, 43);
            SetText(board, "LevelTitle", "HARDNESS & DUCTILITY ASSAY", 29);
            title.SetAsLastSibling();
        }
        if (instructions != null)
        {
            instructions.SetParent(board, false);
            instructions.gameObject.SetActive(true);
            Place(instructions, 0, 151, 960, 37);
            SetText(board, "Instructions", "Drag a sample to the bench. Use STRIKE and PRESS to compare its behavior.", 17);
            instructions.SetAsLastSibling();
        }
        Place(board, "SampleBank", -340, -40, 260, 258);
        Place(board, "TestStation", 35, -34, 430, 258);
        Place(board, "EvidenceBoard", 385, -40, 230, 258);
        Place(root, "MenuButton", -535, 294, 105, 40);

        // Dedicated lower control dock: all three action buttons share one
        // row; progress and feedback have their own rows, inside the canvas.
        // The dock sits behind the buttons but in front of the background.
        var dock = Find(root, "Level2ControlDock");
        if (dock == null)
        {
            var go = new GameObject("Level2ControlDock", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            dock = go.transform;
            var image = go.GetComponent<Image>();
            image.color = new Color32(10, 32, 58, 252);
            image.raycastTarget = false;
        }
        dock.gameObject.SetActive(true);
        Place(dock, 0, -269, 1030, 176);
        // Put it after the lab board but before interactive controls so it
        // never blocks the bench or the drag targets.
        var strike = Find(root, "StrikeButton");
        if (strike != null) dock.SetSiblingIndex(strike.GetSiblingIndex());

        Place(root, "StrikeButton", -312, -248, 278, 46);
        Place(root, "PressButton", 0, -248, 278, 46);
        Place(root, "SubmitButton", 312, -248, 278, 46);
        SetText(root, "StrikeButton", "STRIKE SAMPLE", 17);
        SetText(root, "PressButton", "PRESS SAMPLE", 17);
        SetText(root, "SubmitButton", "SUBMIT LAB RESULTS", 16);
        Place(root, "Progress", 0, -300, 520, 29);
        SetText(root, "Progress", null, 18);

        var strip = Find(root, "Level2FeedbackStrip");
        if (strip == null)
        {
            var go = new GameObject("Level2FeedbackStrip", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            strip = go.transform;
            var img = go.GetComponent<Image>();
            img.color = new Color32(8, 29, 51, 245);
            img.raycastTarget = false;
        }
        Place(strip, 0, -336, 1000, 28);
        strip.gameObject.SetActive(true);
        var stripImage = strip.GetComponent<Image>();
        if (stripImage != null) { stripImage.color = new Color32(6, 23, 43, 250); stripImage.raycastTarget = false; }
        var feedback = Find(root, "Feedback");
        if (feedback != null)
        {
            strip.SetSiblingIndex(feedback.GetSiblingIndex());
            Place(feedback, 0, -336, 970, 25);
            SetText(root, "Feedback", null, 14);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Level 2 UI updated", "Scene layout saved. Enter Play Mode to verify the result.", "OK");
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
        var t = Find(root, name);
        if (t != null) t.gameObject.SetActive(false);
    }
    private static void Place(Transform root, string name, float x, float y, float w, float h)
        => Place(Find(root, name), x, y, w, h);
    private static void Place(Transform target, float x, float y, float w, float h)
    {
        if (target == null) return;
        var rt = target.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
        rt.pivot = new Vector2(.5f, .5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
    private static void SetText(Transform root, string name, string content, float size)
    {
        var t = Find(root, name);
        if (t == null) return;
        var label = t.GetComponent<TMP_Text>() ?? t.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;
        if (content != null) label.text = content;
        label.fontSize = size;
        label.enableAutoSizing = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
    }
}
#endif
