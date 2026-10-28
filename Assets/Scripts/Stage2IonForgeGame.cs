using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Stage2IonForgeGame : MonoBehaviour
{
    public int Level=1;
    public Sprite EnergySprite;
    public GameObject PauseButtonPrefab;
    public GameObject PauseMenuPrefab;
    private readonly List<Stage2IonDraggable> tokens=new List<Stage2IonDraggable>();
    private readonly List<Stage2IonDropZone> zones=new List<Stage2IonDropZone>();
    private readonly List<Image> icons=new List<Image>();
    private GameObject failedPanel,completedPanel,pausePanel,energyPanel;
    private TMP_Text feedback,scoreText,finalScore,energyText,timerText,progressText;
    private Button retryButton;
    private int accepted,mistakes;
    private int variant=-1;
    private int chargeChoice=-1;
    private readonly bool[] finishedVariants=new bool[2];
    private TMP_Text titleLabel,instructionLabel,donorSymbol,acceptorSymbol,transferExplanation;
    private bool failed,completed,paused;
    private float nextRefresh;
    private const int TotalSlots=12;
    private static readonly Color Gold=new Color32(255,209,92,255);
    private static readonly Color Blue=new Color32(53,189,244,255);
    private static readonly Color Navy=new Color32(9,21,43,255);
    private static readonly Color Panel=new Color32(21,45,77,255);
    public bool CanInteract => !failed&&!completed&&!paused&&Time.timeScale>0 &&
        EnergyManager.Instance!=null && EnergyManager.Instance.HasEnergy;
    private int Needed => Level==1?1:Level==2?2:TotalSlots;
    private string Heading => Level==1?"SINGLE ELECTRON TRANSFER | NaCl & KCl":
        Level==2?"DIVALENT TRANSFER | MgO & MgCl2":"IONIC CRYSTAL LATTICE | NaCl";
    private string Instruction => Level==1?
        "Transfer sodium's one valence electron to chlorine. Drag the electron into the Cl receiver.":
        Level==2?"Transfer magnesium's TWO valence electrons to oxygen. Fill both receiver slots.":
        "Build an alternating Na+ / Cl- ionic lattice. Drag 3 positive ions and 3 negative ions into matching slots.";

    private void Start()
    {
        Time.timeScale=1f;
        SetupInput();
        if(EnergyManager.Instance==null)
            new GameObject("SharedEnergyManager",typeof(EnergyManager));
        if(GameObject.Find("Stage2Level"+Level+"Canvas")==null)BuildEditableUI();
        Bind();
        RefreshEnergy();
    }
    public void BuildEditableUI()
    {
        if(GameObject.Find("Stage2Level"+Level+"Canvas")!=null)return;
        var root=new GameObject("Stage2Level"+Level+"Canvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1f;

        var background=Image(root.transform,"Background",Vector2.zero,new Vector2(1280,720),Navy);
        background.raycastTarget=false;
        background.rectTransform.anchorMin=Vector2.zero;
        background.rectTransform.anchorMax=Vector2.one;
        background.rectTransform.offsetMin=Vector2.zero;
        background.rectTransform.offsetMax=Vector2.zero;

        Label(root.transform,"StageHeading","STAGE 2   |   LEVEL "+Level,20,
            new Vector2(0,325),new Vector2(750,37),Blue);
        Label(root.transform,"LevelHeading",Heading,33,
            new Vector2(0,280),new Vector2(910,55),Color.white);
        Label(root.transform,"Instructions",Instruction,17,
            new Vector2(0,229),new Vector2(1050,49),new Color32(211,230,247,255));
        Label(root.transform,"ScoreText","SCORE\\n000000",20,
            new Vector2(530,293),new Vector2(135,68),Gold);
        Label(root.transform,"EnergyTitle","ENERGY",14,new Vector2(-515,337),
            new Vector2(135,24),Color.white);
        for(int i=0;i<5;i++)
        {
            var icon=Image(root.transform,"Energy"+(i+1),
                new Vector2(-548+i*47,290),new Vector2(38,45),Color.white);
            icon.sprite=EnergySprite;icon.preserveAspect=true;
            if(EnergySprite==null)icon.color=Gold;
        }
        Label(root.transform,"EnergyText","5 / 5",14,new Vector2(-308,337),
            new Vector2(90,25),Color.white);

        var board=Image(root.transform,"ExperimentBoard",new Vector2(0,-9),
            new Vector2(1070,393),Panel);
        board.raycastTarget=false;
        var outline=board.gameObject.AddComponent<Outline>();
        outline.effectColor=new Color32(57,119,168,185);
        outline.effectDistance=new Vector2(2,2);

        if(Level<3)BuildTransfer(board.transform);
        else BuildLattice(board.transform);

        Label(root.transform,"ProgressText","PROGRESS  0 / "+Needed,18,
            new Vector2(0,-247),new Vector2(640,34),Blue);
        Label(root.transform,"FeedbackText","Drag the correct particle into the highlighted target.",
            17,new Vector2(0,-280),new Vector2(950,42),Gold);
        Button(root.transform,"ResetButton","RESET",new Vector2(-160,-328),
            new Vector2(230,52),new Color32(67,85,118,255));
        Button(root.transform,"CheckButton","FORM BOND",new Vector2(160,-328),
            new Vector2(230,52),new Color32(9,131,135,255));

        if(PauseButtonPrefab!=null)
        {
            var go=Instantiate(PauseButtonPrefab,root.transform,false);
            go.name="Stage2PauseButton";
            var t=go.GetComponent<RectTransform>();
            if(t!=null){t.anchorMin=new Vector2(0,1);t.anchorMax=new Vector2(0,1);
                t.anchoredPosition=new Vector2(60,-56);}
        }
        else Button(root.transform,"Stage2PauseButton","MENU",
            new Vector2(-523,236),new Vector2(150,50),new Color32(44,79,116,255));

        if(PauseMenuPrefab!=null)
        {
            pausePanel=Instantiate(PauseMenuPrefab,root.transform,false);
            pausePanel.name="PauseMenuPanel";
            var r=pausePanel.GetComponent<RectTransform>();
            if(r!=null){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
                r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
        }
        else
        {
            pausePanel=Modal(root.transform,"PauseMenuPanel","GAME PAUSED","");
            Button(pausePanel.transform,"ContinueButton","CONTINUE",
                new Vector2(0,25),new Vector2(315,58),Blue);
            Button(pausePanel.transform,"LevelSelectionButton","LEVEL SELECTION",
                new Vector2(0,-66),new Vector2(315,58),Blue);
            Button(pausePanel.transform,"StageSelectionButton","STAGE SELECTION",
                new Vector2(0,-155),new Vector2(315,58),Blue);
        }
        pausePanel.SetActive(false);

        failedPanel=Modal(root.transform,"FailedPanel","STAGE FAILED!",
            "Maximum wrong attempts reached.\\nOne energy has been deducted.");
        Button(failedPanel.transform,"RetryButton","RETRY",
            new Vector2(-148,-120),new Vector2(270,60),new Color32(20,100,151,255));
        Button(failedPanel.transform,"FailedLevelSelectionButton","LEVEL SELECTION",
            new Vector2(148,-120),new Vector2(270,60),new Color32(54,82,124,255));
        failedPanel.SetActive(false);

        completedPanel=Modal(root.transform,"CompletedPanel",
            Level==3?"STAGE 2 COMPLETE!":"LEVEL COMPLETE!",
            "Ionic bonding challenge successfully completed.");
        Label(completedPanel.transform,"FinalScoreText","FINAL SCORE: 000000",
            28,new Vector2(0,-35),new Vector2(605,56),Gold);
        Button(completedPanel.transform,"NextLevelButton",
            Level==3?"STAGE SELECTION":"NEXT LEVEL",
            new Vector2(150,-147),new Vector2(265,60),new Color32(9,131,135,255));
        Button(completedPanel.transform,"CompletedLevelSelectionButton","LEVEL SELECTION",
            new Vector2(-150,-147),new Vector2(265,60),new Color32(54,82,124,255));
        completedPanel.SetActive(false);

        energyPanel=Modal(root.transform,"OutOfEnergyPanel","OUT OF ENERGY!",
            "Energy regenerates automatically.");
        Label(energyPanel.transform,"EnergyTimer","NEXT ENERGY IN 15:00",24,
            new Vector2(0,-42),new Vector2(620,55),Gold);
        Button(energyPanel.transform,"EnergyLevelSelectionButton","LEVEL SELECTION",
            new Vector2(0,-143),new Vector2(310,58),Blue);
        energyPanel.SetActive(false);
    }

    private void BuildTransfer(Transform parent)
    {
        var left=Level==1?"Na":"Mg";
        var right=Level==1?"Cl":"O";
        Label(parent,"DonorLabel","ELECTRON DONOR",17,
            new Vector2(-300,177),new Vector2(350,29),Blue);
        Button(parent,"VariantAButton",Level==1?"SELECT Na":"SELECT MgO",
            new Vector2(-385,136),new Vector2(165,37),new Color32(26,111,145,255));
        Button(parent,"VariantBButton",Level==1?"SELECT K":"SELECT MgCl2",
            new Vector2(-190,136),new Vector2(165,37),new Color32(26,111,145,255));
        Label(parent,"ReceiverLabel","ELECTRON ACCEPTOR",17,
            new Vector2(275,147),new Vector2(350,34),Blue);
        var cation=Image(parent,"DonorAtom",new Vector2(-285,57),
            new Vector2(142,126),new Color32(38,111,151,255));
        Label(cation.transform,"Symbol",left,61,Vector2.zero,new Vector2(138,120),Color.white);
        var anion=Image(parent,"AcceptorAtom",new Vector2(287,57),
            new Vector2(142,126),new Color32(121,65,106,255));
        Label(anion.transform,"Symbol",right,61,Vector2.zero,new Vector2(138,120),Color.white);
        Label(parent,"TransferArrow","> > >",30,new Vector2(0,62),
            new Vector2(240,50),Gold);
        Label(parent,"Explanation",Level==1?
            "Na loses 1 e- to become Na+. Cl gains 1 e- to become Cl-.":
            "Mg loses 2 e- to become Mg2+. O gains 2 e- to become O2-.",
            18,new Vector2(0,-52),new Vector2(900,48),Color.white);
        Button(parent,"ChargeAButton","CHARGE A",new Vector2(-185,-172),
            new Vector2(285,39),new Color32(56,78,116,255));
        Button(parent,"ChargeBButton","CHARGE B",new Vector2(180,-172),
            new Vector2(285,39),new Color32(56,78,116,255));
        for(int i=0;i<Needed;i++)
        {
            float x=(i-(Needed-1)/2f)*115f;
            Zone(parent,"ElectronReceiver"+i,0,
                new Vector2(265+x,-112),new Vector2(113,61));
            Token(parent,"DonorElectron"+i,0,
                new Vector2(-301+x,-112),new Vector2(80,59),
                new Color32(248,193,71,255),"e-");
        }
    }
    private void BuildLattice(Transform parent)
    {
        Label(parent,"LatticeLabel","ALTERNATE OPPOSITELY CHARGED IONS",20,
            new Vector2(0,145),new Vector2(850,42),Blue);
        for(int i=0;i<TotalSlots;i++)
        {
            int kind=((i/4)+(i%4))%2==0?1:2;
            Zone(parent,"LatticeSlot"+i,kind,
                new Vector2(-270+(i%4)*180,78-(i/4)*83),new Vector2(115,65));
        }
        var bank=Image(parent,"IonBank",new Vector2(0,-161),
            new Vector2(850,94),new Color32(11,31,54,255));
        bank.raycastTarget=false;
        for(int i=0;i<6;i++)
        {
            Token(bank.transform,"Sodium"+i,1,new Vector2(-383+i*73,18),
                new Vector2(62,39),new Color32(53,174,212,255),"Na+");
            Token(bank.transform,"Chloride"+i,2,new Vector2(-383+i*73,-19),
                new Vector2(62,39),new Color32(242,188,84,255),"Cl-");
        }
    }

    private void Zone(Transform parent,string name,int kind,Vector2 pos,Vector2 size)
    {
        var img=Image(parent,name,pos,size,new Color32(42,88,122,255));
        var zone=img.gameObject.AddComponent<Stage2IonDropZone>();
        zone.RequiredKind=kind;
        Label(img.transform,"SlotLabel",kind==0?"DROP e-":kind==1?"DROP +":"DROP -",
            18,Vector2.zero,size,Gold);
    }
    private void Token(Transform parent,string name,int kind,Vector2 pos,Vector2 size,Color color,string symbol)
    {
        var img=Image(parent,name,pos,size,color);
        img.gameObject.AddComponent<CanvasGroup>();
        var drag=img.gameObject.AddComponent<Stage2IonDraggable>();
        drag.Kind=kind;
        Label(img.transform,"Symbol",symbol,25,Vector2.zero,size,new Color32(9,25,44,255));
    }
    private Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchoredPosition=pos;r.sizeDelta=size;
        var img=go.GetComponent<Image>();img.color=color;return img;
    }
    private TMP_Text Label(Transform parent,string name,string value,int font,
        Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchoredPosition=pos;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();
        t.text=value;t.fontSize=font;t.color=color;
        t.alignment=TextAlignmentOptions.Center;
        t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;
        return t;
    }
    private Button Button(Transform parent,string name,string text,Vector2 pos,Vector2 size,Color color)
    {
        var img=Image(parent,name,pos,size,color);
        var b=img.gameObject.AddComponent<Button>();
        b.targetGraphic=img;
        Label(img.transform,"Text",text,19,Vector2.zero,size,Color.white);
        return b;
    }
    private GameObject Modal(Transform parent,string name,string title,string description)
    {
        var overlay=Image(parent,name,Vector2.zero,new Vector2(1280,720),
            new Color32(5,13,28,238));
        var r=overlay.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
        r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
        var card=Image(overlay.transform,"ModalCard",Vector2.zero,new Vector2(710,405),Panel);
        card.raycastTarget=false;
        Label(overlay.transform,"Title",title,39,new Vector2(0,117),
            new Vector2(690,70),name=="FailedPanel"?new Color32(255,97,102,255):Gold);
        Label(overlay.transform,"Description",description,21,
            new Vector2(0,37),new Vector2(660,80),Color.white);
        return overlay.gameObject;
    }
    private Transform Find(Transform root,string name)
    {
        foreach(var x in root.GetComponentsInChildren<Transform>(true))
            if(x.name==name)return x;
        return null;
    }
    private void SetupInput()
    {
        var events=FindObjectOfType<EventSystem>();
        if(events==null)events=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var old in events.GetComponents<StandaloneInputModule>())old.enabled=false;
        if(events.GetComponent<InputSystemUIInputModule>()==null)
        {
            var module=events.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
    private void Wire(Transform root,string name,Action action)
    {
        foreach(var b in root.GetComponentsInChildren<Button>(true))
        {
            if(b.name!=name)continue;
            b.onClick=new Button.ButtonClickedEvent();
            b.onClick.AddListener(()=>action());
            b.interactable=true;
        }
    }
    private void Bind()
    {
        var root=GameObject.Find("Stage2Level"+Level+"Canvas");
        if(root==null)return;
        var t=root.transform;
        feedback=Find(t,"FeedbackText")?.GetComponent<TMP_Text>();
        progressText=Find(t,"ProgressText")?.GetComponent<TMP_Text>();
        scoreText=Find(t,"ScoreText")?.GetComponent<TMP_Text>();
        finalScore=Find(t,"FinalScoreText")?.GetComponent<TMP_Text>();
        energyText=Find(t,"EnergyText")?.GetComponent<TMP_Text>();
        timerText=Find(t,"EnergyTimer")?.GetComponent<TMP_Text>();
        failedPanel=Find(t,"FailedPanel")?.gameObject;
        completedPanel=Find(t,"CompletedPanel")?.gameObject;
        pausePanel=Find(t,"PauseMenuPanel")?.gameObject;
        energyPanel=Find(t,"OutOfEnergyPanel")?.gameObject;
        retryButton=Find(t,"RetryButton")?.GetComponent<Button>();
        titleLabel=Find(t,"LevelHeading")?.GetComponent<TMP_Text>();
        instructionLabel=Find(t,"Instructions")?.GetComponent<TMP_Text>();
        donorSymbol=Find(t,"DonorAtom")?.GetComponentInChildren<TMP_Text>(true);
        acceptorSymbol=Find(t,"AcceptorAtom")?.GetComponentInChildren<TMP_Text>(true);
        transferExplanation=Find(t,"Explanation")?.GetComponent<TMP_Text>();
        if(Level<3)EnsureVariantButtons(t);
        tokens.Clear();zones.Clear();icons.Clear();
        foreach(var token in t.GetComponentsInChildren<Stage2IonDraggable>(true))
        {
            token.Initialize(this);tokens.Add(token);
        }
        foreach(var zone in t.GetComponentsInChildren<Stage2IonDropZone>(true))
        {
            zone.Game=this;zone.ResetZone();zones.Add(zone);
        }
        for(int i=0;i<5;i++)
        {
            var icon=Find(t,"Energy"+(i+1))?.GetComponent<Image>();
            if(icon!=null)icons.Add(icon);
        }
        Wire(t,"CheckButton",CheckAnswer);
        Wire(t,"VariantAButton",()=>SelectVariant(0));
        Wire(t,"VariantBButton",()=>SelectVariant(1));
        Wire(t,"ChargeAButton",()=>SelectCharge(0));
        Wire(t,"ChargeBButton",()=>SelectCharge(1));
        Wire(t,"ResetButton",Reset);
        Wire(t,"Stage2PauseButton",Pause);
        Wire(t,"ContinueButton",Resume);
        Wire(t,"LevelSelectionButton",()=>Navigate("Stage2LevelSelection"));
        Wire(t,"StageSelectionButton",()=>Navigate("StageSelection"));
        Wire(t,"RetryButton",Retry);
        Wire(t,"FailedLevelSelectionButton",()=>Navigate("Stage2LevelSelection"));
        Wire(t,"NextLevelButton",()=>Navigate(Level==3?"StageSelection":"Stage2Level"+(Level+1)));
        Wire(t,"CompletedLevelSelectionButton",()=>Navigate("Stage2LevelSelection"));
        Wire(t,"EnergyLevelSelectionButton",()=>Navigate("Stage2LevelSelection"));
        accepted=mistakes=0;paused=failed=completed=false;
        variant=-1; chargeChoice=-1; finishedVariants[0]=finishedVariants[1]=false;
        foreach(var p in new[]{pausePanel,failedPanel,completedPanel,energyPanel})
            if(p!=null)p.SetActive(false);
        if(scoreText!=null)scoreText.text="SCORE\\n000000";
        if(progressText!=null)progressText.text="PROGRESS  0 / "+Needed;
        if(feedback!=null)feedback.text=Level<3?"SELECT A COMPOUND TO BEGIN (both variants required).":"Arrange alternating ions; every site matters.";
        if(Level<3)RefreshVariant();
    }
    // Editor-safe: upgrade existing saved scenes without deleting their UI or progress.
    public void UpgradeChallengeUI()
    {
        var board=GameObject.Find("Stage2Level"+Level+"Canvas");
        if(board==null)return;
        var root=Find(board.transform,"ExperimentBoard");
        if(root==null)return;
        if(Level==3)
        {
            UpgradeLattice(root,board.transform);
            return;
        }
        if(Find(root,"ChargeAButton")==null)
        {
            Button(root,"ChargeAButton","CHARGE A",new Vector2(-185,-172),
                new Vector2(285,39),new Color32(56,78,116,255));
            Button(root,"ChargeBButton","CHARGE B",new Vector2(180,-172),
                new Vector2(285,39),new Color32(56,78,116,255));
        }
        if(Find(root,"VariantAButton")==null)
        {
            Button(root,"VariantAButton",Level==1?"SELECT Na":"SELECT MgO",
                new Vector2(-385,136),new Vector2(165,37),new Color32(26,111,145,255));
            Button(root,"VariantBButton",Level==1?"SELECT K":"SELECT MgCl2",
                new Vector2(-190,136),new Vector2(165,37),new Color32(26,111,145,255));
        }
        var donor=Find(root,"DonorLabel")?.GetComponent<RectTransform>();
        if(donor!=null)donor.anchoredPosition=new Vector2(-300,177);
        var head=Find(board.transform,"LevelHeading")?.GetComponent<TMP_Text>();
        if(head!=null)head.text=Heading;
        var instr=Find(board.transform,"Instructions")?.GetComponent<TMP_Text>();
        if(instr!=null)instr.text=Instruction;
        var check=Find(board.transform,"CheckButton")?.GetComponentInChildren<TMP_Text>(true);
        if(check!=null)check.text="FORM BOND";
    }

    private void UpgradeLattice(Transform board,Transform canvas)
    {
        var heading=Find(canvas,"LevelHeading")?.GetComponent<TMP_Text>();
        if(heading!=null)heading.text=Heading;
        var instructions=Find(canvas,"Instructions")?.GetComponent<TMP_Text>();
        if(instructions!=null)instructions.text=Instruction;
        var progress=Find(canvas,"ProgressText")?.GetComponent<TMP_Text>();
        if(progress!=null)progress.text="PROGRESS  0 / 12";
        for(int i=0;i<12;i++)
        {
            int kind=((i/4)+(i%4))%2==0?1:2;
            var zone=Find(board,"LatticeSlot"+i);
            if(zone==null)
                Zone(board,"LatticeSlot"+i,kind,
                    new Vector2(-270+(i%4)*180,78-(i/4)*83),new Vector2(115,65));
            else
            {
                var z=zone.GetComponent<Stage2IonDropZone>();
                if(z!=null)z.RequiredKind=kind;
                var rect=zone.GetComponent<RectTransform>();
                if(rect!=null){rect.anchoredPosition=new Vector2(-270+(i%4)*180,78-(i/4)*83);
                    rect.sizeDelta=new Vector2(115,65);}
            }
        }
        var bank=Find(board,"IonBank");
        if(bank!=null)
        {
            var rect=bank.GetComponent<RectTransform>();
            if(rect!=null)rect.sizeDelta=new Vector2(850,94);
            for(int i=0;i<6;i++)
            {
                foreach(var kind in new[]{1,2})
                {
                    string tokenName=(kind==1?"Sodium":"Chloride")+i;
                    var token=Find(bank,tokenName);
                    var pos=new Vector2(-383+i*73,kind==1?18:-19);
                    if(token==null)
                        Token(bank,tokenName,kind,pos,new Vector2(62,39),
                            kind==1?new Color32(53,174,212,255):new Color32(242,188,84,255),
                            kind==1?"Na+":"Cl-");
                    else
                    {
                        var rectToken=token.GetComponent<RectTransform>();
                        if(rectToken!=null){rectToken.anchoredPosition=pos;rectToken.sizeDelta=new Vector2(62,39);}
                    }
                }
            }
        }
    }

    private void EnsureVariantButtons(Transform root)
    {
        UpgradeChallengeUI();
    }
    private void SelectVariant(int v)
    {
        if(!CanInteract || Level==3)return;
        if(finishedVariants[v])
        {
            if(feedback!=null)feedback.text="Already completed. Choose the remaining compound.";
            return;
        }
        variant=v;
        accepted=0;
        chargeChoice=-1;
        foreach(var item in tokens)item.Initialize(this);
        foreach(var item in zones)item.ResetZone();
        RefreshVariant();
    }
    private void SelectCharge(int choice)
    {
        if(!CanInteract || Level>=3 || variant<0)return;
        chargeChoice=choice;
        if(feedback!=null)feedback.text="Charge option "+(choice==0?"A":"B")+
            " chosen. Transfer all electrons, then press FORM BOND to verify.";
    }

    private void RefreshVariant()
    {
        if(Level>=3)return;
        if(titleLabel!=null)titleLabel.text=Heading;
        if(instructionLabel!=null)instructionLabel.text=Instruction;
        if(progressText!=null)progressText.text="COMPOUNDS "+(Convert.ToInt32(finishedVariants[0])+Convert.ToInt32(finishedVariants[1]))+"/2  |  ELECTRONS "+accepted+"/"+Needed;
        if(donorSymbol!=null)donorSymbol.text=Level==1?(variant==1?"K":"Na"):"Mg";
        if(acceptorSymbol!=null)acceptorSymbol.text=Level==1?"Cl":(variant==1?"2Cl":"O");
        if(transferExplanation!=null)
        {
            transferExplanation.text=Level==1?
                "Donor has ONE outer electron; chlorine starts with SEVEN. Determine the resulting ion charges.":
                (variant==1?"Magnesium gives one electron to EACH chlorine atom. Determine charges and the ratio.":
                    "Magnesium gives both outer electrons to oxygen. Determine the resulting ion charges.");
        }
        var canvas=GameObject.Find("Stage2Level"+Level+"Canvas");
        if(canvas!=null)
        {
            var root=canvas.transform;
            var a=Find(root,"ChargeAButton")?.GetComponentInChildren<TMP_Text>(true);
            var b=Find(root,"ChargeBButton")?.GetComponentInChildren<TMP_Text>(true);
            if(a!=null)a.text=Level==1?(variant==1?"K- & Cl+":"Na+ & Cl-"):
                (variant==1?"Mg2+ & 2Cl-":"Mg+ & O-");
            if(b!=null)b.text=Level==1?(variant==1?"K+ & Cl-":"Na- & Cl+"):
                (variant==1?"Mg+ & 2Cl+":"Mg2+ & O2-");
        }
        if(Level==2)
        {
            foreach(var z in zones)
            {
                var txt=z.GetComponentInChildren<TMP_Text>(true);
                if(txt!=null)txt.text=variant==1?"Cl +1 e-":"O +1 e-";
            }
        }
        if(feedback!=null && variant>=0)
            feedback.text="Selected: "+(Level==1?(variant==1?"KCl":"NaCl"):(variant==1?"MgCl2":"MgO"))+
                ". Transfer electrons and press FORM BOND.";
    }

    public string GetAcceptedLabel(int kind) => kind==0?"e-":kind==1?"Na+":"Cl-";
    public void AcceptedDrop(int kind)
    {
        if(!CanInteract)return;
        accepted++;
        if(progressText!=null)progressText.text=Level<3?
            "COMPOUNDS "+(Convert.ToInt32(finishedVariants[0])+Convert.ToInt32(finishedVariants[1]))+"/2  |  ELECTRONS "+accepted+"/"+Needed:
            "LATTICE IONS "+accepted+"/"+Needed;
        if(feedback!=null)feedback.text=Level==1?"Electron transferred. Press FORM BOND to check ion formation.":
            Level==2?"Electrons transferred: "+accepted+" / 2":
            "Ions in lattice: "+accepted+" / 12";
    }
    public void WrongAttempt()
    {
        if(!CanInteract)return;
        mistakes++;
        if(mistakes==1)
        {
            if(feedback!=null)feedback.text=Level==3?
                "HINT: Positive and negative ions must alternate in the lattice.":
                "HINT: Drag each donor electron into the matching receiver slot.";
        }
        else
        {
            failed=true;
            EnergyManager.Instance.LoseEnergy();
            if(failedPanel!=null){failedPanel.SetActive(true);failedPanel.transform.SetAsLastSibling();}
            if(retryButton!=null)retryButton.interactable=EnergyManager.Instance.HasEnergy;
            RefreshEnergy();
        }
    }
    private void CheckAnswer()
    {
        if(!CanInteract)return;
        if(Level<3 && variant<0){if(feedback!=null)feedback.text="Choose Na/K or MgO/MgCl2 first.";return;}
        if(accepted<Needed){WrongAttempt();return;}
        if(Level<3 && chargeChoice<0)
        {
            if(feedback!=null)feedback.text="Choose the correct ionic charges (A or B) before FORM BOND.";
            return;
        }
        if(Level<3 && chargeChoice!=(Level==1?(variant==0?0:1):(variant==0?1:0)))
        {WrongAttempt();return;}
        if(Level<3)
        {
            finishedVariants[variant]=true;
            if(!finishedVariants[0] || !finishedVariants[1])
            {
                string product=Level==1?(variant==0?"NaCl":"KCl"):(variant==0?"MgO":"MgCl2");
                if(feedback!=null)feedback.text=product+" formed! Opposite ions attract. Complete the OTHER compound.";
                variant=-1;chargeChoice=-1;accepted=0;
                foreach(var item in tokens)item.Initialize(this);
                foreach(var item in zones)item.ResetZone();
                RefreshVariant();
                return;
            }
        }
        completed=true;
        int score=mistakes==0?100:75;
        PlayerPrefs.SetInt("Stage2Level"+Level+"Completed",1);
        PlayerPrefs.SetInt("Stage2Level"+Level+"Score",score);
        if(Level<3)PlayerPrefs.SetInt("Stage2Level"+(Level+1)+"Unlocked",1);
        else PlayerPrefs.SetInt("Stage2Completed",1);
        PlayerPrefs.Save();
        if(scoreText!=null)scoreText.text="SCORE\\n"+score.ToString("D6");
        if(finalScore!=null)finalScore.text="FINAL SCORE: "+score.ToString("D6");
        if(completedPanel!=null){completedPanel.SetActive(true);completedPanel.transform.SetAsLastSibling();}
    }
    private void Reset()
    {
        if(!CanInteract)return;
        foreach(var t in tokens)t.Initialize(this);
        foreach(var z in zones)z.ResetZone();
        accepted=0;
        if(progressText!=null)progressText.text="PROGRESS  0 / "+Needed;
        if(feedback!=null)feedback.text="Reset complete. No penalty.";
    }
    private void Retry()
    {
        if(EnergyManager.Instance!=null&&EnergyManager.Instance.HasEnergy)
            Navigate("Stage2Level"+Level);
    }
    private void Pause()
    {
        if(!CanInteract)return;
        paused=true;Time.timeScale=0;
        if(pausePanel!=null){pausePanel.SetActive(true);pausePanel.transform.SetAsLastSibling();}
    }
    private void Resume()
    {
        paused=false;Time.timeScale=1;
        if(pausePanel!=null)pausePanel.SetActive(false);
    }
    private void Navigate(string name)
    {
        Time.timeScale=1;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {Debug.LogError("Stage 2: missing scene in Build Settings: "+name);return;}
        SceneManager.LoadScene(name);
    }
    private void Update()
    {
        if(Time.unscaledTime<nextRefresh)return;
        nextRefresh=Time.unscaledTime+.25f;
        RefreshEnergy();
    }
    private void RefreshEnergy()
    {
        var energy=EnergyManager.Instance;
        if(energy==null)return;
        energy.RefreshEnergy();
        for(int i=0;i<icons.Count;i++)
            if(icons[i]!=null)icons[i].enabled=i<energy.CurrentEnergy;
        if(energyText!=null)energyText.text=energy.CurrentEnergy+" / 5";
        if(energyPanel!=null)
        {
            if(!energy.HasEnergy)
            {
                energyPanel.SetActive(true);
                energyPanel.transform.SetAsLastSibling();
                int remaining=energy.SecondsUntilNextEnergy();
                if(timerText!=null)timerText.text="NEXT ENERGY IN "+
                    (remaining/60).ToString("00")+":"+(remaining%60).ToString("00");
            }
            else if(energyPanel.activeSelf)energyPanel.SetActive(false);
        }
    }
    private void OnDestroy(){Time.timeScale=1;}
}
