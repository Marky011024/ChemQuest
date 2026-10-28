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
    private TMP_Text feedback,score,finalScore,timer,energyCount,modeLabel,forceLabel;
    private Button retryButton, switchButton, checkButton;
    private Slider forceSlider;
    private RectTransform upperLayer, lowerLayer;
    private int mistakes,metalPlaced,electronsPlaced,terminalsPlaced;
    private bool failed,completed,paused,powerOn,observedDrift,forceApplied;
    private float nextTick,driftClock;
    private const string MetalKey="Stage4SelectedMetal";
    private static readonly Color Bg=new Color32(10,22,47,255);
    private static readonly Color Panel=new Color32(23,44,76,255);
    private static readonly Color Gold=new Color32(255,207,89,255);
    private static readonly Color Blue=new Color32(56,189,248,255);
    public bool CanInteract => !failed&&!completed&&!paused&&Time.timeScale>0f &&
        EnergyManager.Instance!=null&&EnergyManager.Instance.HasEnergy;
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
        Bind();
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
            new Vector2(0,322),new Vector2(690,36),Blue);
        Label(root.transform,"LevelTitle",Title,31,
            new Vector2(0,277),new Vector2(960,47),Color.white);
        Label(root.transform,"Instructions",
            Level==1?"Select Cu or Mg, drag 6 metal cores into the lattice and 4 free electrons into the cloud.":
            Level==2?"Attach terminals: NEGATIVE on left, POSITIVE on right. Turn on voltage to observe electron drift.":
            "Apply force and observe metal layers slide while delocalized electrons keep the cores bonded.",
            17,new Vector2(0,227),new Vector2(1040,54),new Color32(183,203,223,255));
        score=Label(root.transform,"ScoreText","SCORE\n000000",19,new Vector2(533,294),new Vector2(135,68),Gold);
        Label(root.transform,"EnergyTitle","ENERGY",14,new Vector2(-526,338),new Vector2(110,24),Color.white);
        for(int i=0;i<5;i++)
        {
            var icon=Rect(root.transform,"Energy"+(i+1),new Vector2(-550+i*44,292),
                new Vector2(36,47),Color.white);
            icon.sprite=EnergySprite;icon.preserveAspect=true;
            if(EnergySprite==null)icon.color=Gold;
        }
        Label(root.transform,"EnergyCount","5/5",15,new Vector2(-349,334),new Vector2(85,25),Color.white);
        var board=Rect(root.transform,"ExperimentBoard",new Vector2(0,14),
            new Vector2(1020,350),Panel);
        board.raycastTarget=false;

        if(Level==1)BuildLevel1(board.transform);
        if(Level==2)BuildLevel2(board.transform);
        if(Level==3)BuildLevel3(board.transform);

        feedback=Label(root.transform,"FeedbackText","Follow the instructions to complete the metallic bonding model.",
            16,new Vector2(0,-253),new Vector2(1050,49),Gold);
        Button(root.transform,"CheckButton","CHECK",new Vector2(0,-318),
            new Vector2(250,55),new Color32(12,132,135,255));
        Button(root.transform,"ResetButton","RESET",new Vector2(-268,-318),
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
    }

    private void BuildLevel1(Transform board)
    {
        Label(board,"LatticeHeading","METAL CATION LATTICE",19,new Vector2(-199,145),
            new Vector2(490,34),Blue);
        Label(board,"CloudHeading","DELOCALIZED ELECTRONS",19,new Vector2(276,145),
            new Vector2(440,34),Blue);
        Button(board,"CopperButton","COPPER (Cu)",new Vector2(-300,94),
            new Vector2(168,47),new Color32(107,65,43,255));
        Button(board,"MagnesiumButton","MAGNESIUM (Mg)",new Vector2(-110,94),
            new Vector2(193,47),new Color32(56,94,114,255));
        Label(board,"CurrentMetal","Selected: Copper (Cu)",16,
            new Vector2(-210,50),new Vector2(420,35),Gold);
        for(int i=0;i<6;i++)
        {
            int x=i%3,y=i/3;
            AddZone(board,"LatticeSlot"+i,0,new Vector2(-352+x*115,-10-y*70),new Vector2(64,52));
        }
        for(int i=0;i<4;i++)
            AddZone(board,"CloudSlot"+i,1,new Vector2(130+(i%2)*158,-1-(i/2)*87),new Vector2(85,58));
        var bank=Rect(board,"MetalTokenBank",new Vector2(0,-145),new Vector2(935,54),
            new Color32(12,28,51,255));
        bank.raycastTarget=false;
        for(int i=0;i<6;i++)
            AddToken(bank.transform,"MetalCore"+i,0,new Vector2(-368+i*91,0),
                new Color32(75,155,190,255),"M⁺");
        for(int i=0;i<4;i++)
            AddToken(bank.transform,"FreeElectron"+i,1,
                new Vector2(203+i*73,0),new Color32(244,189,75,255),"e⁻");
    }

    private void BuildLevel2(Transform board)
    {
        Label(board,"SlabTitle","METAL SLAB  |  VOLTAGE EXPERIMENT",20,
            new Vector2(0,144),new Vector2(670,34),Blue);
        var slab=Rect(board,"MetalSlab",new Vector2(0,22),new Vector2(600,186),
            new Color32(31,75,105,255));
        slab.raycastTarget=false;
        for(int i=0;i<9;i++)
        {
            int x=i%3,y=i/3;
            Label(slab.transform,"Core"+i,"M⁺",28,
                new Vector2(-190+x*190,58-y*58),new Vector2(75,42),Dim);
        }
        for(int i=0;i<7;i++)
            Label(slab.transform,"DriftingElectron"+i,"e⁻",22,
                new Vector2(-245+i*76,0),new Vector2(56,35),Gold);
        AddZone(board,"NegativeTerminalSlot",2,new Vector2(-379,40),new Vector2(115,94));
        AddZone(board,"PositiveTerminalSlot",3,new Vector2(379,40),new Vector2(115,94));
        var bank=Rect(board,"TerminalBank",new Vector2(0,-112),
            new Vector2(510,66),new Color32(12,28,51,255));
        bank.raycastTarget=false;
        AddToken(bank.transform,"NegativeTerminal",2,new Vector2(-123,0),
            new Color32(82,155,200,255),"−");
        AddToken(bank.transform,"PositiveTerminal",3,new Vector2(123,0),
            new Color32(235,149,70,255),"+");
        Button(board,"VoltageSwitch","SWITCH: OFF",new Vector2(0,-175),
            new Vector2(260,42),new Color32(87,103,133,255));
        Label(board,"DriftLabel","Electron drift: OFF  |  conventional current flows opposite",
            14,new Vector2(0,-158),new Vector2(740,28),Dim);
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
                new Vector2(0,layer==0?54:-21),new Vector2(730,68),
                new Color32(35,77,107,255));
            layerGO.raycastTarget=false;
            for(int i=0;i<6;i++)
                Label(layerGO.transform,"MetalCore"+i,"M⁺",30,
                    new Vector2(-308+i*123,0),new Vector2(77,50),Blue);
        }
        for(int i=0;i<7;i++)
            Label(board,"ElectronCloud"+i,"e⁻",22,
                new Vector2(-343+i*111,16),new Vector2(56,34),Gold);
        var sliderGO=new GameObject("ForceSlider",typeof(RectTransform),typeof(Slider));
        sliderGO.transform.SetParent(board,false);
        var rect=sliderGO.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(0,-107);
        rect.sizeDelta=new Vector2(590,33);
        var bg=Rect(sliderGO.transform,"Background",Vector2.zero,
            new Vector2(590,16),new Color32(55,87,113,255));
        var fill=Rect(sliderGO.transform,"Fill",new Vector2(-294,0),
            new Vector2(590,16),new Color32(18,157,143,255));
        var handle=Rect(sliderGO.transform,"Handle",new Vector2(-294,0),
            new Vector2(34,39),Gold);
        var slide=sliderGO.GetComponent<Slider>();slide.direction=Slider.Direction.LeftToRight;
        slide.minValue=0;slide.maxValue=1;slide.value=0;
        slide.fillRect=fill.rectTransform;slide.handleRect=handle.rectTransform;
        slide.targetGraphic=handle;
        Label(board,"ForceValue","FORCE: 0%  |  LAYERS ALIGNED",17,
            new Vector2(0,-149),new Vector2(690,32),Gold);
        Label(board,"BindingNote","Delocalized electrons continue to attract metal cores as layers move.",
            14,new Vector2(0,-181),new Vector2(780,27),Dim);
    }

    private void AddZone(Transform parent,string name,int kind,Vector2 p,Vector2 size)
    {
        var image=Rect(parent,name,p,size,new Color32(39,78,108,255));
        var zone=image.gameObject.AddComponent<Stage4DropZone>();zone.Kind=kind;
        Label(image.transform,"SlotLabel",kind==0?"＋":kind==1?"·":kind==2?"−":"＋",25,
            Vector2.zero,size,Gold);
    }
    private void AddToken(Transform parent,string name,int kind,Vector2 p,Color color,string symbol)
    {
        var image=Rect(parent,name,p,new Vector2(50,45),color);
        image.gameObject.AddComponent<CanvasGroup>();
        var drag=image.gameObject.AddComponent<Stage4Draggable>();drag.Kind=kind;
        Label(image.transform,"Symbol",symbol,23,Vector2.zero,new Vector2(47,43),
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
    }

    public string GetPlacedSymbol(int kind) =>
        kind==0?Metal+"⁺":kind==1?"e⁻":kind==2?"−":"+";

    public void ZoneFilled(int kind)
    {
        if(kind==0)metalPlaced++;
        else if(kind==1)electronsPlaced++;
        else terminalsPlaced++;
        if(feedback!=null)feedback.text=Level==1
            ? "Lattice: "+metalPlaced+"/6 cores   Cloud: "+electronsPlaced+"/4 electrons"
            : "Terminals attached: "+terminalsPlaced+"/2";
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
            if(label!=null)label.text=symbol+"⁺";
        }
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
    private void OnForceChanged(float value)
    {
        if(Level!=3 || upperLayer==null || forceLabel==null)return;
        upperLayer.anchoredPosition=new Vector2(value*110f,54);
        forceLabel.text="FORCE: "+Mathf.RoundToInt(value*100f)+"%  |  LAYERS SHIFTING";
        if(value>=0.75f)forceApplied=true;
    }
    private void Check()
    {
        if(!CanInteract)return;
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
                "HINT: Place ALL metal cores in the fixed grid and electrons in the cloud.":
                Level==2?"HINT: Negative left, positive right; switch ON and observe electron drift.":
                "HINT: Apply sufficient force using the slider; metal layers must slide.";
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
        if(Level==2)
        {
            powerOn=observedDrift=false;driftClock=0f;
            var text=switchButton?.GetComponentInChildren<TMP_Text>(true);
            if(text!=null)text.text="SWITCH: OFF";
        }
        if(Level==3&&forceSlider!=null){forceApplied=false;forceSlider.value=0;}
        if(feedback!=null)feedback.text="Reset complete. No penalty.";
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
            if(driftClock>=2f)observedDrift=true;
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
