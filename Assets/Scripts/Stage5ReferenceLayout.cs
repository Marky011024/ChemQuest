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
        skin.transform.SetSiblingIndex(board.GetSiblingIndex());

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
        var ink=NewImage(notebook,"AccurateNotebookPanel",null,Navy);
        Rect(ink.transform,Vector2.zero,new Vector2(278,315));
        ink.raycastTarget=false;
        liveNotebook=NewText(ink.transform,"ActualNotebook","A: Not tested\n\nB: Not tested\n\nC: Not tested",
            18,new Vector2(0,-13),new Vector2(254,223),Color.white);
        var title=NewText(ink.transform,"ActualNotebookTitle","LAB NOTEBOOK",23,
            new Vector2(0,133),new Vector2(264,35),Gold);
        title.fontStyle=FontStyles.Bold;
        liveNotebook.alignment=TextAlignmentOptions.TopLeft;
        liveNotebook.margin=new Vector4(8,6,6,3);

        // Put a transparent drop target on the real picture, not an artificial box.
        if(station!=null)
        {
            var activeStation=station.GetComponent<Image>();
            if(activeStation!=null){activeStation.color=new Color(1,1,1,0f);activeStation.raycastTarget=true;}
            // Close-up action visual: heater, striking hammer, hydraulic press,
            // or wired conductivity apparatus, whichever was actually tested.
            preview=NewImage(station,"ActiveExperimentCloseup",null,Color.white);
            Rect(preview.transform,new Vector2(0,-4),new Vector2(425,245));
            preview.preserveAspect=true;
            preview.raycastTarget=false;
            previewHome=preview.rectTransform.anchoredPosition;
            preview.gameObject.SetActive(false);
            liveSample=NewText(station,"ActiveSampleName","DRAG A SAMPLE",22,
                new Vector2(0,148),new Vector2(390,34),Gold);
            liveSample.fontStyle=FontStyles.Bold;
            liveStatus=NewText(station,"RealExperimentStatus","",17,
                new Vector2(0,-149),new Vector2(455,28),Cyan);
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
        if(score!=null)
        {
            Rect(score.transform,new Vector2(555,303),new Vector2(128,62));
            score.fontSize=17;
            score.color=Gold;
            score.fontStyle=FontStyles.Bold;
            score.alignment=TextAlignmentOptions.Center;
        }
        var energyCount=Find(sceneRoot,"EnergyCount")?.GetComponent<TMP_Text>();
        if(energyCount!=null)
        {
            Rect(energyCount.transform,new Vector2(-312,309),new Vector2(78,35));
            energyCount.fontSize=20;energyCount.fontStyle=FontStyles.Bold;
        }
        var energyLabel=Find(sceneRoot,"EnergyLabel")?.GetComponent<TMP_Text>();
        if(energyLabel!=null)energyLabel.enabled=false;
        for(int i=1;i<=5;i++)
        {
            var energyImg=Find(sceneRoot,"Energy"+i)?.GetComponent<Image>();
            if(energyImg!=null)energyImg.enabled=false;
        }
        // Keep the real action buttons above the background image.
        if(level==1)PlaceLevel1Controls();
        if(level==2)PlaceLevel2Controls();
        if(level==3)PlaceLevel3Controls();
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

    public void SampleChanged(int index)
    {
        if(!ready)return;
        selected=index;
        if(liveSample!=null)liveSample.text="SAMPLE "+(char)('A'+index);
        if(liveStatus!=null)liveStatus.text="SELECT THE TEST ACTION";
        if(preview!=null)preview.gameObject.SetActive(false);
        actionEffect=0;
    }

    public void ThermalResult(int sample)
    {
        if(!ready)return;
        actionEffect=1;
        SetResult(sample==0?"HeatMelt":null,sample==0?
            "SAMPLE A MELTS":"SOLID AFTER HEATING");
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
        if(liveStatus!=null)liveStatus.text=
            (state==0?"SOLID":state==1?"MOLTEN":"AQUEOUS")+" - USE TEST PROBE";
    }

    public void ProbeResult(int sample,bool conducting)
    {
        if(!ready)return;
        // The illustration with mobile ions depicts IONIC sample B only.
        actionEffect=4;
        SetResult(sample==1&&conducting?"ConductivityOn":null,
            conducting?"BULB ON - CONDUCTS":"BULB OFF - DOES NOT CONDUCT");
    }

    public void SyncNotebook(string actual)
    {
        if(!ready||liveNotebook==null)return;
        // Prefer the actual live record in the original EvidenceText field.
        liveNotebook.text=string.IsNullOrWhiteSpace(actual)?
            "A: Not tested\n\nB: Not tested\n\nC: Not tested":actual;
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
