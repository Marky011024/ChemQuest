using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Stage 1 Level 1 only: in-game hands-on guide. Does not simulate clicks/drags,
// modify puzzle progress, scoring, energy, or any other scene.
public sealed class Stage1Level1NpcTutorial : MonoBehaviour
{
    private const string SceneName = "Stage1Level1";
    // v2 is intentional: players who saw the old static popup get the new walkthrough.
    private const string KeyPrefix = "CQ_Stage1L1_InteractiveGuide_v2_";
    private static readonly Color Navy = new Color(.055f, .14f, .24f, .96f);
    private static readonly Color Teal = new Color(.04f, .52f, .61f, 1);
    private static readonly Color Gold = new Color(1f, .76f, .30f, 1);
    private static readonly Color Cream = new Color(1f, .79f, .58f, 1);

    private Canvas canvas;
    private GameObject guideRoot;
    private GameObject guideButton;
    private Image handMarker;
    private Image halo;
    private Image trail;
    private Text heading;
    private Text dialogue;
    private Text progress;
    private Button nextButton;
    private Text nextLabel;
    private int step;
    private float animationClock;
    private bool activeGuide;
    private string completionKey;
    private ReactionFlaskDropZone flask;
    private SubMicroscopicViewManager microscopic;
    private BondProgressManager bonds;
    private RectTransform zincRect;
    private RectTransform flaskRect;
    private RectTransform microscopicButtonRect;
    private RectTransform bondRect;
    private RectTransform hydrogenRect;
    private RectTransform hydrogen2Rect;
    private RectTransform draggingHydrogen;
    private RectTransform hydrogenDropTarget;
    private RectTransform[] textRefs;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Attach(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Attach(scene); }

    private static void Attach(Scene scene)
    {
        if (scene.name != SceneName) return;
        if (FindFirstObjectByType<Stage1Level1NpcTutorial>() != null) return;
        var host = new GameObject("ProfAtom_Stage1Level1_InteractiveGuide");
        SceneManager.MoveGameObjectToScene(host, scene);
        host.AddComponent<Stage1Level1NpcTutorial>();
        Debug.Log("CHEMQUEST: Prof. Atom interactive hand guide attached.");
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneName) { Destroy(gameObject); return; }
        completionKey = ProfileKey();
        DiscoverTargets();
        BuildInterface();
        if (PlayerPrefs.GetInt(completionKey, 0) == 0) StartGuide();
    }

    private static string ProfileKey()
    {
        var identity = (PlayerPrefs.GetString("StudentName", "").Trim() + "|" +
                        PlayerPrefs.GetString("StudentSchool", "").Trim() + "|" +
                        PlayerPrefs.GetString("StudentSection", "").Trim()).ToUpperInvariant();
        using (var sha = SHA256.Create())
            return KeyPrefix + BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(identity)), 0, 12).Replace("-", "");
    }

    private void DiscoverTargets()
    {
        flask = FindFirstObjectByType<ReactionFlaskDropZone>();
        microscopic = FindFirstObjectByType<SubMicroscopicViewManager>();
        bonds = FindFirstObjectByType<BondProgressManager>();
        zincRect = FindNamedRect("ZincContainer");
        flaskRect = FindNamedRect("ReactionFlask");
        microscopicButtonRect = FindNamedRect("SubMicroscopicButton");
        bondRect = FindNamedRect("HClBond1");
        hydrogenRect = FindNamedRect("Hydrogen1");
        hydrogen2Rect = FindNamedRect("Hydrogen2");
    }

    // Find named UI even under inactive panels, but only within this scene.
    private RectTransform FindNamedRect(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
                if (rect.name == name) return rect;
        }
        return null;
    }

    private static Image Panel(Transform parent, string name, Color color,
        Vector2 min, Vector2 max, Vector2 pos, Vector2 size, bool blocks = true)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = min; rect.anchorMax = max;
        rect.anchoredPosition = pos; rect.sizeDelta = size;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = blocks;
        return image;
    }

    private static RectTransform Box(Transform parent, string name, Color color,
        Vector2 anchor, Vector2 pos, Vector2 size, bool blocks = true)
    {
        return (RectTransform)Panel(parent, name, color, anchor, anchor, pos, size, blocks).transform;
    }

    private static Text Caption(Transform parent, string name, string value, int fontSize,
        Color color, TextAnchor alignment, Vector2 anchor, Vector2 pos, Vector2 size,
        FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = anchor; r.anchoredPosition = pos; r.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value; text.fontSize = fontSize; text.color = color;
        text.alignment = alignment; text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    // Place dialogue relative to the actual top-left corner instead of using
    // Unity's default centered pivot (which caused drifting/overlapping text).
    private static Text TopLeftCaption(Transform parent, string name, string value,
        int fontSize, Color color, float left, float top, float width, float height,
        FontStyle style = FontStyle.Normal)
    {
        var text = Caption(parent, name, value, fontSize, color, TextAnchor.UpperLeft,
            new Vector2(0, 1), Vector2.zero, new Vector2(width, height), style);
        var rect = (RectTransform)text.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(left, -top);
        text.resizeTextForBestFit = false;
        return text;
    }

    private static Button MakeButton(Transform parent, string name, string caption,
        Color color, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick)
    {
        var panel = Box(parent, name, color, anchor, pos, size);
        var button = panel.gameObject.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        button.onClick.AddListener(() => onClick());
        Caption(panel, "Label", caption, 20, Color.white, TextAnchor.MiddleCenter,
            new Vector2(.5f, .5f), Vector2.zero, size, FontStyle.Bold);
        return button;
    }

    private void BuildInterface()
    {
        var root = new GameObject("ProfAtomGuideCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;

        guideButton = MakeButton(root.transform, "GuideReplayButton", "? GUIDE", Teal,
            new Vector2(1, 1), new Vector2(-105, -40), new Vector2(180, 46), StartGuide).gameObject;

        // No fullscreen raycast blocker: the player can still interact with actual zinc,
        // flask, sub-microscopic button and atom bonds behind this overlay.
        guideRoot = Panel(root.transform, "InGameTutorialLayer",
            new Color(0, 0, 0, 0), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, false).gameObject;

        // Pale highlight tracks the target. The drawn hand animates between real UI positions.
        halo = Panel(guideRoot.transform, "TargetGlow",
            new Color(1f, .84f, .24f, .24f), new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), Vector2.zero, new Vector2(104, 90), false);
        trail = Panel(guideRoot.transform, "DragPath",
            new Color(.98f, .83f, .35f, .66f), new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), Vector2.zero, new Vector2(8, 8), false);
        var hand = Box(guideRoot.transform, "AnimatedTutorialHand", new Color(0, 0, 0, 0),
            new Vector2(.5f, .5f), Vector2.zero, new Vector2(92, 106), false);
        handMarker = hand.GetComponent<Image>();
        Sprite handSprite = Resources.Load<Sprite>("ChemQuestTutorial/TutorialHand");
        if (handSprite != null)
        {
            handMarker.sprite = handSprite;
            handMarker.color = Color.white;
            handMarker.preserveAspect = true;
        }
        else
        {
            // Keep the old drawn hand as fallback until the PNG is imported.
            DrawHand(hand);
            Debug.LogWarning("CHEMQUEST: TutorialHand sprite missing from Resources/ChemQuestTutorial.");
        }

        // Bottom HUD strip leaves the zinc/flask and Sub-Microscopic View
        // button accessible. All labels use fixed top-left coordinates.
        var speech = Box(guideRoot.transform, "ProfAtomDialogueBar", Navy,
            new Vector2(.5f, 0), new Vector2(0, 79), new Vector2(1180, 148));
        Box(speech, "GoldLine", Gold, new Vector2(.5f, 1), new Vector2(0, -3),
            new Vector2(1180, 6), false);

        var portrait = Box(speech, "ProfAtomPortrait", new Color(.08f, .31f, .39f, 1),
            new Vector2(0, .5f), new Vector2(79, 0), new Vector2(150, 138), false);
        Sprite atomSprite = Resources.Load<Sprite>("ChemQuestTutorial/ProfAtom");
        if (atomSprite != null)
        {
            var portraitImage = Panel(portrait, "ProfAtomGeneratedArt", Color.white,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, false);
            portraitImage.sprite = atomSprite;
            portraitImage.preserveAspect = true;
        }
        else
        {
            DrawScientist(portrait);
            Debug.LogWarning("CHEMQUEST: ProfAtom sprite missing from Resources/ChemQuestTutorial.");
        }

        // Name tag stays below the NPC's face, not on top of the dialogue.
        var nameTag = Box(speech, "NPCNameBackground", new Color(.04f, .13f, .23f, .96f),
            new Vector2(0, 0), new Vector2(79, 19), new Vector2(150, 28), false);
        Caption(nameTag, "ProfAtomLabel", "PROF. ATOM", 17, Gold,
            TextAnchor.MiddleCenter, new Vector2(.5f, .5f), Vector2.zero,
            new Vector2(146, 26), FontStyle.Bold);

        heading = TopLeftCaption(speech, "StepHeading", "", 22, Gold,
            180, 13, 705, 36, FontStyle.Bold);
        dialogue = TopLeftCaption(speech, "Speech", "", 20, Color.white,
            180, 58, 755, 66);
        dialogue.lineSpacing = 1.05f;
        progress = Caption(speech, "StepNumber", "", 16,
            new Color(.72f, .92f, .95f, 1), TextAnchor.MiddleCenter,
            new Vector2(0, 0), new Vector2(236, 16), new Vector2(112, 24));

        MakeButton(speech, "Skip", "SKIP", new Color(.35f, .39f, .46f, 1),
            new Vector2(1, 1), new Vector2(-198, -31), new Vector2(122, 43), FinishGuide);
        nextButton = MakeButton(speech, "Next", "NEXT", Teal,
            new Vector2(1, 1), new Vector2(-65, -31), new Vector2(118, 43), NextStep);
        nextLabel = nextButton.GetComponentInChildren<Text>();
        guideRoot.SetActive(false);
    }

    private static void DrawScientist(RectTransform p)
    {
        var c = new Vector2(.5f, .5f);
        Box(p, "Hair", new Color(.14f, .11f, .21f), c, new Vector2(0, 27), new Vector2(62, 58), false);
        Box(p, "Face", Cream, c, new Vector2(0, 20), new Vector2(52, 47), false);
        Box(p, "Fringe", new Color(.14f, .11f, .21f), c, new Vector2(0, 45), new Vector2(65, 15), false);
        Box(p, "LeftEye", Navy, c, new Vector2(-13, 22), new Vector2(6, 7), false);
        Box(p, "RightEye", Navy, c, new Vector2(13, 22), new Vector2(6, 7), false);
        Box(p, "Smile", new Color(.6f, .25f, .29f), c, new Vector2(0, 6), new Vector2(16, 4), false);
        Box(p, "LabCoat", Color.white, c, new Vector2(0, -32), new Vector2(100, 56), false);
        Box(p, "Shirt", Teal, c, new Vector2(0, -28), new Vector2(28, 48), false);
        Box(p, "LabBadge", Gold, c, new Vector2(27, -29), new Vector2(12, 12), false);
    }

    private static void DrawHand(RectTransform hand)
    {
        var center = new Vector2(.5f, .5f);
        // Simple pixel-art pointing hand, no external sprite or font needed.
        Box(hand, "Shadow", new Color(.11f, .12f, .18f, .9f), center, new Vector2(6, -4),
            new Vector2(49, 53), false);
        Box(hand, "Palm", Cream, center, new Vector2(0, -13), new Vector2(48, 53), false);
        Box(hand, "PointingFinger", Cream, center, new Vector2(-16, 28), new Vector2(17, 62), false);
        Box(hand, "Thumb", Cream, center, new Vector2(31, -6), new Vector2(19, 26), false);
        Box(hand, "Cuff", Teal, center, new Vector2(0, -42), new Vector2(55, 16), false);
        Box(hand, "Sparkle", Gold, center, new Vector2(-30, 60), new Vector2(12, 12), false);
    }

    private void StartGuide()
    {
        if (guideRoot == null) return;
        DiscoverTargets();
        step = 0;
        activeGuide = true;
        animationClock = 0f;
        guideRoot.SetActive(true);
        guideButton.SetActive(false);
        ShowStep();
    }

    private void ShowStep()
    {
        animationClock = 0f;
        draggingHydrogen = null;
        hydrogenDropTarget = null;
        switch (step)
        {
            case 0:
                heading.text = "WELCOME, CHEMIST!";
                dialogue.text = "Hi! I'm Prof. Atom. I will show you exactly where to tap and drag. The real game stays visible while I guide you!";
                break;
            case 1:
                heading.text = "DRAG ZINC INTO THE FLASK";
                dialogue.text = "Touch and hold the zinc (Zn) dish. Follow my moving hand to the HCl flask, then release. Your turn!";
                break;
            case 2:
                heading.text = "OBSERVE THE REACTION";
                dialogue.text = "Great! See the bubbles? Gas forming is evidence of a chemical change. Let's look closer at the atoms.";
                break;
            case 3:
                heading.text = "OPEN THE ATOM VIEW";
                dialogue.text = "Tap SUB-MICROSCOPIC VIEW where my hand is pointing. You can see the tiny particles!";
                break;
            case 4:
                heading.text = "BREAK THE H-Cl BONDS";
                dialogue.text = "Tap both H-Cl bonds as my hand shows, then bring the hydrogen atoms together to form H2.";
                break;
            default:
                heading.text = "BRING THE TWO H ATOMS TOGETHER";
                dialogue.text = "Touch and hold either H, then drag it toward the other H to form H2. My hand will point at your drop target!";
                break;
        }
        progress.text = "STEP " + (step + 1) + " / 6";
        bool needsAction = step == 1 || step == 3 || step == 4;
        nextLabel.text = step == 5 ? "DONE" : needsAction ? "DO IT!" : "NEXT";
        // Students advance by performing real game actions, not by skipping
        // the drag-and-drop and atom interactions with the Next button.
        nextButton.interactable = !needsAction;
        if (needsAction && !HasActionTarget())
        {
            // Avoid trapping users if a different Unity scene variant has no target.
            nextButton.interactable = true;
            nextLabel.text = "NEXT";
        }
    }

    private bool HasActionTarget()
    {
        if (step == 1) return zincRect != null && flask != null;
        if (step == 3) return microscopicButtonRect != null;
        if (step == 4) return bonds != null && bondRect != null;
        return false;
    }

    private void NextStep()
    {
        if (step == 1 && HasActionTarget() && flask != null && !flask.zincAdded) return;
        if (step == 3 && HasActionTarget())
        {
            var atomPanel = FindNamedRect("SubMicroscopicPanel");
            if (atomPanel != null && !atomPanel.gameObject.activeInHierarchy) return;
        }
        if (step == 4 && HasActionTarget() && bonds != null && !bonds.BothBondsAreBroken) return;
        if (step >= 5) { FinishGuide(); return; }
        step++;
        ShowStep();
    }

    // Called from the existing DraggableHydrogen script on actual pointer drag.
    // The target hand is a visual-only overlay, never intercepts input.
    public void OnHydrogenDragBegin(RectTransform dragged)
    {
        if (!activeGuide || dragged == null || bonds == null ||
            !bonds.BothBondsAreBroken) return;
        if (step < 5) { step = 5; ShowStep(); }
        if (step != 5) return;
        draggingHydrogen = dragged;
        hydrogenDropTarget = dragged == hydrogenRect ? hydrogen2Rect :
            dragged == hydrogen2Rect ? hydrogenRect : null;
        if (hydrogenDropTarget != null && !hydrogenDropTarget.gameObject.activeInHierarchy)
            hydrogenDropTarget = null;
        if (hydrogenDropTarget != null)
        {
            heading.text = "MOVE H TO THE OTHER H!";
            dialogue.text = "Keep holding H and drag it toward the highlighted other H. Release when they meet to form H2!";
        }
    }

    public void OnHydrogenDragEnd(RectTransform dragged)
    {
        if (draggingHydrogen != dragged) return;
        draggingHydrogen = null;
        hydrogenDropTarget = null;
        if (activeGuide && step == 5 && guideRoot != null)
        {
            heading.text = "BRING THE HYDROGEN ATOMS TOGETHER";
            dialogue.text = "Drag one H toward the other H to form H2. Watch where my hand points!";
        }
    }

    private void FinishGuide()
    {
        draggingHydrogen = null;
        hydrogenDropTarget = null;
        activeGuide = false;
        guideRoot.SetActive(false);
        guideButton.SetActive(true);
        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.Save();
    }

    private Vector2 LocalPosition(RectTransform target)
    {
        if (target == null) return Vector2.zero;
        var sourceCanvas = target.GetComponentInParent<Canvas>();
        Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? sourceCanvas.worldCamera : null;
        var screen = RectTransformUtility.WorldToScreenPoint(sourceCamera, target.TransformPoint(target.rect.center));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)guideRoot.transform, screen, null, out var local);
        return local;
    }

    private void Update()
    {
        if (guideRoot == null) return;

        // The BOW assessment is a separate full-screen interaction. Never let
        // the guide sit above its answers or leave the animated hand floating
        // over a question. The regular replay button returns afterwards.
        bool bowVisible = GameObject.Find("Stage1BowLearningCheck") != null;
        if (bowVisible)
        {
            if (activeGuide) FinishGuide();
            if (guideButton != null) guideButton.SetActive(false);
            return;
        }
        if (!activeGuide)
        {
            if (guideButton != null && !guideButton.activeSelf)
                guideButton.SetActive(true);
            return;
        }
        animationClock += Time.unscaledDeltaTime;
        // Tutorial stays synchronized with real student interactions; never automates them.
        if (step == 1 && flask != null && flask.zincAdded) { step = 2; ShowStep(); }
        if (step == 3 && microscopicButtonRect != null)
        {
            var panel = FindNamedRect("SubMicroscopicPanel");
            if (panel != null && panel.gameObject.activeInHierarchy) { step = 4; ShowStep(); }
        }
        if (step == 4 && bonds != null && bonds.BothBondsAreBroken)
        {
            step = 5; ShowStep();
        }

        RectTransform from = null, to = null;
        bool drag = false;
        switch (step)
        {
            case 1:
                from = zincRect; to = flaskRect; drag = true;
                break;
            case 2:
                from = flaskRect; break;
            case 3:
                from = microscopicButtonRect; break;
            case 4:
                from = bondRect;
                if (from != null && !from.gameObject.activeInHierarchy)
                    from = FindNamedRect("HClBond2");
                break;
            case 5:
                // Before dragging, demonstrate H1 -> H2. During the drag,
                // keep the hand fixed on the destination so the player knows
                // exactly where to drop the selected hydrogen atom.
                from = hydrogenDropTarget != null ? hydrogenDropTarget : hydrogenRect;
                to = hydrogenDropTarget != null ? null : hydrogen2Rect;
                drag = draggingHydrogen == null && to != null;
                break;
        }
        bool show = from != null && from.gameObject.activeInHierarchy;
        if (step == 5 && draggingHydrogen != null && hydrogenDropTarget == null)
            show = false;
        halo.gameObject.SetActive(show);
        handMarker.gameObject.SetActive(show);
        trail.gameObject.SetActive(show && drag && to != null);
        if (!show) return;
        Vector2 start = LocalPosition(from);
        Vector2 end = drag && to != null ? LocalPosition(to) : start;
        float fraction = drag ? Mathf.PingPong(animationClock * .65f, 1f) : 0f;
        var position = Vector2.Lerp(start, end, Mathf.SmoothStep(0, 1, fraction));
        if (!drag) position += new Vector2(0, Mathf.Sin(animationClock * 4f) * 12f);
        ((RectTransform)handMarker.transform).anchoredPosition = position + new Vector2(38, -8);
        ((RectTransform)halo.transform).anchoredPosition = drag ? end : start;
        halo.color = new Color(1, .84f, .24f, .18f + .17f * Mathf.Abs(Mathf.Sin(animationClock * 3f)));
        if (drag && to != null)
        {
            var segment = (RectTransform)trail.transform;
            segment.anchoredPosition = (start + end) * .5f;
            segment.sizeDelta = new Vector2((end - start).magnitude, 6);
            segment.localEulerAngles = new Vector3(0, 0,
                Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg);
        }
    }
}
