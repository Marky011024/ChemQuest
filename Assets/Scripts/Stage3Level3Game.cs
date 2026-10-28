using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage3Level3Game : MonoBehaviour
{
    [Header("Stage 1 shared UI")]
    [SerializeField] private Sprite energySprite;
    [SerializeField] private GameObject pauseButtonPrefab;
    [SerializeField] private GameObject pauseMenuPrefab;

    private readonly List<Stage3Level3PairDrag>[] tokens =
        { new List<Stage3Level3PairDrag>(), new List<Stage3Level3PairDrag>() };
    private readonly List<Stage3Level3PairDropZone>[] zones =
        { new List<Stage3Level3PairDropZone>(), new List<Stage3Level3PairDropZone>() };
    private readonly List<Image> energyIcons = new List<Image>();
    private GameObject[] puzzles = new GameObject[2];
    private GameObject pausePanel, failedPanel, energyPanel, transitionPanel, completedPanel;
    private TMP_Text feedback, scoreText, finalScoreText, timerText, energyCount;
    private Button retryButton;
    private int[] placed = new int[2];
    private int phase, mistakes;
    private bool paused, failed, completed, transitioning;
    private float nextUpdate;
    private static readonly Color Back=new Color32(10,22,47,255);
    private static readonly Color Panel=new Color32(23,44,76,255);
    private static readonly Color Accent=new Color32(56,189,248,255);
    private static readonly Color Gold=new Color32(255,207,89,255);
    private static readonly Color Dim=new Color32(158,179,207,255);

    public bool CanInteract => !paused && !failed && !completed && !transitioning &&
        Time.timeScale>0f && EnergyManager.Instance!=null && EnergyManager.Instance.HasEnergy;

    private void Start()
    {
        Time.timeScale=1f;
        EnsureInput();
        if(EnergyManager.Instance==null)
            new GameObject("SharedEnergyManager",typeof(EnergyManager));
        if(GameObject.Find("Stage3Level3Canvas")==null) BuildInEditor();
        BindScene();
        UpdateEnergy();
    }

    // Called by the Unity Editor builder: creates *saved* Hierarchy objects before Play.
    public void BuildInEditor()
    {
        if(GameObject.Find("Stage3Level3Canvas")!=null) return; // preserve manual layout edits
        var root=new GameObject("Stage3Level3Canvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1f;

        var bg=Image(root.transform,"Background",Vector2.zero,new Vector2(1280,720),Back);
        bg.raycastTarget=false;
        var br=bg.rectTransform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;
        br.offsetMin=Vector2.zero;br.offsetMax=Vector2.zero;

        Text(root.transform,"StageTitle","STAGE 3  |  LEVEL 3",20,
            new Vector2(0,320),new Vector2(640,38),Accent);
        Text(root.transform,"LevelTitle","DOUBLE BOND CHALLENGE  -  O₂ & CO₂",30,
            new Vector2(0,275),new Vector2(890,47),Color.white);
        Text(root.transform,"Instruction",
            "Drag electron PAIRS into their correct bond or lone-pair positions.\nO₂ has one double bond; CO₂ has two C=O double bonds.",
            17,new Vector2(0,222),new Vector2(960,58),Dim);
        scoreText=Text(root.transform,"ScoreText","SCORE\n000000",20,
            new Vector2(532,296),new Vector2(140,70),Gold);
        Text(root.transform,"EnergyHeading","ENERGY",14,
            new Vector2(-525,335),new Vector2(130,25),Color.white);
        for(int i=0;i<5;i++)
        {
            var e=Image(root.transform,"Energy"+(i+1),
                new Vector2(-546+i*46,293),new Vector2(38,47),Color.white);
            e.sprite=energySprite;e.preserveAspect=true;
            if(energySprite==null)e.color=Gold;
        }
        Text(root.transform,"EnergyCount","5 / 5",15,
            new Vector2(-344,333),new Vector2(95,25),Dim);

        puzzles[0]=BuildOxygen(root.transform);
        puzzles[1]=BuildCarbonDioxide(root.transform);
        puzzles[1].SetActive(false);

        feedback=Text(root.transform,"FeedbackText",
            "O₂: Place 2 bonding pairs between oxygen atoms and 2 lone pairs around EACH oxygen.",
            16,new Vector2(0,-248),new Vector2(1050,49),Gold);

        Button(root.transform,"CheckButton","CHECK MOLECULE",
            new Vector2(0,-322),new Vector2(260,53),new Color32(7,134,140,255));
        Button(root.transform,"ResetButton","RESET PAIRS",
            new Vector2(-273,-322),new Vector2(197,53),new Color32(78,93,121,255));

        if(pauseButtonPrefab!=null)
        {
            var pause=Instantiate(pauseButtonPrefab,root.transform,false);
            pause.name="Level3PauseButton";
            var rect=pause.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(0,1);
                rect.anchoredPosition=new Vector2(62,-60);
            }
        }
        else Button(root.transform,"Level3PauseButton","MENU",
            new Vector2(-515,222),new Vector2(145,50),new Color32(39,74,110,255));

        pausePanel=MakeModal(root.transform,"PauseMenuPanel","GAME PAUSED","");
        if(pauseMenuPrefab!=null)
        {
            DestroyObject(pausePanel);
            pausePanel=Instantiate(pauseMenuPrefab,root.transform,false);
            pausePanel.name="PauseMenuPanel";
            var rect=pausePanel.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
                rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            }
        }
        else
        {
            Button(pausePanel.transform,"ContinueButton","CONTINUE",
                new Vector2(0,15),new Vector2(280,60),new Color32(7,134,140,255));
            Button(pausePanel.transform,"LevelSelectionButton","LEVEL SELECTION",
                new Vector2(0,-75),new Vector2(310,60),new Color32(64,83,119,255));
            Button(pausePanel.transform,"StageSelectionButton","STAGE SELECTION",
                new Vector2(0,-155),new Vector2(310,60),new Color32(64,83,119,255));
        }
        pausePanel.SetActive(false);

        failedPanel=MakeModal(root.transform,"FailedPanel","STAGE FAILED!",
            "Two incorrect attempts. One shared energy was deducted.");
        Button(failedPanel.transform,"RetryButton","RETRY",
            new Vector2(0,-85),new Vector2(280,65),new Color32(7,134,140,255));
        Button(failedPanel.transform,"FailedLevelSelectionButton","LEVEL SELECTION",
            new Vector2(0,-162),new Vector2(280,54),new Color32(64,83,119,255));
        failedPanel.SetActive(false);

        transitionPanel=MakeModal(root.transform,"OxygenCompletePanel","O₂ COMPLETE!",
            "One O=O double bond, four oxygen lone pairs. Each O has an octet.");
        Button(transitionPanel.transform,"NextMoleculeButton","NEXT: CO₂",
            new Vector2(0,-116),new Vector2(270,65),new Color32(7,134,140,255));
        transitionPanel.SetActive(false);

        completedPanel=MakeModal(root.transform,"Stage3CompletePanel","STAGE 3 COMPLETE!",
            "O₂ and CO₂ double bonds formed successfully!");
        Text(completedPanel.transform,"FinalScoreText","FINAL SCORE: 000000",29,
            new Vector2(0,-44),new Vector2(590,65),Gold);
        Button(completedPanel.transform,"CompleteLevelSelectionButton","LEVEL SELECTION",
            new Vector2(-145,-155),new Vector2(265,60),new Color32(64,83,119,255));
        Button(completedPanel.transform,"CompleteStageSelectionButton","SELECT STAGE",
            new Vector2(145,-155),new Vector2(265,60),new Color32(7,134,140,255));
        completedPanel.SetActive(false);

        energyPanel=MakeModal(root.transform,"OutOfEnergyPanel","OUT OF ENERGY!",
            "Shared energy is empty. Wait for regeneration.");
        Text(energyPanel.transform,"EnergyTimerText","Next Energy In: 15:00",25,
            new Vector2(0,-55),new Vector2(560,65),Gold);
        Button(energyPanel.transform,"EnergyBackButton","BACK TO LEVEL SELECTION",
            new Vector2(0,-150),new Vector2(360,65),new Color32(7,134,140,255));
        energyPanel.SetActive(false);
    }

    private GameObject BuildOxygen(Transform root)
    {
        var board=Image(root,"OxygenPuzzle",new Vector2(0,18),
            new Vector2(1050,360),Panel);
        board.raycastTarget=false;
        Text(board.transform,"PuzzleTitle","PUZZLE 1 / 2: OXYGEN  O₂",21,
            new Vector2(0,150),new Vector2(670,36),Accent);
        Text(board.transform,"LeftOxygen","O",70,new Vector2(-170,60),
            new Vector2(100,99),Color.white);
        Text(board.transform,"RightOxygen","O",70,new Vector2(170,60),
            new Vector2(100,99),Color.white);
        Text(board.transform,"OxygenDoubleBondLabel","TWO SHARED PAIRS  =  O=O",15,
            new Vector2(0,112),new Vector2(330,26),Dim);
        Zone(board.transform,"O2Bond1",true,new Vector2(-33,62),new Vector2(58,47));
        Zone(board.transform,"O2Bond2",true,new Vector2(33,62),new Vector2(58,47));
        Zone(board.transform,"O2LoneL1",false,new Vector2(-292,113),new Vector2(76,43));
        Zone(board.transform,"O2LoneL2",false,new Vector2(-292,14),new Vector2(76,43));
        Zone(board.transform,"O2LoneR1",false,new Vector2(292,113),new Vector2(76,43));
        Zone(board.transform,"O2LoneR2",false,new Vector2(292,14),new Vector2(76,43));
        BuildBank(board.transform,"O2ElectronBank",2,4);
        return board.gameObject;
    }

    private GameObject BuildCarbonDioxide(Transform root)
    {
        var board=Image(root,"CarbonDioxidePuzzle",new Vector2(0,18),
            new Vector2(1050,360),Panel);
        board.raycastTarget=false;
        Text(board.transform,"PuzzleTitle","PUZZLE 2 / 2: CARBON DIOXIDE  CO₂",21,
            new Vector2(0,150),new Vector2(670,36),Accent);
        Text(board.transform,"LeftOxygen","O",68,new Vector2(-300,63),
            new Vector2(90,90),Color.white);
        Text(board.transform,"Carbon","C",68,new Vector2(0,63),
            new Vector2(90,90),Accent);
        Text(board.transform,"RightOxygen","O",68,new Vector2(300,63),
            new Vector2(90,90),Color.white);
        Text(board.transform,"BondGuide","O = C = O  •  TWO PAIRS IN EACH DOUBLE BOND",14,
            new Vector2(0,115),new Vector2(700,28),Dim);
        Zone(board.transform,"CO2LeftBond1",true,new Vector2(-180,67),new Vector2(64,45));
        Zone(board.transform,"CO2LeftBond2",true,new Vector2(-100,67),new Vector2(64,45));
        Zone(board.transform,"CO2RightBond1",true,new Vector2(100,67),new Vector2(64,45));
        Zone(board.transform,"CO2RightBond2",true,new Vector2(180,67),new Vector2(64,45));
        Zone(board.transform,"CO2LoneL1",false,new Vector2(-397,113),new Vector2(76,43));
        Zone(board.transform,"CO2LoneL2",false,new Vector2(-397,15),new Vector2(76,43));
        Zone(board.transform,"CO2LoneR1",false,new Vector2(397,113),new Vector2(76,43));
        Zone(board.transform,"CO2LoneR2",false,new Vector2(397,15),new Vector2(76,43));
        BuildBank(board.transform,"CO2ElectronBank",4,4);
        return board.gameObject;
    }

    private void Zone(Transform parent,string name,bool isBond,Vector2 pos,Vector2 size)
    {
        var image=Image(parent,name,pos,size,new Color32(40,82,113,255));
        image.raycastTarget=true;
        var zone=image.gameObject.AddComponent<Stage3Level3PairDropZone>();
        zone.RequiresBondPair=isBond;
        Text(image.transform,"PairText","○○",23,Vector2.zero,size,Gold);
    }

    private void BuildBank(Transform parent,string name,int bonds,int lone)
    {
        var bank=Image(parent,name,new Vector2(0,-111),
            new Vector2(910,63),new Color32(15,33,59,255));
        bank.raycastTarget=false;
        Text(bank.transform,"BankTitle","DRAG PAIRS  →",13,
            new Vector2(-386,0),new Vector2(140,43),Dim);
        int total=bonds+lone;
        float stride=79f;
        for(int i=0;i<total;i++)
        {
            bool bond=i<bonds;
            var token=Image(bank.transform,"PairToken"+(i+1),
                new Vector2(-222+i*stride,0),new Vector2(57,46),
                bond?new Color32(45,142,190,255):new Color32(210,151,59,255));
            token.raycastTarget=true;
            token.gameObject.AddComponent<CanvasGroup>();
            var drag=token.gameObject.AddComponent<Stage3Level3PairDrag>();
            drag.IsBondPair=bond;
            Text(token.transform,"PairLabel","••",24,Vector2.zero,
                new Vector2(54,42),new Color32(8,25,48,255));
        }
        Text(parent,"BankLegend","BLUE = SHARED BOND PAIR     GOLD = LONE PAIR",
            14,new Vector2(0,-160),new Vector2(660,29),Dim);
    }

    private Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        var image=go.GetComponent<Image>();image.color=color;return image;
    }

    private TMP_Text Text(Transform parent,string name,string value,int size,
        Vector2 pos,Vector2 dimensions,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;
        rect.sizeDelta=dimensions;
        var text=go.GetComponent<TextMeshProUGUI>();
        text.text=value;text.fontSize=size;text.color=color;
        text.alignment=TextAlignmentOptions.Center;
        text.textWrappingMode=TextWrappingModes.Normal;
        text.raycastTarget=false;
        return text;
    }

    private Button Button(Transform parent,string name,string label,
        Vector2 pos,Vector2 size,Color color)
    {
        var image=Image(parent,name,pos,size,color);
        var button=image.gameObject.AddComponent<Button>();
        Text(image.transform,"Text",label,20,Vector2.zero,
            size-new Vector2(12,5),Color.white);
        return button;
    }

    private GameObject MakeModal(Transform parent,string name,string title,string message)
    {
        var dim=Image(parent,name,Vector2.zero,new Vector2(1280,720),
            new Color32(5,13,27,238));
        var card=Image(dim.transform,"ModalCard",Vector2.zero,new Vector2(700,400),Panel);
        card.raycastTarget=false;
        Text(dim.transform,"Title",title,38,new Vector2(0,115),
            new Vector2(660,69),Gold);
        Text(dim.transform,"Message",message,21,new Vector2(0,40),
            new Vector2(650,73),Color.white);
        return dim.gameObject;
    }

    private void DestroyObject(GameObject go)
    {
        if(go==null)return;
#if UNITY_EDITOR
        if(!Application.isPlaying)DestroyImmediate(go);
        else Destroy(go);
#else
        Destroy(go);
#endif
    }

    private Transform Find(Transform root,string name)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }

    private void WireButton(Transform root,string name,Action action)
    {
        foreach(var button in root.GetComponentsInChildren<Button>(true))
        {
            if(button.name!=name)continue;
            button.onClick=new Button.ButtonClickedEvent();
            button.onClick.AddListener(()=>action());
        }
    }

    private void EnsureInput()
    {
        var system=FindObjectOfType<EventSystem>();
        if(system==null)system=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var old in system.GetComponents<StandaloneInputModule>())old.enabled=false;
        if(system.GetComponent<InputSystemUIInputModule>()==null)
        {
            var module=system.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    private void BindScene()
    {
        var rootGO=GameObject.Find("Stage3Level3Canvas");
        if(rootGO==null)return;
        var root=rootGO.transform;
        puzzles[0]=Find(root,"OxygenPuzzle")?.gameObject;
        puzzles[1]=Find(root,"CarbonDioxidePuzzle")?.gameObject;
        pausePanel=Find(root,"PauseMenuPanel")?.gameObject;
        failedPanel=Find(root,"FailedPanel")?.gameObject;
        energyPanel=Find(root,"OutOfEnergyPanel")?.gameObject;
        transitionPanel=Find(root,"OxygenCompletePanel")?.gameObject;
        completedPanel=Find(root,"Stage3CompletePanel")?.gameObject;
        feedback=Find(root,"FeedbackText")?.GetComponent<TMP_Text>();
        scoreText=Find(root,"ScoreText")?.GetComponent<TMP_Text>();
        finalScoreText=Find(root,"FinalScoreText")?.GetComponent<TMP_Text>();
        timerText=Find(root,"EnergyTimerText")?.GetComponent<TMP_Text>();
        energyCount=Find(root,"EnergyCount")?.GetComponent<TMP_Text>();
        retryButton=Find(root,"RetryButton")?.GetComponent<Button>();
        energyIcons.Clear();
        for(int i=0;i<5;i++)
        {
            var image=Find(root,"Energy"+(i+1))?.GetComponent<Image>();
            if(image!=null)energyIcons.Add(image);
        }
        for(int p=0;p<2;p++)
        {
            zones[p].Clear();tokens[p].Clear();
            if(puzzles[p]==null)continue;
            foreach(var z in puzzles[p].GetComponentsInChildren<Stage3Level3PairDropZone>(true))
            {
                z.Game=this;z.ResetZone();zones[p].Add(z);
                var t=z.GetComponentInChildren<TMP_Text>(true);
                if(t!=null)t.text="○○";
            }
            foreach(var t in puzzles[p].GetComponentsInChildren<Stage3Level3PairDrag>(true))
            {
                t.Init(this);t.Restore();tokens[p].Add(t);
            }
            placed[p]=0;
        }
        WireButton(root,"CheckButton",CheckMolecule);
        WireButton(root,"ResetButton",ResetPairs);
        WireButton(root,"Level3PauseButton",Pause);
        WireButton(root,"ContinueButton",Resume);
        WireButton(root,"LevelSelectionButton",()=>GoTo("Stage3LevelSelection"));
        WireButton(root,"StageSelectionButton",()=>GoTo("StageSelection"));
        WireButton(root,"RetryButton",Retry);
        WireButton(root,"FailedLevelSelectionButton",()=>GoTo("Stage3LevelSelection"));
        WireButton(root,"NextMoleculeButton",NextMolecule);
        WireButton(root,"CompleteLevelSelectionButton",()=>GoTo("Stage3LevelSelection"));
        WireButton(root,"CompleteStageSelectionButton",()=>GoTo("StageSelection"));
        WireButton(root,"EnergyBackButton",()=>GoTo("Stage3LevelSelection"));
        foreach(var button in root.GetComponentsInChildren<Button>(true))
            if(button.name=="StageSelectionButton")
            {
                var t=button.GetComponentInChildren<TMP_Text>(true);
                if(t!=null)t.text="STAGE SELECTION";
            }
        phase=0;mistakes=0;paused=failed=completed=transitioning=false;
        Time.timeScale=1f;
        if(puzzles[0]!=null)puzzles[0].SetActive(true);
        if(puzzles[1]!=null)puzzles[1].SetActive(false);
        foreach(var modal in new[]{pausePanel,failedPanel,energyPanel,transitionPanel,completedPanel})
            if(modal!=null)modal.SetActive(false);
        if(feedback!=null)feedback.text=
            "O₂: Place 2 shared bonding pairs and 4 lone pairs (2 around each O).";
        if(scoreText!=null)scoreText.text="SCORE\n000000";
    }

    public void PairPlaced(Stage3Level3PairDropZone zone)
    {
        if(!CanInteract || !zones[phase].Contains(zone))return;
        placed[phase]++;
        var t=zone.GetComponentInChildren<TMP_Text>(true);
        if(t!=null)t.text="••";
        var image=zone.GetComponent<Image>();
        if(image!=null)image.color=new Color32(28,141,117,255);
        if(feedback!=null)feedback.text="Correct pair! "+placed[phase]+"/"+zones[phase].Count+" positions filled.";
    }

    public void WrongPlacement()
    {
        if(!CanInteract)return;
        mistakes++;
        if(mistakes==1)
        {
            if(feedback!=null)feedback.text=
                "HINT: Place BLUE pairs between atoms, GOLD lone pairs outside oxygen atoms.";
        }
        else
        {
            failed=true;
            EnergyManager.Instance.LoseEnergy(); // once, on second mistake
            if(failedPanel!=null)failedPanel.SetActive(true);
            if(retryButton!=null)retryButton.interactable=EnergyManager.Instance.HasEnergy;
            UpdateEnergy();
        }
    }

    private void CheckMolecule()
    {
        if(!CanInteract)return;
        if(placed[phase]!=zones[phase].Count)
        {
            WrongPlacement();
            return;
        }
        transitioning=true;
        if(phase==0)
        {
            if(transitionPanel!=null)
            {
                transitionPanel.SetActive(true);
                transitionPanel.transform.SetAsLastSibling();
            }
        }
        else Complete();
    }

    private void NextMolecule()
    {
        phase=1;transitioning=false;
        if(transitionPanel!=null)transitionPanel.SetActive(false);
        if(puzzles[0]!=null)puzzles[0].SetActive(false);
        if(puzzles[1]!=null)puzzles[1].SetActive(true);
        if(feedback!=null)feedback.text=
            "CO₂: Each C=O double bond needs 2 shared pairs; each O has 2 lone pairs.";
    }

    private void Complete()
    {
        completed=true;
        int score=mistakes==0?100:75;
        PlayerPrefs.SetInt("Stage3Level3Completed",1);
        PlayerPrefs.SetInt("Stage3Level3Score",score);
        PlayerPrefs.SetInt("Stage3Completed",1);
        PlayerPrefs.Save();
        if(scoreText!=null)scoreText.text="SCORE\n"+score.ToString("D6");
        if(finalScoreText!=null)finalScoreText.text="FINAL SCORE: "+score.ToString("D6");
        if(completedPanel!=null)
        {
            completedPanel.SetActive(true);
            completedPanel.transform.SetAsLastSibling();
        }
    }

    private void ResetPairs()
    {
        if(!CanInteract)return;
        foreach(var zone in zones[phase])
        {
            zone.ResetZone();
            var label=zone.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)label.text="○○";
            var img=zone.GetComponent<Image>();
            if(img!=null)img.color=new Color32(40,82,113,255);
        }
        foreach(var token in tokens[phase])token.Restore();
        placed[phase]=0;
        if(feedback!=null)feedback.text="Pairs reset. No penalty for using RESET.";
    }

    private void Update()
    {
        if(Time.unscaledTime<nextUpdate)return;
        nextUpdate=Time.unscaledTime+.35f;
        UpdateEnergy();
    }

    private void UpdateEnergy()
    {
        var shared=EnergyManager.Instance;
        if(shared==null)return;
        shared.RefreshEnergy();
        for(int i=0;i<energyIcons.Count;i++)
            if(energyIcons[i]!=null)energyIcons[i].enabled=i<shared.CurrentEnergy;
        if(energyCount!=null)energyCount.text=shared.CurrentEnergy+" / 5";
        if(energyPanel==null)return;
        bool empty=!shared.HasEnergy;
        if(empty)
        {
            if(!energyPanel.activeSelf)energyPanel.SetActive(true);
            energyPanel.transform.SetAsLastSibling();
            int seconds=shared.SecondsUntilNextEnergy();
            if(timerText!=null)timerText.text="Next Energy In: "+
                (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
        }
        else if(energyPanel.activeSelf)energyPanel.SetActive(false);
    }

    private void Pause()
    {
        if(!CanInteract)return;
        paused=true;
        if(pausePanel!=null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }
        Time.timeScale=0f;
    }
    private void Resume()
    {
        paused=false;Time.timeScale=1f;
        if(pausePanel!=null)pausePanel.SetActive(false);
    }
    private void Retry()
    {
        if(EnergyManager.Instance!=null && EnergyManager.Instance.HasEnergy)
            GoTo("Stage3Level3");
    }
    private void GoTo(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("ChemQuest: "+scene+" not enabled in Build Settings.");
            return;
        }
        SceneManager.LoadScene(scene);
    }
    private void OnDestroy(){Time.timeScale=1f;}
}
