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

        // Reserve more space for notebook text without squeezing the sample bank.
        Place(board, "SampleBank", -402, -34, 215, 260);
        Place(board, "TestStation", -91, -33, 380, 262);
        Place(board, "EvidenceBoard", 323, -26, 359, 300);
        Place(root, "MenuButton", -535, 295, 112, 42);

        var evidencePanel = Find(root, "EvidenceBoard");
        var evidence = Find(root, "EvidenceText");
        if (evidencePanel != null && evidence != null)
        {
            // A scrollable notebook allows readable 14pt text while keeping
            // every observation available, including additional probe results.
            var viewport = Find(evidencePanel, "Level3NotebookViewport");
            if (viewport == null)
            {
                var go = new GameObject("Level3NotebookViewport",
                    typeof(RectTransform), typeof(Image),
                    typeof(RectMask2D), typeof(ScrollRect));
                go.transform.SetParent(evidencePanel, false);
                viewport = go.transform;
                var image = go.GetComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;
            }
            Place(viewport, 0, -23, 338, 238);
            var viewRect = viewport.GetComponent<RectTransform>();
            var scroll = viewport.GetComponent<ScrollRect>();
            if (evidence.parent != viewport) evidence.SetParent(viewport, false);
            evidence.SetAsLastSibling();
            var evidenceRect = evidence.GetComponent<RectTransform>();
            evidenceRect.anchorMin = new Vector2(0, 1);
            evidenceRect.anchorMax = new Vector2(1, 1);
            evidenceRect.pivot = new Vector2(.5f, 1);
            evidenceRect.anchoredPosition = Vector2.zero;
            evidenceRect.sizeDelta = new Vector2(-12, 0);
            evidenceRect.offsetMin = new Vector2(6, evidenceRect.offsetMin.y);
            evidenceRect.offsetMax = new Vector2(-6, evidenceRect.offsetMax.y);
            var fitter = evidence.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = evidence.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var tmp = evidence.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.fontSize = 17;
                tmp.enableAutoSizing = false;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.raycastTarget = false;
            }
            scroll.viewport = viewRect;
            scroll.content = evidenceRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 34f;
            scroll.inertia = false;
            scroll.verticalNormalizedPosition = 1f;
            // TMP measurement is ready immediately in edit mode: size the
            // content explicitly so a ScrollRect can actually move it.
            if (tmp != null)
            {
                tmp.ForceMeshUpdate();
                var width = Mathf.Max(10f, viewRect.rect.width - 16f);
                var requiredHeight = tmp.GetPreferredValues(tmp.text, width, 0f).y;
                evidenceRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(viewRect.rect.height + 1f, requiredHeight + 16f));
            }
        }
        Place(root, "EvidenceTitle", 0, 119, 338, 32);
        SetText(Find(root, "EvidenceTitle"), "LAB NOTEBOOK", 17);

        // Each row has its own space: tests, classification and feedback.
        Place(root, "SolidButton", -368, -247, 158, 41);
        Place(root, "MoltenButton", -190, -247, 158, 41);
        Place(root, "AqueousButton", -12, -247, 158, 41);
        Place(root, "ProbeButton", 221, -247, 241, 41);

        // Give progress a dedicated strip above the test controls.
        Place(root, "Progress", 0, -207, 760, 29);
        SetText(Find(root, "Progress"), null, 18);
        var progressText = Find(root, "Progress")?.GetComponent<TMP_Text>();
        if (progressText != null)
        {
            progressText.color = new Color32(80, 225, 255, 255);
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.alpha = 1f;
        }
        Place(root, "Bin0", -366, -287, 250, 43);
        Place(root, "Bin1", -78, -287, 250, 43);
        Place(root, "Bin2", 210, -287, 250, 43);
        foreach (var name in new[] { "Bin0", "Bin1", "Bin2" })
        {
            var bin = Find(root, name);
            if (bin != null)
            {
                var label = Find(bin, "Label");
                Place(label, 0, 0, 238, 40);
                SetText(label, null, 13);
            }
        }
        Place(root, "SubmitButton", 455, -287, 205, 43);
        SetText(Find(root, "SubmitButton"), "CHECK CLASSIFICATION", 12);

        // Keep feedback fully above the 720px canvas bottom.
        // The prior text was low-contrast and nearly clipped at the edge.
        var strip = Find(root, "Level3FeedbackStrip");
        if (strip == null)
        {
            var go = new GameObject("Level3FeedbackStrip",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(root, false);
            strip = go.transform;
        }
        var stripImage = strip.GetComponent<Image>();
        if (stripImage == null) stripImage = strip.gameObject.AddComponent<Image>();
        stripImage.color = new Color32(9, 32, 58, 255);
        stripImage.raycastTarget = false;
        Place(strip, 0, -328, 1050, 32);
        strip.gameObject.SetActive(true);
        var feedback = Find(root, "Feedback");
        if (feedback != null)
        {
            // Preserve modal ordering: insert the strip immediately before
            // the feedback text, leaving success/fail panels above both.
            int feedbackOrder = feedback.GetSiblingIndex();
            strip.SetSiblingIndex(feedbackOrder);
            Place(feedback, 0, -328, 1000, 28);
            SetText(feedback, null, 16);
            var feedbackText = feedback.GetComponent<TMP_Text>();
            if (feedbackText != null)
            {
                feedbackText.color = new Color32(255, 221, 142, 255);
                feedbackText.alignment = TextAlignmentOptions.Center;
                feedbackText.textWrappingMode = TextWrappingModes.NoWrap;
                feedbackText.overflowMode = TextOverflowModes.Ellipsis;
                feedbackText.alpha = 1f;
                feedbackText.raycastTarget = false;
            }
            feedback.gameObject.SetActive(true);
        }

        // Fix draw order: LabBoard must be underneath the footer controls,
        // otherwise progress and feedback can disappear behind the board.
        // Keep the BOW, pause, fail and completion overlays above all controls.
        var controlOrder = new[] {
            "Progress", "SolidButton", "MoltenButton", "AqueousButton",
            "ProbeButton", "Bin0", "Bin1", "Bin2", "SubmitButton"
        };
        strip.SetAsLastSibling();
        foreach (var name in controlOrder)
        {
            var control = Find(root, name);
            if (control != null)
            {
                control.gameObject.SetActive(true);
                control.SetAsLastSibling();
            }
        }
        if (feedback != null) feedback.SetAsLastSibling();
        foreach (var name in new[] {
            "PausePanel", "PauseMenuPanel", "FailedPanel", "CompletePanel",
            "OutOfEnergyPanel", "Stage5Level3BowCheck"
        })
        {
            var modal = Find(root, name);
            if (modal != null) modal.SetAsLastSibling();
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
