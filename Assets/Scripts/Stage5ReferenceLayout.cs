using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Artwork-first UI for Stage 5. Loads the EXACT three approved reference
// screens from Resources while retaining live Unity drag and click targets.
// A screenshot is visual only; the existing Button, Slider, Drop and scoring
// components remain the sole source of gameplay behavior.
public sealed class Stage5ReferenceLayout : MonoBehaviour
{
    private int level, selected=-1, phase;
    private Transform sceneRoot;
    private RectTransform canvas, station;
    private Image preview;
    private TMP_Text liveSample, liveNotebook, liveStatus;
    private Stage5VisualLabDemo previous;
    private bool ready;
    private int actionEffect;
    private float effectTime;
    private Vector2 previewHome;
    private Image dynamicHeatKnob;
    private Image dynamicHeatFill;
    private Image heatingSample;
    private Image thermometerFill;
    private TMP_Text thermometerValue;
    private TMP_Text dynamicPlateHeat;
    private TMP_Text dynamicHeatReadout;
    private TMP_Text dynamicProgress;
    private TMP_Text dynamicScore;
    private TMP_Text dynamicEnergy;
    private Image conductivityBulbMask;
    private Image conductivityResultPlate;
    private TMP_Text conductivityResultText;
    private Stage5ReferenceTouchController touchController;

    private static readonly Color Navy=new Color32(5,29,57,250);
    private static readonly Color Gold=new Color32(255,207,109,255);
    private static readonly Color Cyan=new Color32(76,216,247,255);

    public bool Initialize(int currentLevel,Transform gameRoot,Stage5VisualLabDemo previousDemo)
    {
        level=currentLevel;
        sceneRoot=gameRoot;
        previous=previousDemo;
        canvas=gameRoot as RectTransform;
        if(canvas==null)return false;

        var imageSprite=ResourceSprite("Level"+level);
        if(imageSprite==null)
        {
            Debug.LogWarning("CHEMQUEST Stage5: Reference artwork not installed in Assets/Resources/ChemQuestStage5Reference; original UI remains active.");
            return false;
        }

        var board=Find(sceneRoot,"LabBoard");
        if(board==null)return false;
        // Backdrop is behind ALL functional UI objects and does not intercept touches.
        var skin=NewImage(canvas,"ExactStage5ReferenceBackdrop",imageSprite,Color.white);
        var bgRect=skin.rectTransform;
        bgRect.anchorMin=Vector2.zero;bgRect.anchorMax=Vector2.one;
        bgRect.offsetMin=Vector2.zero;bgRect.offsetMax=Vector2.zero;
        // The complete reference image must sit ABOVE all stale Unity panels.
        // Previously the serialized lab panels covered its buttons with large
        // dark rectangles. Input now belongs solely to the transparent touch
        // controller added after the art.
        skin.transform.SetAsLastSibling();

        // Hide the old procedural text boxes / panels, not their controls.
        foreach(var name in new[]{"LabBoard","SampleBank","TestStation","EvidenceBoard"})
            Transparent(Find(sceneRoot,name),true);
        foreach(var name in new[]{"BoardTitle","Stage5ReferencePrompt","Stage5Level1Prompt",
            "BankLabel","EvidenceTitle","SelectedSample","ExperimentResult",
            "StationLabel","TestIndicator","Progress","Feedback","HeatLabel","HeatValueText"})
        {
            var t=Find(sceneRoot,name);
            if(t==null)continue;
            var text=t.GetComponent<TMP_Text>();
            if(text!=null)text.enabled=false;
            var im=t.GetComponent<Image>();
            if(im!=null)im.color=Color.clear;
        }

        // Place the functional drag and drop regions exactly over the drawn
        // sample cards, the central workbench and the illustrated controls.
        var sampleBank=Find(sceneRoot,"SampleBank");
        Rect(sampleBank,new Vector2(-404,0),new Vector2(265,314));
        for(int i=0;i<3;i++)
        {
            var sample=Find(sampleBank,"Sample"+i);
            Rect(sample,new Vector2(0,64-i*79),new Vector2(234,65));
            var draggable=sample?.GetComponent<Stage5SampleDrag>();
            if(draggable!=null)draggable.RefreshHome();
            Transparent(sample,true);
            var label=sample?.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)label.enabled=false;
        }

        station=Find(sceneRoot,"TestStation") as RectTransform;
        Rect(station,new Vector2(-12,2),new Vector2(550,330));

        // Hide the old placeholder simulation. The detailed reference artwork
        // is the visible instrument; our close-up is the actual test result.
        if(previous!=null) previous.HideForReferenceArt();

        var notebook=Find(sceneRoot,"EvidenceBoard");
        Rect(notebook,new Vector2(400,6),new Vector2(282,325));
        // The generated mockups have incorrect pre-filled results.
        // Mask ONLY the notebook entries and replace them with real observations.
        // Fully opaque notebook mask: the approved reference is a *sample
        // screenshot* with example answers baked into its pixels. Leaving
        // those visible underneath the live answers caused the overlaps.
        var ink=NewImage(canvas,"AccurateNotebookPanel",null,new Color32(5,29,57,255));
        Rect(ink.transform,new Vector2(399,-2),new Vector2(284,315));
        ink.raycastTarget=false;
        liveNotebook=NewText(ink.transform,"ActualNotebook","A: Not tested\n\nB: Not tested\n\nC: Not tested",
            15,new Vector2(0,-12),new Vector2(262,237),Color.white);
        var title=NewText(ink.transform,"ActualNotebookTitle","LAB NOTEBOOK",22,
            new Vector2(0,124),new Vector2(264,35),Gold);
        title.fontStyle=FontStyles.Bold;
        liveNotebook.alignment=TextAlignmentOptions.TopLeft;
        liveNotebook.margin=new Vector4(9,12,9,4);
        liveNotebook.enableAutoSizing=true;
        liveNotebook.fontSizeMin=12;
        liveNotebook.fontSizeMax=15;
        liveNotebook.overflowMode=TextOverflowModes.Ellipsis;
        liveNotebook.lineSpacing=3f;

        // Put a transparent drop target on the real picture, not an artificial box.
        if(station!=null)
        {
            var activeStation=station.GetComponent<Image>();
            if(activeStation!=null){activeStation.color=new Color(1,1,1,0f);activeStation.raycastTarget=true;}
            // Close-up action visual: heater, striking hammer, hydraulic press,
            // or wired conductivity apparatus, whichever was actually tested.
            preview=NewImage(canvas,"ActiveExperimentCloseup",null,Color.white);
            Rect(preview.transform,new Vector2(-11,-3),new Vector2(402,236));
            preview.preserveAspect=true;
            preview.raycastTarget=false;
            previewHome=preview.rectTransform.anchoredPosition;
            preview.gameObject.SetActive(false);
            // The sample label is baked into the reference picture as
            // "Sample B/C". Cover it completely and redraw the selected one.
            var tag=NewImage(canvas,"ActiveSampleTag",null,new Color32(5,29,57,255));
            Rect(tag.transform,new Vector2(-11,132),new Vector2(224,37));
            liveSample=NewText(tag.transform,"ActiveSampleName","CHOOSE A SAMPLE",17,
                Vector2.zero,new Vector2(214,31),Gold);
            liveSample.fontStyle=FontStyles.Bold;
            // The earlier status plate covered the illustrated
            // CRACKS/FLATTENS labels and Level3 phase options. Keep its
            // information in memory for feedback but hide it visually:
            // the notebook contains the authoritative recorded result.
            liveStatus=NewText(canvas,"RealExperimentStatus","",15,
                new Vector2(-11,-136),new Vector2(400,27),Cyan);
            liveStatus.enabled=false;
        }

        foreach(var name in new[]{"TestButton","StrikeButton","PressButton","ProbeButton",
            "SolidButton","MoltenButton","AqueousButton","SubmitButton"})
        {
            var control=Find(sceneRoot,name);
            Transparent(control,true);
            // The artwork already contains the caption; avoid a second
            // offset copy of the same words while preserving Button.onClick.
            var caption=control?.GetComponentInChildren<TMP_Text>(true);
            if(caption!=null)caption.enabled=false;
        }

        // The pause/menu button and score/energy are real interactive/readout
        // values, placed over the exact corresponding artwork coordinates.
        var menu=Find(sceneRoot,"MenuButton");
        Rect(menu,new Vector2(-596,314),new Vector2(67,65));
        Transparent(menu,true);
        var menuText=menu?.GetComponentInChildren<TMP_Text>(true);
        if(menuText!=null)menuText.enabled=false;
        var score=Find(sceneRoot,"Score")?.GetComponent<TMP_Text>();
        // These are drawn in the new live HUD. Disable old labels rather
        // than moving them on top of the text already present in the art.
        if(score!=null)score.enabled=false;
        var energyCount=Find(sceneRoot,"EnergyCount")?.GetComponent<TMP_Text>();
        if(energyCount!=null)energyCount.enabled=false;
        var energyLabel=Find(sceneRoot,"EnergyLabel")?.GetComponent<TMP_Text>();
        if(energyLabel!=null)energyLabel.enabled=false;
        for(int i=1;i<=5;i++)
        {
            var energyImg=Find(sceneRoot,"Energy"+i)?.GetComponent<Image>();
            if(energyImg!=null)energyImg.enabled=false;
        }
        // Live readouts are redrawn above the static reference art.
        // Opaque patches over the numbers baked into Level1/2/3 reference
        // pictures. There is now only ONE score and ONE energy counter.
        var scoreMask=NewImage(canvas,"ReferenceScoreMask",null,new Color32(7,29,57,255));
        Rect(scoreMask.transform,new Vector2(557,310),new Vector2(114,29));
        dynamicScore=NewText(scoreMask.transform,"ReferenceLiveScore","000000",18,
            Vector2.zero,new Vector2(113,26),Gold);
        dynamicScore.fontStyle=FontStyles.Bold;

        var energyMask=NewImage(canvas,"ReferenceEnergyMask",null,new Color32(7,29,57,255));
        Rect(energyMask.transform,new Vector2(-308,313),new Vector2(77,28));
        dynamicEnergy=NewText(energyMask.transform,"ReferenceLiveEnergy","5 / 5",18,
            Vector2.zero,new Vector2(75,26),Color.white);
        dynamicEnergy.fontStyle=FontStyles.Bold;
        if(level<=2)
        {
            var progressPanel=NewImage(canvas,"ReferenceProgressPlate",null,new Color32(5,29,57,255));
            Rect(progressPanel.transform,new Vector2(0,-303),new Vector2(310,36));
            dynamicProgress=NewText(progressPanel.transform,"ReferenceProgress","SAMPLES TESTED 0 / 3",
                16,Vector2.zero,new Vector2(302,30),Cyan);
        }
        if(level==1)
        {
            BuildEmptyHeatingBench();
            // Entire slider art is rebuilt on top of the STATIC screenshot:
            // the reference thumb was baked at 175 C and caused two knobs.
            // This overlay is a display only; the mapped touch controller
            // still owns the real slider interaction.
            var sliderMask=NewImage(canvas,"CleanSliderBackground",null,new Color32(5,29,57,255));
            Rect(sliderMask.transform,new Vector2(-9,-221),new Vector2(602,48));
            var heatTrack=NewImage(canvas,"CleanHeatTrack",null,new Color32(34,91,133,255));
            Rect(heatTrack.transform,new Vector2(-9,-221),new Vector2(580,18));
            var fill=NewImage(canvas,"CleanHeatFill",null,new Color32(235,139,36,255));
            dynamicHeatFill=fill;
            Rect(fill.transform,new Vector2(-289,-221),new Vector2(0,12));
            var knob=NewImage(canvas,"ReferenceHeatKnob",null,Gold);
            dynamicHeatKnob=knob;
            Rect(knob.transform,new Vector2(-294,-221),new Vector2(21,28));

            // Mask both the pre-painted value and old live readout before
            // showing one unambiguous percentage reading.
            var heatPlate=NewImage(canvas,"ReferenceHeatReadoutPlate",null,new Color32(5,29,57,255));
            Rect(heatPlate.transform,new Vector2(414,-222),new Vector2(244,39));
            dynamicHeatReadout=NewText(heatPlate.transform,"ReferenceHeatReadout",
                "HEAT LEVEL: 0%",16,Vector2.zero,new Vector2(233,32),Gold);

            // The 175 C on the hotplate is already replaced by the single
            // HotplateActualHeatMask created in BuildEmptyHeatingBench().
        }
        if(level==3)
        {
            // The illustration shows a lit bulb as an example. Cover the
            // baked-in ON state until the player's real probe result is known.
            conductivityBulbMask=NewImage(canvas,"DynamicLampMask",null,Navy);
            Rect(conductivityBulbMask.transform,new Vector2(-13,107),new Vector2(82,80));
            conductivityResultPlate=NewImage(canvas,"DynamicProbeResult",null,Navy);
            Rect(conductivityResultPlate.transform,new Vector2(219,91),new Vector2(151,70));
            conductivityResultText=NewText(conductivityResultPlate.transform,
                "DynamicProbeResultLabel","NOT TESTED",15,Vector2.zero,
                new Vector2(146,60),Gold);
            conductivityResultText.fontStyle=FontStyles.Bold;
            // The mockup omits the game's final validation control.
            // Draw a visible one above the art, mapped to action #4.
            var classify=NewImage(canvas,"ReferenceCheckClassification",null,
                new Color32(7,135,148,250));
            Rect(classify.transform,new Vector2(412,-223),new Vector2(236,48));
            var checkCaption=NewText(classify.transform,"CheckCaption",
                "CHECK CLASSIFICATION",15,Vector2.zero,new Vector2(226,42),Color.white);
            checkCaption.fontStyle=FontStyles.Bold;
        }
        // One mapped raycast surface handles both mouse and Android touch.
        // It sits above all artwork and legacy transparent controls.
        var hit=NewImage(canvas,"Stage5MappedTouchSurface",null,Color.clear);
        hit.raycastTarget=true;
        touchController=hit.gameObject.AddComponent<Stage5ReferenceTouchController>();
        touchController.Setup(GetComponent<Stage5ElementCrucibleGame>(),this,level);
        ready=true;
        Debug.Log("CHEMQUEST: Exact Stage 5 reference artwork and functional hit targets loaded for Level "+level);
        return true;
    }

    private void PlaceLevel1Controls()
    {
        var slider=Find(sceneRoot,"HeatSlider");
        Rect(slider,new Vector2(-42,-220),new Vector2(565,48));
        if(slider!=null)
        {
            var hit=slider.GetComponent<Image>();
            if(hit==null)hit=slider.gameObject.AddComponent<Image>();
            hit.color=Color.clear;hit.raycastTarget=true;
            foreach(var item in slider.GetComponentsInChildren<Image>(true))
                if(item!=hit)item.color=Color.clear;
        }
        Rect(Find(sceneRoot,"TestButton"),new Vector2(-360,-290),new Vector2(345,60));
        Rect(Find(sceneRoot,"SubmitButton"),new Vector2(349,-290),new Vector2(345,60));
        Rect(Find(sceneRoot,"Progress"),new Vector2(0,-290),new Vector2(285,50));
    }

    private void PlaceLevel2Controls()
    {
        Rect(Find(sceneRoot,"StrikeButton"),new Vector2(-350,-290),new Vector2(340,65));
        Rect(Find(sceneRoot,"PressButton"),new Vector2(0,-290),new Vector2(333,65));
        Rect(Find(sceneRoot,"SubmitButton"),new Vector2(350,-290),new Vector2(340,65));
    }

    private void PlaceLevel3Controls()
    {
        Rect(Find(sceneRoot,"SolidButton"),new Vector2(-150,-166),new Vector2(170,68));
        Rect(Find(sceneRoot,"MoltenButton"),new Vector2(24,-166),new Vector2(170,68));
        Rect(Find(sceneRoot,"AqueousButton"),new Vector2(196,-166),new Vector2(170,68));
        Rect(Find(sceneRoot,"ProbeButton"),new Vector2(-420,-302),new Vector2(230,60));
        for(int i=0;i<3;i++)
        {
            var bin=Find(sceneRoot,"Bin"+i);
            Rect(bin,new Vector2(-135+i*254,-303),new Vector2(235,57));
            Transparent(bin,true);
            var label=bin?.GetComponentInChildren<TMP_Text>(true);
            if(label!=null)label.enabled=false;
        }
        var submit=Find(sceneRoot,"SubmitButton");
        Rect(submit,new Vector2(452,-234),new Vector2(207,45));
        // This important classification action is not on the illustrated reference.
        // Keep one explicit visible interactive control so the game is completable.
        var image=submit?.GetComponent<Image>();
        if(image!=null){image.color=new Color32(15,136,150,245);image.raycastTarget=true;}
        var text=submit?.GetComponentInChildren<TMP_Text>(true);
        if(text!=null){text.enabled=true;text.text="CHECK CLASSIFICATION";text.fontSize=14;}
    }


    // The original reference screenshot has a permanently melted cube and
    // 175 C already drawn into its pixels. Cover those objects and replace
    // them with a live empty heater, sample sprite, and 0-100% thermometer.
    private void BuildEmptyHeatingBench()
    {
        if(level!=1 || canvas==null)return;
        var cover=NewImage(canvas,"EmptyHeaterCover",null,new Color32(5,27,52,255));
        Rect(cover.transform,new Vector2(-24,65),new Vector2(298,142));
        var ring=NewImage(canvas,"EmptyHotPlateRim",null,new Color32(232,101,33,255));
        Rect(ring.transform,new Vector2(-24,-6),new Vector2(250,25));
        var surface=NewImage(canvas,"EmptyHotPlateSurface",null,new Color32(57,77,98,255));
        Rect(surface.transform,new Vector2(-24,5),new Vector2(242,33));
        heatingSample=NewImage(canvas,"ActualSampleOnHeater",null,Color.white);
        Rect(heatingSample.transform,new Vector2(-24,53),new Vector2(108,100));
        heatingSample.preserveAspect=true;
        heatingSample.gameObject.SetActive(false);

        var meter=NewImage(canvas,"HeatThermometerBlank",null,new Color32(7,26,51,255));
        Rect(meter.transform,new Vector2(198,44),new Vector2(88,216));
        var rim=NewImage(canvas,"HeatThermometerTrack",null,new Color32(47,129,176,255));
        Rect(rim.transform,new Vector2(194,36),new Vector2(15,149));
        var groove=NewImage(canvas,"HeatThermometerGroove",null,new Color32(10,20,39,255));
        Rect(groove.transform,new Vector2(194,36),new Vector2(9,141));
        thermometerFill=NewImage(canvas,"LiveHeatMercury",null,new Color32(255,131,61,255));
        Rect(thermometerFill.transform,new Vector2(194,-27),new Vector2(8,1));
        thermometerValue=NewText(canvas,"ThermometerPercent","0%",17,
            new Vector2(226,111),new Vector2(60,25),Gold);
        var thermTitle=NewText(canvas,"ThermometerTitle","HEAT",13,
            new Vector2(224,135),new Vector2(60,20),Gold);
        thermTitle.fontStyle=FontStyles.Bold;
        for(int i=0;i<=4;i++)
            NewText(canvas,"HeatTick"+i,(i*25).ToString(),11,
                new Vector2(226,-27+i*32),new Vector2(32,19),Color.white);
        // This LED mask covers the baked 175 C and is kept up to date.
        var led=NewImage(canvas,"HotplateActualHeatMask",null,new Color32(12,27,52,255));
        Rect(led.transform,new Vector2(-27,-69),new Vector2(101,36));
        dynamicPlateHeat=NewText(led.transform,"LiveHotplateHeat","0%",17,
            Vector2.zero,new Vector2(95,29),Gold);
    }

    private void ShowHeatingSample(int index)
    {
        if(heatingSample==null || index<0 || index>2)return;
        heatingSample.sprite=ResourceSprite("DragSample"+index);
        heatingSample.gameObject.SetActive(heatingSample.sprite!=null);
    }

    public void ReferenceHeatChanged(float value)
    {
        if(!ready||level!=1)return;
        float fraction=Mathf.Clamp01(value/100f);
        if(dynamicHeatKnob!=null)
        {
            var pos=dynamicHeatKnob.rectTransform.anchoredPosition;
            pos.x=Mathf.Lerp(-294f,276f,fraction);
            dynamicHeatKnob.rectTransform.anchoredPosition=pos;
        }
        if(dynamicHeatFill!=null)
        {
            float width=570f*fraction;
            Rect(dynamicHeatFill.transform,new Vector2(-294f+width*.5f,-221f),
                new Vector2(width,12f));
        }
        string reading=Mathf.RoundToInt(value)+"%";
        if(dynamicHeatReadout!=null)
            dynamicHeatReadout.text="HEAT LEVEL: "+reading;
        if(dynamicPlateHeat!=null) dynamicPlateHeat.text=reading;
        if(thermometerValue!=null) thermometerValue.text=reading;
        if(thermometerFill!=null)
        {
            float h=129f*fraction;
            Rect(thermometerFill.transform,new Vector2(194f,-27f+h*.5f),
                new Vector2(8f,Mathf.Max(1f,h)));
        }
    }

    public void ReferencePhaseSelected(int selectedPhase)
    {
        if(!ready||level!=3)return;
        phase=selectedPhase;
        if(liveStatus!=null)
            liveStatus.text=(phase==0?"SOLID":phase==1?"MOLTEN":"AQUEOUS")+" SELECTED";
    }

    public void ReferenceSetProgress(string text)
    {
        if(dynamicProgress!=null)dynamicProgress.text=text;
    }

    public void ReferenceSetIndicators(int score,int energy)
    {
        if(dynamicScore!=null)dynamicScore.text=score.ToString("D6");
        if(dynamicEnergy!=null)dynamicEnergy.text=energy+" / 5";
    }

    public void SampleChanged(int index)
    {
        if(!ready)return;
        selected=index;
        if(liveSample!=null)liveSample.text="SAMPLE "+(char)('A'+index);
        if(liveStatus!=null)liveStatus.text="SELECT THE TEST ACTION";
        if(preview!=null) preview.gameObject.SetActive(false);
        if(level==1)
        {
            ShowHeatingSample(index);
            if(liveStatus!=null)liveStatus.text="ADJUST HEAT, THEN PRESS APPLY HEAT";
        }
        if(level==3)SetConductivityVisual(false,false);
        actionEffect=0;
    }

    public void ThermalResult(int sample)
    {
        if(!ready)return;
        actionEffect=1;
        if(sample==0)
        {
            if(heatingSample!=null) heatingSample.gameObject.SetActive(false);
            SetResult("HeatMelt","SAMPLE A MELTS");
        }
        else
        {
            ShowHeatingSample(sample);
            SetResult(null,"SOLID AFTER HEATING");
        }
    }

    public void MechanicalResult(int sample,bool pressed)
    {
        if(!ready)return;
        var art=sample==1&&!pressed?"StrikeShatter":
            sample==2&&pressed?"PressFlatten":null;
        actionEffect=pressed?3:2;
        SetResult(art,sample==1&&!pressed?"HAMMER SMASHES: BRITTLE":
            sample==2&&pressed?"PRESS FLATTENS: MALLEABLE":
            "OBSERVATION RECORDED");
    }

    public void PhaseChanged(int state)
    {
        if(!ready)return;
        phase=state;
        if(preview!=null)preview.gameObject.SetActive(false);
        SetConductivityVisual(false,false);
        if(liveStatus!=null)liveStatus.text=
            (state==0?"SOLID":state==1?"MOLTEN":"AQUEOUS")+" - USE TEST PROBE";
    }

    public void ProbeResult(int sample,bool conducting)
    {
        if(!ready)return;
        // The illustration with mobile ions depicts IONIC sample B only.
        actionEffect=4;
        SetConductivityVisual(true,conducting);
        SetResult(sample==1&&conducting?"ConductivityOn":null,
            conducting?"BULB ON - CONDUCTS":"BULB OFF - DOES NOT CONDUCT");
    }

    private void SetConductivityVisual(bool tested,bool conducting)
    {
        if(level!=3)return;
        if(conductivityBulbMask!=null)
            conductivityBulbMask.color=conducting&&tested?
                new Color(0,0,0,0):Navy;
        if(conductivityResultPlate!=null)
            conductivityResultPlate.color=conducting&&tested?
                new Color32(10,94,54,245):Navy;
        if(conductivityResultText!=null)
        {
            conductivityResultText.text=!tested?"NOT TESTED":
                conducting?"CONDUCTS":"NO CONDUCTION";
            conductivityResultText.color=conducting&&tested?
                new Color32(167,255,190,255):Gold;
        }
    }

    public void SyncNotebook(string actual)
    {
        if(!ready||liveNotebook==null)return;
        // Only used as a safe placeholder until the game supplies concise
        // verified observations. Never render the original overlong log,
        // which overflowed the three notebook rows in Level 3.
        if(string.IsNullOrWhiteSpace(actual))
            SyncCompactNotebook("NOT TESTED","NOT TESTED","NOT TESTED");
    }

    public void SyncCompactNotebook(string a,string b,string c)
    {
        if(!ready||liveNotebook==null)return;
        liveNotebook.text=
            "<color=#7ECDEF><b>A</b></color>  "+a+"\n\n"+
            "<color=#D4A5FF><b>B</b></color>  "+b+"\n\n"+
            "<color=#7FF3DC><b>C</b></color>  "+c;
    }

    private void SetResult(string spriteName,string message)
    {
        if(liveStatus!=null)liveStatus.text=message;
        if(preview==null)return;
        var sprite=string.IsNullOrEmpty(spriteName)?null:ResourceSprite(spriteName);
        preview.gameObject.SetActive(sprite!=null);
        if(sprite!=null)
        {
            preview.sprite=sprite;
            effectTime=0f;
            preview.color=new Color(1f,1f,1f,0f);
            preview.rectTransform.localScale=Vector3.one*.88f;
        }
    }

    private void Update()
    {
        if(!ready || preview==null || !preview.gameObject.activeSelf)return;
        effectTime+=Time.deltaTime;
        float t=Mathf.Clamp01(effectTime/(actionEffect==1?1.75f:
            actionEffect==3?1.25f:.65f));
        float smooth=t*t*(3f-2f*t);
        var rt=preview.rectTransform;
        // Melt: gradually appear and spread. Strike: rapid downward impact,
        // rebound and settle. Press: slower mechanical compression.
        float startHeight=actionEffect==2?75f:actionEffect==3?50f:15f;
        float y=previewHome.y+startHeight*(1f-smooth);
        if(actionEffect==2 && effectTime<.55f)
        {
            float impact=Mathf.Exp(-effectTime*9f)*Mathf.Sin(effectTime*57f);
            rt.anchoredPosition=new Vector2(previewHome.x+impact*9f,y);
        }
        else rt.anchoredPosition=new Vector2(previewHome.x,y);
        rt.localScale=new Vector3(
            Mathf.Lerp(actionEffect==1?.84f:.92f,1f,smooth),
            Mathf.Lerp(actionEffect==3?1.10f:.90f,1f,smooth),1);
        preview.color=new Color(1f,1f,1f,smooth);
    }

    private static Sprite ResourceSprite(string name)
    {
        var key="ChemQuestStage5Reference/"+name;
        var sprite=Resources.Load<Sprite>(key);
        if(sprite!=null)return sprite;
        var texture=Resources.Load<Texture2D>(key);
        return texture==null?null:Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),
            new Vector2(.5f,.5f),100f);
    }

    private static TMP_Text NewText(Transform parent,string name,string text,int font,
        Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        Rect(go.transform,pos,size);
        var result=go.GetComponent<TextMeshProUGUI>();
        result.text=text;result.color=color;result.fontSize=font;
        result.alignment=TextAlignmentOptions.Center;
        result.textWrappingMode=TextWrappingModes.Normal;
        result.raycastTarget=false;
        return result;
    }

    private static Image NewImage(Transform parent,string name,Sprite sprite,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var image=go.GetComponent<Image>();
        image.sprite=sprite;image.color=color;image.raycastTarget=false;
        return image;
    }

    private static void Transparent(Transform t,bool preserveRaycasts)
    {
        if(t==null)return;
        var image=t.GetComponent<Image>();
        if(image!=null)
        {
            image.color=Color.clear;
            if(!preserveRaycasts)image.raycastTarget=false;
        }
        // Remove decorative glows from the now-transparent hit zones.
        foreach(var outline in t.GetComponents<Outline>())outline.enabled=false;
    }

    private static void Rect(Transform t,Vector2 position,Vector2 size)
    {
        var r=t as RectTransform;
        if(r==null)return;
        r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
        r.pivot=new Vector2(.5f,.5f);
        r.anchoredPosition=position;
        r.sizeDelta=size;
    }

    private static Transform Find(Transform p,string name)
    {
        if(p==null)return null;
        foreach(var t in p.GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
}
