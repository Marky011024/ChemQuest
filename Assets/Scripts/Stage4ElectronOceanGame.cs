using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class Stage4ElectronOceanGame : MonoBehaviour
{
    [Header("Set by Stage 4 Builder")]
    public int Level=1;
    public Sprite EnergySprite;
    public GameObject PauseButtonPrefab;
    public GameObject PauseMenuPrefab;
    private readonly List<Stage4Draggable> tokens=new List<Stage4Draggable>();
    private readonly List<Stage4DropZone> zones=new List<Stage4DropZone>();
    private readonly List<Image> energyIcons=new List<Image>();
    private readonly List<RectTransform> driftElectrons=new List<RectTransform>();
    private GameObject pausePanel, failedPanel, winPanel, noEnergyPanel, latticeGroup;
    private TMP_Text feedback,progressText,score,finalScore,timer,energyCount,modeLabel,forceLabel;
    private Button retryButton, switchButton, checkButton;
    private Slider forceSlider;
    private RectTransform upperLayer, lowerLayer;
    private GameObject driftPuzzlePanel;
    private TMP_Text driftPuzzleFeedback;
    private Stage4DriftSlot[] driftPuzzleSlots;
    private Stage4DriftTile[] driftPuzzleTiles;
    private GameObject[] metalChallengePanels;
    private Stage4DriftSlot[][] metalChallengeSlots;
    private Stage4DriftTile[][] metalChallengeTiles;
    private TMP_Text[] metalChallengeMessages;
    private int metalChallengeStep;
    private int mistakes,metalPlaced,electronsPlaced,terminalsPlaced;
    private bool failed,completed,paused,powerOn,observedDrift,forceApplied;
    private float nextTick,driftClock;
    private const string MetalKey="Stage4SelectedMetal";
    private static readonly Color Bg=new Color32(10,22,47,255);
    private static readonly Color Panel=new Color32(23,44,76,255);
    private static readonly Color Gold=new Color32(255,207,89,255);
    private static readonly Color Blue=new Color32(56,189,248,255);
    private static readonly Color Dim=new Color32(183,203,223,255);
    public bool CanInteract => !failed&&!completed&&!paused&&Time.timeScale>0f &&
        EnergyManager.Instance!=null&&EnergyManager.Instance.HasEnergy;
    public bool IsLevel1 => Level==1;
    private string Metal => PlayerPrefs.GetString(MetalKey,"Cu")=="Mg"?"Mg":"Cu";
    private string Title => Level==1?"LATTICE & ELECTRON CLOUD":
        Level==2?"VOLTAGE DRIFT & CONDUCTIVITY":"MALLEABILITY & STRESS TEST";

    private void Start()
    {
        Time.timeScale=1f;
        SetupInput();
        if(EnergyManager.Instance==null)
            new GameObject("SharedEnergyManager",typeof(EnergyManager));
        if(GameObject.Find("Stage4Level"+Level+"Canvas")==null)BuildEditorScene();
        if(IsLevel1)EnsureLevel1Decoys();
        Bind();
        if(Level==2)EnsureDriftPuzzle();
        if(Level==3)
        {
            EnsureMetalChallenges();
            PolishLevel3Presentation();
            PolishLevel3GameUILayout();
        }
        UpdateEnergy();
    }

    public void BuildEditorScene()
    {
        if(GameObject.Find("Stage4Level"+Level+"Canvas")!=null)return;
        var root=new GameObject("Stage4Level"+Level+"Canvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var cv=root.GetComponent<Canvas>();cv.renderMode=RenderMode.ScreenSpaceOverlay;
        var s=root.GetComponent<CanvasScaler>();
        s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution=new Vector2(1280,720);
        s.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight=1f;

        var bg=Rect(root.transform,"Background",Vector2.zero,new Vector2(1280,720),Bg);
        bg.raycastTarget=false;
        var br=bg.rectTransform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;
        br.offsetMin=Vector2.zero;br.offsetMax=Vector2.zero;

        Label(root.transform,"StageTitle","STAGE 4  |  LEVEL "+Level,20,
            new Vector2(0,325),new Vector2(690,36),Blue);
        Label(root.transform,"LevelTitle",Title,31,
            new Vector2(0,276),new Vector2(960,47),Color.white);
        Label(root.transform,"Instructions",
            Level==1?"Select Cu or Mg. Place matching metal cores and electrons; ignore decoys.":
            Level==2?"Attach negative and positive terminals, switch ON, and observe electron drift.":
            "Apply force and observe metal layers slide while delocalized electrons keep the cores bonded.",
            17,Level==1?new Vector2(0,-239):
                Level==2?new Vector2(0,101):new Vector2(0,223),
            Level==1?new Vector2(1000,20):
                Level==2?new Vector2(1020,28):new Vector2(1040,42),
            new Color32(183,203,223,255));
        score=Label(root.transform,"ScoreText","SCORE\n000000",19,new Vector2(533,294),new Vector2(135,68),Gold);
        Label(root.transform,"EnergyTitle","ENERGY",14,new Vector2(-500,338),new Vector2(110,24),Color.white);
        for(int i=0;i<5;i++)
        {
            var icon=Rect(root.transform,"Energy"+(i+1),new Vector2(-520+i*44,290),
                new Vector2(36,47),Color.white);
            icon.sprite=EnergySprite;icon.preserveAspect=true;
            if(EnergySprite==null)icon.color=Gold;
        }
        Label(root.transform,"EnergyCount","5/5",15,new Vector2(-304,334),new Vector2(85,25),Color.white);
        var board=Rect(root.transform,"ExperimentBoard",
            Level==1||Level==2?new Vector2(0,-35):new Vector2(0,-26),
            Level==1?new Vector2(1110,520):
                Level==2?new Vector2(1110,530):new Vector2(1110,444),Panel);
        board.raycastTarget=false;

        if(Level==1)BuildLevel1(board.transform);
        if(Level==2)BuildLevel2(board.transform);
        if(Level==3)BuildLevel3(board.transform);

        if(Level==1)
            Label(root.transform,"ProgressText",
                "SLOTS FILLED: 0/10  |  LATTICE: 0/6  |  CLOUD: 0/4",
                15,new Vector2(0,-261),new Vector2(900,20),Blue);
        feedback=Label(root.transform,"FeedbackText",Level==1
                ?"Decoys can be placed. CHECK evaluates the final model."
                :Level==3?"Drag the slider to apply force, then press CHECK.":
                    "Follow the instructions to complete the metallic bonding model.",
            16,Level==1?new Vector2(0,-283):
                Level==2?new Vector2(0,-275):new Vector2(0,-266),
            Level==1?new Vector2(1020,20):
                Level==2?new Vector2(1030,25):new Vector2(1080,45),Gold);
        Button(root.transform,"CheckButton","CHECK",new Vector2(0,-326),
            new Vector2(250,55),new Color32(12,132,135,255));
        Button(root.transform,"ResetButton","RESET",new Vector2(-268,-326),
            new Vector2(196,55),new Color32(72,88,118,255));

        if(PauseButtonPrefab!=null)
        {
            var pause=Instantiate(PauseButtonPrefab,root.transform,false);
            pause.name="Stage4PauseButton";
            var r=pause.GetComponent<RectTransform>();
            if(r!=null){r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(0,1);r.anchoredPosition=new Vector2(59,-56);}
        }
        else Button(root.transform,"Stage4PauseButton","MENU",
            new Vector2(-518,220),new Vector2(145,52),new Color32(41,77,114,255));

        if(PauseMenuPrefab!=null)
        {
            pausePanel=Instantiate(PauseMenuPrefab,root.transform,false);
            pausePanel.name="PauseMenuPanel";
            var r=pausePanel.GetComponent<RectTransform>();
            if(r!=null){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
        }
        else
        {
            pausePanel=Modal(root.transform,"PauseMenuPanel","GAME PAUSED","");
            Button(pausePanel.transform,"ContinueButton","CONTINUE",
                new Vector2(0,15),new Vector2(300,57),Blue);
            Button(pausePanel.transform,"LevelSelectionButton","LEVEL SELECTION",
                new Vector2(0,-73),new Vector2(300,57),Blue);
            Button(pausePanel.transform,"StageSelectionButton","STAGE SELECTION",
                new Vector2(0,-158),new Vector2(300,57),Blue);
        }
        pausePanel.SetActive(false);

        failedPanel=Modal(root.transform,"FailedPanel","STAGE FAILED!",
            "Two wrong attempts. One energy has been deducted.");
        Button(failedPanel.transform,"RetryButton","RETRY",new Vector2(0,-93),
            new Vector2(286,61),new Color32(12,132,135,255));
        Button(failedPanel.transform,"FailedLevelSelectionButton","LEVEL SELECTION",
            new Vector2(0,-161),new Vector2(296,50),new Color32(70,86,119,255));
        failedPanel.SetActive(false);

        winPanel=Modal(root.transform,"CompletePanel","LEVEL COMPLETE!",
            Level==3?"Malleability and ductility challenges successfully completed!":
                "Metallic bonding model successfully completed.");
        finalScore=Label(winPanel.transform,"FinalScoreText","FINAL SCORE: 000000",27,
            new Vector2(0,-36),new Vector2(610,56),Gold);
        Button(winPanel.transform,"NextButton",Level==3?"STAGE SELECTION":"NEXT LEVEL",
            new Vector2(144,-151),new Vector2(255,58),new Color32(12,132,135,255));
        Button(winPanel.transform,"LevelSelectButton","LEVEL SELECTION",
            new Vector2(-144,-151),new Vector2(255,58),new Color32(70,86,119,255));
        winPanel.SetActive(false);

        noEnergyPanel=Modal(root.transform,"OutOfEnergyPanel","OUT OF ENERGY!",
            "Your shared energy will regenerate automatically.");
        Label(noEnergyPanel.transform,"EnergyTimerText","Next Energy In: 15:00",25,
            new Vector2(0,-48),new Vector2(570,60),Gold);
        Button(noEnergyPanel.transform,"EnergyBackButton","LEVEL SELECTION",
            new Vector2(0,-144),new Vector2(300,58),new Color32(12,132,135,255));
        noEnergyPanel.SetActive(false);
        PolishEditorScene();
    }

    // Reusable Edit-Mode polish: keeps every gameplay element in the Hierarchy.
    // Safe to rerun after design adjustments. Does not replace existing levels.
    public void PolishEditorScene()
    {
        var go=GameObject.Find("Stage4Level"+Level+"Canvas");
        if(go==null)return;
        var root=go.transform;
        var bg=Find(root,"Background")?.GetComponent<Image>();
        if(bg!=null)bg.color=new Color32(8,19,39,255);

        var board=Find(root,"ExperimentBoard")?.GetComponent<Image>();
        if(board!=null)
        {
            board.color=new Color32(19,43,70,255);
            var shadow=board.GetComponent<Shadow>();
            if(shadow==null)shadow=board.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(0f,.015f,.06f,.72f);
            shadow.effectDistance=new Vector2(0,-7);
            shadow.useGraphicAlpha=true;
            var outline=board.GetComponent<Outline>();
            if(outline==null)outline=board.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color32(47,108,149,180);
            outline.effectDistance=new Vector2(1.5f,1.5f);
        }
        foreach(var heading in new[]{"StageTitle","LevelTitle","LatticeHeading","CloudHeading",
            "SlabTitle","LayerHeading"})
        {
            var text=Find(root,heading)?.GetComponent<TMP_Text>();
            if(text==null)continue;
            text.fontStyle=FontStyles.Bold;
            text.enableAutoSizing=true;
            text.fontSizeMin=15;
            text.fontSizeMax=heading=="LevelTitle"?32:25;
        }
        var instruction=Find(root,"Instructions")?.GetComponent<TMP_Text>();
        if(instruction!=null)
        {
            instruction.enableAutoSizing=true;
            instruction.fontSizeMin=14;
            instruction.fontSizeMax=18;
            instruction.color=new Color32(221,234,249,255);
        }
        var status=Find(root,"FeedbackText")?.GetComponent<TMP_Text>();
        if(status!=null)
        {
            status.enableAutoSizing=true;
            status.fontSizeMin=12;
            status.fontSizeMax=18;
            status.fontStyle=FontStyles.Bold;
        }

        foreach(var b in root.GetComponentsInChildren<Button>(true))
        {
            var colors=b.colors;
            colors.highlightedColor=new Color32(225,245,255,255);
            colors.pressedColor=new Color32(153,208,232,255);
            colors.selectedColor=new Color32(208,237,252,255);
            colors.disabledColor=new Color32(107,122,145,160);
            colors.fadeDuration=.15f;
            b.colors=colors;
            var label=b.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)
            {
                label.fontStyle=FontStyles.Bold;
                label.enableAutoSizing=true;
                label.fontSizeMin=12;
                label.fontSizeMax=20;
            }
        }
        // Stronger, distinct zones for metal cores, electrons and terminals.
        foreach(var zone in root.GetComponentsInChildren<Stage4DropZone>(true))
        {
            var image=zone.GetComponent<Image>();
            if(image!=null)image.color=zone.Kind==0
                ?new Color32(39,91,125,255)
                :zone.Kind==1?new Color32(104,85,50,255)
                :new Color32(57,94,123,255);
            var outline=zone.GetComponent<Outline>();
            if(outline==null)outline=zone.gameObject.AddComponent<Outline>();
            outline.effectColor=zone.Kind==1?new Color32(251,196,83,210):
                new Color32(83,195,241,210);
            outline.effectDistance=new Vector2(1.5f,1.5f);
            var label=zone.GetComponentInChildren<TMP_Text>(true);
            if(label!=null){label.fontStyle=FontStyles.Bold;label.color=Gold;}
        }
        foreach(var token in root.GetComponentsInChildren<Stage4Draggable>(true))
        {
            var image=token.GetComponent<Image>();
            if(image==null)continue;
            var shadow=token.GetComponent<Shadow>();
            if(shadow==null)shadow=token.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(0,0,.05f,.7f);
            shadow.effectDistance=new Vector2(1,-3);
        }
        // Re-layout existing saved scenes too: no deletion of Hierarchy objects.
        var boardRect=Find(root,"ExperimentBoard")?.GetComponent<RectTransform>();
        if(boardRect!=null)
        {
            boardRect.anchoredPosition=Level==1||Level==2?new Vector2(0,-35):new Vector2(0,-3);
            boardRect.sizeDelta=Level==1?new Vector2(1110,520):
                Level==2?new Vector2(1110,530):new Vector2(1110,398);
        }
        SetPosition(root,"FeedbackText",Level==1?new Vector2(0,-283):
            Level==2?new Vector2(0,-275):new Vector2(0,-266));
        if(Level==2)
        {
            SetPosition(root,"StageTitle",new Vector2(0,322));
            SetPosition(root,"LevelTitle",new Vector2(0,277));
            var stage=Find(root,"StageTitle");if(stage!=null)stage.gameObject.SetActive(false);
            var level=Find(root,"LevelTitle");if(level!=null)level.gameObject.SetActive(false);
            SetPosition(root,"Instructions",new Vector2(0,101));
            SetPosition(root,"SlabTitle",new Vector2(0,180));
            SetPosition(root,"MetalSlab",new Vector2(0,18));
            SetPosition(root,"NegativeTerminalSlot",new Vector2(-379,20));
            SetPosition(root,"PositiveTerminalSlot",new Vector2(379,20));
            SetPosition(root,"TerminalBank",new Vector2(0,-130));
            SetPosition(root,"DriftLabel",new Vector2(0,-168));
            SetPosition(root,"VoltageSwitch",new Vector2(0,-208));
            for(int i=0;i<9;i++)
                SetPosition(root,"Core"+i,new Vector2(-190+(i%3)*190,
                    i/3==0?72:i/3==1?26:-72));
            for(int i=0;i<7;i++)
                SetPosition(root,"DriftingElectron"+i,new Vector2(-245+i*76,-23));
        }
        if(Level==1)
        {
            SetPosition(root,"Instructions",new Vector2(0,-239));
            SetPosition(root,"ProgressText",new Vector2(0,-261));
        }
        SetPosition(root,"CheckButton",new Vector2(0,-326));
        SetPosition(root,"ResetButton",new Vector2(-268,-326));
        if(Level==1)
        {
            SetPosition(root,"LatticeHeading",new Vector2(-230,167));
            SetPosition(root,"CloudHeading",new Vector2(270,167));
            SetPosition(root,"CopperButton",new Vector2(-320,116));
            SetPosition(root,"MagnesiumButton",new Vector2(-122,116));
            SetPosition(root,"CurrentMetal",new Vector2(-230,68));
            SetPosition(root,"MetalTokenBank",new Vector2(0,-157));
            for(int i=0;i<6;i++)
            {
                var zone=Find(root,"LatticeSlot"+i)?.GetComponent<RectTransform>();
                if(zone!=null)
                {
                    zone.anchoredPosition=new Vector2(-375+(i%3)*124,-(i/3)*79);
                    zone.sizeDelta=new Vector2(76,66);
                }
                SetPosition(root,"MetalCore"+i,new Vector2(-409+i*105,0));
            }
            for(int i=0;i<4;i++)
            {
                var zone=Find(root,"CloudSlot"+i)?.GetComponent<RectTransform>();
                if(zone!=null)
                {
                    zone.anchoredPosition=new Vector2(150+(i%2)*165,-(i/2)*85);
                    zone.sizeDelta=new Vector2(94,69);
                }
                SetPosition(root,"FreeElectron"+i,new Vector2(244+i*76,0));
            }
            var tokenBank=Find(root,"MetalTokenBank")?.GetComponent<RectTransform>();
            if(tokenBank!=null)tokenBank.sizeDelta=new Vector2(1024,62);
        }
        foreach(var token in root.GetComponentsInChildren<Stage4Draggable>(true))
        {
            var rect=token.GetComponent<RectTransform>();
            if(rect!=null)rect.sizeDelta=new Vector2(58,50);
            var label=token.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)
            {
                if(token.Kind==0)label.text=Metal=="Mg"?"Mg2+":"Cu+";
                else if(token.Kind==1)label.text="e-";
                else if(token.Kind==2)label.text="-";
                else if(token.Kind==3)label.text="+";
                else if(token.Kind>=4 && Level==1)
                {
                    label.fontSize=17;
                    label.enableAutoSizing=true;
                    label.fontSizeMin=14;
                    label.fontSizeMax=17;
                    label.textWrappingMode=TextWrappingModes.NoWrap;
                    var labelRect=label.GetComponent<RectTransform>();
                    if(labelRect!=null)labelRect.sizeDelta=new Vector2(56,43);
                }
            }
        }
        foreach(var label in root.GetComponentsInChildren<TMP_Text>(true))
        {
            // Avoid fallback-font warnings caused by unsupported unicode
            // superscript symbols in user-created metal-core labels.
            if(label.text=="M⁺")label.text="M+";
            if(label.text=="e⁻")label.text="e-";
            if(label.text=="＋")label.text="+";
            if(label.text=="−")label.text="-";
        }
        if(Level==2)
        {
            SetPosition(root,"TerminalBank",new Vector2(0,-91));
            SetPosition(root,"VoltageSwitch",new Vector2(0,-156));
            SetPosition(root,"DriftLabel",new Vector2(0,-127));
        }
    }

    private void SetPosition(Transform root,string name,Vector2 position)
    {
        var rect=Find(root,name)?.GetComponent<RectTransform>();
        if(rect!=null)rect.anchoredPosition=position;
    }

    private void BuildLevel1(Transform board)
    {
        Label(board,"LatticeHeading","METAL CATION LATTICE",19,new Vector2(-230,167),
            new Vector2(490,34),Blue);
        Label(board,"CloudHeading","DELOCALIZED ELECTRONS",19,new Vector2(270,167),
            new Vector2(440,34),Blue);
        Button(board,"CopperButton","COPPER (Cu)",new Vector2(-320,116),
            new Vector2(168,47),new Color32(107,65,43,255));
        Button(board,"MagnesiumButton","MAGNESIUM (Mg)",new Vector2(-122,116),
            new Vector2(193,47),new Color32(56,94,114,255));
        Label(board,"CurrentMetal","Selected: Copper (Cu)",16,
            new Vector2(-230,68),new Vector2(420,35),Gold);
        for(int i=0;i<6;i++)
        {
            int x=i%3,y=i/3;
            AddZone(board,"LatticeSlot"+i,0,new Vector2(-375+x*124,0-y*79),new Vector2(76,66));
        }
        for(int i=0;i<4;i++)
            AddZone(board,"CloudSlot"+i,1,new Vector2(150+(i%2)*165,0-(i/2)*85),new Vector2(94,69));
        var bank=Rect(board,"MetalTokenBank",new Vector2(0,-157),new Vector2(1024,62),
            new Color32(12,28,51,255));
        bank.raycastTarget=false;
        for(int i=0;i<6;i++)
            AddToken(bank.transform,"MetalCore"+i,0,new Vector2(-409+i*105,0),
                new Color32(75,155,190,255),"M+");
        for(int i=0;i<4;i++)
            AddToken(bank.transform,"FreeElectron"+i,1,
                new Vector2(244+i*76,0),new Color32(244,189,75,255),"e-");
    }

    // Keep six matching metal cores and four genuine electrons, plus four
    // visually distinct distractors. Scene objects are preserved if already saved
    // by the optional Edit Mode seeder.
    public void EnsureLevel1Decoys()
    {
        if(!IsLevel1)return;
        var root=GameObject.Find("Stage4Level1Canvas");
        if(root==null)return;
        var bank=Find(root.transform,"MetalTokenBank");
        if(bank==null)return;
        var templateCore=Find(bank,"MetalCore0")?.GetComponent<Stage4Draggable>();
        var templateElectron=Find(bank,"FreeElectron0")?.GetComponent<Stage4Draggable>();
        if(templateCore==null||templateElectron==null)return;

        string[] names={"DecoyFe","DecoyAl","DecoyProton","DecoyNeutron"};
        string[] symbols={"Fe2+","Al3+","p+","n0"};
        int[] kinds={4,4,5,5};
        for(int i=0;i<names.Length;i++)
        {
            var existing=Find(bank,names[i]);
            if(existing!=null)continue;
            var source=kinds[i]==4?templateCore:templateElectron;
            var clone=Instantiate(source.gameObject,bank,false);
            clone.name=names[i];
            var token=clone.GetComponent<Stage4Draggable>();
            token.Kind=kinds[i];
            var label=clone.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)
            {
                label.text=symbols[i];
                label.fontSize=17;
                label.enableAutoSizing=true;
                label.fontSizeMin=14;
                label.fontSizeMax=17;
                label.textWrappingMode=TextWrappingModes.NoWrap;
                var labelRect=label.GetComponent<RectTransform>();
                if(labelRect!=null)labelRect.sizeDelta=new Vector2(56,43);
            }
            var image=clone.GetComponent<Image>();
            if(image!=null)image.color=new Color32(151,119,201,255);
        }

        // All fourteen choices are centered and visible at the start of Play.
        // The order intentionally mixes legitimate ions/electrons and decoys.
        string[] ordered={"MetalCore0","FreeElectron0","DecoyFe",
            "MetalCore1","FreeElectron1","MetalCore2","DecoyProton",
            "MetalCore3","FreeElectron2","MetalCore4","DecoyAl",
            "FreeElectron3","MetalCore5","DecoyNeutron"};
        for(int i=0;i<ordered.Length;i++)
        {
            var rect=Find(bank,ordered[i])?.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchoredPosition=new Vector2(-455+i*70,0);
                rect.sizeDelta=new Vector2(58,50);
            }
        }
        var bankRect=bank.GetComponent<RectTransform>();
        if(bankRect!=null)bankRect.sizeDelta=new Vector2(1024,62);
    }

    private void BuildLevel2(Transform board)
    {
        Label(board,"SlabTitle","METAL SLAB  |  VOLTAGE EXPERIMENT",24,
            new Vector2(0,180),new Vector2(880,38),Blue);
        var slab=Rect(board,"MetalSlab",new Vector2(0,18),new Vector2(600,200),
            new Color32(31,75,105,255));
        slab.raycastTarget=false;
        for(int i=0;i<9;i++)
        {
            int x=i%3,y=i/3;
            Label(slab.transform,"Core"+i,"M+",28,
                new Vector2(-190+x*190,y==0?72:y==1?26:-72),
                new Vector2(75,42),Dim);
        }
        for(int i=0;i<7;i++)
            Label(slab.transform,"DriftingElectron"+i,"e-",22,
                new Vector2(-245+i*76,-23),new Vector2(56,35),Gold);
        AddZone(board,"NegativeTerminalSlot",2,new Vector2(-379,20),new Vector2(115,94));
        AddZone(board,"PositiveTerminalSlot",3,new Vector2(379,20),new Vector2(115,94));
        var bank=Rect(board,"TerminalBank",new Vector2(0,-130),
            new Vector2(510,52),new Color32(12,28,51,255));
        bank.raycastTarget=false;
        AddToken(bank.transform,"NegativeTerminal",2,new Vector2(-123,0),
            new Color32(82,155,200,255),"-");
        AddToken(bank.transform,"PositiveTerminal",3,new Vector2(123,0),
            new Color32(235,149,70,255),"+");
        Button(board,"VoltageSwitch","SWITCH: OFF",new Vector2(0,-208),
            new Vector2(260,42),new Color32(87,103,133,255));
        Label(board,"DriftLabel","Electron drift: OFF  |  conventional current flows opposite",
            14,new Vector2(0,-168),new Vector2(740,28),Dim);
    }

    private void BuildLevel3(Transform board)
    {
        Label(board,"LayerHeading","METALLIC LATTICE UNDER STRESS",20,
            new Vector2(0,143),new Vector2(610,32),Blue);
        Label(board,"ForceInfo","Apply force with the slider to shift the upper metal layer.",
            17,new Vector2(0,110),new Vector2(750,35),Dim);
        for(int layer=0;layer<2;layer++)
        {
            var layerGO=Rect(board,layer==0?"UpperMetalLayer":"LowerMetalLayer",
                new Vector2(0,layer==0?56:-33),new Vector2(730,58),
                new Color32(35,77,107,255));
            layerGO.raycastTarget=false;
            for(int i=0;i<6;i++)
                Label(layerGO.transform,"MetalCore"+i,"M+",30,
                    new Vector2(-308+i*123,0),new Vector2(77,50),Blue);
        }
        for(int i=0;i<7;i++)
            Label(board,"ElectronCloud"+i,"e-",19,
                new Vector2(-336+i*112,12),new Vector2(47,22),Gold);
        var sliderGO=new GameObject("ForceSlider",typeof(RectTransform),typeof(Slider));
        sliderGO.transform.SetParent(board,false);
        var rect=sliderGO.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=new Vector2(0,-86);
        rect.sizeDelta=new Vector2(650,42);
        var bg=Rect(sliderGO.transform,"Background",Vector2.zero,
            new Vector2(650,16),new Color32(55,87,113,255));
        var fill=Rect(sliderGO.transform,"Fill",Vector2.zero,
            new Vector2(650,16),new Color32(18,157,143,255));
        var handle=Rect(sliderGO.transform,"Handle",Vector2.zero,
            new Vector2(30,34),Gold);
        var slide=sliderGO.GetComponent<Slider>();slide.direction=Slider.Direction.LeftToRight;
        slide.minValue=0;slide.maxValue=1;slide.value=0;
        slide.fillRect=fill.rectTransform;slide.handleRect=handle.rectTransform;
        slide.targetGraphic=handle;
        Label(board,"ForceValue","FORCE: 0%  |  LAYERS ALIGNED",17,
            new Vector2(0,-126),new Vector2(690,28),Gold);
        Label(board,"BindingNote","Delocalized electrons continue to attract metal cores as layers move.",
            14,new Vector2(0,-155),new Vector2(880,22),Dim);
    }

    private void AddZone(Transform parent,string name,int kind,Vector2 p,Vector2 size)
    {
        var image=Rect(parent,name,p,size,new Color32(39,78,108,255));
        var zone=image.gameObject.AddComponent<Stage4DropZone>();zone.Kind=kind;
        Label(image.transform,"SlotLabel",kind==0?"+":kind==1?".":kind==2?"-":"+",25,
            Vector2.zero,size,Gold);
    }
    private void AddToken(Transform parent,string name,int kind,Vector2 p,Color color,string symbol)
    {
        var image=Rect(parent,name,p,new Vector2(58,50),color);
        image.gameObject.AddComponent<CanvasGroup>();
        var drag=image.gameObject.AddComponent<Stage4Draggable>();drag.Kind=kind;
        Label(image.transform,"Symbol",symbol,23,Vector2.zero,new Vector2(55,48),
            new Color32(9,26,51,255));
    }
    private Image Rect(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        var image=go.GetComponent<Image>();image.color=color;return image;
    }
    private TMP_Text Label(Transform parent,string name,string value,int fontSize,
        Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.fontSize=fontSize;
        t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.Normal;
        t.color=color;t.raycastTarget=false;
        return t;
    }
    private Button Button(Transform parent,string name,string value,Vector2 pos,Vector2 size,Color color)
    {
        var image=Rect(parent,name,pos,size,color);
        var button=image.gameObject.AddComponent<Button>();
        Label(image.transform,"Text",value,19,Vector2.zero,size-new Vector2(12,4),Color.white);
        return button;
    }
    private GameObject Modal(Transform parent,string name,string heading,string description)
    {
        var shade=Rect(parent,name,Vector2.zero,new Vector2(1280,720),
            new Color32(5,14,29,240));
        var card=Rect(shade.transform,"Card",Vector2.zero,
            new Vector2(700,400),Panel);
        card.raycastTarget=false;
        Label(shade.transform,"Heading",heading,37,new Vector2(0,114),new Vector2(670,75),Gold);
        Label(shade.transform,"Description",description,21,
            new Vector2(0,40),new Vector2(660,65),Color.white);
        return shade.gameObject;
    }
    private Transform Find(Transform root,string name)
    {
        foreach(var x in root.GetComponentsInChildren<Transform>(true))
            if(x.name==name)return x;
        return null;
    }
    private void Wire(Transform root,string name,Action action)
    {
        foreach(var b in root.GetComponentsInChildren<Button>(true))
        {
            if(b.name!=name)continue;
            b.onClick=new Button.ButtonClickedEvent();
            b.onClick.AddListener(()=>action());
        }
    }
    private void SetupInput()
    {
        var e=FindObjectOfType<EventSystem>();
        if(e==null)e=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var old in e.GetComponents<StandaloneInputModule>())old.enabled=false;
        if(e.GetComponent<InputSystemUIInputModule>()==null)
        {
            var module=e.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
    // Keeps all completion/navigation controls visible as editable scene objects.
    // Runtime Bind() attaches the actual OnClick handlers whenever scene opens.
    public void EnsureNavigationButtons()
    {
        var root=GameObject.Find("Stage4Level"+Level+"Canvas");
        if(root==null)return;
        foreach(var name in new[]{"NextButton","LevelSelectButton","LevelSelectionButton",
            "StageSelectionButton","FailedLevelSelectionButton","EnergyBackButton","RetryButton"})
        {
            var target=Find(root.transform,name);
            if(target==null)
            {
                Debug.LogWarning("Stage 4 Level "+Level+" missing navigation button: "+name);
                continue;
            }
            var button=target.GetComponent<Button>();
            if(button==null)button=target.gameObject.AddComponent<Button>();
            button.interactable=true;
            var image=target.GetComponent<Image>();
            if(image!=null)
            {
                image.raycastTarget=true;
                if(button.targetGraphic==null)button.targetGraphic=image;
            }
            var text=target.GetComponentInChildren<TMP_Text>(true);
            if(text!=null)text.raycastTarget=false;
        }
    }

    private void Bind()
    {
        var root=GameObject.Find("Stage4Level"+Level+"Canvas");
        if(root==null)return;
        var t=root.transform;
        pausePanel=Find(t,"PauseMenuPanel")?.gameObject;
        failedPanel=Find(t,"FailedPanel")?.gameObject;
        winPanel=Find(t,"CompletePanel")?.gameObject;
        noEnergyPanel=Find(t,"OutOfEnergyPanel")?.gameObject;
        feedback=Find(t,"FeedbackText")?.GetComponent<TMP_Text>();
        progressText=Find(t,"ProgressText")?.GetComponent<TMP_Text>();
        score=Find(t,"ScoreText")?.GetComponent<TMP_Text>();
        finalScore=Find(t,"FinalScoreText")?.GetComponent<TMP_Text>();
        timer=Find(t,"EnergyTimerText")?.GetComponent<TMP_Text>();
        energyCount=Find(t,"EnergyCount")?.GetComponent<TMP_Text>();
        modeLabel=Find(t,"CurrentMetal")?.GetComponent<TMP_Text>();
        forceLabel=Find(t,"ForceValue")?.GetComponent<TMP_Text>();
        retryButton=Find(t,"RetryButton")?.GetComponent<Button>();
        checkButton=Find(t,"CheckButton")?.GetComponent<Button>();
        forceSlider=Find(t,"ForceSlider")?.GetComponent<Slider>();
        upperLayer=Find(t,"UpperMetalLayer")?.GetComponent<RectTransform>();
        lowerLayer=Find(t,"LowerMetalLayer")?.GetComponent<RectTransform>();
        switchButton=Find(t,"VoltageSwitch")?.GetComponent<Button>();
        energyIcons.Clear();
        for(int i=0;i<5;i++)
        {
            var img=Find(t,"Energy"+(i+1))?.GetComponent<Image>();
            if(img!=null)energyIcons.Add(img);
        }
        tokens.Clear();zones.Clear();driftElectrons.Clear();
        foreach(var drag in t.GetComponentsInChildren<Stage4Draggable>(true))
        {
            drag.Bind(this);drag.ResetToken();tokens.Add(drag);
        }
        foreach(var zone in t.GetComponentsInChildren<Stage4DropZone>(true))
        {
            zone.Game=this;zone.ResetZone();zones.Add(zone);
        }
        for(int i=0;i<7;i++)
        {
            var electron=Find(t,"DriftingElectron"+i)?.GetComponent<RectTransform>();
            if(electron!=null)driftElectrons.Add(electron);
        }
        Wire(t,"CheckButton",Check);
        Wire(t,"ResetButton",Reset);
        Wire(t,"Stage4PauseButton",Pause);
        Wire(t,"ContinueButton",Resume);
        Wire(t,"LevelSelectionButton",()=>Go("Stage4LevelSelection"));
        Wire(t,"StageSelectionButton",()=>Go("StageSelection"));
        Wire(t,"RetryButton",()=>Go("Stage4Level"+Level));
        Wire(t,"FailedLevelSelectionButton",()=>Go("Stage4LevelSelection"));
        Wire(t,"LevelSelectButton",()=>Go("Stage4LevelSelection"));
        Wire(t,"NextButton",()=>Go(Level==3?"StageSelection":"Stage4Level"+(Level+1)));
        Wire(t,"EnergyBackButton",()=>Go("Stage4LevelSelection"));
        EnsureNavigationButtons();
        Wire(t,"CopperButton",()=>SelectMetal("Cu"));
        Wire(t,"MagnesiumButton",()=>SelectMetal("Mg"));
        Wire(t,"VoltageSwitch",ToggleVoltage);
        foreach(var b in t.GetComponentsInChildren<Button>(true))
            if(b.name=="StageSelectionButton")
            {
                var tmp=b.GetComponentInChildren<TMP_Text>(true);
                if(tmp!=null)tmp.text="STAGE SELECTION";
            }
        if(forceSlider!=null)
        {
            FixLevel3ForceSliderLayout();
            forceSlider.onValueChanged.RemoveAllListeners();
            forceSlider.onValueChanged.AddListener(OnForceChanged);
            forceSlider.value=0f;
        }
        metalPlaced=electronsPlaced=terminalsPlaced=mistakes=0;
        paused=failed=completed=powerOn=observedDrift=forceApplied=false;
        if(pausePanel!=null)pausePanel.SetActive(false);
        if(failedPanel!=null)failedPanel.SetActive(false);
        if(winPanel!=null)winPanel.SetActive(false);
        if(noEnergyPanel!=null)noEnergyPanel.SetActive(false);
        if(score!=null)score.text="SCORE\n000000";
        if(modeLabel!=null)modeLabel.text="Selected: "+(Metal=="Cu"?"Copper (Cu)":"Magnesium (Mg)");
        UpdateLevel1Progress();
    }

    public string GetPlacedSymbol(int kind) =>
        kind==0?(Metal=="Mg"?"Mg2+":"Cu+"):kind==1?"e-":kind==2?"-":"+";

    public void ZoneFilled(int kind)
    {
        if(kind==0)metalPlaced++;
        else if(kind==1)electronsPlaced++;
        else terminalsPlaced++;
        if(Level==1)
        {
            UpdateLevel1Progress();
        }
        else if(feedback!=null)feedback.text="Terminals attached: "+terminalsPlaced+"/2";
    }
    private void UpdateLevel1Progress()
    {
        if(Level!=1 || progressText==null)return;
        int filled=0;
        foreach(var zone in zones)
            if((zone.Kind==0 || zone.Kind==1) && zone.Occupied)filled++;
        progressText.text="SLOTS FILLED: "+filled+"/10  |  LATTICE: "+
            metalPlaced+"/6  |  CLOUD: "+electronsPlaced+"/4";
    }
    private void SelectMetal(string symbol)
    {
        if(!CanInteract||Level!=1)return;
        if(metalPlaced>0||electronsPlaced>0)
        {
            if(feedback!=null)feedback.text="Use RESET before changing the selected metal.";
            return;
        }
        PlayerPrefs.SetString(MetalKey,symbol);PlayerPrefs.Save();
        if(modeLabel!=null)modeLabel.text="Selected: "+(Metal=="Cu"?"Copper (Cu)":"Magnesium (Mg)");
        foreach(var token in tokens)
        {
            if(token.Kind!=0)continue;
            var label=token.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)label.text=symbol=="Mg"?"Mg2+":"Cu+";
        }
    }

    // Part 3 is a drag-and-drop interpretation task, not a multiple-choice quiz.
    // Keep it hidden until the terminals are connected and drift is observed.
    // Can also be serialized into the scene using the Editor menu item.
    public void EnsureDriftPuzzle()
    {
        if(Level!=2)return;
        var root=GameObject.Find("Stage4Level2Canvas");
        if(root==null)return;
        var existing=Find(root.transform,"DriftChallengeOverlay");
        if(existing==null)
        {
            var overlay=Rect(root.transform,"DriftChallengeOverlay",Vector2.zero,
                new Vector2(1280,720),new Color32(5,13,30,226));
            overlay.raycastTarget=true;
            var card=Rect(overlay.transform,"DriftChallengeCard",new Vector2(0,-15),
                new Vector2(1040,515),new Color32(23,44,76,255));
            card.raycastTarget=true;
            Label(card.transform,"ChallengeHeading","PART 3  |  ELECTRON DRIFT CHALLENGE",28,
                new Vector2(0,210),new Vector2(950,42),Blue);
            Label(card.transform,"ChallengeInstructions",
                "Drag the correct behavior tiles into all three slots. Decoys are allowed; CHECK MODEL validates.",
                16,new Vector2(0,165),new Vector2(950,36),Dim);

            string[] captions={"ELECTRON DRIFT","CONVENTIONAL CURRENT","METAL CATIONS"};
            string[] answers={"electron-right","current-left","ions-still"};
            for(int i=0;i<3;i++)
            {
                float x=-323+i*323;
                Label(card.transform,"ChallengeSlotHeading"+i,captions[i],17,
                    new Vector2(x,117),new Vector2(300,33),Blue);
                var slot=Rect(card.transform,"ChallengeDropSlot"+i,new Vector2(x,61),
                    new Vector2(294,79),new Color32(43,86,120,255));
                slot.gameObject.AddComponent<Stage4DriftSlot>().CorrectAnswerId=answers[i];
                Label(slot.transform,"Placeholder","DROP HERE",16,
                    Vector2.zero,new Vector2(270,48),new Color32(169,198,220,255));
            }

            var tray=Rect(card.transform,"ChallengeTileTray",new Vector2(0,-91),
                new Vector2(962,147),new Color32(11,29,51,255));
            tray.raycastTarget=false;
            string[] names={"ElectronRight","CurrentRight","IonsStill",
                "ElectronLeft","CurrentLeft","IonsMove"};
            string[] values={"ELECTRON -->","CURRENT -->","IONS STATIONARY",
                "ELECTRON <--","CURRENT <--","IONS MOVE"};
            string[] tileIds={"electron-right","current-right","ions-still",
                "electron-left","current-left","ions-move"};
            for(int i=0;i<names.Length;i++)
            {
                float x=-311+(i%3)*311;
                float y=i<3?37:-38;
                var tile=Rect(tray.transform,"DriftTile"+names[i],new Vector2(x,y),
                    new Vector2(290,52),new Color32(35,118,151,255));
                tile.gameObject.AddComponent<CanvasGroup>();
                tile.gameObject.AddComponent<Stage4DriftTile>().AnswerId=tileIds[i];
                var label=Label(tile.transform,"TileText",values[i],17,
                    Vector2.zero,new Vector2(276,45),Color.white);
                label.textWrappingMode=TextWrappingModes.NoWrap;
                label.enableAutoSizing=true;
                label.fontSizeMin=13;label.fontSizeMax=17;
            }

            Label(card.transform,"ChallengeFeedback",
                "Build the model. Wrong tiles only count after CHECK MODEL.",15,
                new Vector2(0,-183),new Vector2(940,29),Gold);
            Button(card.transform,"ChallengeResetButton","RESET TILES",
                new Vector2(-159,-232),new Vector2(252,53),new Color32(70,86,119,255));
            Button(card.transform,"ChallengeCheckButton","CHECK MODEL",
                new Vector2(159,-232),new Vector2(252,53),new Color32(12,132,135,255));
            existing=overlay.transform;
        }
        driftPuzzlePanel=existing.gameObject;
        var canvas=root.GetComponent<Canvas>();
        var trayTransform=Find(existing,"ChallengeTileTray") as RectTransform;
        driftPuzzleTiles=existing.GetComponentsInChildren<Stage4DriftTile>(true);
        driftPuzzleSlots=existing.GetComponentsInChildren<Stage4DriftSlot>(true);
        foreach(var tile in driftPuzzleTiles)
            tile.Configure(tile.AnswerId,trayTransform,Find(existing,"DriftChallengeCard"),canvas);
        driftPuzzleFeedback=Find(existing,"ChallengeFeedback")?.GetComponent<TMP_Text>();
        Wire(existing,"ChallengeResetButton",ResetDriftTiles);
        Wire(existing,"ChallengeCheckButton",CheckDriftPuzzle);
        ResetDriftTiles();
        driftPuzzlePanel.SetActive(false);
    }

    private void ResetDriftTiles()
    {
        if(driftPuzzleSlots!=null)
            foreach(var slot in driftPuzzleSlots)slot.ResetSlot();
        if(driftPuzzleTiles!=null)
            foreach(var tile in driftPuzzleTiles)tile.ReturnHome();
        if(driftPuzzleFeedback!=null)
            driftPuzzleFeedback.text="Arrange the directions, then press CHECK MODEL.";
    }

    private void CheckDriftPuzzle()
    {
        if(Level!=2 || driftPuzzlePanel==null || !driftPuzzlePanel.activeSelf || !CanInteract)return;
        int filled=0;
        bool correct=driftPuzzleSlots!=null && driftPuzzleSlots.Length==3;
        if(driftPuzzleSlots!=null)
            foreach(var slot in driftPuzzleSlots)
            {
                if(slot.IsFilled)filled++;
                if(!slot.IsCorrect)correct=false;
            }
        if(filled<3)
        {
            if(driftPuzzleFeedback!=null)
                driftPuzzleFeedback.text="Fill all three slots before checking. No penalty yet.";
            return;
        }
        if(!correct)
        {
            RegisterMistake();
            if(!failed && driftPuzzleFeedback!=null)
                driftPuzzleFeedback.text=
                    "INCORRECT MODEL. Check electron drift, current direction, and stationary ions.";
            return;
        }
        driftPuzzlePanel.SetActive(false);
        Complete();
    }

    private void ToggleVoltage()
    {
        if(Level!=2||!CanInteract)return;
        if(terminalsPlaced<2)
        {
            if(feedback!=null)feedback.text="Attach both terminals first (negative left, positive right).";
            RegisterMistake();return;
        }
        powerOn=!powerOn;
        var text=switchButton?.GetComponentInChildren<TMP_Text>(true);
        if(text!=null)text.text=powerOn?"SWITCH: ON":"SWITCH: OFF";
        if(powerOn)
        {
            driftClock=0;observedDrift=false;
            if(feedback!=null)feedback.text="Electron drift is toward the POSITIVE terminal (right).";
        }
    }

    // Stage 4 Level 3: one observed stress demonstration, then two
    // drag-and-drop checks. Only a fully filled incorrect CHECK MODEL
    // consumes a wrong attempt; placement, RESET and incomplete checks do not.
    public void EnsureMetalChallenges()
    {
        if(Level!=3)return;
        var root=GameObject.Find("Stage4Level3Canvas");
        if(root==null)return;
        var canvas=root.GetComponent<Canvas>();
        if(canvas==null)return;
        metalChallengePanels=new GameObject[2];
        metalChallengeSlots=new Stage4DriftSlot[2][];
        metalChallengeTiles=new Stage4DriftTile[2][];
        metalChallengeMessages=new TMP_Text[2];
        for(int phase=0;phase<2;phase++)
        {
            string overlayName="MetalChallengeOverlay"+(phase+1);
            var overlay=Find(root.transform,overlayName);
            if(overlay==null)
                overlay=BuildMetalChallenge(root.transform,phase);
            metalChallengePanels[phase]=overlay.gameObject;
            metalChallengeSlots[phase]=overlay.GetComponentsInChildren<Stage4DriftSlot>(true);
            metalChallengeTiles[phase]=overlay.GetComponentsInChildren<Stage4DriftTile>(true);
            var tray=Find(overlay,"MetalChallengeTray") as RectTransform;
            var card=Find(overlay,"MetalChallengeCard");
            foreach(var tile in metalChallengeTiles[phase])
                tile.Configure(tile.AnswerId,tray,card,canvas);
            metalChallengeMessages[phase]=Find(overlay,"MetalChallengeFeedback")?.GetComponent<TMP_Text>();
            int captured=phase;
            Wire(overlay,"MetalChallengeReset",()=>ResetMetalTiles(captured));
            Wire(overlay,"MetalChallengeCheck",()=>CheckMetalChallenge(captured));
            ResetMetalTiles(phase);
            overlay.gameObject.SetActive(false);
        }
        metalChallengeStep=0;
    }

    private Transform BuildMetalChallenge(Transform root,int phase)
    {
        bool sheet=phase==0;
        var overlay=Rect(root,"MetalChallengeOverlay"+(phase+1),Vector2.zero,
            new Vector2(1280,720),new Color32(5,13,30,235));
        overlay.raycastTarget=true;
        var card=Rect(overlay.transform,"MetalChallengeCard",new Vector2(0,-10),
            new Vector2(1060,540),new Color32(23,44,76,255));
        card.raycastTarget=true;
        Label(card.transform,"MetalChallengeHeading",
            sheet?"PART 2 | MALLEABILITY: FORM A SHEET":
                  "PART 3 | DUCTILITY: FORM A WIRE",
            26,new Vector2(0,228),new Vector2(990,42),Blue);
        Label(card.transform,"MetalChallengeInstruction",
            sheet?"Slide the metallic layers into a sheet without breaking the metallic bond.":
                  "Stretch the metallic structure into a wire without losing metallic bonding.",
            16,new Vector2(0,179),new Vector2(990,36),Dim);
        string[] targets=sheet?
            new[]{"LAYER RESPONSE","BONDING FORCE","FINAL SHAPE"}:
            new[]{"DRAWING ACTION","BONDING CONDITION","FINAL SHAPE"};
        string[] correct=sheet?
            new[]{"layers-slide","electrons-bond","sheet-forms"}:
            new[]{"layers-extend","bond-stays","wire-forms"};
        for(int i=0;i<3;i++)
        {
            float x=-337+i*337;
            Label(card.transform,"MetalSlotTitle"+i,targets[i],17,
                new Vector2(x,130),new Vector2(310,30),Blue);
            var slot=Rect(card.transform,"MetalDropSlot"+i,new Vector2(x,71),
                new Vector2(305,82),new Color32(43,86,120,255));
            slot.gameObject.AddComponent<Stage4DriftSlot>().CorrectAnswerId=correct[i];
            Label(slot.transform,"Placeholder","DROP HERE",16,Vector2.zero,
                new Vector2(285,53),Dim);
        }
        var tray=Rect(card.transform,"MetalChallengeTray",new Vector2(0,-100),
            new Vector2(998,155),new Color32(11,29,51,255));
        tray.raycastTarget=false;
        string[] ids=sheet?
            new[]{"layers-slide","sheet-breaks","sheet-forms",
                  "electrons-stop","electrons-bond","cores-escape"}:
            new[]{"wire-forms","bond-breaks","layers-extend",
                  "ions-scatter","bond-stays","sheet-forms-decoy"};
        string[] labels=sheet?
            new[]{"LAYERS SLIDE","METAL CRACKS","SHEET FORMS",
                  "ELECTRONS STOP","ELECTRONS HOLD BOND","CORES ESCAPE"}:
            new[]{"WIRE FORMS","BONDS BREAK","STRUCTURE EXTENDS",
                  "IONS SCATTER","METALLIC BOND REMAINS","SHEET FORMS"};
        for(int i=0;i<ids.Length;i++)
        {
            float x=-330+(i%3)*330;
            float y=i<3?42:-39;
            var tile=Rect(tray.transform,"MetalTile"+phase+"_"+i,new Vector2(x,y),
                new Vector2(306,56),new Color32(36,117,151,255));
            tile.gameObject.AddComponent<CanvasGroup>();
            tile.gameObject.AddComponent<Stage4DriftTile>().AnswerId=ids[i];
            var text=Label(tile.transform,"Text",labels[i],17,
                Vector2.zero,new Vector2(294,48),Color.white);
            text.enableAutoSizing=true;text.fontSizeMin=13;text.fontSizeMax=17;
            text.textWrappingMode=TextWrappingModes.NoWrap;
        }
        Label(card.transform,"MetalChallengeFeedback",
            "Place all three tiles, then CHECK MODEL. Decoys count only on CHECK.",15,
            new Vector2(0,-195),new Vector2(990,27),Gold);
        Button(card.transform,"MetalChallengeReset","RESET TILES",
            new Vector2(-170,-240),new Vector2(255,51),
            new Color32(70,86,119,255));
        Button(card.transform,"MetalChallengeCheck","CHECK MODEL",
            new Vector2(170,-240),new Vector2(255,51),
            new Color32(12,132,135,255));
        return overlay.transform;
    }

    private void ShowMetalChallenge(int phase)
    {
        if(metalChallengePanels==null || phase<0 || phase>=2)return;
        metalChallengeStep=phase+1;
        foreach(var panel in metalChallengePanels)
            if(panel!=null)panel.SetActive(false);
        if(metalChallengePanels[phase]!=null)
        {
            metalChallengePanels[phase].SetActive(true);
            metalChallengePanels[phase].transform.SetAsLastSibling();
        }
    }

    private void ResetMetalTiles(int phase)
    {
        if(metalChallengeSlots==null || phase<0 || phase>=metalChallengeSlots.Length)return;
        foreach(var slot in metalChallengeSlots[phase])slot.ResetSlot();
        foreach(var tile in metalChallengeTiles[phase])tile.ReturnHome();
        if(metalChallengeMessages[phase]!=null)
            metalChallengeMessages[phase].text="Arrange all three properties, then CHECK MODEL.";
    }

    private void CheckMetalChallenge(int phase)
    {
        if(Level!=3 || !CanInteract || metalChallengePanels==null ||
            phase<0 || phase>1 || !metalChallengePanels[phase].activeSelf)return;
        int filled=0;
        bool correct=metalChallengeSlots[phase]!=null &&
                     metalChallengeSlots[phase].Length==3;
        foreach(var slot in metalChallengeSlots[phase])
        {
            if(slot.IsFilled)filled++;
            if(!slot.IsCorrect)correct=false;
        }
        if(filled<3)
        {
            metalChallengeMessages[phase].text=
                "Fill the three slots first. No penalty for incomplete models.";
            return;
        }
        if(!correct)
        {
            RegisterMistake();
            if(!failed && metalChallengeMessages[phase]!=null)
                metalChallengeMessages[phase].text=phase==0?
                    "HINT: Layers slide while delocalized electrons keep bonding intact.":
                    "HINT: A metallic structure can elongate while metallic bonding remains.";
            return;
        }
        metalChallengePanels[phase].SetActive(false);
        if(phase==0)ShowMetalChallenge(1);
        else Complete();
    }


    // Repair both newly built and already serialized Level 3 slider objects.
    // Unity's Slider component controls fill/handle anchors at runtime, so
    // they must not retain fixed -294 X offsets from an older scene.
    public void FixLevel3ForceSliderLayout()
    {
        if(Level!=3)return;
        var root=GameObject.Find("Stage4Level3Canvas");
        if(root==null)return;
        var slider=Find(root.transform,"ForceSlider")?.GetComponent<Slider>();
        if(slider==null)return;

        RectTransform track=slider.GetComponent<RectTransform>();
        track.anchorMin=track.anchorMax=new Vector2(.5f,.5f);
        track.pivot=new Vector2(.5f,.5f);
        track.anchoredPosition=new Vector2(0,-86);
        track.sizeDelta=new Vector2(650,42);

        var back=Find(track,"Background")?.GetComponent<RectTransform>();
        if(back!=null)
        {
            back.anchorMin=back.anchorMax=new Vector2(.5f,.5f);
            back.pivot=new Vector2(.5f,.5f);
            back.anchoredPosition=Vector2.zero;
            back.sizeDelta=new Vector2(650,16);
        }
        var fill=slider.fillRect;
        if(fill!=null)
        {
            fill.anchorMin=new Vector2(0,0.5f);
            fill.anchorMax=new Vector2(0,0.5f);
            fill.pivot=new Vector2(0,0.5f);
            fill.anchoredPosition=Vector2.zero;
            fill.sizeDelta=new Vector2(0,16);
            // Constrain fill to a thin strip even when the Slider adjusts its width.
            fill.offsetMin=new Vector2(0,-8);
            fill.offsetMax=new Vector2(0,8);
        }
        var handle=slider.handleRect;
        if(handle!=null)
        {
            handle.anchorMin=handle.anchorMax=new Vector2(0,0.5f);
            handle.pivot=new Vector2(.5f,.5f);
            handle.anchoredPosition=Vector2.zero;
            handle.sizeDelta=new Vector2(30,34);
        }
        slider.direction=Slider.Direction.LeftToRight;
        slider.minValue=0;
        slider.maxValue=1;
        slider.wholeNumbers=false;
        slider.value=0;
    }

    private void OnForceChanged(float value)
    {
        if(Level!=3 || upperLayer==null || forceLabel==null)return;
        upperLayer.anchoredPosition=new Vector2(value*90f,56);
        forceLabel.text="FORCE: "+Mathf.RoundToInt(value*100f)+"%  |  "+
            (value<=0.001f?"LAYERS ALIGNED":"LAYERS SHIFTING");
        if(value>=0.75f && !forceApplied)
        {
            forceApplied=true;
            if(feedback!=null)feedback.text=
                "Metal layers shifted while bonding remained. Press CHECK for the Malleability Puzzle.";
        }
    }
    private void Check()
    {
        if(!CanInteract)return;
        if(IsLevel1)
        {
            int filled=0;
            bool allCorrect=true;
            foreach(var zone in zones)
            {
                if(zone.Kind!=0 && zone.Kind!=1)continue;
                if(zone.Occupied)filled++;
                if(!zone.CorrectPlacement)allCorrect=false;
            }
            if(filled<10)
            {
                if(feedback!=null)feedback.text=
                    "Fill all 6 lattice slots and 4 cloud slots, then press CHECK.";
                return;
            }
            if(!allCorrect)
            {
                RegisterMistake();
                if(!failed&&feedback!=null)feedback.text=
                    "INCORRECT MODEL! Remove the decoys. Press RESET to try again.";
                return;
            }
        }
        if(Level==3)
        {
            if(!forceApplied)
            {
                if(feedback!=null)feedback.text="Apply at least 75% force before starting the puzzles.";
                return;
            }
            if(metalChallengePanels==null)EnsureMetalChallenges();
            if(metalChallengePanels!=null)ShowMetalChallenge(0);
            else if(feedback!=null)feedback.text="Malleability challenge unavailable. Reopen scene.";
            return;
        }
        if(Level==2 && terminalsPlaced==2 && observedDrift)
        {
            if(driftPuzzlePanel==null)EnsureDriftPuzzle();
            if(driftPuzzlePanel!=null)
            {
                driftPuzzlePanel.SetActive(true);
                driftPuzzlePanel.transform.SetAsLastSibling();
                return;
            }
            if(feedback!=null)feedback.text="Direction challenge unavailable. Reopen the level.";
            return;
        }
        bool correct=Level==1?metalPlaced==6 && electronsPlaced==4:
            Level==2?terminalsPlaced==2 && observedDrift:
            forceApplied;
        if(!correct)
        {
            RegisterMistake();
            return;
        }
        Complete();
    }
    public void RegisterMistake()
    {
        if(!CanInteract)return;
        mistakes++;
        if(mistakes==1)
        {
            if(feedback!=null)feedback.text=Level==1?
                "HINT: Remove decoys, then use RESET to arrange the correct metal cores and electrons.":
                Level==2?"HINT: Negative left, positive right; switch ON and observe electron drift.":
                "HINT: Observe sliding layers, then arrange the metal property tiles correctly.";
        }
        else
        {
            failed=true;
            EnergyManager.Instance.LoseEnergy();
            if(failedPanel!=null)
            {
                failedPanel.SetActive(true);
                failedPanel.transform.SetAsLastSibling();
            }
            if(retryButton!=null)retryButton.interactable=EnergyManager.Instance.HasEnergy;
            UpdateEnergy();
        }
    }
    private void Complete()
    {
        completed=true;
        int scoreValue=mistakes==0?100:75;
        PlayerPrefs.SetInt("Stage4Level"+Level+"Completed",1);
        PlayerPrefs.SetInt("Stage4Level"+Level+"Score",scoreValue);
        if(Level<3)PlayerPrefs.SetInt("Stage4Level"+(Level+1)+"Unlocked",1);
        else PlayerPrefs.SetInt("Stage4Completed",1);
        PlayerPrefs.Save();
        if(score!=null)score.text="SCORE\n"+scoreValue.ToString("D6");
        if(finalScore!=null)finalScore.text="FINAL SCORE: "+scoreValue.ToString("D6");
        if(winPanel!=null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }
    }
    private void Reset()
    {
        if(!CanInteract)return;
        foreach(var t in tokens)t.ResetToken();
        foreach(var z in zones)z.ResetZone();
        metalPlaced=electronsPlaced=terminalsPlaced=0;
        UpdateLevel1Progress();
        if(Level==2)
        {
            if(driftPuzzlePanel!=null)driftPuzzlePanel.SetActive(false);
            ResetDriftTiles();
            powerOn=observedDrift=false;driftClock=0f;
            var text=switchButton?.GetComponentInChildren<TMP_Text>(true);
            if(text!=null)text.text="SWITCH: OFF";
        }
        if(Level==3)
        {
            forceApplied=false;
            metalChallengeStep=0;
            if(forceSlider!=null)forceSlider.value=0;
            if(metalChallengePanels!=null)
                for(int i=0;i<metalChallengePanels.Length;i++)
                {
                    if(metalChallengePanels[i]!=null)metalChallengePanels[i].SetActive(false);
                    ResetMetalTiles(i);
                }
        }
        if(feedback!=null)feedback.text=IsLevel1?
            "Reset: arrange matching cores and electrons; avoid decoys.":
            Level==3?"Drag the slider to apply force, then press CHECK.":
                "Reset complete. No penalty.";
    }
    private void Update()
    {
        if(Time.unscaledTime>=nextTick)
        {
            nextTick=Time.unscaledTime+.3f;
            UpdateEnergy();
        }
        if(Level==2&&powerOn&&CanInteract)
        {
            driftClock+=Time.deltaTime;
            for(int i=0;i<driftElectrons.Count;i++)
            {
                var r=driftElectrons[i];
                float x=-245+Mathf.Repeat(i*76+Time.timeSinceLevelLoad*70,490);
                r.anchoredPosition=new Vector2(x,0);
            }
            if(driftClock>=2f && !observedDrift)
            {
                observedDrift=true;
                if(feedback!=null)feedback.text=
                    "Electron drift observed. Press CHECK to begin the direction puzzle.";
            }
        }
    }


    // Make the existing Stage4Level3 scene match the approved clean UI mockup.
    // Applies to serialized scenes AND newly generated UI without affecting
    // any of the other Stage 4 levels or puzzle/energy logic.
    public void PolishLevel3GameUILayout()
    {
        if(Level!=3)return;
        var canvas=GameObject.Find("Stage4Level3Canvas");
        if(canvas==null)return;
        var root=canvas.transform;
        var board=Find(root,"ExperimentBoard")?.GetComponent<RectTransform>();
        if(board==null)return;

        // Grow downward only; leave the illustrated top banner unobstructed.
        board.anchorMin=board.anchorMax=new Vector2(.5f,.5f);
        board.anchoredPosition=new Vector2(0,-26);
        board.sizeDelta=new Vector2(1110,444);

        var boardImage=board.GetComponent<Image>();
        if(boardImage!=null)boardImage.color=new Color32(18,42,74,255);
        var outline=board.GetComponent<Outline>();
        if(outline==null)outline=board.gameObject.AddComponent<Outline>();
        outline.effectColor=new Color32(44,174,210,185);
        outline.effectDistance=new Vector2(2,-2);

        var heading=Find(board,"LayerHeading")?.GetComponent<TMP_Text>();
        if(heading!=null)
        {
            heading.fontSize=25;
            heading.enableAutoSizing=true;
            heading.fontSizeMin=20;
            heading.fontSizeMax=25;
            heading.rectTransform.anchoredPosition=new Vector2(0,145);
            heading.rectTransform.sizeDelta=new Vector2(850,42);
        }
        var sub=Find(board,"ForceInfo")?.GetComponent<TMP_Text>();
        if(sub!=null)
        {
            sub.fontSize=16;
            sub.rectTransform.anchoredPosition=new Vector2(0,109);
            sub.rectTransform.sizeDelta=new Vector2(830,30);
        }

        // Place all electrons in the open gap instead of on a lattice edge.
        for(int layer=0;layer<2;layer++)
        {
            var r=Find(board,layer==0?"UpperMetalLayer":"LowerMetalLayer")?.GetComponent<RectTransform>();
            if(r==null)continue;
            r.anchoredPosition=new Vector2(layer==0?forceSlider!=null?forceSlider.value*90f:0:0,
                layer==0?56:-33);
            r.sizeDelta=new Vector2(730,58);
            var img=r.GetComponent<Image>();
            if(img!=null)img.color=new Color32(31,92,124,255);
            var edge=r.GetComponent<Outline>();
            if(edge==null)edge=r.gameObject.AddComponent<Outline>();
            edge.effectColor=new Color32(51,180,216,255);
            edge.effectDistance=new Vector2(1,1);
        }
        for(int i=0;i<7;i++)
        {
            var electron=Find(board,"ElectronCloud"+i)?.GetComponent<TMP_Text>();
            if(electron==null)continue;
            electron.rectTransform.anchoredPosition=new Vector2(-336+i*112,12);
            electron.rectTransform.sizeDelta=new Vector2(47,22);
            electron.fontSize=19;
            electron.alignment=TextAlignmentOptions.Center;
        }

        // The force indicator, explanation and feedback remain fully inside
        // the board; bottom controls are visibly separated from them.
        FixLevel3ForceSliderLayout();
        var indicator=Find(board,"ForceValue")?.GetComponent<TMP_Text>();
        if(indicator!=null)
        {
            indicator.rectTransform.anchoredPosition=new Vector2(0,-126);
            indicator.rectTransform.sizeDelta=new Vector2(750,28);
            indicator.fontSize=18;
        }
        var note=Find(board,"BindingNote")?.GetComponent<TMP_Text>();
        if(note!=null)
        {
            note.rectTransform.anchoredPosition=new Vector2(0,-155);
            note.rectTransform.sizeDelta=new Vector2(900,22);
            note.fontSize=13;
        }

        var strip=Find(root,"Level3FeedbackStrip")?.GetComponent<Image>();
        if(strip==null)
        {
            strip=Rect(root,"Level3FeedbackStrip",new Vector2(0,-221),
                new Vector2(980,42),new Color32(12,57,85,255));
            strip.transform.SetSiblingIndex(board.GetSiblingIndex()+1);
        }
        strip.rectTransform.anchorMin=strip.rectTransform.anchorMax=new Vector2(.5f,.5f);
        strip.rectTransform.anchoredPosition=new Vector2(0,-221);
        strip.rectTransform.sizeDelta=new Vector2(980,42);
        strip.color=new Color32(12,57,85,255);
        strip.raycastTarget=false;
        var stripOutline=strip.GetComponent<Outline>();
        if(stripOutline==null)stripOutline=strip.gameObject.AddComponent<Outline>();
        stripOutline.effectColor=new Color32(36,191,211,205);
        stripOutline.effectDistance=new Vector2(1,1);

        var status=Find(root,"FeedbackText")?.GetComponent<TMP_Text>();
        if(status!=null)
        {
            status.rectTransform.anchorMin=status.rectTransform.anchorMax=new Vector2(.5f,.5f);
            status.rectTransform.anchoredPosition=new Vector2(0,-221);
            status.rectTransform.sizeDelta=new Vector2(935,38);
            // Saved scenes can still contain the old generic instruction.
            // Update that initial copy without overwriting active puzzle hints.
            if(string.IsNullOrWhiteSpace(status.text) ||
                status.text=="Follow the instructions to complete the metallic bonding model.")
                status.text="Drag the slider to apply force, then press CHECK.";
            status.enableAutoSizing=true;
            status.fontSizeMin=13;
            status.fontSizeMax=17;
            status.color=Color.white;
            status.alignment=TextAlignmentOptions.Center;
            status.textWrappingMode=TextWrappingModes.Normal;
        }
        foreach(var spec in new[]{
            new {Name="ResetButton",X=-144f,Width=220f},
            new {Name="CheckButton",X=144f,Width=220f}})
        {
            var button=Find(root,spec.Name)?.GetComponent<RectTransform>();
            if(button==null)continue;
            button.anchorMin=button.anchorMax=new Vector2(.5f,.5f);
            button.anchoredPosition=new Vector2(spec.X,-287);
            button.sizeDelta=new Vector2(spec.Width,55);
        }
    }

    // Also works with scenes previously serialized in Edit Mode.
    // The illustrated background already contains the STAGE 4 banner,
    // and the experiment board has its own readable heading. Hide the
    // duplicated floating labels that overlap the illustrated banner.
    public void PolishLevel3Presentation()
    {
        if(Level!=3)return;
        var root=GameObject.Find("Stage4Level3Canvas");
        if(root==null)return;
        foreach(var name in new[]{"StageTitle","LevelTitle","Instructions"})
        {
            var duplicate=Find(root.transform,name);
            if(duplicate!=null)duplicate.gameObject.SetActive(false);
        }
        var complete=Find(root.transform,"CompletePanel");
        if(complete!=null)
        {
            var description=Find(complete,"Description")?.GetComponent<TMP_Text>();
            if(description!=null)
            {
                description.text="Malleability and ductility challenges successfully completed!";
                description.enableAutoSizing=true;
                description.fontSizeMin=17;
                description.fontSizeMax=21;
                description.textWrappingMode=TextWrappingModes.Normal;
            }
        }
    }

    private void UpdateEnergy()
    {
        var energy=EnergyManager.Instance;
        if(energy==null)return;
        energy.RefreshEnergy();
        for(int i=0;i<energyIcons.Count;i++)
            if(energyIcons[i]!=null)energyIcons[i].enabled=i<energy.CurrentEnergy;
        if(energyCount!=null)energyCount.text=energy.CurrentEnergy+"/5";
        if(noEnergyPanel==null)return;
        if(!energy.HasEnergy)
        {
            if(!noEnergyPanel.activeSelf)noEnergyPanel.SetActive(true);
            noEnergyPanel.transform.SetAsLastSibling();
            int seconds=energy.SecondsUntilNextEnergy();
            if(timer!=null)timer.text="Next Energy In: "+
                (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
        }
        else if(noEnergyPanel.activeSelf)noEnergyPanel.SetActive(false);
    }
    private void Pause()
    {
        if(!CanInteract)return;
        paused=true;Time.timeScale=0f;
        if(pausePanel!=null){pausePanel.SetActive(true);pausePanel.transform.SetAsLastSibling();}
    }
    private void Resume()
    {
        paused=false;Time.timeScale=1f;
        if(pausePanel!=null)pausePanel.SetActive(false);
    }
    private void Go(string name)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {
            Debug.LogError("Stage 4: Missing scene in Build Settings: "+name);
            return;
        }
        SceneManager.LoadScene(name);
    }
    private void OnDestroy(){Time.timeScale=1f;}
}
