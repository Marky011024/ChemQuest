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
    private TMP_Text scoreText, feedbackText;
    private GameObject failurePanel, completionPanel, pausePanel;
    private Button retryButton;
    private int[] slots = new int[4];
    private int wrongAttempts;
    private bool failed, completed, paused;

    private static readonly Color Background = new Color32(10, 22, 47, 255);
    private static readonly Color Panel = new Color32(23, 44, 76, 255);
    private static readonly Color Accent = new Color32(56, 189, 248, 255);
    private static readonly Color Gold = new Color32(255, 207, 89, 255);

    private Font legacyFont;

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
        Label(root.transform, "EnergyLabel", "ENERGY", 14, new Vector2(-515, 324), new Vector2(140, 30), new Color32(196,220,239,255));
        for (int i=0; i<5; i++)
            energyIcons.Add(Rectangle(root.transform, "Energy"+(i+1), new Vector2(-565+i*37, 291), new Vector2(25,25), Gold));

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
        MakeButton(root.transform,"PauseButton","PAUSE",new Vector2(486,-318),new Vector2(165,54),Pause,new Color32(62,76,112,255));
        MakeButton(root.transform,"ExitButton","LEVEL SELECT",new Vector2(-487,-318),new Vector2(169,54),()=>Navigate("Stage3LevelSelection"),new Color32(62,76,112,255));

        failurePanel=Overlay(root.transform,"FailedOverlay","STAGE FAILED!","Two incorrect checks. One energy was consumed.");
        retryButton=MakeButton(failurePanel.transform,"Retry","RETRY",new Vector2(0,-60),new Vector2(220,56),Retry,new Color32(9,131,132,255));
        MakeButton(failurePanel.transform,"Back","LEVEL SELECT",new Vector2(0,-130),new Vector2(230,52),()=>Navigate("Stage3LevelSelection"),new Color32(85,94,124,255));
        failurePanel.SetActive(false);

        completionPanel=Overlay(root.transform,"SuccessOverlay","NH<sub>3</sub> MOLECULE COMPLETE!","STAGE 3 LEVEL 3 UNLOCKED!");
        MakeButton(completionPanel.transform,"Next","CONTINUE TO LEVEL 3",new Vector2(0,-65),new Vector2(330,59),()=>Navigate("Stage3Level3"),new Color32(9,131,132,255));
        MakeButton(completionPanel.transform,"Back","LEVEL SELECT",new Vector2(0,-139),new Vector2(230,52),()=>Navigate("Stage3LevelSelection"),new Color32(85,94,124,255));
        completionPanel.SetActive(false);

        pausePanel=Overlay(root.transform,"PauseOverlay","PAUSED","Resume the ammonia puzzle or return to level selection.");
        MakeButton(pausePanel.transform,"Resume","RESUME",new Vector2(0,-55),new Vector2(210,55),Resume,new Color32(9,131,132,255));
        MakeButton(pausePanel.transform,"Back","LEVEL SELECT",new Vector2(0,-122),new Vector2(225,52),()=>Navigate("Stage3LevelSelection"),new Color32(85,94,124,255));
        pausePanel.SetActive(false);
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
        for(int i=0;i<energyIcons.Count;i++)if(energyIcons[i]!=null)energyIcons[i].enabled=i<value;
    }
    private void Update(){if(Time.frameCount%30==0)RefreshEnergy();}
    private void Retry(){if(EnergyManager.Instance!=null && EnergyManager.Instance.HasEnergy)Navigate("Stage3Level2");}
    private void Pause(){if(!CanPlay())return;paused=true;pausePanel.SetActive(true);Time.timeScale=0;}
    private void Resume(){paused=false;pausePanel.SetActive(false);Time.timeScale=1;}
    private void Navigate(string name){Time.timeScale=1;SceneManager.LoadScene(name);}
    private void OnDestroy(){Time.timeScale=1;}
}
