using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Self-contained, scene-specific guide. No scene/prefab edits or inspector hookups required.
// It appears once for each registered student and can be replayed at any time.
public sealed class Stage1Level1NpcTutorial : MonoBehaviour
{
    private const string SceneName = "Stage1Level1";
    private const string KeyPrefix = "CQ_Stage1L1_NpcTutorial_v1_";
    private static readonly string[] Headings =
    {
        "WELCOME TO THE CHEMQUEST LAB!",
        "HOW TO CONTROL THE PUZZLE",
        "STEP 1 - START THE REACTION",
        "STEP 2 - LOOK AT THE ATOMS",
        "STEP 3 - EXPLORE THE BONDS",
        "FINISH THE LEVEL"
    };
    private static readonly string[] Instructions =
    {
        "Hello, young chemist! I am Prof. Atom, your lab guide. Let's learn how to play Stage 1, Level 1 together.",
        "Tap buttons to choose actions. To move a chemical, touch and hold it, then drag it to the correct location and release.",
        "Find ZINC (Zn), then drag it into the flask containing dilute hydrochloric acid (HCl). Watch for bubbles: they show that gas is forming!",
        "After the zinc reacts, tap SUB-MICROSCOPIC VIEW to explore what happens to the particles during the reaction.",
        "Tap each H-Cl bond to break it. Then follow the on-screen directions to bring hydrogen atoms together and form H2.",
        "Follow the prompts, complete the chemistry puzzle and answer the BOW Learning Check. Wrong answers may affect your score or energy. Good luck!"
    };

    private Canvas canvas;
    private GameObject dimmer;
    private GameObject replayButtonObject;
    private Text headingText;
    private Text bodyText;
    private Text progressText;
    private Button backButton;
    private Button nextButton;
    private Text nextLabel;
    private int page;
    private bool opened;
    private string completedKey;

    // The game normally starts on TitleScreen/MainMenu, not Stage1Level1.
    // Register for every scene transition, not just the startup scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterTutorialSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachToLevel(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AttachToLevel(scene);
    }

    private static void AttachToLevel(Scene scene)
    {
        if (scene.name != SceneName) return;
        if (FindFirstObjectByType<Stage1Level1NpcTutorial>() != null) return;
        var host = new GameObject("ProfAtom_Stage1Level1_Tutorial");
        SceneManager.MoveGameObjectToScene(host, scene);
        host.AddComponent<Stage1Level1NpcTutorial>();
        Debug.Log("CHEMQUEST: Prof. Atom tutorial attached to Stage1Level1.");
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            Destroy(gameObject);
            return;
        }
        completedKey = ProfileKey();
        BuildUI();
        if (PlayerPrefs.GetInt(completedKey, 0) == 0) OpenTutorial();
    }

    private static string ProfileKey()
    {
        string name = PlayerPrefs.GetString("StudentName", "").Trim();
        string school = PlayerPrefs.GetString("StudentSchool", "").Trim();
        string section = PlayerPrefs.GetString("StudentSection", "").Trim();
        string identity = (name + "|" + school + "|" + section).ToUpperInvariant();
        using (var sha = SHA256.Create())
        {
            byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(identity));
            return KeyPrefix + BitConverter.ToString(digest, 0, 12).Replace("-", "");
        }
    }

    private static Image Rectangle(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rt = (RectTransform)obj.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var image = obj.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static RectTransform Box(Transform parent, string name, Color color,
        Vector2 anchor, Vector2 pos, Vector2 size)
    {
        return (RectTransform)Rectangle(parent, name, color, anchor, anchor, pos, size).transform;
    }

    private static Text Label(Transform parent, string name, string value, int fontSize,
        Color color, TextAnchor alignment, Vector2 anchor, Vector2 pos, Vector2 size,
        FontStyle fontStyle = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = value;
        t.fontSize = fontSize;
        t.fontStyle = fontStyle;
        t.color = color;
        t.alignment = alignment;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return t;
    }

    private static Button ActionButton(Transform parent, string name, string caption,
        Color background, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick)
    {
        var rect = Box(parent, name, background, anchor, pos, size);
        var btn = rect.gameObject.AddComponent<Button>();
        btn.targetGraphic = rect.GetComponent<Image>();
        btn.onClick.AddListener(() => onClick());
        Label(rect, "Caption", caption, 22, Color.white, TextAnchor.MiddleCenter,
            new Vector2(.5f, .5f), Vector2.zero, size);
        return btn;
    }

    private static readonly Color Navy = new Color(.055f, .14f, .24f, 1);
    private static readonly Color Teal = new Color(.04f, .52f, .61f, 1);
    private static readonly Color Gold = new Color(1f, .74f, .32f, 1);

    private void BuildUI()
    {
        var root = new GameObject("ProfAtom_TutorialCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        // Always-available replay access, unobtrusive during gameplay.
        var replay = ActionButton(root.transform, "ReplayTutorial", "?  GUIDE",
            Teal, new Vector2(1, 1), new Vector2(-112, -38), new Vector2(185, 48),
            OpenTutorial);
        replayButtonObject = replay.gameObject;

        dimmer = Rectangle(root.transform, "ModalDimmer",
            new Color(.008f, .025f, .055f, .77f), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero).gameObject;
        var modal = Box(dimmer.transform, "TutorialCard", Navy,
            new Vector2(.5f, .5f), Vector2.zero, new Vector2(1040, 435));
        Box(modal, "TopAccent", Gold, new Vector2(.5f, 1), new Vector2(0, -6), new Vector2(1040, 12));

        // Pixel-style friendly scientist NPC composed of simple colored UI shapes.
        var npcArea = Box(modal, "NPCBackground", new Color(.08f, .32f, .42f),
            new Vector2(0, .5f), new Vector2(158, 16), new Vector2(265, 355));
        DrawScientist(npcArea);
        Label(npcArea, "NpcName", "PROF. ATOM", 24, Gold, TextAnchor.MiddleCenter,
            new Vector2(.5f, 0), new Vector2(0, 23), new Vector2(240, 40), FontStyle.Bold);

        // Use a consistent top-left pivot so the heading and dialogue never
        // intersect at different game-view resolutions / Android aspect ratios.
        headingText = Label(modal, "Heading", "", 27, Gold, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(310, -36), new Vector2(675, 62), FontStyle.Bold);
        var headingRect = (RectTransform)headingText.transform;
        headingRect.pivot = new Vector2(0, 1);

        bodyText = Label(modal, "Instruction", "", 24, Color.white, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(310, -128), new Vector2(675, 195));
        var bodyRect = (RectTransform)bodyText.transform;
        bodyRect.pivot = new Vector2(0, 1);
        bodyText.lineSpacing = 1.12f;

        progressText = Label(modal, "Progress", "", 19, new Color(.66f, .88f, .94f),
            TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(310, 34), new Vector2(280, 32));
        ((RectTransform)progressText.transform).pivot = new Vector2(0, 0);

        backButton = ActionButton(modal, "BackButton", "BACK",
            new Color(.28f, .38f, .54f), new Vector2(1, 0),
            new Vector2(-383, 52), new Vector2(150, 54), Previous);
        ActionButton(modal, "SkipButton", "SKIP",
            new Color(.3f, .36f, .43f), new Vector2(1, 0),
            new Vector2(-220, 52), new Vector2(140, 54), CloseTutorial);
        nextButton = ActionButton(modal, "NextButton", "NEXT",
            Teal, new Vector2(1, 0), new Vector2(-72, 52), new Vector2(140, 54), Next);
        nextLabel = nextButton.GetComponentInChildren<Text>();
        dimmer.SetActive(false);
    }

    private static void DrawScientist(RectTransform parent)
    {
        var center = new Vector2(.5f, .5f);
        // Hair, face, eyes, coat and atom badge: visible even without sprite assets.
        Box(parent, "Hair", new Color(.15f, .12f, .22f), center, new Vector2(0, 89), new Vector2(114, 92));
        Box(parent, "Face", new Color(.98f, .75f, .53f), center, new Vector2(0, 71), new Vector2(92, 88));
        Box(parent, "Fringe", new Color(.15f, .12f, .22f), center, new Vector2(0, 111), new Vector2(108, 22));
        Box(parent, "LeftEye", Navy, center, new Vector2(-25, 78), new Vector2(10, 12));
        Box(parent, "RightEye", Navy, center, new Vector2(25, 78), new Vector2(10, 12));
        Box(parent, "Smile", new Color(.65f, .28f, .32f), center, new Vector2(0, 51), new Vector2(28, 6));
        Box(parent, "Coat", Color.white, center, new Vector2(0, -46), new Vector2(154, 136));
        Box(parent, "Shirt", Teal, center, new Vector2(0, -35), new Vector2(50, 112));
        Box(parent, "LeftLapel", new Color(.8f, .9f, .95f), center, new Vector2(-48, -25), new Vector2(42, 88));
        Box(parent, "RightLapel", new Color(.8f, .9f, .95f), center, new Vector2(48, -25), new Vector2(42, 88));
        Box(parent, "Badge", Gold, center, new Vector2(48, -69), new Vector2(24, 24));
    }

    private void OpenTutorial()
    {
        if (dimmer == null) return;
        opened = true;
        page = 0;
        dimmer.SetActive(true);
        // Ensure modal appears above the small replay action.
        dimmer.transform.SetAsLastSibling();
        Refresh();
    }

    private void Refresh()
    {
        headingText.text = Headings[page];
        bodyText.text = Instructions[page];
        progressText.text = "TIP " + (page + 1) + " OF " + Headings.Length;
        backButton.interactable = page > 0;
        nextLabel.text = page == Headings.Length - 1 ? "LET'S PLAY!" : "NEXT";
    }

    private void Previous()
    {
        if (page > 0) { page--; Refresh(); }
    }

    private void Next()
    {
        if (page < Headings.Length - 1) { page++; Refresh(); }
        else CloseTutorial();
    }

    private void CloseTutorial()
    {
        opened = false;
        dimmer.SetActive(false);
        PlayerPrefs.SetInt(completedKey, 1);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        // No global timeScale changes: gameplay and other scenes remain untouched.
    }
}
