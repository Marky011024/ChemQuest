using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Stage3Level2AmmoniaGame : MonoBehaviour
{
    private readonly List<Button> electronButtons = new List<Button>();
    private readonly List<TMP_Text> slotTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> electronPairVisuals = new List<TMP_Text>();
    private readonly List<Image> energyIcons = new List<Image>();
    [Header("Stage 1 Assets")]
    [SerializeField] private Sprite energySprite;
    [SerializeField] private GameObject stage1PauseButtonPrefab;
    [SerializeField] private GameObject stage1PauseMenuPrefab;
    private TMP_Text scoreText, feedbackText;
    private GameObject failurePanel, completionPanel, pausePanel, outOfEnergyPanel;
    private TMP_Text energyTimerText;
    private bool outOfEnergyVisible;
    private Button retryButton;
    private int[] slots = new int[4];
    private int wrongAttempts;
    private bool failed, completed, paused;

    private static readonly Color Background = new Color32(10, 22, 47, 255);
    private static readonly Color Panel = new Color32(23, 44, 76, 255);
    private static readonly Color Accent = new Color32(56, 189, 248, 255);
    private static readonly Color Gold = new Color32(255, 207, 89, 255);

    private Font legacyFont;
    private TMP_Text finalScoreText, energyStatusText;

    private void Start()
    {
        Time.timeScale = 1;
        var old = GameObject.Find("Stage3Level2GeneratedCanvas");
        if (old != null) Destroy(old);
        SetupInputSystem();
        EnsureEnergy();
        BuildUI();
        RefreshEnergy();
    }

    private void SetupInputSystem()
    {
        var system = FindObjectOfType<EventSystem>();
        if (system == null)
            system = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        // Project uses the new Input System; the legacy UI module throws
        // InvalidOperationException when Active Input Handling is Input System.
        foreach (var legacy in system.GetComponents<StandaloneInputModule>())
            legacy.enabled = false;

        if (system.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = system.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    private void EnsureEnergy()
    {
        if (EnergyManager.Instance == null)
            new GameObject("SharedEnergyManager", typeof(EnergyManager));
    }

    private TMP_Text Label(Transform parent, string name, string message, int size, Vector2 position, Vector2 dimensions, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = message; t.fontSize = size; t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }

    private Image Rectangle(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position; rect.sizeDelta = size;
        var image = go.GetComponent<Image>(); image.color = color;
        return image;
    }

    private Button MakeButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Action click, Color color)
    {
        var image = Rectangle(parent, name, position, size, color);
        var button = image.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => click());
        Label(image.transform, "Text", label, 21, Vector2.zero, size - new Vector2(12, 4), Color.white);
        return button;
    }

    private void BuildUI()
    {
        var root = new GameObject("Stage3Level2GeneratedCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        // Fit the whole HUD in a short Game view instead of cropping the buttons.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        var bg = Rectangle(root.transform, "Background", Vector2.zero, new Vector2(1280, 720), Background);
        bg.rectTransform.anchorMin = Vector2.zero; bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero; bg.rectTransform.offsetMax = Vector2.zero;

        // Layout v2: centered Lewis structure; separate interactive controls.
        Label(root.transform, "Title", "STAGE 3  /  LEVEL 2", 18, new Vector2(0, 322), new Vector2(650, 36), Accent);
        Label(root.transform, "Subtitle", "AMMONIA  •  NH<sub>3</sub>", 36, new Vector2(0, 273), new Vector2(730, 48), Color.white);
        Label(root.transform, "Instructions",
            "N has 5 valence electrons; each H has 1. Build THREE bonding pairs and ONE lone pair.\nTap each electron slot twice to place its 2 electrons.",
            17, new Vector2(0, 218), new Vector2(980, 61), new Color32(196, 220, 239, 255));

        scoreText = Label(root.transform, "Score", "SCORE\n000000", 20, new Vector2(530, 291), new Vector2(145, 72), Gold);
        // Same five-energy HUD convention as Stage 1; lightning icon replaces plain square.
        // Reuse the EXACT Stage 1 energy image (Assets/Art/Energy.png).
        // The editor builder assigns the sprite reference so it works in player builds.
        Label(root.transform,"EnergyLabel","ENERGY",13,new Vector2(-540,331),new Vector2(120,22),Color.white);
        for(int i=0;i<5;i++)
        {
            var icon=Rectangle(root.transform,"Energy"+(i+1),
                new Vector2(-550+i*47,286),new Vector2(40,51),Color.white);
            icon.sprite=energySprite;
            icon.preserveAspect=true;
            if(energySprite==null)
                Debug.LogWarning("Stage3Level2: Stage 1 Energy sprite missing. Run Tools > ChemQuest > Build Stage 3 Level 2.");
            energyIcons.Add(icon);
        }
        energyStatusText=Label(root.transform,"EnergyStatus","5 / 5",14,
            new Vector2(-365,323),new Vector2(100,23),Color.white);

        Rectangle(root.transform, "MoleculeCardShadow", new Vector2(0, 27), new Vector2(750, 313), new Color32(12,32,56,255));
        Rectangle(root.transform, "MoleculeCard", new Vector2(0, 34), new Vector2(735, 305), Panel);
        Label(root.transform, "StructureLabel", "LEWIS DOT STRUCTURE", 17, new Vector2(0, 168), new Vector2(400, 30), Accent);

        // The lone pair is ABOVE nitrogen and the three N-H pairs are BETWEEN atoms.
        Label(root.transform, "Nitrogen", "N", 74, new Vector2(0, 50), new Vector2(110, 95), Accent);
        Label(root.transform, "HydrogenLeft", "H", 66, new Vector2(-240, 50), new Vector2(95, 90), Color.white);
        Label(root.transform, "HydrogenRight", "H", 66, new Vector2(240, 50), new Vector2(95, 90), Color.white);
        Label(root.transform, "HydrogenBottom", "H", 66, new Vector2(0, -78), new Vector2(95, 85), Color.white);

        // Guides make the three individual single bonds visible before electrons are placed.
        var guide = new Color32(59, 100, 129, 255);
        Rectangle(root.transform, "SingleBondGuideLeft", new Vector2(-122, 47), new Vector2(64, 3), guide);
        Rectangle(root.transform, "SingleBondGuideRight", new Vector2(122, 47), new Vector2(64, 3), guide);
        Rectangle(root.transform, "SingleBondGuideBottom", new Vector2(0, -12), new Vector2(3, 37), guide);
        electronPairVisuals.Add(Label(root.transform,"SharedPairLeft","○  ○",35,new Vector2(-122,65),new Vector2(110,44),Gold));
        electronPairVisuals.Add(Label(root.transform,"SharedPairRight","○  ○",35,new Vector2(122,65),new Vector2(110,44),Gold));
        electronPairVisuals.Add(Label(root.transform,"SharedPairBottom","○  ○",35,new Vector2(0,-15),new Vector2(120,42),Gold));
        electronPairVisuals.Add(Label(root.transform,"NitrogenLonePair","○  ○",35,new Vector2(0,115),new Vector2(115,42),Gold));

        // The buttons are BELOW the molecule, never overlapping the Lewis diagram.
        var names = new [] { "LEFT N-H", "RIGHT N-H", "BOTTOM N-H", "N LONE PAIR" };
        var positions = new [] { new Vector2(-378,-188),new Vector2(-126,-188),
            new Vector2(126,-188),new Vector2(378,-188) };
        for(int i=0; i<4; i++)
        {
            int slot=i;
            var zone=Rectangle(root.transform,"ElectronSlot"+i,positions[i],new Vector2(232,64),new Color32(35,78,108,255));
            slotTexts.Add(Label(zone.transform,"Count",names[i]+"\n0 / 2  •  TAP TO ADD",16,Vector2.zero,new Vector2(220,59),Color.white));
            var button=zone.gameObject.AddComponent<Button>();
            button.onClick.AddListener(()=>AddElectron(slot));
            electronButtons.Add(button);
        }
        feedbackText=Label(root.transform,"Feedback","Place two electrons into each of the four labeled slots.",18,new Vector2(0,-251),new Vector2(1000,41),Gold);
        MakeButton(root.transform,"CheckButton","CHECK MOLECULE",new Vector2(0,-318),new Vector2(260,54),CheckAnswer,new Color32(9,131,132,255));
        MakeButton(root.transform,"ResetButton","RESET",new Vector2(-270,-318),new Vector2(180,54),ResetPairs,new Color32(85,94,124,255));
        // Use the SAME exact hamburger/menu prefab as Stage 1.
        if(stage1PauseButtonPrefab != null)
        {
            var menuButton=Instantiate(stage1PauseButtonPrefab,root.transform,false);
            menuButton.name="PauseButton";
            var rect=menuButton.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=new Vector2(0,1);
                rect.anchorMax=new Vector2(0,1);
                rect.pivot=new Vector2(0.5f,0.5f);
                rect.anchoredPosition=new Vector2(62,-63);
            }
            var button=menuButton.GetComponent<Button>();
            if(button!=null)
            {
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(Pause);
            }
        }
        else
            MakeButton(root.transform,"PauseButton","MENU",new Vector2(-532,222),
                new Vector2(160,52),Pause,new Color32(40,74,108,255));

        // Match Stage 1: centered 700x400 failure card, large retry beneath title.
        failurePanel=ModalPanel(root.transform,"Level2FailedPanel","STAGE FAILED!",
            "Two incorrect answers. One energy has been used.");
        retryButton=MakeButton(failurePanel.transform,"RetryButton","RETRY",new Vector2(0,-80),
            new Vector2(260,70),Retry,new Color32(9,131,132,255));
        MakeButton(failurePanel.transform,"ReturnButton","LEVEL SELECTION",
            new Vector2(0,-158),new Vector2(270,50),()=>Navigate("Stage3LevelSelection"),new Color32(68,86,119,255));
        failurePanel.SetActive(false);

        // Stage 1 complete screen: final score above a side-by-side selection / next layout.
        completionPanel=ModalPanel(root.transform,"Level2CompletePanel","LEVEL COMPLETE!","AMMONIA (NH<sub>3</sub>) FORMED");
        finalScoreText=Label(completionPanel.transform,"FinalScoreText","FINAL SCORE: 000000",29,
            new Vector2(0,-32),new Vector2(570,64),Gold);
        MakeButton(completionPanel.transform,"LevelSelectionButton","LEVEL SELECTION",
            new Vector2(-125,-160),new Vector2(230,55),
            ()=>Navigate("Stage3LevelSelection"),new Color32(69,88,122,255));
        MakeButton(completionPanel.transform,"NextLevelButton","NEXT LEVEL",
            new Vector2(125,-160),new Vector2(230,55),
            ()=>Navigate("Stage3Level3"),new Color32(9,131,132,255));
        completionPanel.SetActive(false);

        // Exact Stage 1 pause panel prefab, with this level's own navigation.
        if(stage1PauseMenuPrefab!=null)
        {
            pausePanel=Instantiate(stage1PauseMenuPrefab,root.transform,false);
            pausePanel.name="PauseMenuPanel";
            var rect=pausePanel.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=Vector2.zero;
                rect.anchorMax=Vector2.one;
                rect.pivot=new Vector2(0.5f,0.5f);
                rect.offsetMin=Vector2.zero;
                rect.offsetMax=Vector2.zero;
            }
            WireMenuButton(pausePanel.transform,"ContinueButton",Resume);
            WireMenuButton(pausePanel.transform,"LevelSelectionButton",
                ()=>Navigate("Stage3LevelSelection"));
            WireMenuButton(pausePanel.transform,"StageSelectionButton",
                ()=>Navigate("StageSelection"));
            RenameMenuButton(pausePanel.transform,"StageSelectionButton","STAGE SELECTION");
        }
        else
        {
            pausePanel=ModalPanel(root.transform,"PauseMenuPanel","GAME PAUSED",
                "Choose an option below.");
            MakeButton(pausePanel.transform,"ResumeButton","RESUME",new Vector2(0,-12),
                new Vector2(260,58),Resume,new Color32(9,131,132,255));
            MakeButton(pausePanel.transform,"LevelSelectButton","LEVEL SELECTION",
                new Vector2(0,-91),new Vector2(280,58),
                ()=>Navigate("Stage3LevelSelection"),new Color32(69,88,122,255));
            MakeButton(pausePanel.transform,"StageSelectButton","SELECT STAGE",
                new Vector2(0,-161),new Vector2(280,55),
                ()=>Navigate("StageSelection"),new Color32(69,88,122,255));
        }
        pausePanel.SetActive(false);

        // Stage 1 OutOfEnergyUI behavior: automatically block gameplay at zero
        // and display countdown based on the shared EnergyManager clock.
        outOfEnergyPanel=ModalPanel(root.transform,"OutOfEnergyPanel",
            "OUT OF ENERGY!","You need energy to continue playing.");
        energyTimerText=Label(outOfEnergyPanel.transform,"EnergyTimerText",
            "Next Energy In: 15:00",25,new Vector2(0,-49),
            new Vector2(600,60),Gold);
        MakeButton(outOfEnergyPanel.transform,"BackToSelectionButton",
            "BACK TO LEVEL SELECTION",new Vector2(0,-137),new Vector2(330,62),
            ()=>Navigate("Stage3LevelSelection"),new Color32(9,131,132,255));
        outOfEnergyPanel.SetActive(false);
        UpdateOutOfEnergyUI();
    }

    private void WireMenuButton(Transform parent,string objectName,Action callback)
    {
        foreach(var button in parent.GetComponentsInChildren<Button>(true))
        {
            if(button.gameObject.name!=objectName)continue;
            // Replace original prefab's persistent Stage 1 scene bindings.
            button.onClick=new Button.ButtonClickedEvent();
            button.onClick.AddListener(()=>callback());
            return;
        }
        Debug.LogWarning("Stage 1 pause prefab button missing: "+objectName);
    }

    private void RenameMenuButton(Transform parent,string objectName,string label)
    {
        foreach(var button in parent.GetComponentsInChildren<Button>(true))
        {
            if(button.gameObject.name!=objectName)continue;
            var text=button.GetComponentInChildren<TMP_Text>(true);
            if(text!=null)text.text=label;
            return;
        }
    }

    private GameObject ModalPanel(Transform parent,string name,string heading,string description)
    {
        var dim=Rectangle(parent,name,Vector2.zero,new Vector2(1280,720),
            new Color32(5,12,28,225));
        var card=Rectangle(dim.transform,"Panel",Vector2.zero,new Vector2(700,400),
            new Color32(22,44,76,255));
        Label(dim.transform,"Heading",heading,37,new Vector2(0,104),
            new Vector2(640,80),Gold);
        Label(dim.transform,"Description",description,22,new Vector2(0,42),
            new Vector2(635,65),Color.white);
        return dim.gameObject;
    }

    private GameObject Overlay(Transform parent,string name,string heading,string message)
    {
        var image=Rectangle(parent,name,Vector2.zero,new Vector2(1280,720),new Color32(6,14,32,245));
        Label(image.transform,"Heading",heading,38,new Vector2(0,120),new Vector2(950,90),Gold);
        Label(image.transform,"Message",message,24,new Vector2(0,40),new Vector2(950,74),Color.white);
        return image.gameObject;
    }

    private void AddElectron(int slot)
    {
        if (!CanPlay() || slots[slot]>=2) return;
        slots[slot]++;
        slotTexts[slot].text=new[]{"LEFT N-H","RIGHT N-H","BOTTOM N-H","N LONE PAIR"}[slot]+"\n"+slots[slot]+" / 2  •  TAP TO ADD";
        electronPairVisuals[slot].text = slots[slot] == 1 ? "●  ○" : "●  ●";
        feedbackText.text="Electron placed. Three bonding pairs + one lone pair = NH<sub>3</sub>.";
    }

    private bool CanPlay() => !paused && !failed && !completed && EnergyManager.Instance != null && EnergyManager.Instance.HasEnergy;

    private void CheckAnswer()
    {
        if (!CanPlay()) return;
        bool correct=true;
        for(int i=0;i<slots.Length;i++) if(slots[i]!=2) correct=false;
        if(correct)
        {
            completed=true;
            int score=wrongAttempts==0?100:75;
            scoreText.text="SCORE\n"+score.ToString("D6");
            if(finalScoreText!=null) finalScoreText.text="FINAL SCORE: "+score.ToString("D6");
            PlayerPrefs.SetInt("Stage3Level2Completed",1);
            PlayerPrefs.SetInt("Stage3Level2Score",score);
            PlayerPrefs.SetInt("Stage3Level3Unlocked",1);
            PlayerPrefs.Save();
            completionPanel.SetActive(true);
            return;
        }
        wrongAttempts++;
        if(wrongAttempts==1)
        {
            feedbackText.text="HINT: Nitrogen forms 3 single bonds and has 1 lone electron pair. Fill every pair (2/2).";
            feedbackText.color=Gold;
            ResetPairs(false);
        }
        else
        {
            failed=true;
            EnergyManager.Instance.LoseEnergy();
            RefreshEnergy();
            retryButton.interactable=EnergyManager.Instance.HasEnergy;
            failurePanel.SetActive(true);
            UpdateOutOfEnergyUI(); // If this was the last energy, zero-energy panel takes priority.
        }
    }

    private void ResetPairs() => ResetPairs(true);
    private void ResetPairs(bool feedback)
    {
        if(!CanPlay())return;
        for(int i=0;i<slots.Length;i++)
        {
            slots[i]=0;
            slotTexts[i].text=new[]{"LEFT N-H","RIGHT N-H","BOTTOM N-H","N LONE PAIR"}[i]+"\n0 / 2  •  TAP TO ADD";
            electronPairVisuals[i].text="○  ○";
        }
        if(feedback) feedbackText.text="Pairs reset. No wrong attempt charged.";
    }
    private void RefreshEnergy()
    {
        int value=EnergyManager.Instance == null ? 0 : EnergyManager.Instance.CurrentEnergy;
        for(int i=0;i<energyIcons.Count;i++)
        {
            if(energyIcons[i]!=null)
            {
                energyIcons[i].enabled = i < value;
            }
        }
        if(energyStatusText!=null) energyStatusText.text=value+" / 5";
    }
    private void Update()
    {
        // Same regeneration countdown as Stage 1, shown in realtime even when paused.
        if(Time.frameCount%15==0)
        {
            RefreshEnergy();
            UpdateOutOfEnergyUI();
        }
    }

    private void UpdateOutOfEnergyUI()
    {
        if(outOfEnergyPanel==null || EnergyManager.Instance==null)return;
        var energy=EnergyManager.Instance;
        bool empty=!energy.HasEnergy;
        if(empty)
        {
            if(!outOfEnergyPanel.activeSelf)
                outOfEnergyPanel.SetActive(true);
            // Make the modal topmost so failed/retry buttons cannot be used at zero.
            outOfEnergyPanel.transform.SetAsLastSibling();
            int seconds=energy.SecondsUntilNextEnergy();
            if(energyTimerText!=null)
                energyTimerText.text="Next Energy In: "+
                    (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
        }
        else if(outOfEnergyPanel.activeSelf)
        {
            outOfEnergyPanel.SetActive(false);
        }
        outOfEnergyVisible=empty;
    }
    private void Retry()
    {
        if(EnergyManager.Instance!=null && EnergyManager.Instance.HasEnergy)
            Navigate("Stage3Level2");
        else
            UpdateOutOfEnergyUI();
    }
    private void Pause(){if(!CanPlay())return;paused=true;pausePanel.SetActive(true);Time.timeScale=0;}
    private void Resume(){paused=false;pausePanel.SetActive(false);Time.timeScale=1;}
    private void Navigate(string name)
    {
        Time.timeScale=1;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {
            Debug.LogError("ChemQuest navigation: "+name+" is not in Build Settings.");
            return;
        }
        SceneManager.LoadScene(name);
    }
    private void OnDestroy(){Time.timeScale=1;}
}
