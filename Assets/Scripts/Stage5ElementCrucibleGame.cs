using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ChemQuest Stage 5: gameplay follows the revised Element Crucible manuscript.
// Samples are fictional teaching examples, not identified real chemicals.
public class Stage5ElementCrucibleGame : MonoBehaviour
{
    public int Level=1;
    public Sprite EnergySprite;
    public Sprite BackgroundSprite;
    private static readonly Color Navy=new Color32(9,23,45,255);
    private static readonly Color Panel=new Color32(23,48,78,255);
    private static readonly Color Teal=new Color32(20,139,147,255);
    private static readonly Color Cyan=new Color32(80,198,243,255);
    private static readonly Color Gold=new Color32(255,206,100,255);
    private readonly string[] samples={"A","B","C"};
    private readonly string[] thermal={"Softens/melts at low setting","Remains solid under this heating range","Remains solid under this heating range"};
    private readonly string[] mechanical={"Soft material; little shape retention","Brittle: chips/fragments after impact","Malleable: bends/flattens without shattering"};
    private readonly string[] types={"MOLECULAR","IONIC","METALLIC"};
    private int selected=-1,wrong,completedCount;
    private bool failed,completed,paused;
    private readonly bool[] examined=new bool[3];
    private readonly bool[] classified=new bool[3];
    private readonly bool[,] probed=new bool[3,3];
    private int[] selectedBin={-1,-1,-1};
    private string stateName="SOLID";
    private int stateIndex;
    private TMP_Text feedback,progress,scoreText,finalScore,energyText,timer,selectedLabel,evidenceText,resultLabel;
    private GameObject failPanel,completePanel,pausePanel,emptyPanel;
    private Slider heat;
    private Transform root;
    private float nextEnergyUpdate;
    public bool CanInteract=>!failed&&!completed&&!paused&&Time.timeScale>0&&
        EnergyManager.Instance!=null&&EnergyManager.Instance.HasEnergy;
    private string Header=>Level==1?"THERMAL RESISTANCE TRIAL":
        Level==2?"HARDNESS & DUCTILITY ASSAY":"SOLUTION & PHASE CONDUCTIVITY";
    private string Instructions=>Level==1?
        "Drag each unknown sample onto the heating bench. Adjust heat and TEST to record evidence.":
        Level==2?"Drag samples onto the mechanical bench. Use STRIKE and PRESS to compare breaking and deformation.":
        "Test SOLID, MOLTEN and AQUEOUS conditions when applicable. Then drag A/B/C into the classification bins.";

    private void Start()
    {
        Time.timeScale=1;
        EnsureInput();
        if(EnergyManager.Instance==null)new GameObject("SharedEnergyManager",typeof(EnergyManager));
        if(GameObject.Find("Stage5Level"+Level+"Canvas")==null)BuildEditableUI();
        root=GameObject.Find("Stage5Level"+Level+"Canvas").transform;
        Bind();
        RefreshEnergy();
    }

    public void BuildEditableUI()
    {
        if(GameObject.Find("Stage5Level"+Level+"Canvas")!=null)return;
        var canvasGO=new GameObject("Stage5Level"+Level+"Canvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.matchWidthOrHeight=1f;
        var back=Image(canvasGO.transform,"Background",Vector2.zero,new Vector2(1280,720),Navy);
        back.sprite=BackgroundSprite;back.raycastTarget=false;
        back.rectTransform.anchorMin=Vector2.zero;back.rectTransform.anchorMax=Vector2.one;
        back.rectTransform.offsetMin=Vector2.zero;back.rectTransform.offsetMax=Vector2.zero;
        if(BackgroundSprite!=null)
        {
            var shade=Image(canvasGO.transform,"DarkOverlay",Vector2.zero,
                new Vector2(1280,720),new Color32(5,16,35,222));
            shade.raycastTarget=false;shade.rectTransform.anchorMin=Vector2.zero;
            shade.rectTransform.anchorMax=Vector2.one;
            shade.rectTransform.offsetMin=Vector2.zero;shade.rectTransform.offsetMax=Vector2.zero;
        }
        Txt(canvasGO.transform,"StageText","STAGE 5   |   LEVEL "+Level,19,new Vector2(0,333),
            new Vector2(760,31),Cyan);
        Txt(canvasGO.transform,"LevelTitle",Header,31,new Vector2(0,290),
            new Vector2(980,51),Color.white);
        Txt(canvasGO.transform,"Instructions",Instructions,18,new Vector2(0,244),
            new Vector2(1070,50),Color.white);
        Txt(canvasGO.transform,"Score","SCORE\n000000",18,new Vector2(540,313),
            new Vector2(125,56),Gold);
        Txt(canvasGO.transform,"EnergyLabel","ENERGY",13,new Vector2(-555,350),
            new Vector2(120,24),Color.white);
        for(int i=0;i<5;i++)
        {
            var img=Image(canvasGO.transform,"Energy"+(i+1),new Vector2(-552+i*43,307),
                new Vector2(35,41),EnergySprite==null?Gold:Color.white);
            if(EnergySprite!=null){img.sprite=EnergySprite;img.preserveAspect=true;}
            img.raycastTarget=false;
        }
        Txt(canvasGO.transform,"EnergyCount","5 / 5",14,
            new Vector2(-315,340),new Vector2(80,27),Color.white);
        Btn(canvasGO.transform,"MenuButton","MENU",new Vector2(-535,238),
            new Vector2(132,44),new Color32(46,84,122,255));

        var board=Image(canvasGO.transform,"LabBoard",new Vector2(0,-6),
            new Vector2(1080,420),Panel);board.raycastTarget=false;
        Txt(board.transform,"BoardTitle",Level==1?"HEATING WORKBENCH":Level==2?
            "MECHANICAL PROPERTY WORKBENCH":"ELECTRICAL CONDUCTIVITY WORKBENCH",19,
            new Vector2(0,182),new Vector2(820,36),Cyan);
        var bank=Image(board.transform,"SampleBank",new Vector2(-340,-25),
            new Vector2(260,275),new Color32(12,33,58,255));bank.raycastTarget=false;
        Txt(bank.transform,"BankLabel","UNKNOWN SAMPLES",18,new Vector2(0,111),
            new Vector2(240,35),Color.white);
        for(int i=0;i<3;i++)
        {
            var token=Btn(bank.transform,"Sample"+i,"SAMPLE "+samples[i],
                new Vector2(0,55-i*62),new Vector2(194,53),
                i==0?new Color32(75,124,175,255):i==1?
                new Color32(124,93,148,255):new Color32(72,143,130,255));
            // Button remains for keyboard/touch accessibility; drag is the primary interaction.
            token.gameObject.AddComponent<CanvasGroup>();
            token.gameObject.AddComponent<Stage5SampleDrag>().SampleIndex=i;
        }
        var station=Image(board.transform,"TestStation",new Vector2(35,2),
            new Vector2(430,284),new Color32(14,38,67,255));
        station.gameObject.AddComponent<Stage5SampleDrop>().BinIndex=-1;
        Txt(station.transform,"StationLabel","DROP A SAMPLE HERE",19,
            new Vector2(0,94),new Vector2(395,45),Gold);
        Txt(station.transform,"SelectedSample","NO SAMPLE LOADED",22,
            new Vector2(0,20),new Vector2(398,52),Color.white);
        Txt(station.transform,"ExperimentResult","Observe the selected sample",16,
            new Vector2(0,-73),new Vector2(392,82),Cyan);
        var indicator=Image(station.transform,"TestIndicator",new Vector2(0,-123),
            new Vector2(100,12),new Color32(70,90,120,255));
        indicator.raycastTarget=false;
        var panel=Image(board.transform,"EvidenceBoard",new Vector2(385,-20),
            new Vector2(230,296),new Color32(12,33,58,255));panel.raycastTarget=false;
        Txt(panel.transform,"EvidenceTitle","LAB NOTEBOOK",18,new Vector2(0,119),
            new Vector2(215,34),Gold);
        Txt(panel.transform,"EvidenceText","A: NOT TESTED\n\nB: NOT TESTED\n\nC: NOT TESTED",
            15,new Vector2(0,-10),new Vector2(212,215),Color.white);

        if(Level==1)
        {
            Txt(canvasGO.transform,"HeatLabel","HEAT SETTING",16,
                new Vector2(-310,-238),new Vector2(180,31),Gold);
            var sliderGO=new GameObject("HeatSlider",typeof(RectTransform),typeof(Slider));
            sliderGO.transform.SetParent(canvasGO.transform,false);
            var rt=sliderGO.GetComponent<RectTransform>();
            rt.sizeDelta=new Vector2(310,35);rt.anchoredPosition=new Vector2(-90,-242);
            var bg=Image(sliderGO.transform,"Track",Vector2.zero,
                new Vector2(310,12),new Color32(45,85,116,255));
            var fill=Image(sliderGO.transform,"Fill",new Vector2(-75,0),
                new Vector2(155,12),Teal);
            var knob=Image(sliderGO.transform,"Handle",new Vector2(0,0),
                new Vector2(25,30),Gold);
            var s=sliderGO.GetComponent<Slider>();
            s.targetGraphic=knob;s.fillRect=fill.rectTransform;
            s.handleRect=knob.rectTransform;s.minValue=0;s.maxValue=100;s.value=0;
            Btn(canvasGO.transform,"TestButton","APPLY HEAT / OBSERVE",
                new Vector2(330,-242),new Vector2(320,48),Teal);
        }
        else if(Level==2)
        {
            Btn(canvasGO.transform,"StrikeButton","STRIKE SAMPLE",
                new Vector2(-150,-242),new Vector2(260,51),new Color32(123,77,104,255));
            Btn(canvasGO.transform,"PressButton","PRESS SAMPLE",
                new Vector2(160,-242),new Vector2(260,51),Teal);
        }
        else
        {
            Btn(canvasGO.transform,"SolidButton","SOLID",new Vector2(-342,-236),
                new Vector2(160,43),new Color32(58,89,137,255));
            Btn(canvasGO.transform,"MoltenButton","MOLTEN",new Vector2(-155,-236),
                new Vector2(160,43),new Color32(58,89,137,255));
            Btn(canvasGO.transform,"AqueousButton","AQUEOUS",new Vector2(30,-236),
                new Vector2(160,43),new Color32(58,89,137,255));
            Btn(canvasGO.transform,"ProbeButton","TEST PROBE",new Vector2(260,-236),
                new Vector2(245,43),Teal);
            for(int j=0;j<3;j++)
            {
                var bin=Image(canvasGO.transform,"Bin"+j,
                    new Vector2(-330+j*330,-307),new Vector2(285,57),
                    new Color32(23,92,120,255));
                bin.gameObject.AddComponent<Stage5SampleDrop>().BinIndex=j;
                Txt(bin.transform,"Label",j==0?"IONIC COMPOUND":j==1?
                    "COVALENT MOLECULE":"METALLIC ELEMENT",16,Vector2.zero,
                    new Vector2(272,52),Color.white);
            }
        }
        Txt(canvasGO.transform,"Progress","PROGRESS  0 / 3",18,new Vector2(0,-278),
            new Vector2(400,31),Cyan);
        Txt(canvasGO.transform,"Feedback","Perform the experiment and record observations.",16,
            new Vector2(0,-353),new Vector2(980,36),Gold);
        Btn(canvasGO.transform,"SubmitButton",Level==3?"CHECK CLASSIFICATION":"SUBMIT LAB RESULTS",
            new Vector2(440,-352),new Vector2(230,45),Teal);

        pausePanel=Modal(canvasGO.transform,"PausePanel","GAME PAUSED","Your experiment has been paused.");
        Btn(pausePanel.transform,"ContinueButton","CONTINUE",new Vector2(0,5),
            new Vector2(290,58),Teal);
        Btn(pausePanel.transform,"PauseLevelButton","LEVEL SELECTION",new Vector2(0,-78),
            new Vector2(290,58),new Color32(45,83,123,255));
        Btn(pausePanel.transform,"PauseStageButton","STAGE SELECTION",new Vector2(0,-159),
            new Vector2(290,58),new Color32(45,83,123,255));
        pausePanel.SetActive(false);
        failPanel=Modal(canvasGO.transform,"FailedPanel","STAGE FAILED!",
            "Second incorrect submission. One energy has been deducted.");
        Btn(failPanel.transform,"RetryButton","RETRY",new Vector2(-156,-130),
            new Vector2(270,61),Teal);
        Btn(failPanel.transform,"FailLevelButton","LEVEL SELECTION",new Vector2(156,-130),
            new Vector2(270,61),new Color32(45,83,123,255));
        failPanel.SetActive(false);
        completePanel=Modal(canvasGO.transform,"CompletePanel",
            Level==3?"ELEMENT CRUCIBLE COMPLETE!":"LEVEL COMPLETE!",
            "Workbench observations recorded successfully.");
        Txt(completePanel.transform,"FinalScore","FINAL SCORE 000000",27,
            new Vector2(0,-33),new Vector2(640,50),Gold);
        Btn(completePanel.transform,"NextButton",Level==3?"STAGE SELECTION":"NEXT LEVEL",
            new Vector2(160,-139),new Vector2(280,58),Teal);
        Btn(completePanel.transform,"CompleteLevelButton","LEVEL SELECTION",
            new Vector2(-160,-139),new Vector2(280,58),new Color32(45,83,123,255));
        completePanel.SetActive(false);
        emptyPanel=Modal(canvasGO.transform,"OutOfEnergyPanel","OUT OF ENERGY",
            "Energy automatically regenerates every 15 minutes.");
        Txt(emptyPanel.transform,"EnergyTimer","NEXT ENERGY 15:00",23,
            new Vector2(0,-55),new Vector2(620,44),Gold);
        Btn(emptyPanel.transform,"EmptyLevelButton","LEVEL SELECTION",
            new Vector2(0,-152),new Vector2(300,55),Teal);
        emptyPanel.SetActive(false);
    }

    private static Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color tint)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();
        r.sizeDelta=size;r.anchoredPosition=pos;
        var image=go.GetComponent<Image>();image.color=tint;
        return image;
    }
    private static TMP_Text Txt(Transform parent,string name,string value,int font,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();
        r.sizeDelta=size;r.anchoredPosition=pos;
        var txt=go.GetComponent<TextMeshProUGUI>();txt.text=value;
        txt.fontSize=font;txt.color=color;txt.alignment=TextAlignmentOptions.Center;
        txt.raycastTarget=false;txt.textWrappingMode=TextWrappingModes.Normal;
        return txt;
    }
    private static Button Btn(Transform parent,string name,string caption,Vector2 pos,Vector2 size,Color color)
    {
        var image=Image(parent,name,pos,size,color);
        var btn=image.gameObject.AddComponent<Button>();
        btn.targetGraphic=image;
        Txt(image.transform,"Caption",caption,18,Vector2.zero,size,Color.white);
        return btn;
    }
    private static GameObject Modal(Transform parent,string name,string heading,string info)
    {
        var backdrop=Image(parent,name,Vector2.zero,new Vector2(1280,720),new Color32(5,13,29,236));
        var r=backdrop.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
        r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
        var card=Image(backdrop.transform,"ModalCard",Vector2.zero,
            new Vector2(720,420),Panel);card.raycastTarget=false;
        Txt(backdrop.transform,"Title",heading,36,new Vector2(0,121),new Vector2(680,71),Gold);
        Txt(backdrop.transform,"Description",info,20,new Vector2(0,40),
            new Vector2(675,100),Color.white);
        return backdrop.gameObject;
    }
    private static Transform Find(Transform parent,string name)
    {
        if(parent==null)return null;
        foreach(var child in parent.GetComponentsInChildren<Transform>(true))
            if(child.name==name)return child;
        return null;
    }
    private static void EnsureInput()
    {
        var eventSystem=FindObjectOfType<EventSystem>();
        if(eventSystem==null)eventSystem=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var legacy in eventSystem.GetComponents<StandaloneInputModule>())legacy.enabled=false;
        if(eventSystem.GetComponent<InputSystemUIInputModule>()==null)
        {
            var input=eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            input.AssignDefaultActions();
        }
    }
    private void Wire(string name,Action action)
    {
        foreach(var b in root.GetComponentsInChildren<Button>(true))
        {
            if(b.name!=name)continue;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(()=>action());
        }
    }
    private void Bind()
    {
        progress=Find(root,"Progress")?.GetComponent<TMP_Text>();
        feedback=Find(root,"Feedback")?.GetComponent<TMP_Text>();
        scoreText=Find(root,"Score")?.GetComponent<TMP_Text>();
        finalScore=Find(root,"FinalScore")?.GetComponent<TMP_Text>();
        energyText=Find(root,"EnergyCount")?.GetComponent<TMP_Text>();
        timer=Find(root,"EnergyTimer")?.GetComponent<TMP_Text>();
        selectedLabel=Find(root,"SelectedSample")?.GetComponent<TMP_Text>();
        resultLabel=Find(root,"ExperimentResult")?.GetComponent<TMP_Text>();
        evidenceText=Find(root,"EvidenceText")?.GetComponent<TMP_Text>();
        heat=Find(root,"HeatSlider")?.GetComponent<Slider>();
        pausePanel=Find(root,"PauseMenuPanel")?.gameObject;
        if(pausePanel==null)pausePanel=Find(root,"PausePanel")?.gameObject;
        failPanel=Find(root,"FailedPanel")?.gameObject;
        completePanel=Find(root,"CompletePanel")?.gameObject;
        emptyPanel=Find(root,"OutOfEnergyPanel")?.gameObject;
        foreach(var d in root.GetComponentsInChildren<Stage5SampleDrag>(true))
        {
            d.Game=this;d.ResetToken();
            int index=d.SampleIndex;
            var b=d.GetComponent<Button>();
            if(b!=null){b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>LoadSample(index));}
        }
        foreach(var target in root.GetComponentsInChildren<Stage5SampleDrop>(true))target.Game=this;
        Wire("MenuButton",Pause);
        Wire("PauseButton",Pause);
        Wire("Stage5PauseButton",Pause);
        Wire("ContinueButton",Resume);
        Wire("PauseLevelButton",()=>Go("Stage5LevelSelection"));
        Wire("LevelSelectionButton",()=>Go("Stage5LevelSelection"));
        Wire("PauseStageButton",()=>Go("StageSelection"));
        Wire("StageSelectionButton",()=>Go("StageSelection"));
        Wire("RetryButton",()=>Go("Stage5Level"+Level));
        Wire("FailLevelButton",()=>Go("Stage5LevelSelection"));
        Wire("CompleteLevelButton",()=>Go("Stage5LevelSelection"));
        Wire("EmptyLevelButton",()=>Go("Stage5LevelSelection"));
        Wire("NextButton",()=>Go(Level==3?"StageSelection":"Stage5Level"+(Level+1)));
        Wire("TestButton",ApplyHeat);
        Wire("StrikeButton",()=>MechanicalTest(false));
        Wire("PressButton",()=>MechanicalTest(true));
        Wire("SolidButton",()=>SelectState(0));
        Wire("MoltenButton",()=>SelectState(1));
        Wire("AqueousButton",()=>SelectState(2));
        Wire("ProbeButton",Probe);
        Wire("SubmitButton",Submit);
        foreach(var panel in new[]{pausePanel,failPanel,completePanel,emptyPanel})
            if(panel!=null)panel.SetActive(false);
        selected=-1;wrong=0;completedCount=0;failed=completed=paused=false;
        for(int i=0;i<3;i++){examined[i]=classified[i]=false;selectedBin[i]=-1;
            for(int j=0;j<3;j++)probed[i,j]=false;}
        UpdateEvidence();
        RefreshBins();
        if(feedback!=null)feedback.text=Instructions;
    }
    public void DropSample(int sample,int bin)
    {
        if(!CanInteract)return;
        if(bin<0){LoadSample(sample);return;}
        if(Level!=3)return;
        selectedBin[sample]=bin;
        RefreshBins();
        if(feedback!=null)feedback.text="Placed sample "+samples[sample]+
            " in "+(bin==0?"IONIC":bin==1?"MOLECULAR":"METALLIC")+" bin. Press CHECK to submit.";
    }
    private void RefreshBins()
    {
        if(Level!=3)return;
        for(int bin=0;bin<3;bin++)
        {
            var panel=Find(root,"Bin"+bin);
            if(panel==null)continue;
            string type=bin==0?"IONIC COMPOUND":bin==1?"COVALENT MOLECULE":"METALLIC ELEMENT";
            string placed="";
            for(int sample=0;sample<3;sample++)
                if(selectedBin[sample]==bin)placed+=" "+samples[sample];
            var label=Find(panel,"Label")?.GetComponent<TMP_Text>();
            if(label!=null)label.text=type+(placed.Length>0?" |"+placed:"");
        }
    }

    private void LoadSample(int index)
    {
        if(!CanInteract)return;
        selected=index;
        if(selectedLabel!=null)selectedLabel.text="SAMPLE "+samples[index]+" LOADED";
        if(resultLabel!=null)resultLabel.text="Ready for "+(Level==1?"heat test":Level==2?"strike/press test":"probe test");
        SetIndicator(new Color32(70,90,120,255));
        if(feedback!=null)feedback.text="Sample "+samples[index]+" placed on workbench.";
    }
    private void ApplyHeat()
    {
        if(!CanInteract||Level!=1)return;
        if(selected<0){Info("Drag a sample onto the heating bench first.");return;}
        float amount=heat==null?0:heat.value;
        if(amount<60){Info("Increase the heat above 60% to complete this comparison.");return;}
        examined[selected]=true;
        resultLabel.text=thermal[selected];
        ReactionEffect("THERMAL TEST COMPLETE");
        SetIndicator(selected==0?new Color32(255,177,71,255):new Color32(80,180,239,255));
        Info("Recorded thermal observation for sample "+samples[selected]+
            ". Compare all three; melting behavior alone is not conclusive.");
        UpdateEvidence();
    }
    private readonly bool[,] actions=new bool[3,2];
    private void MechanicalTest(bool press)
    {
        if(!CanInteract||Level!=2)return;
        if(selected<0){Info("Place a sample in the mechanical station first.");return;}
        actions[selected,press?1:0]=true;
        string response=selected==0?(press?"Soft sample compresses":"Surface is soft"):
            selected==1?(press?"Crystal resists deformation":"Brittle sample breaks into pieces"):
            (press?"Metallic sample flattens":"Metal retains cohesion after impact");
        resultLabel.text=response;
        ReactionEffect(press?"PRESS OBSERVATION":"IMPACT OBSERVATION");
        SetIndicator(selected==1?new Color32(240,105,115,255):
            selected==2?new Color32(75,202,165,255):new Color32(255,205,104,255));
        if(actions[selected,0]&&actions[selected,1])examined[selected]=true;
        Info("Observation saved: "+samples[selected]+". Test both STRIKE and PRESS.");
        UpdateEvidence();
    }
    private void SelectState(int s)
    {
        if(!CanInteract||Level!=3)return;
        stateIndex=s;
        stateName=s==0?"SOLID":s==1?"MOLTEN":"AQUEOUS";
        Info("Selected state: "+stateName+". Place sample and click TEST PROBE.");
    }
    private void Probe()
    {
        if(!CanInteract||Level!=3)return;
        if(selected<0){Info("Place a sample on the conductivity station first.");return;}
        if(selected==2&&stateIndex==2)
        {
            Info("Aqueous metal is not modeled as a dissolved metal sample. Try solid or molten.");
            return;
        }
        probed[selected,stateIndex]=true;
        bool conducts=selected==1?stateIndex!=0:selected==2;
        if(resultLabel!=null)resultLabel.text="PROBE: "+(conducts?"LAMP ON — CONDUCTS":"LAMP OFF — NO DETECTABLE CONDUCTION");
        SetIndicator(conducts?new Color32(79,231,151,255):new Color32(80,91,119,255));
        ReactionEffect(conducts?"CONDUCTIVITY DETECTED":"NO CONDUCTIVITY");
        Info("Recorded "+stateName+" conductivity for sample "+samples[selected]+".");
        UpdateEvidence();
    }
    private void Submit()
    {
        if(!CanInteract)return;
        if(Level<3)
        {
            for(int i=0;i<3;i++)
                if(!examined[i]){WrongAttempt("Not all samples have complete observations.");return;}
            CompleteLevel();
            return;
        }
        for(int i=0;i<3;i++)
        {
            if(!probed[i,0] || !probed[i,1] || (i==1&&!probed[i,2]))
            {
                Info("Test each sample in solid and molten states, plus aqueous solution for the ionic sample.");
                return;
            }
        }
        // A = molecular, B = ionic, C = metallic in this simplified example set.
        for(int i=0;i<3;i++)
        {
            int correct=i==0?1:i==1?0:2;
            if(selectedBin[i]!=correct)
            {WrongAttempt("Use the thermal, mechanical, and conductivity evidence together.");return;}
        }
        CompleteLevel();
    }
    private void WrongAttempt(string message)
    {
        wrong++;
        if(wrong==1){Info("HINT: "+message+" You have one remaining submission.");return;}
        failed=true;
        EnergyManager.Instance.LoseEnergy();
        if(failPanel!=null){failPanel.SetActive(true);failPanel.transform.SetAsLastSibling();}
        var retry=Find(root,"RetryButton")?.GetComponent<Button>();
        if(retry!=null)retry.interactable=EnergyManager.Instance.HasEnergy;
        RefreshEnergy();
    }
    private void CompleteLevel()
    {
        completed=true;
        int points=wrong==0?100:75;
        PlayerPrefs.SetInt("Stage5Level"+Level+"Completed",1);
        PlayerPrefs.SetInt("Stage5Level"+Level+"Score",points);
        if(Level<3)PlayerPrefs.SetInt("Stage5Level"+(Level+1)+"Unlocked",1);
        else PlayerPrefs.SetInt("Stage5Completed",1);
        PlayerPrefs.Save();
        if(scoreText!=null)scoreText.text="SCORE\n"+points.ToString("D6");
        if(finalScore!=null)finalScore.text="FINAL SCORE "+points.ToString("D6");
        if(completePanel!=null){completePanel.SetActive(true);completePanel.transform.SetAsLastSibling();}
    }
    private void UpdateEvidence()
    {
        if(evidenceText==null)return;
        string text="";
        for(int i=0;i<3;i++)
        {
            string value=Level==1?(examined[i]?thermal[i]:"NOT TESTED"):
                Level==2?(examined[i]?mechanical[i]:"NEEDS STRIKE + PRESS"):
                ("Thermal: "+thermal[i]+"\\nMechanical: "+mechanical[i]+"\\nProbe: "+
                 (probed[i,0]?"Solid tested":"Solid ?")+" / "+
                 (probed[i,1]?"Molten tested":"Molten ?")+
                 (i==1?(probed[i,2]?" / Aqueous tested":" / Aqueous ?"):""));
            text+=samples[i]+": "+value+"\n\n";
        }
        evidenceText.text=text;
        int count=0;
        for(int i=0;i<3;i++)if(examined[i])count++;
        if(progress!=null)progress.text=Level==3?"EXAMINE STATES & CLASSIFY SAMPLES":
            "SAMPLES TESTED  "+count+" / 3";
    }
    private void SetIndicator(Color color)
    {
        var indicator=Find(root,"TestIndicator")?.GetComponent<Image>();
        if(indicator!=null)indicator.color=color;
    }
    private void ReactionEffect(string message)
    {
        var effect=FindFirstObjectByType<ChemQuestReactionEffects>();
        if(effect!=null)effect.ExperimentFeedback(message);
    }
    private void Info(string text){if(feedback!=null)feedback.text=text;}
    private void Pause()
    {
        if(!CanInteract)return;paused=true;Time.timeScale=0;
        if(pausePanel!=null){pausePanel.SetActive(true);pausePanel.transform.SetAsLastSibling();}
    }
    private void Resume()
    {
        paused=false;Time.timeScale=1;
        if(pausePanel!=null)pausePanel.SetActive(false);
    }
    private void Go(string name)
    {
        Time.timeScale=1;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {Debug.LogError("Stage 5 missing scene in Build Settings: "+name);return;}
        SceneManager.LoadScene(name);
    }
    private void Update()
    {
        if(Time.unscaledTime<nextEnergyUpdate)return;
        nextEnergyUpdate=Time.unscaledTime+.4f;
        RefreshEnergy();
    }
    private void RefreshEnergy()
    {
        var manager=EnergyManager.Instance;if(manager==null)return;
        manager.RefreshEnergy();
        for(int i=0;i<5;i++)
        {
            var icon=Find(root,"Energy"+(i+1))?.GetComponent<Image>();
            if(icon!=null)icon.enabled=i<manager.CurrentEnergy;
        }
        if(energyText!=null)energyText.text=manager.CurrentEnergy+" / 5";
        if(emptyPanel!=null)
        {
            if(!manager.HasEnergy)
            {
                emptyPanel.SetActive(true);emptyPanel.transform.SetAsLastSibling();
                int seconds=manager.SecondsUntilNextEnergy();
                if(timer!=null)timer.text="NEXT ENERGY "+(seconds/60).ToString("00")+":"+
                    (seconds%60).ToString("00");
            }
            else if(emptyPanel.activeSelf)emptyPanel.SetActive(false);
        }
    }
    private void OnDestroy(){Time.timeScale=1;}
}
