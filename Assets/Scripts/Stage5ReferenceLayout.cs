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
    private Image mechanicalCube;
    private Image mechanicalTopMask, mechanicalFragmentsMask, mechanicalPressMask;
    private Image[] mechanicalShards;
    private Image[] pressCrystalShards;
    private Image mechanicalFlattened;
    private static Sprite copperPlateSprite;
    private Image copperStrikeDent;
    private Image copperFlattenedShine;
    private Image waxStrikeDent;
    // Assign valid imported PNGs in Inspector when available. Avoid
    // Resources.Load on the four WaxDent files: broken Unity VirtualArtifacts
    // cause native import assertions before the fallback can execute.
    [SerializeField] private Sprite[] waxDentFrameSprites = new Sprite[4];
    private Sprite[] waxDentFrames;
    private RectTransform animatedHammerPivot;
    private Image animatedHammerHead;
    private Image hammerImpactFlash;
    private RectTransform animatedPressHead;
    private RectTransform animatedPressShaft;
    private Vector2 pressHeadHome=new Vector2(152,5);
    private Vector2 pressShaftHome=new Vector2(152,42);
    private Image pressImpactGlow;
    private bool hammerHasStruck;
    private Vector2 strikeSampleStart=new Vector2(-145,-50);
    private Vector2 pressSampleStart=new Vector2(150,-50);
    private float pressTravel=40f;
    private int mechanicalMode; // 0=loaded, 1=strike, 2=press
    private float mechanicalTime;
    private int mechanicalSample=-1;
    private Image moltenPool;
    private Image moltenHighlight;
    private bool isMelting;
    private float meltingTime;
    private static readonly Color WarmLiquid = new Color32(255,183,119,243);
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
    private Stage5Level3ModularUI modularLevel3;

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

        // Editor-baked objects survive stopping Play Mode and can be
        // inspected/edited in the Hierarchy. Rebind their references instead
        // of layering another set of UI objects over the saved scene.
        if(Find(canvas,"ExactStage5ReferenceBackdrop")!=null &&
           Find(canvas,"Stage5MappedTouchSurface")!=null)
        {
            RestoreStage5Backdrop(imageSprite);
            RebindBakedHierarchy();
            return true;
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
        RestoreStage5Backdrop(imageSprite);

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
            // Keep the selected-sample caption above the heater masking
            // artwork instead of letting the new backdrop clip its letters.
            Rect(tag.transform,new Vector2(-11,119),new Vector2(194,26));
            liveSample=NewText(tag.transform,"ActiveSampleName","CHOOSE A SAMPLE",15,
                Vector2.zero,new Vector2(188,26),Gold);
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
            // The old screenshot has faint progress text above its plaque.
            // Cover the entire center strip, but keep outside the action buttons.
            Rect(progressPanel.transform,new Vector2(0,-292),new Vector2(320,62));
            dynamicProgress=NewText(progressPanel.transform,"ReferenceProgress","SAMPLES TESTED 0 / 3",
                16,new Vector2(0,-11),new Vector2(308,30),Cyan);
        }
        if(level==2)
            BuildMechanicalWorkbench();
        if(level==1)
        {
            BuildEmptyHeatingBench();
            // The heater's cover is generated later than the tag and used
            // to clip it. Re-order only the label above that cover.
            if(liveSample!=null && liveSample.transform.parent!=null)
                liveSample.transform.parent.SetAsLastSibling();
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
                "0%",17,Vector2.zero,new Vector2(233,32),Gold);
            dynamicHeatReadout.fontStyle=FontStyles.Bold;

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
        // The three real-material buttons are actual editable UI Images,
        // not part of the background. Touch control remains unchanged.
        if(level==1 || level==2)RefreshStage5MaterialButtons();
        // One mapped raycast surface handles both mouse and Android touch.
        // It sits above all artwork and legacy transparent controls.
        var hit=NewImage(canvas,"Stage5MappedTouchSurface",null,Color.clear);
        hit.raycastTarget=true;
        touchController=hit.gameObject.AddComponent<Stage5ReferenceTouchController>();
        touchController.Setup(GetComponent<Stage5ElementCrucibleGame>(),this,level);
        if(level==3)PolishLevel3Reference();
        ready=true;
        Debug.Log("CHEMQUEST: Exact Stage 5 reference artwork and functional hit targets loaded for Level "+level);
        return true;
    }

    private static readonly string[] Stage5MaterialNames={
        "PARAFFIN WAX","SODIUM CHLORIDE","COPPER"
    };

    // Levels 1 and 2 reuse separate editable material button sprites from
    // Level 3 while preserving the original A/B/C drag and hit targets.
    // Bake and Play reuse the saved RectTransforms, not fixed coordinates.
    private void RefreshStage5MaterialButtons()
    {
        if((level!=1 && level!=2) || canvas==null)return;
        string containerName="Stage5Level"+level+"MaterialButtons";
        var row=Find(canvas,containerName) as RectTransform;
        bool newRow=row==null;
        if(row==null)
        {
            var root=new GameObject(containerName,
                typeof(RectTransform));
            root.transform.SetParent(canvas,false);
            row=root.GetComponent<RectTransform>();
        }
        // Once saved by Bake, respect user changes to the entire container.
        if(newRow)Rect(row,Vector2.zero,new Vector2(1280,720));
        row.gameObject.SetActive(true);
        // Level 1's full reference screenshot has the ORIGINAL three button
        // borders baked into the pixels. Small per-button covers left their
        // lower / right edges visible. One continuous dark inner-bank cover
        // hides ALL three original buttons and halos without masking the
        // UNKNOWN SAMPLES header or the outer panel outline.
        var innerMaskTransform=Find(row,"OriginalSampleBankCover");
        bool newInnerMask=innerMaskTransform==null;
        var innerMask=newInnerMask?
            NewImage(row,"OriginalSampleBankCover",null,new Color32(5,29,57,255)):
            innerMaskTransform.GetComponent<Image>();
        if(innerMask!=null)
        {
            innerMask.sprite=null;
            innerMask.overrideSprite=null;
            innerMask.color=new Color32(5,29,57,255);
            innerMask.type=Image.Type.Simple;
            innerMask.raycastTarget=false;
            innerMask.gameObject.SetActive(true);
            if(newInnerMask)
                Rect(innerMask.rectTransform,new Vector2(-404,-16),
                    new Vector2(270,246));
            innerMask.transform.SetAsFirstSibling();
        }
        // Existing small masks remain in the saved editable Hierarchy, but
        // the broad cover is the authoritative eraser for prepainted buttons.
        for(int i=0;i<3;i++)
        {
            var coverName="OriginalSampleCover_"+(char)('A'+i);
            var coverTransform=Find(row,coverName);
            bool newCover=coverTransform==null;
            Image cover=newCover?
                NewImage(row,coverName,null,new Color32(5,29,57,255)):
                coverTransform.GetComponent<Image>();
            if(cover!=null)
            {
                cover.gameObject.SetActive(true);
                cover.sprite=null;
                cover.overrideSprite=null;
                cover.type=Image.Type.Simple;
                cover.color=new Color32(5,29,57,255);
                cover.raycastTarget=false;
                // Do not undo inspector RectTransform edits when entering
                // Play Mode. Defaults apply only on first construction.
                if(newCover)
                    Rect(cover.rectTransform,new Vector2(-404,64-i*79),
                        new Vector2(264,80));
                // The covers must render behind all final material buttons.
                cover.transform.SetAsFirstSibling();
            }
        }
        for(int i=0;i<3;i++)
        {
            var name="MaterialButton_"+(char)('A'+i);
            var objectTransform=Find(row,name);
            bool newButton=objectTransform==null;
            Image image;
            if(newButton)
                image=NewImage(row,name,null,Color.white);
            else
                image=objectTransform.GetComponent<Image>();
            if(image==null)continue;
            var sprite=Resources.Load<Sprite>(
                "ChemQuestStage5Modular/Sample"+(char)('A'+i));
            image.overrideSprite=null;
            if(sprite!=null)image.sprite=sprite;
            if(image.sprite==null)
            {
                Debug.LogWarning("Stage 5 Level "+level+": Missing Sample"+
                    (char)('A'+i)+" sprite in ChemQuestStage5Modular.");
                image.gameObject.SetActive(false);
                continue;
            }
            image.gameObject.SetActive(true);
            image.color=Color.white;
            image.type=Image.Type.Simple;
            image.preserveAspect=true;
            image.raycastTarget=false;
            // Level 1 artwork uses narrower cards than Level 3. Never
            // stretch the source sprite or cover neighbouring elements.
            // Keep exactly the size/position saved in the Unity scene.
            // Reapplying the defaults each Play caused the old artwork to
            // peek out after the student manually aligned these buttons.
            if(newButton)
                Rect(image.transform,new Vector2(-404,64-i*79),
                    new Vector2(232,64));
        }
        // These images cover the old reference-art SAMPLE A/B/C buttons,
        // but do not intercept mapped dragging/clicking.
        var touch=Find(canvas,"Stage5MappedTouchSurface");
        if(touch!=null)row.SetSiblingIndex(touch.GetSiblingIndex());
    }

    // Keep the Stage 5 Level 3 labels and status panels separate.
    // Called for both a fresh Bake and an already-baked Unity scene.
    private void PolishLevel3Reference()
    {
        if(level!=3 || canvas==null)return;

        // The screenshot includes a preprinted sample prompt slightly below
        // the live selected-sample tag. Cover both with one compact label.
        var tag=Find(canvas,"ActiveSampleTag") as RectTransform;
        if(tag!=null)
        {
            // Cover both the static painted caption and the stale prompt.
            // The background shows SAMPLE C above the dynamic SAMPLE A.
            Rect(tag,new Vector2(-11,149),new Vector2(204,53));
            var image=tag.GetComponent<Image>();
            if(image!=null)image.color=new Color32(5,29,57,255);
        }
        if(liveSample==null)
            liveSample=Find(canvas,"ActiveSampleName")?.GetComponent<TMP_Text>();
        if(liveSample!=null)
        {
            Rect(liveSample.transform,new Vector2(0,0),new Vector2(196,28));
            liveSample.fontSize=17;
            liveSample.alignment=TextAlignmentOptions.Center;
        }

        // Avoid a dark rectangle over the center of the conducting apparatus.
        if(conductivityBulbMask!=null)
        {
            Rect(conductivityBulbMask.transform,new Vector2(-100,62),
                new Vector2(42,38));
            conductivityBulbMask.raycastTarget=false;
        }
        if(conductivityResultPlate!=null)
        {
            Rect(conductivityResultPlate.transform,new Vector2(220,86),
                new Vector2(134,46));
            conductivityResultPlate.raycastTarget=false;
        }
        if(conductivityResultText!=null)
        {
            Rect(conductivityResultText.transform,Vector2.zero,new Vector2(126,39));
            conductivityResultText.fontSize=13;
            conductivityResultText.enableAutoSizing=true;
            conductivityResultText.fontSizeMin=11;
            conductivityResultText.fontSizeMax=13;
            conductivityResultText.alignment=TextAlignmentOptions.Center;
        }

        // Keep all three phase observations inside the narrow notebook.
        var ink=Find(canvas,"AccurateNotebookPanel") as RectTransform;
        if(ink!=null)
        {
            Rect(ink,new Vector2(400,-2),new Vector2(280,315));
            var inkImage=ink.GetComponent<Image>();
            if(inkImage!=null)inkImage.color=new Color32(5,29,57,255);
        }
        if(liveNotebook==null)
            liveNotebook=Find(canvas,"ActualNotebook")?.GetComponent<TMP_Text>();
        if(liveNotebook!=null)
        {
            Rect(liveNotebook.transform,new Vector2(0,-22),new Vector2(263,232));
            liveNotebook.enableAutoSizing=false;
            liveNotebook.fontSize=17;
            liveNotebook.lineSpacing=4;
            liveNotebook.margin=new Vector4(7,8,7,3);
            liveNotebook.alignment=TextAlignmentOptions.TopLeft;
            liveNotebook.overflowMode=TextOverflowModes.Ellipsis;
        }
        var notebookTitle=Find(canvas,"ActualNotebookTitle")?.GetComponent<TMP_Text>();
        if(notebookTitle!=null)
        {
            Rect(notebookTitle.transform,new Vector2(0,122),new Vector2(260,32));
            notebookTitle.fontSize=20;
        }
        // Real Image and TMP objects, serialized when the editor Bake menu runs.
        // The original scene backdrop remains untouched behind these visuals.
        modularLevel3=GetComponent<Stage5Level3ModularUI>();
        if(modularLevel3==null)
            modularLevel3=gameObject.AddComponent<Stage5Level3ModularUI>();
        modularLevel3.Build(canvas);
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
    // Level 2: mask the sample/result poses painted permanently into the
    // reference art. Leave the detailed hammer and press machines visible,
    // then show the sample and results only after the player's actions.
    private void BuildMechanicalWorkbench()
    {
        if(canvas==null)return;
        mechanicalTopMask=NewImage(canvas,"MechanicalEmptySampleMask",null,
            new Color32(5,29,57,255));
        Rect(mechanicalTopMask.transform,new Vector2(-12,75),new Vector2(112,73));
        mechanicalFragmentsMask=NewImage(canvas,"MechanicalShatteredResultMask",null,
            new Color32(5,29,57,255));
        Rect(mechanicalFragmentsMask.transform,new Vector2(-145,-52),new Vector2(118,26));
        // The cleaned Level2.png contains no prepainted test results.
        mechanicalFragmentsMask.enabled=false;
        mechanicalPressMask=NewImage(canvas,"MechanicalPressedResultMask",null,
            new Color32(5,29,57,255));
        Rect(mechanicalPressMask.transform,new Vector2(152,-52),new Vector2(118,26));
        mechanicalPressMask.enabled=false;
        // Cover the shattered/flattened examples already painted into the
        // reference PNG. Real results stay hidden until STRIKE/PRESS is used.
        // Redraw clean empty testing platforms above the masks.
        DecorateMechanicalWorkBench();


        mechanicalCube=NewImage(canvas,"ActualMechanicalSample",null,Color.white);
        Rect(mechanicalCube.transform,new Vector2(-12,67),new Vector2(66,60));
        mechanicalCube.preserveAspect=true;
        mechanicalCube.gameObject.SetActive(false);
        EnsureWaxDentAnimation();

        // Angular fractured crystal fragments, not rounded purple circles.
        mechanicalShards=new Image[9];
        var fragmentSprite=MakeCrystalShardSprite();
        for(int i=0;i<mechanicalShards.Length;i++)
        {
            mechanicalShards[i]=NewImage(canvas,"MechanicalFragment"+i,
                fragmentSprite,new Color32(207,187,255,255));
            Rect(mechanicalShards[i].transform,new Vector2(-145,-99),
                new Vector2(18+(i%3)*4,16+((i+1)%3)*4));
            mechanicalShards[i].gameObject.SetActive(false);
        }
        EnsurePressCrystalShards();
        mechanicalFlattened=NewImage(canvas,"MechanicalFlattenedMetal",
            MakeCopperPlateSprite(),Color.white);
        Rect(mechanicalFlattened.transform,new Vector2(152,-102),
            new Vector2(14,10));
        mechanicalFlattened.gameObject.SetActive(false);
        EnsureCopperEffects();
        BuildAnimatedHammer();
        BuildAnimatedPress();
    }

    // The four wax-only frames are independent UI image assets. The animated
    // hammer and platform remain in the original Level 2 hierarchy; only
    // the specimen image changes to avoid duplicate hammers/platforms.
    private void EnsureWaxDentAnimation()
    {
        if(level!=2 || canvas==null)return;
        if(waxDentFrames==null || waxDentFrames.Length!=4)
            waxDentFrames=new Sprite[4];
        // Inspector references are optional. If assets are absent/corrupt,
        // AnimateMechanicalResult keeps a visible scale deformation instead.
        for(int i=0;i<4;i++)
            waxDentFrames[i]=waxDentFrameSprites!=null &&
                i<waxDentFrameSprites.Length?waxDentFrameSprites[i]:null;

        var existing=Find(canvas,"WaxStrikeDentAnimation")?.GetComponent<Image>();
        if(existing==null)
        {
            existing=NewImage(canvas,"WaxStrikeDentAnimation",null,Color.white);
            Rect(existing.rectTransform,new Vector2(-145,-61),new Vector2(88,69));
        }
        waxStrikeDent=existing;
        waxStrikeDent.raycastTarget=false;
        waxStrikeDent.preserveAspect=true;
        waxStrikeDent.color=Color.white;
        waxStrikeDent.gameObject.SetActive(false);
    }

    // Reuse separate scene Images for Copper so both results have the
    // actual orange metallic hue rather than the pale generic sample art.
    private void EnsureCopperEffects()
    {
        if(level!=2 || canvas==null)return;
        var dent=Find(canvas,"CopperStrikeDent")?.GetComponent<Image>();
        bool newDent=dent==null;
        if(newDent)dent=NewImage(canvas,"CopperStrikeDent",MakeOvalSprite(),
            new Color32(113,47,28,245));
        copperStrikeDent=dent;
        if(newDent)Rect(dent.transform,new Vector2(-145,-76),new Vector2(29,9));
        dent.raycastTarget=false;
        dent.gameObject.SetActive(false);

        var shine=Find(canvas,"CopperFlattenedHighlight")?.GetComponent<Image>();
        bool newShine=shine==null;
        if(newShine)shine=NewImage(canvas,"CopperFlattenedHighlight",MakeOvalSprite(),
            new Color32(255,191,124,230));
        copperFlattenedShine=shine;
        if(newShine)Rect(shine.transform,new Vector2(150,-98),new Vector2(12,4));
        shine.raycastTarget=false;
        // The plated sprite contains its own bevel and specular reflection.
        // Keep the old highlight object disabled to avoid a flat white oval.
        shine.gameObject.SetActive(false);
        if(mechanicalFlattened!=null)
        {
            mechanicalFlattened.sprite=MakeCopperPlateSprite();
            mechanicalFlattened.overrideSprite=null;
            mechanicalFlattened.color=Color.white;
            mechanicalFlattened.type=Image.Type.Simple;
            mechanicalFlattened.preserveAspect=false;
            mechanicalFlattened.raycastTarget=false;
        }
    }

    // A beveled copper plate sprite: broad flat top, visible dark front
    // thickness and a metallic specular line. Self-generated to avoid missing
    // external PNGs and broken Sprite Editor slicing metadata.
    private static Sprite MakeCopperPlateSprite()
    {
        if(copperPlateSprite!=null)return copperPlateSprite;
        const int width=160,height=56;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
        texture.name="Stage5CopperPressedPlate";
        texture.filterMode=FilterMode.Bilinear;
        texture.wrapMode=TextureWrapMode.Clamp;
        Color transparent=Color.clear;
        Color topLight=new Color32(244,150,85,255);
        Color topDark=new Color32(190,72,36,255);
        Color side=new Color32(116,47,31,255);
        Color bottom=new Color32(89,38,30,255);
        for(int y=0;y<height;y++)
        for(int x=0;x<width;x++)
        {
            float u=(x+.5f)/width;
            float v=(y+.5f)/height;
            // Rounded bevel, with the lower band showing thickness.
            float edge=Mathf.Min(u,1f-u);
            bool inside=edge>.025f && v>.17f && v<.82f &&
                        !(edge<.065f && (v<.26f || v>.73f));
            Color pixel=transparent;
            if(inside)
            {
                if(v<.36f)
                    pixel=Color.Lerp(bottom,side,(v-.17f)/.19f);
                else if(v<.48f)
                    pixel=Color.Lerp(side,topDark,(v-.36f)/.12f);
                else
                {
                    pixel=Color.Lerp(topDark,topLight,
                        Mathf.Clamp01((v-.48f)/.30f));
                    // Controlled warm metallic streak across upper face.
                    float glint=Mathf.Exp(-Mathf.Pow((u-.37f)*5f,2f));
                    if(v>.67f && v<.74f)
                        pixel=Color.Lerp(pixel,new Color32(255,214,152,255),.48f*glint);
                }
                if(edge<.045f)
                    pixel=Color.Lerp(pixel,side,.35f);
            }
            texture.SetPixel(x,y,pixel);
        }
        texture.Apply(false,false);
        copperPlateSprite=Sprite.Create(texture,new Rect(0,0,width,height),
            new Vector2(.5f,.5f),100f);
        return copperPlateSprite;
    }

    // An independent group for brittle crystals crushed by the hydraulic
    // press. Reuse objects on Play/Bake so they remain editable in Hierarchy.
    private void EnsurePressCrystalShards()
    {
        if(level!=2 || canvas==null)return;
        if(pressCrystalShards==null || pressCrystalShards.Length!=9)
            pressCrystalShards=new Image[9];
        var sprite=MakeCrystalShardSprite();
        for(int i=0;i<pressCrystalShards.Length;i++)
        {
            var name="PressCrystalFragment"+i;
            var item=Find(canvas,name)?.GetComponent<Image>();
            bool created=item==null;
            if(created)
                item=NewImage(canvas,name,sprite,new Color32(229,219,255,255));
            item.sprite=sprite;
            item.overrideSprite=null;
            item.color=new Color32(229,219,255,255);
            item.preserveAspect=true;
            item.raycastTarget=false;
            if(created)
                Rect(item.rectTransform,new Vector2(150,-85),
                    new Vector2(16+(i%3)*4,15+((i+1)%3)*4));
            item.gameObject.SetActive(false);
            pressCrystalShards[i]=item;
        }
    }

    private void DecorateMechanicalWorkBench()
    {
        if(canvas==null)return;
        // The revised Level2.png already contains both lowered platforms.
        // Disable the old floating overlays, including in previously baked scenes.
        foreach(string name in new[]{"StrikeBenchSurface","PressBenchSurface",
            "StrikeBenchGlow","PressBenchGlow"})
        {
            var old=Find(canvas,name)?.GetComponent<Image>();
            if(old!=null)old.enabled=false;
        }
    }

    // Generate a faceted, transparent polygon that resembles fractured salt.
    // Works without extra PNG files and remains a separate editable Image.
    private static Sprite MakeCrystalShardSprite()
    {
        const int size=64;
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
        texture.filterMode=FilterMode.Bilinear;
        var clear=new Color(0,0,0,0);
        var border=new Color32(122,99,177,255);
        var facet=new Color32(229,221,255,255);
        var shadow=new Color32(159,139,215,255);
        for(int y=0;y<size;y++)
        for(int x=0;x<size;x++)
        {
            float u=x/(float)(size-1), v=y/(float)(size-1);
            // A chipped six-sided crystal with a slanted top face.
            bool inside=u>.08f && u<.91f && v>.09f && v<.91f &&
                (u+v)>.32f && (u+v)<1.68f &&
                (u-v)<.58f && (v-u)<.64f;
            if(!inside){texture.SetPixel(x,y,clear);continue;}
            bool rim=u<.13f || u>.85f || v<.15f || v>.85f ||
                (u+v)<.39f || (u+v)>1.61f;
            var color=rim?border:(u+v>.97f?shadow:facet);
            texture.SetPixel(x,y,color);
        }
        texture.Apply();
        return Sprite.Create(texture,new Rect(0,0,size,size),
            new Vector2(.5f,.5f),100f);
    }

    // A real articulated hammer, saved as regular Unity UI objects by the
    // Stage 5 Bake command. The original screenshot contains a decorative
    // static hammer; this animated foreground hammer visibly performs the
    // strike rather than showing only a text change.
    private void BuildAnimatedHammer()
    {
        if(level!=2||canvas==null)return;
        var existing=Find(canvas,"AnimatedStrikeHammerPivot") as RectTransform;
        if(existing!=null)
        {
            animatedHammerPivot=existing;
            animatedHammerHead=Find(canvas,"AnimatedStrikeHammerHead")?.GetComponent<Image>();
            hammerImpactFlash=Find(canvas,"HammerImpactFlash")?.GetComponent<Image>();
            ResetHammer();
            return;
        }

        var pivotObject=new GameObject("AnimatedStrikeHammerPivot",
            typeof(RectTransform));
        pivotObject.transform.SetParent(canvas,false);
        animatedHammerPivot=pivotObject.GetComponent<RectTransform>();
        Rect(animatedHammerPivot,new Vector2(-238,-25),new Vector2(1,1));

        // Hammer arm extends from the fixed hinge; the tip travels in an
        // arc when the pivot rotates (instead of sliding a flat illustration).
        var arm=NewImage(animatedHammerPivot,"AnimatedHammerHandle",null,
            new Color32(110,143,170,255));
        Rect(arm.transform,new Vector2(46,0),new Vector2(99,14));
        var grip=NewImage(animatedHammerPivot,"AnimatedHammerGrip",null,
            new Color32(49,73,98,255));
        Rect(grip.transform,new Vector2(24,0),new Vector2(39,8));
        var steel=NewImage(animatedHammerPivot,"AnimatedStrikeHammerHead",null,
            new Color32(197,221,241,255));
        animatedHammerHead=steel;
        Rect(steel.transform,new Vector2(101,0),new Vector2(33,59));
        var face=NewImage(animatedHammerPivot,"HammerStrikeFace",null,
            new Color32(91,123,158,255));
        Rect(face.transform,new Vector2(119,0),new Vector2(11,56));
        var band=NewImage(animatedHammerPivot,"HammerSteelHighlight",null,
            new Color32(240,249,255,255));
        Rect(band.transform,new Vector2(93,0),new Vector2(5,51));
        var hinge=NewImage(canvas,"AnimatedHammerHinge",MakeOvalSprite(),
            new Color32(217,179,109,255));
        Rect(hinge.transform,new Vector2(-238,-25),new Vector2(24,24));

        hammerImpactFlash=NewImage(canvas,"HammerImpactFlash",MakeOvalSprite(),
            new Color32(255,227,126,0));
        Rect(hammerImpactFlash.transform,new Vector2(-145,-94),new Vector2(78,30));
        hammerImpactFlash.gameObject.SetActive(false);
        ResetHammer();
    }

    private void ResetHammer()
    {
        hammerHasStruck=false;
        if(animatedHammerPivot!=null)
        {
            animatedHammerPivot.gameObject.SetActive(level==2);
            animatedHammerPivot.localRotation=Quaternion.Euler(0,0,53f);
        }
        if(hammerImpactFlash!=null)hammerImpactFlash.gameObject.SetActive(false);
    }

    private void AnimateHammer(float progress,float impactPoint)
    {
        if(animatedHammerPivot==null)return;
        float angle;
        if(progress<.20f)
        {
            // Lift farther back before the blow.
            float u=Mathf.Clamp01(progress/.20f);
            angle=Mathf.Lerp(53f,78f,u*u*(3f-2f*u));
        }
        else if(progress<impactPoint)
        {
            // A fast downward swing with a noticeable final acceleration.
            float u=Mathf.Clamp01((progress-.20f)/(impactPoint-.20f));
            angle=Mathf.Lerp(78f,-39f,u*u);
        }
        else if(progress<.82f)
        {
            // Rebound up after contact.
            float u=Mathf.Clamp01((progress-impactPoint)/(.82f-impactPoint));
            angle=Mathf.Lerp(-39f,28f,1f-(1f-u)*(1f-u));
        }
        else
        {
            float u=Mathf.Clamp01((progress-.82f)/.18f);
            angle=Mathf.Lerp(28f,53f,u);
        }
        // The hammer pivot is already aligned to the strike platform in
        // the saved scene. Do not add a target-angle offset: that rotation
        // can swing the head away from the sample.
        animatedHammerPivot.localRotation=Quaternion.Euler(0,0,angle);
        bool hit=progress>=impactPoint && progress<impactPoint+.13f;
        if(hammerImpactFlash!=null)
        {
            hammerImpactFlash.gameObject.SetActive(hit);
            if(hit)
            {
                float fade=1f-Mathf.Clamp01((progress-impactPoint)/.13f);
                hammerImpactFlash.color=new Color(1f,.92f,.51f,.85f*fade);
            }
        }
        if(progress>=impactPoint && !hammerHasStruck)
        {
            hammerHasStruck=true;
            Debug.Log("CHEMQUEST Stage5 Level2: Hammer contacted the sample.");
        }
    }

    // Compact press overlay: the reference art displays the idle machine.
    // Re-baking upgrades existing parts rather than making duplicates.
    private void BuildAnimatedPress()
    {
        if(level!=2||canvas==null)return;
        var shaft=Find(canvas,"AnimatedPressShaft")?.GetComponent<Image>();
        bool newShaft=shaft==null;
        if(newShaft)shaft=NewImage(canvas,"AnimatedPressShaft",null,new Color32(145,183,213,255));
        animatedPressShaft=shaft.rectTransform;
        if(newShaft)Rect(animatedPressShaft,new Vector2(152,42),new Vector2(16,47));
        var shine=Find(animatedPressShaft,"PressShaftHighlight")?.GetComponent<Image>();
        bool newShine=shine==null;
        if(newShine)shine=NewImage(animatedPressShaft,"PressShaftHighlight",null,new Color32(227,241,253,255));
        if(newShine)Rect(shine.transform,new Vector2(-3,0),new Vector2(4,43));

        var head=Find(canvas,"AnimatedPressHead")?.GetComponent<Image>();
        bool newHead=head==null;
        if(newHead)head=NewImage(canvas,"AnimatedPressHead",null,new Color32(103,159,204,255));
        animatedPressHead=head.rectTransform;
        if(newHead)Rect(animatedPressHead,new Vector2(152,5),new Vector2(99,17));
        var face=Find(animatedPressHead,"AnimatedPressContactPlate")?.GetComponent<Image>();
        bool newFace=face==null;
        if(newFace)face=NewImage(animatedPressHead,"AnimatedPressContactPlate",null,new Color32(217,232,245,255));
        if(newFace)Rect(face.transform,new Vector2(0,-10),new Vector2(103,6));
        var rim=Find(animatedPressHead,"AnimatedPressHeadRim")?.GetComponent<Image>();
        bool newRim=rim==null;
        if(newRim)rim=NewImage(animatedPressHead,"AnimatedPressHeadRim",null,new Color32(249,179,89,255));
        if(newRim)Rect(rim.transform,new Vector2(0,9),new Vector2(96,4));

        pressImpactGlow=Find(canvas,"PressImpactGlow")?.GetComponent<Image>();
        bool newGlow=pressImpactGlow==null;
        if(newGlow)
            pressImpactGlow=NewImage(canvas,"PressImpactGlow",MakeOvalSprite(),new Color32(255,203,107,0));
        if(newGlow)Rect(pressImpactGlow.transform,new Vector2(152,-87),new Vector2(100,18));
        pressHeadHome=animatedPressHead.anchoredPosition;
        pressShaftHome=animatedPressShaft.anchoredPosition;
        ResetPress();
    }

    private void ResetPress()
    {
        if(animatedPressHead!=null)
        {
            animatedPressHead.anchoredPosition=pressHeadHome;
            animatedPressHead.gameObject.SetActive(true);
        }
        if(animatedPressShaft!=null)
        {
            animatedPressShaft.anchoredPosition=pressShaftHome;
            animatedPressShaft.gameObject.SetActive(true);
        }
        if(pressImpactGlow!=null)pressImpactGlow.gameObject.SetActive(false);
    }

    private void AnimatePress(float progress,float impactPoint)
    {
        if(animatedPressHead==null||animatedPressShaft==null)return;
        if(!animatedPressHead.gameObject.activeSelf) animatedPressHead.gameObject.SetActive(true);
        if(!animatedPressShaft.gameObject.activeSelf) animatedPressShaft.gameObject.SetActive(true);
        // Longer, clearly readable press sequence: lift, slow descent,
        // hold pressure while the specimen reacts, then retract.
        float offset;
        if(progress<.16f)
        {
            float u=Mathf.Clamp01(progress/.16f);
            offset=-22f*u*u*(3f-2f*u);
        }
        else if(progress<impactPoint)
        {
            float u=Mathf.Clamp01((progress-.16f)/(impactPoint-.16f));
            u=u*u*(3f-2f*u);
            offset=Mathf.Lerp(-22f,pressTravel,u);
        }
        else if(progress<.82f) offset=pressTravel;
        else
        {
            float u=Mathf.Clamp01((progress-.82f)/.18f);
            offset=Mathf.Lerp(pressTravel,0f,u*u*(3f-2f*u));
        }
        animatedPressHead.anchoredPosition=pressHeadHome+
            Vector2.down*offset;
        animatedPressShaft.anchoredPosition=pressShaftHome+
            Vector2.down*offset;
        bool contact=progress>=impactPoint&&progress<.82f;
        if(pressImpactGlow!=null)
        {
            pressImpactGlow.gameObject.SetActive(contact);
            if(contact)pressImpactGlow.color=new Color(1f,.78f,.36f,.3f+.2f*Mathf.Sin((progress-impactPoint)*25f));
        }
    }

    private void ShowMechanicalSample(int index)
    {
        if(level!=2 || mechanicalCube==null)return;
        mechanicalSample=index;
        mechanicalMode=0;
        mechanicalTime=0f;
        ResetHammer();
        ResetPress();
        if(mechanicalShards!=null)foreach(var shard in mechanicalShards)
            if(shard!=null)shard.gameObject.SetActive(false);
        if(pressCrystalShards!=null)foreach(var shard in pressCrystalShards)
            if(shard!=null)shard.gameObject.SetActive(false);
        if(mechanicalFlattened!=null)mechanicalFlattened.gameObject.SetActive(false);
        if(copperStrikeDent!=null)copperStrikeDent.gameObject.SetActive(false);
        if(copperFlattenedShine!=null)copperFlattenedShine.gameObject.SetActive(false);
        if(waxStrikeDent!=null)waxStrikeDent.gameObject.SetActive(false);
        var sprite=Stage5MaterialCubeSprites.Get(index);
        if(sprite==null)sprite=CreateFallbackSampleSprite(index);
        mechanicalCube.sprite=sprite;
        mechanicalCube.rectTransform.localScale=Vector3.one;
        mechanicalCube.rectTransform.anchoredPosition=new Vector2(-12,67);
        mechanicalCube.color=Color.white;
        mechanicalCube.gameObject.SetActive(true);
    }

    private void AnimateMechanicalResult()
    {
        mechanicalTime+=Time.deltaTime;
        // Do not speed up the existing hammer. Give the hydraulic press
        // substantially longer so its approach, compression and release
        // are visible to learners (2.8 seconds vs 1.25 for STRIKE).
        float duration=mechanicalMode==2?2.8f:1.25f;
        float t=Mathf.Clamp01(mechanicalTime/duration);
        // Specimen reactions start at contact, never before.
        float impactPoint=mechanicalMode==2?.60f:.55f;
        // Preserve the moving hammer and hydraulic press during each test.
        if(mechanicalMode==1)AnimateHammer(t,impactPoint);
        if(mechanicalMode==2)AnimatePress(t,impactPoint);
        // Complete deformation during the press's contact hold, so
        // learners see the finished material before the press lifts away.
        float sampleEnd=mechanicalMode==2?.81f:1f;
        float sampleT=Mathf.Clamp01((t-impactPoint)/(sampleEnd-impactPoint));
        float ease=sampleT*sampleT*(3f-2f*sampleT);
        bool fractured=mechanicalSample==1 && mechanicalMode==1;
        bool dentedWax=mechanicalSample==0 && mechanicalMode==1;
        bool dentedCopper=mechanicalSample==2 && mechanicalMode==1;
        bool pressBrittle=mechanicalSample==1 && mechanicalMode==2;
        bool malleable=mechanicalSample==2 && mechanicalMode==2;
        // Paraffin wax is soft: visibly compress it when the press is used.
        // Unlike copper it keeps a squashed cube, rather than changing into
        // the metallic sheet sprite. This makes SOFT / DEFORMS observable.
        bool softWax=mechanicalSample==0 && mechanicalMode==2;
        Vector2 origin=mechanicalMode==1?strikeSampleStart:pressSampleStart;
        Vector2 end=origin+new Vector2(0,softWax?-28f:-43f);
        if(mechanicalCube!=null)
        {
            var rt=mechanicalCube.rectTransform;
            rt.anchoredPosition=Vector2.Lerp(origin,end,ease);
            if(fractured || pressBrittle)
                rt.localScale=Vector3.one*Mathf.Max(.02f,1f-ease);
            else if(dentedWax)
                rt.localScale=new Vector3(1f+ease*.12f,1f-ease*.34f,1f);
            else if(dentedCopper)
                rt.localScale=new Vector3(1f+ease*.26f,1f-ease*.32f,1f);
            else if(softWax)
                rt.localScale=new Vector3(1f+ease*.55f,1f-ease*.62f,1f);
            else if(malleable)
                rt.localScale=new Vector3(1f+ease*.85f,1f-ease*.78f,1f);
            else rt.localScale=Vector3.one;
            if(softWax)
                mechanicalCube.color=Color.Lerp(Color.white,
                    new Color(1f,.88f,.72f,1f),ease*.35f);
            if(dentedCopper || malleable)
                mechanicalCube.color=Color.Lerp(Color.white,
                    new Color(1f,.72f,.52f,1f),ease*.62f);
            if((fractured || pressBrittle) && t>=1f)
                mechanicalCube.gameObject.SetActive(false);
        }
        if(dentedWax && waxStrikeDent!=null)
        {
            // The first half uses the moving Unity hammer. At the impact,
            // step through the prepared wax sprites, then retain the dent.
            float dentProgress=Mathf.Clamp01((t-impactPoint)/(.94f-impactPoint));
            bool haveFrames=waxDentFrames!=null && waxDentFrames.Length==4 &&
                waxDentFrames[0]!=null && waxDentFrames[1]!=null &&
                waxDentFrames[2]!=null && waxDentFrames[3]!=null;
            if(haveFrames && t>=impactPoint)
            {
                int frame=Mathf.Min(3,Mathf.FloorToInt(dentProgress*4f));
                waxStrikeDent.sprite=waxDentFrames[frame];
                waxStrikeDent.rectTransform.anchoredPosition=
                    strikeSampleStart+new Vector2(0,-11);
                waxStrikeDent.gameObject.SetActive(true);
                if(mechanicalCube!=null)mechanicalCube.gameObject.SetActive(false);
            }
            else if(!haveFrames && t>=impactPoint)
            {
                // If sprites have not yet been imported, an unmistakable
                // scale deformation still renders instead of a static cube.
                if(mechanicalCube!=null)
                {
                    mechanicalCube.gameObject.SetActive(true);
                    mechanicalCube.rectTransform.localScale=
                        new Vector3(1f+ease*.45f,1f-ease*.53f,1f);
                }
            }
        }
        // The dark, shallow indentation appears only AFTER impact,
        // stays on the left platform, and follows the dented copper cube.
        if(dentedCopper && copperStrikeDent!=null)
        {
            copperStrikeDent.gameObject.SetActive(t>=impactPoint);
            if(t>=impactPoint)
            {
                copperStrikeDent.rectTransform.anchoredPosition=
                    strikeSampleStart+new Vector2(0,-25f-43f*ease);
                copperStrikeDent.rectTransform.sizeDelta=
                    new Vector2(10f+19f*ease,3f+6f*ease);
            }
        }
        if(fractured && mechanicalShards!=null)
        {
            float spread=Mathf.Clamp01((t-impactPoint)/(1f-impactPoint));
            for(int i=0;i<mechanicalShards.Length;i++)
            {
                var shard=mechanicalShards[i];
                if(shard==null)continue;
                shard.gameObject.SetActive(spread>.01f);
                var angle=(float)i/mechanicalShards.Length*Mathf.PI*2f;
                shard.rectTransform.anchoredPosition=new Vector2(
                    strikeSampleStart.x+Mathf.Cos(angle)*57f*spread,
                    strikeSampleStart.y-43f+Mathf.Sin(angle)*15f*spread);
                shard.rectTransform.localEulerAngles=
                    new Vector3(0,0,angle*Mathf.Rad2Deg+spread*30f);
            }
        }
        // PRESSING SODIUM CHLORIDE: crystals fracture at contact, then
        // scatter across the RIGHT testing platform and remain visible.
        if(pressBrittle && pressCrystalShards!=null)
        {
            float spread=sampleT;
            for(int i=0;i<pressCrystalShards.Length;i++)
            {
                var shard=pressCrystalShards[i];
                if(shard==null)continue;
                shard.gameObject.SetActive(spread>.01f);
                float angle=(float)i/pressCrystalShards.Length*Mathf.PI*2f;
                shard.rectTransform.anchoredPosition=new Vector2(
                    pressSampleStart.x+Mathf.Cos(angle)*47f*spread,
                    pressSampleStart.y-34f+Mathf.Sin(angle)*12f*spread);
                shard.rectTransform.localEulerAngles=new Vector3(
                    0,0,angle*Mathf.Rad2Deg+spread*24f);
            }
        }
        if(malleable && mechanicalFlattened!=null)
        {
            float flatten=sampleT;
            mechanicalFlattened.gameObject.SetActive(flatten>.01f);
            Rect(mechanicalFlattened.transform,
                pressSampleStart+new Vector2(0,-49),
                new Vector2(22+104*flatten,12+16*flatten));
            // Do not layer the previous bright oval over the new metal plate.
            if(copperFlattenedShine!=null)
                copperFlattenedShine.gameObject.SetActive(false);
            if(t>=1f && mechanicalCube!=null)
                mechanicalCube.gameObject.SetActive(false);
        }
        if(t>=1f)
        {
            mechanicalMode=0;
            ResetHammer();
            ResetPress();
        }
    }

    private void BuildEmptyHeatingBench()
    {
        if(level!=1 || canvas==null)return;
        var cover=NewImage(canvas,"EmptyHeaterCover",null,new Color32(5,27,52,255));
        Rect(cover.transform,new Vector2(-24,65),new Vector2(298,142));
        // An oval with a glowing orange rim resembles the illustrated
        // heater, instead of a rectangular placeholder.
        var oval=MakeOvalSprite();
        var glow=NewImage(canvas,"HotplateOrangeGlow",oval,
            new Color32(230,101,41,230));
        Rect(glow.transform,new Vector2(-24,-9),new Vector2(281,60));
        var ring=NewImage(canvas,"EmptyHotPlateRim",oval,new Color32(243,155,75,255));
        Rect(ring.transform,new Vector2(-24,-2),new Vector2(268,52));
        var surface=NewImage(canvas,"EmptyHotPlateSurface",oval,new Color32(49,65,85,255));
        Rect(surface.transform,new Vector2(-24,4),new Vector2(249,42));
        var inset=NewImage(canvas,"EmptyHotPlateInset",oval,new Color32(24,39,56,255));
        Rect(inset.transform,new Vector2(-24,8),new Vector2(219,27));
        heatingSample=NewImage(canvas,"ActualSampleOnHeater",null,Color.white);
        Rect(heatingSample.transform,new Vector2(-24,34),new Vector2(108,100));
        heatingSample.preserveAspect=true;
        heatingSample.gameObject.SetActive(false);
        // Hidden until the player heats Sample A. These dynamically animated
        // liquid layers live on the real heater, not in a screenshot preview.
        var liquidSprite=CreateLiquidPuddleSprite();
        moltenPool=NewImage(canvas,"HeatedSampleLiquidPool",liquidSprite,
            new Color32(255,255,255,0));
        Rect(moltenPool.transform,new Vector2(-24,17),new Vector2(5,10));
        moltenPool.gameObject.SetActive(false);
        moltenHighlight=NewImage(canvas,"HeatedSampleLiquidShine",MakeOvalSprite(),
            new Color32(255,236,210,0));
        Rect(moltenHighlight.transform,new Vector2(-24,22),new Vector2(5,3));
        moltenHighlight.gameObject.SetActive(false);

        var meter=NewImage(canvas,"HeatThermometerBlank",null,new Color32(7,26,51,255));
        Rect(meter.transform,new Vector2(198,44),new Vector2(88,216));
        var rim=NewImage(canvas,"HeatThermometerTrack",null,new Color32(47,129,176,255));
        Rect(rim.transform,new Vector2(194,36),new Vector2(15,149));
        var groove=NewImage(canvas,"HeatThermometerGroove",null,new Color32(10,20,39,255));
        Rect(groove.transform,new Vector2(194,36),new Vector2(9,141));
        thermometerFill=NewImage(canvas,"LiveHeatMercury",null,new Color32(255,131,61,255));
        Rect(thermometerFill.transform,new Vector2(194,-27),new Vector2(8,1));
        // This earlier top-of-scale percentage overlapped the "100" tick.
        // Use the hotplate LED and the bottom slider readout for live percent.
        thermometerValue=null;
        var thermTitle=NewText(canvas,"ThermometerTitle","HEAT",13,
            new Vector2(224,135),new Vector2(60,20),Gold);
        thermTitle.fontStyle=FontStyles.Bold;
        for(int i=0;i<=4;i++)
        {
            var tick=NewText(canvas,"HeatTick"+i,(i*25).ToString(),10,
                new Vector2(225,-27+i*32),new Vector2(32,17),Color.white);
            tick.alignment=TextAlignmentOptions.Center;
        }
        // This LED mask covers the baked 175 C and is kept up to date.
        var led=NewImage(canvas,"HotplateActualHeatMask",null,new Color32(12,27,52,255));
        Rect(led.transform,new Vector2(-27,-69),new Vector2(101,36));
        dynamicPlateHeat=NewText(led.transform,"LiveHotplateHeat","0%",17,
            Vector2.zero,new Vector2(95,29),Gold);
    }

    private static Sprite MakeOvalSprite()
    {
        const int size=64;
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
        texture.filterMode=FilterMode.Bilinear;
        for(int y=0;y<size;y++)
        for(int x=0;x<size;x++)
        {
            float dx=(x-31.5f)/31.5f;
            float dy=(y-31.5f)/31.5f;
            float radius=Mathf.Sqrt(dx*dx+dy*dy);
            texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((1f-radius)*18f)));
        }
        texture.Apply();
        return Sprite.Create(texture,new Rect(0,0,size,size),
            new Vector2(.5f,.5f),100f);
    }

    // Transparent irregular liquid illustration, created at runtime.
    // No additional sprite import or scene edits required.
    private static Sprite CreateLiquidPuddleSprite()
    {
        const int w=256,h=112;
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);
        tex.filterMode=FilterMode.Bilinear;
        for(int y=0;y<h;y++)
        for(int x=0;x<w;x++)
        {
            float nx=(x-w*.5f)/(w*.48f), ny=(y-h*.5f)/(h*.44f);
            float angle=Mathf.Atan2(ny,nx);
            // Asymmetric molten edge with three subtle drips/lobes.
            float edge=1f+.070f*Mathf.Sin(angle*5f+.4f)+
                .047f*Mathf.Sin(angle*9f+1.3f)+.035f*Mathf.Sin(angle*13f);
            float radius=Mathf.Sqrt(nx*nx+ny*ny);
            float alpha=Mathf.Clamp01((edge-radius)*22f);
            float bottom=Mathf.Clamp01((ny+1f)*.5f);
            float rim=Mathf.Clamp01(1f-Mathf.Abs(radius-edge+.065f)*13f);
            float glaze=Mathf.Exp(-((nx+.27f)*(nx+.27f)/.13f+
                (ny-.37f)*(ny-.37f)/.025f));
            float shine2=Mathf.Exp(-((nx-.38f)*(nx-.38f)/.075f+
                (ny+.03f)*(ny+.03f)/.013f));
            // The sample melts into a glossy translucent SILVERY liquid.
            // Orange is the heater light underneath, not the sample itself.
            // Keep the surface brighter than the hotplate so the liquid is
            // unmistakable against its dark metal heating ring.
            float shine=Mathf.Clamp01(.18f+.28f*bottom+
                .43f*glaze+.32f*shine2+.28f*rim);
            Color baseColor=Color.Lerp(new Color(.55f,.74f,.85f,1f),
                new Color(1f,1f,.98f,1f),shine);
            // Small warm reflection on the lower edge from the hot plate.
            float warm=Mathf.Clamp01((.10f-ny)*.3f);
            baseColor=Color.Lerp(baseColor,new Color(1f,.80f,.65f,1f),warm*.25f);
            tex.SetPixel(x,y,new Color(baseColor.r,baseColor.g,baseColor.b,alpha));
        }
        tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(.5f,.5f),100f);
    }

    private static Sprite CreateFallbackSampleSprite(int index)
    {
        const int w=96,h=96;
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);
        Color baseColor=index==0?new Color(.65f,.89f,1f,1f):
            index==1?new Color(.77f,.63f,.97f,1f):new Color(.61f,.95f,.82f,1f);
        for(int y=0;y<h;y++)
        for(int x=0;x<w;x++)
        {
            float edge=Mathf.Min(Mathf.Min(x,w-1-x),Mathf.Min(y,h-1-y));
            float light=.68f+.27f*(x+y)/(w+h);
            float alpha=Mathf.Clamp01(edge/4f);
            var c=baseColor*light;
            tex.SetPixel(x,y,new Color(c.r,c.g,c.b,alpha));
        }
        tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(.5f,.5f),100f);
    }

    private void ShowHeatingSample(int index)
    {
        if(level!=1)return;
        isMelting=false;
        meltingTime=0f;
        if(moltenPool!=null)moltenPool.gameObject.SetActive(false);
        if(moltenHighlight!=null)moltenHighlight.gameObject.SetActive(false);
        if(heatingSample==null || index<0 || index>2)return;
        heatingSample.sprite=Stage5MaterialCubeSprites.Get(index);
        // Keep a visible specimen even if a particular optional sample PNG
        // is missing from Resources.
        if(heatingSample.sprite==null)
            heatingSample.sprite=CreateFallbackSampleSprite(index);
        heatingSample.rectTransform.localScale=Vector3.one;
        heatingSample.rectTransform.anchoredPosition=new Vector2(-24,34);
        heatingSample.color=Color.white;
        heatingSample.gameObject.SetActive(heatingSample.sprite!=null);
    }

    // A baked scene can retain the Image GameObject after its generated
    // Sprite reference is invalidated by reimporting assets. Restore from the
    // real Level3 PNG on every Bake/Initialize instead of trusting the scene's
    // serialized sprite. Keep it BEHIND modular interactive UI.
    private void RestoreStage5Backdrop(Sprite reference)
    {
        if(canvas==null || reference==null)return;
        var backdrop=Find(canvas,"ExactStage5ReferenceBackdrop") as RectTransform;
        if(backdrop==null)return;
        backdrop.gameObject.SetActive(true);
        var image=backdrop.GetComponent<Image>();
        if(image==null)image=backdrop.gameObject.AddComponent<Image>();
        image.enabled=true;
        image.sprite=reference;
        image.overrideSprite=null;
        image.color=Color.white;
        image.type=Image.Type.Simple;
        image.preserveAspect=false;
        image.raycastTarget=false;
        backdrop.anchorMin=Vector2.zero;
        backdrop.anchorMax=Vector2.one;
        backdrop.offsetMin=Vector2.zero;
        backdrop.offsetMax=Vector2.zero;
        backdrop.localScale=Vector3.one;

        // IMPORTANT: "Background" is an independent scene Image whose
        // original artwork is serialized in Stage5Level1.unity. Never
        // overwrite it with the screenshot/reference Sprite. Doing so can
        // erase the actual colorful lab backdrop if the reference texture
        // changes or is reimported. Keep that artwork as the base layer.
        var baseBackground=Find(canvas,"Background")?.GetComponent<Image>();
        if(baseBackground!=null)
        {
            var owner=GetComponent<Stage5ElementCrucibleGame>();
            if(baseBackground.sprite==null && owner!=null &&
               owner.BackgroundSprite!=null)
                baseBackground.sprite=owner.BackgroundSprite;
            baseBackground.gameObject.SetActive(true);
            baseBackground.enabled=true;
            baseBackground.overrideSprite=null;
            baseBackground.color=Color.white;
            baseBackground.raycastTarget=false;
            baseBackground.transform.SetAsFirstSibling();
        }
        // Draw reference over old procedural UI but under the new modular
        // design. Do not move the modular UI or the touch input layer.
        var modular=Find(canvas,"Stage5Level3_ModularUI");
        if(level==3 && modular!=null)
            backdrop.SetSiblingIndex(Mathf.Max(0,modular.GetSiblingIndex()-1));
        Debug.Log("CHEMQUEST Stage 5: Restored original Level"+level+
            " background sprites in saved scene.");
    }

    private void RebindBakedHierarchy()
    {
        // Older saved scenes predate the new Level 2 physical demonstration.
        // Upgrade them in place rather than creating a second full UI.
        if(level==2 && Find(canvas,"MechanicalEmptySampleMask")==null)
            BuildMechanicalWorkbench();
        station=Find(canvas,"TestStation") as RectTransform;
        liveSample=Find(canvas,"ActiveSampleName")?.GetComponent<TMP_Text>();
        liveNotebook=Find(canvas,"ActualNotebook")?.GetComponent<TMP_Text>();
        liveStatus=Find(canvas,"RealExperimentStatus")?.GetComponent<TMP_Text>();
        preview=Find(canvas,"ActiveExperimentCloseup")?.GetComponent<Image>();
        if(preview!=null)previewHome=preview.rectTransform.anchoredPosition;
        dynamicHeatKnob=Find(canvas,"ReferenceHeatKnob")?.GetComponent<Image>();
        dynamicHeatFill=Find(canvas,"CleanHeatFill")?.GetComponent<Image>();
        heatingSample=Find(canvas,"ActualSampleOnHeater")?.GetComponent<Image>();
        mechanicalCube=Find(canvas,"ActualMechanicalSample")?.GetComponent<Image>();
        mechanicalTopMask=Find(canvas,"MechanicalEmptySampleMask")?.GetComponent<Image>();
        mechanicalFragmentsMask=Find(canvas,"MechanicalShatteredResultMask")?.GetComponent<Image>();
        mechanicalPressMask=Find(canvas,"MechanicalPressedResultMask")?.GetComponent<Image>();
        // Do not reapply template RectTransforms to baked Level 2 masks:
        // the user may already have adjusted them in Edit Mode.
        if(level==2)
        {
            if(mechanicalFragmentsMask!=null)mechanicalFragmentsMask.enabled=false;
            if(mechanicalPressMask!=null)mechanicalPressMask.enabled=false;
            DecorateMechanicalWorkBench();
        }
        mechanicalFlattened=Find(canvas,"MechanicalFlattenedMetal")?.GetComponent<Image>();
        if(level==2)
        {
            EnsureWaxDentAnimation();
            EnsureCopperEffects();
        }
        // The sample, flattened sprite and fragment positions remain as
        // serialized. Only the actual STRIKE/PRESS animation moves them.
        animatedHammerPivot=Find(canvas,"AnimatedStrikeHammerPivot") as RectTransform;
        animatedHammerHead=Find(canvas,"AnimatedStrikeHammerHead")?.GetComponent<Image>();
        hammerImpactFlash=Find(canvas,"HammerImpactFlash")?.GetComponent<Image>();
        // Upgrade an already-baked Level2 scene rather than requiring a new
        // scene: create missing animated hammer in the saved hierarchy.
        if(level==2 && animatedHammerPivot==null)BuildAnimatedHammer();
        // Existing hammer pivot/hinge/flash positions are user-editable:
        // keep their saved geometry instead of resetting on Play.
        if(level==2)BuildAnimatedPress();
        ResetHammer();
        ResetPress();
        mechanicalShards=new Image[9];
        if(level==2)EnsurePressCrystalShards();
        // Upgrade already-baked scenes from round dots to faceted crystals.
        var shardSprite=level==2?MakeCrystalShardSprite():null;
        for(int i=0;i<mechanicalShards.Length;i++)
        {
            mechanicalShards[i]=Find(canvas,"MechanicalFragment"+i)?.GetComponent<Image>();
            if(level==2 && mechanicalShards[i]!=null)
            {
                mechanicalShards[i].sprite=shardSprite;
                mechanicalShards[i].overrideSprite=null;
                mechanicalShards[i].color=new Color32(224,211,255,255);
                mechanicalShards[i].preserveAspect=true;
                mechanicalShards[i].raycastTarget=false;
            }
        }
        moltenPool=Find(canvas,"HeatedSampleLiquidPool")?.GetComponent<Image>();
        moltenHighlight=Find(canvas,"HeatedSampleLiquidShine")?.GetComponent<Image>();
        thermometerFill=Find(canvas,"LiveHeatMercury")?.GetComponent<Image>();
        thermometerValue=Find(canvas,"ThermometerPercent")?.GetComponent<TMP_Text>();
        // The editable heater readout is named LiveHotplateHeat in
        // BuildEmptyHeatingBench(). Older scenes used HotplateHeatPercent.
        // Resolve both so the hotplate LED mirrors the heat slider.
        dynamicPlateHeat=Find(canvas,"LiveHotplateHeat")?.GetComponent<TMP_Text>();
        if(dynamicPlateHeat==null)
            dynamicPlateHeat=Find(canvas,"HotplateHeatPercent")?.GetComponent<TMP_Text>();
        if(dynamicPlateHeat==null && level==1)
            Debug.LogWarning("Stage 5 Level 1: Hotplate heat readout is missing.");
        dynamicHeatReadout=Find(canvas,"ReferenceHeatReadout")?.GetComponent<TMP_Text>();
        dynamicProgress=Find(canvas,"ReferenceProgress")?.GetComponent<TMP_Text>();
        dynamicScore=Find(canvas,"ReferenceLiveScore")?.GetComponent<TMP_Text>();
        dynamicEnergy=Find(canvas,"ReferenceLiveEnergy")?.GetComponent<TMP_Text>();
        conductivityBulbMask=Find(canvas,"DynamicLampMask")?.GetComponent<Image>();
        conductivityResultPlate=Find(canvas,"DynamicProbeResult")?.GetComponent<Image>();
        conductivityResultText=Find(canvas,"DynamicProbeResultLabel")?.GetComponent<TMP_Text>();
        if(level==3)PolishLevel3Reference();
        if(level==1 || level==2)RefreshStage5MaterialButtons();
        touchController=Find(canvas,"Stage5MappedTouchSurface")?.GetComponent<Stage5ReferenceTouchController>();
        if(touchController!=null)
        {
            touchController.Setup(GetComponent<Stage5ElementCrucibleGame>(),this,level);
        }
        if(previous!=null)previous.HideForReferenceArt();
        if(preview!=null)preview.gameObject.SetActive(false);
        if(heatingSample!=null)heatingSample.gameObject.SetActive(false);
        if(moltenPool!=null)moltenPool.gameObject.SetActive(false);
        if(moltenHighlight!=null)moltenHighlight.gameObject.SetActive(false);
        if(mechanicalCube!=null)mechanicalCube.gameObject.SetActive(false);
        if(mechanicalFlattened!=null)mechanicalFlattened.gameObject.SetActive(false);
        if(mechanicalShards!=null)foreach(var shard in mechanicalShards)
            if(shard!=null)shard.gameObject.SetActive(false);
        mechanicalMode=0;mechanicalSample=-1;mechanicalTime=0f;
        isMelting=false;
        meltingTime=0f;
        ready=true;
        if(level==1)ReferenceHeatChanged(0f);
        if(level==3)SetConductivityVisual(false,false);
        Debug.Log("CHEMQUEST Stage 5: Reused saved, editable hierarchy (Level "+level+").");
    }

    public void ReferenceHeatChanged(float value)
    {
        if(!ready||level!=1)return;
        float fraction=Mathf.Clamp01(value/100f);
        // Derive the animated range from the SAVED CleanHeatTrack
        // RectTransform, rather than rebuilding the fill at fixed scene
        // coordinates. This respects user edits to track position and width.
        var track=Find(canvas,"CleanHeatTrack") as RectTransform;
        float left=-294f, right=276f;
        if(track!=null)
        {
            var trackPosition=track.anchoredPosition;
            float trackWidth=track.rect.width;
            // Five points of inset at each end keep the fill inside the rail.
            left=trackPosition.x-trackWidth*track.pivot.x+5f;
            right=trackPosition.x+trackWidth*(1f-track.pivot.x)-5f;
        }
        if(dynamicHeatKnob!=null)
        {
            var pos=dynamicHeatKnob.rectTransform.anchoredPosition;
            pos.x=Mathf.Lerp(left,right,fraction);
            dynamicHeatKnob.rectTransform.anchoredPosition=pos;
        }
        if(dynamicHeatFill!=null)
        {
            // Keep the edited Y, height, scale, and pivot. Only the X
            // position and displayed width should animate with heat.
            var rect=dynamicHeatFill.rectTransform;
            float width=(right-left)*fraction;
            var pos=rect.anchoredPosition;
            pos.x=left+width*rect.pivot.x;
            rect.anchoredPosition=pos;
            var size=rect.sizeDelta;
            size.x=width;
            rect.sizeDelta=size;
        }
        string reading=Mathf.RoundToInt(value)+"%";
        if(dynamicHeatReadout!=null)
            dynamicHeatReadout.text=reading;
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
        if(modularLevel3!=null)modularLevel3.SetPhase(selectedPhase);
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
        if(liveSample!=null)
            liveSample.text=(level==1 || level==2) && index>=0 && index<3?
                Stage5MaterialNames[index]:"SAMPLE "+(char)('A'+index);
        if(liveStatus!=null)liveStatus.text="SELECT THE TEST ACTION";
        if(preview!=null) preview.gameObject.SetActive(false);
        if(level==1)
        {
            ShowHeatingSample(index);
            if(liveStatus!=null)liveStatus.text="ADJUST HEAT, THEN PRESS APPLY HEAT";
        }
        if(level==2)ShowMechanicalSample(index);
        if(level==3)
        {
            SetConductivityVisual(false,false);
            if(modularLevel3!=null)modularLevel3.SetSelected(index);
        }
        actionEffect=0;
    }

    public void ThermalResult(int sample)
    {
        if(!ready)return;
        actionEffect=1;
        if(sample==0)
        {
            // Do NOT remove the cube before rendering the result.
            // Animate the solid shrinking and spreading into a liquid puddle
            // at the precise heater pad location over ~2 seconds.
            if(heatingSample!=null && !heatingSample.gameObject.activeSelf)
                ShowHeatingSample(sample);
            isMelting=true;
            meltingTime=0f;
            if(moltenPool!=null)moltenPool.gameObject.SetActive(true);
            if(moltenHighlight!=null)moltenHighlight.gameObject.SetActive(true);
            if(preview!=null)preview.gameObject.SetActive(false);
            if(liveStatus!=null)liveStatus.text="SAMPLE A IS MELTING";
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
        // This level animates the actual sample rather than overlaying a
        // separate picture of a hammer/press on the full reference artwork.
        if(level==2)
        {
            if(mechanicalSample!=sample)ShowMechanicalSample(sample);
            mechanicalMode=pressed?2:1;
            mechanicalTime=0f;
            // Move the specimen to the appropriate testing platform
            // BEFORE the tool moves, so the hammer strikes a real target.
            // Match the actual two fixed platform locations in the Level 2
            // reference artwork. NEVER derive test Y from the cube's
            // previous animated position: each test would subtract 117
            // again and eventually send the cube below the buttons.
            strikeSampleStart=new Vector2(-145f,-50f);
            pressSampleStart=new Vector2(150f,-50f);
            if(mechanicalCube!=null)
            {
                var start=pressed?pressSampleStart:strikeSampleStart;
                mechanicalCube.rectTransform.anchoredPosition=start;
                mechanicalCube.rectTransform.localScale=Vector3.one;
                mechanicalCube.gameObject.SetActive(true);
            }
            if(pressed)
            {
                // The press front edge (roughly head Y minus 14) should
                // meet the top of the specimen when the cycle reaches impact.
                float sampleTop=pressSampleStart.y+30f;
                pressTravel=Mathf.Clamp(
                    pressHeadHome.y-14f-sampleTop,12f,65f);
            }
            hammerHasStruck=false;
            ResetHammer();
            ResetPress();
            if(mechanicalFlattened!=null)mechanicalFlattened.gameObject.SetActive(false);
            if(copperStrikeDent!=null)copperStrikeDent.gameObject.SetActive(false);
            if(copperFlattenedShine!=null)copperFlattenedShine.gameObject.SetActive(false);
            if(waxStrikeDent!=null)waxStrikeDent.gameObject.SetActive(false);
            if(mechanicalShards!=null)foreach(var shard in mechanicalShards)
                if(shard!=null)shard.gameObject.SetActive(false);
            if(pressCrystalShards!=null)foreach(var shard in pressCrystalShards)
                if(shard!=null)shard.gameObject.SetActive(false);
            if(preview!=null)preview.gameObject.SetActive(false);
            return;
        }
    }

    public void PhaseChanged(int state)
    {
        if(!ready)return;
        phase=state;
        if(modularLevel3!=null)modularLevel3.SetPhase(state);
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
        // Show the verified result in the compact conductivity status,
        // not the huge close-up sprite that covers the phase buttons.
        if(preview!=null)preview.gameObject.SetActive(false);
        if(liveStatus!=null)liveStatus.text=
            conducting?"BULB ON - CONDUCTS":"BULB OFF - DOES NOT CONDUCT";
    }

    private void SetConductivityVisual(bool tested,bool conducting)
    {
        if(level!=3)return;
        if(modularLevel3!=null)modularLevel3.SetConductivity(tested,conducting);
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
        if(level==3)
        {
            if(modularLevel3!=null)modularLevel3.SetNotebook(a,b,c);
            // Compress two-line phase data to one readable row per sample,
            // preserving all observations. Aqueous is N/A for A and C.
            string Compact(string observation)
            {
                return observation.Replace("SOLID ","S:")
                    .Replace("  MOLTEN ","  M:")
                    .Replace("\n     AQ ","\n       Aq:");
            }
            liveNotebook.text=
                "<color=#7ECDEF><b>A</b></color>  "+Compact(a)+"\n"+
                "<color=#D4A5FF><b>B</b></color>  "+Compact(b)+"\n"+
                "<color=#7FF3DC><b>C</b></color>  "+Compact(c);
            return;
        }
        liveNotebook.text=
            "<color=#7ECDEF><b>A</b></color>  "+a+"\n\n"+
            "<color=#D4A5FF><b>B</b></color>  "+b+"\n\n"+
            "<color=#7FF3DC><b>C</b></color>  "+c;
    }

    public void ModularClassificationChanged(int sample,int bin)
    {
        if(level==3 && modularLevel3!=null)
            modularLevel3.SetClassification(sample,bin);
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
        if(!ready)return;
        if(isMelting)AnimateMelting();
        if(level==2 && mechanicalMode!=0)AnimateMechanicalResult();
        if(preview==null || !preview.gameObject.activeSelf)return;
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
    private void AnimateMelting()
    {
        meltingTime+=Time.deltaTime;
        float t=Mathf.Clamp01(meltingTime/2.4f);
        float smooth=t*t*(3f-2f*t);
        if(heatingSample!=null)
        {
            var r=heatingSample.rectTransform;
            r.localScale=new Vector3(1f+.24f*smooth,Mathf.Max(.10f,1f-.90f*smooth),1f);
            r.anchoredPosition=new Vector2(-24f,34f-15f*smooth);
            heatingSample.color=Color.Lerp(Color.white,WarmLiquid,smooth);
            // The remnant disappears at the end, leaving an unmistakable
            // irregular glossy liquid puddle on the heating surface.
            if(t>=1f)heatingSample.gameObject.SetActive(false);
        }
        if(moltenPool!=null)
        {
            var r=moltenPool.rectTransform;
            // Keep the irregular, glossy liquid visible ABOVE the dark
            // heater, not just a flat orange ellipse indistinguishable
            // from the already glowing heating ring.
            Rect(r,new Vector2(-24,23),new Vector2(24+183*smooth,9+47*smooth));
            moltenPool.color=new Color(1f,1f,1f,.10f+.90f*smooth);
        }
        if(moltenHighlight!=null)
        {
            float shimmer=.84f+.14f*Mathf.Sin(meltingTime*7f);
            Rect(moltenHighlight.rectTransform,new Vector2(-42,33),
                new Vector2(5+90*smooth,2+6*smooth));
            moltenHighlight.color=new Color(.96f,.99f,1f,.04f+.69f*smooth*shimmer);
        }
        if(t>=1f)
        {
            isMelting=false;
            if(heatingSample!=null)heatingSample.gameObject.SetActive(false);
            if(liveStatus!=null)liveStatus.text="SAMPLE A MELTED INTO LIQUID";
        }
    }

}
