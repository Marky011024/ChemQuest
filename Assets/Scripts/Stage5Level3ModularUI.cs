using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Stage 5 Level 3: modular, editable presentation only.
// Every card, button face, label, status light and ion is a separate
// Unity GameObject saved by ChemQuest > Stage 5 > Bake Playable UI.
// The existing Stage5ReferenceTouchController still handles ALL inputs;
// none of these visuals intercept raycasts or change gameplay rules.
public sealed class Stage5Level3ModularUI : MonoBehaviour
{
    private RectTransform ui;
    private TMP_Text chosen, resultText, noteA, noteB, noteC, classStatus;
    private Image resultPlate, bulbGlow;
    private Image realBulb, realWire;
    private readonly Image[] sampleCards=new Image[3];
    private readonly Image[] phaseCards=new Image[3];
    private readonly Image[] classCards=new Image[3];
    private int selected=-1,phase=0;
    private bool tested, conducts;
    private static readonly Color Deep=new Color32(5,24,52,255);
    private static readonly Color Panel=new Color32(9,37,72,255);
    private static readonly Color Inner=new Color32(13,47,86,255);
    private static readonly Color Border=new Color32(43,182,240,255);
    private static readonly Color Gold=new Color32(252,206,105,255);
    private static readonly Color Text=new Color32(237,249,255,255);
    private static readonly Color Teal=new Color32(3,142,164,255);
    private static readonly Color Off=new Color32(42,88,132,255);
    private static readonly Color[] SampleColors={
        new Color32(14,109,193,255),
        new Color32(114,62,177,255),
        new Color32(6,160,156,255)
    };

    public void Build(Transform canvas)
    {
        if(canvas==null)return;
        var existing=Find(canvas,"Stage5Level3_ModularUI");
        if(existing==null)
        {
            var go=new GameObject("Stage5Level3_ModularUI",typeof(RectTransform));
            go.transform.SetParent(canvas,false);
            ui=go.GetComponent<RectTransform>();
        }
        else ui=existing as RectTransform;
        if(ui==null)return;
        Place(ui,Vector2.zero,new Vector2(1280,720));
        ui.SetAsLastSibling();

        // Keep the laboratory background file completely unchanged.
        // One opaque clean matte covers only its pre-printed, noninteractive
        // Level 3 screenshot UI, eliminating duplicate titles, sample labels
        // and classification buttons. The modular UI sits above this matte.
        var legacyFrame=Find(ui,"Workbench_CleanBase_Frame");
        if(legacyFrame!=null)legacyFrame.gameObject.SetActive(false);
        var matte=EnsureImage(ui,"CleanGameplayMatte");
        matte.sprite=null;
        matte.color=Deep;
        matte.type=Image.Type.Simple;
        Place(matte.rectTransform,new Vector2(0,-80),
            new Vector2(1124,552));
        matte.rectTransform.SetAsFirstSibling();
        Label(ui,"LevelTitle","SOLUTION & PHASE CONDUCTIVITY",
            new Vector2(0,169),new Vector2(1010,44),30,Gold);
        Label(ui,"Instructions",
            "Test SOLID and MOLTEN states. Test AQUEOUS when applicable, then classify.",
            new Vector2(0,136),new Vector2(1030,24),15,Text);

        var bank=Group(ui,"UnknownSamplesPanel",new Vector2(-421,-41),new Vector2(252,331));
        PanelImage(bank,"PanelBackground",Vector2.zero,new Vector2(252,331),Panel,Border);
        Label(bank,"PanelTitle","UNKNOWN SAMPLES",new Vector2(0,137),new Vector2(241,40),20,Gold);
        InstallHeaderArt(bank,"SampleHeader",new Vector2(0,137),
            new Vector2(244,45),Find(bank,"PanelTitle"));
        for(int i=0;i<3;i++)
        {
            var row=Group(bank,"SampleButton_"+(char)('A'+i),new Vector2(0,57-i*79),new Vector2(228,64));
            sampleCards[i]=PanelImage(row,"ButtonImage",Vector2.zero,new Vector2(228,63),
                SampleColors[i],Gold);
            PanelImage(row,"SampleIcon",new Vector2(-86,0),new Vector2(42,39),
                new Color32(190,236,252,255),new Color32(117,183,240,255));
            Label(row,"SampleLabel","SAMPLE "+(char)('A'+i),new Vector2(21,0),
                new Vector2(170,44),19,Text);
            // The reference design's complete glossy button is an editable
            // sprite on this Image, not painted onto the scene background.
            InstallButtonArt(sampleCards[i],"Sample"+(char)('A'+i),
                Find(row,"SampleIcon_Frame"),Find(row,"SampleLabel"));
        }

        var center=Group(ui,"ConductivityWorkbench",new Vector2(-34,-39),new Vector2(509,331));
        PanelImage(center,"CenterPanel",Vector2.zero,new Vector2(509,331),Panel,Border);
        var selectedTag=Group(center,"SelectedSampleTag",new Vector2(0,146),new Vector2(177,32));
        PanelImage(selectedTag,"TagImage",Vector2.zero,new Vector2(177,32),Teal,Gold);
        chosen=Label(selectedTag,"SelectedSampleText","CHOOSE A SAMPLE",
            Vector2.zero,new Vector2(174,29),16,Text);

        var apparatus=Group(center,"ConductivityApparatus",new Vector2(-38,20),new Vector2(285,191));
        PanelImage(apparatus,"BeakerBody",new Vector2(0,-26),new Vector2(182,106),
            new Color32(9,109,166,255),Border);
        PanelImage(apparatus,"SolutionLiquid",new Vector2(0,-43),new Vector2(166,63),
            new Color32(4,185,226,220),new Color32(86,220,255,255));
        PanelImage(apparatus,"BeakerRim",new Vector2(0,29),new Vector2(193,13),
            new Color32(128,222,255,255),Gold);
        var positive=PanelImage(apparatus,"LeftElectrode",
            new Vector2(-57,26),new Vector2(13,85),
            new Color32(211,113,91,255),Gold);
        var negative=PanelImage(apparatus,"RightElectrode",
            new Vector2(58,26),new Vector2(13,85),
            new Color32(89,150,230,255),Gold);
        InstallButtonArt(positive,"ElectrodePositive");
        InstallButtonArt(negative,"ElectrodeNegative");
        realWire=PanelImage(apparatus,"ConnectingWire",new Vector2(0,75),
            new Vector2(145,32),Color.white,Gold);
        InstallButtonArt(realWire,"WireOff");
        // Keep the connecting wires behind the electrode caps.
        realWire.transform.parent.SetSiblingIndex(3);
        var bulb=Group(apparatus,"ConductivityBulb",
            new Vector2(0,94),new Vector2(55,70));
        bulbGlow=PanelImage(bulb,"BulbGlow",Vector2.zero,
            new Vector2(43,46),Off,Gold);
        var oldBulbSymbol=Label(bulb,"BulbSymbol","*",
            Vector2.zero,new Vector2(35,35),25,Text);
        realBulb=PanelImage(bulb,"BulbStateSprite",Vector2.zero,
            new Vector2(55,70),Color.white,Gold);
        InstallButtonArt(realBulb,"BulbOff",
            Find(bulb,"BulbGlow_Frame"),oldBulbSymbol.transform);
        for(int i=0;i<8;i++)
        {
            var ion=Group(apparatus,"Ion_"+(i+1),
                new Vector2(-66+(i%4)*42,-21-(i/4)*36),new Vector2(27,27));
            var disc=PanelImage(ion,"IonDisc",Vector2.zero,new Vector2(27,27),
                i%2==0?new Color32(36,193,89,255):new Color32(146,55,206,255),Text);
            var charge=Label(ion,"ChargeLabel",i%2==0?"+":"-",
                Vector2.zero,new Vector2(25,25),19,Text);
            InstallButtonArt(disc,i%2==0?"IonPositive":"IonNegative",
                charge.transform);
        }

        var status=Group(center,"ConductivityResultPanel",new Vector2(177,50),new Vector2(118,80));
        PanelImage(status,"ResultBorder",Vector2.zero,new Vector2(118,80),Deep,Gold);
        Label(status,"ResultTitle","CONDUCTIVITY",new Vector2(0,23),new Vector2(112,22),12,Gold);
        resultPlate=PanelImage(status,"ConductivityResultImage",new Vector2(0,-15),
            new Vector2(109,38),Off,Border);
        resultText=Label(status,"ConductivityResultText","NOT TESTED",
            new Vector2(0,-15),new Vector2(105,35),12,Text);
        InstallButtonArt(resultPlate,"ConductivityNotTested",resultText.transform);

        var phases=Group(center,"PhaseSelectionButtons",new Vector2(0,-115),new Vector2(494,80));
        string[] names={"SOLID","MOLTEN","AQUEOUS"};
        string[] symbols={"◆","●","◉"};
        for(int i=0;i<3;i++)
        {
            var item=Group(phases,"PhaseButton_"+names[i],
                new Vector2(-166+i*166,0),new Vector2(156,77));
            phaseCards[i]=PanelImage(item,"ButtonImage",Vector2.zero,
                new Vector2(156,77),Off,Border);
            Label(item,"PhaseIcon",symbols[i],new Vector2(0,15),new Vector2(145,30),24,Gold);
            Label(item,"PhaseLabel",names[i],new Vector2(0,-21),new Vector2(150,28),17,Text);
            InstallButtonArt(phaseCards[i],"Phase"+(i==0?"Solid":i==1?"Molten":"Aqueous"),
                Find(item,"PhaseIcon"),Find(item,"PhaseLabel"));
        }

        var notes=Group(ui,"LabNotebookPanel",new Vector2(417,-41),new Vector2(252,331));
        PanelImage(notes,"PanelBackground",Vector2.zero,new Vector2(252,331),Panel,Border);
        Label(notes,"NotebookTitle","LAB NOTEBOOK",new Vector2(0,136),new Vector2(242,40),21,Gold);
        InstallHeaderArt(notes,"NotebookHeader",new Vector2(0,136),
            new Vector2(244,45),Find(notes,"NotebookTitle"));
        TMP_Text[] notebooks={noteA,noteB,noteC};
        for(int i=0;i<3;i++)
        {
            var item=Group(notes,"NotebookEntry_"+(char)('A'+i),
                new Vector2(0,61-i*93),new Vector2(228,86));
            PanelImage(item,"EntryBackground",Vector2.zero,new Vector2(228,83),Inner,Border);
            Label(item,"SampleLetter",((char)('A'+i)).ToString(),
                new Vector2(-94,0),new Vector2(26,52),19,Gold);
            notebooks[i]=Label(item,"ObservationText","S: --   M: --\nAq: N/A",
                new Vector2(17,0),new Vector2(180,69),14,Text);
            notebooks[i].alignment=TextAlignmentOptions.MidlineLeft;
        }
        noteA=notebooks[0];noteB=notebooks[1];noteC=notebooks[2];

        var bottom=Group(ui,"GameplayActionBar",new Vector2(0,-290),new Vector2(1113,89));
        PanelImage(bottom,"ActionBarPanel",Vector2.zero,new Vector2(1113,89),Panel,Border);
        var probe=ButtonFace(bottom,"TestProbeButton",
            new Vector2(-411,-4),new Vector2(255,61),"⚡ TEST PROBE",Teal);
        InstallButtonArt(probe,"TestProbe",
            Find(bottom,"TestProbeButton")!=null?
                Find(Find(bottom,"TestProbeButton"),"ButtonText"):null);
        string[] bins={"IONIC COMPOUND","COVALENT MOLECULE","METALLIC ELEMENT"};
        int[] bx={-122,137,414};
        for(int i=0;i<3;i++)
        {
            var face=ButtonFace(bottom,"ClassificationButton_"+i,
                new Vector2(bx[i],-4),new Vector2(i==2?255:244,61),bins[i],Off);
            classCards[i]=face;
            var classRoot=Find(bottom,"ClassificationButton_"+i);
            InstallButtonArt(face,i==0?"ClassIonic":i==1?"ClassCovalent":"ClassMetallic",
                Find(classRoot,"ButtonText"));
        }
        var classify=Group(ui,"CheckClassificationButton",new Vector2(418,-222),
            new Vector2(249,46));
        var classifyFace=PanelImage(classify,"ButtonImage",
            Vector2.zero,new Vector2(249,46),Teal,Gold);
        var classifyLabel=Label(classify,"ButtonText",
            "CHECK CLASSIFICATION",Vector2.zero,new Vector2(243,39),15,Text);
        InstallButtonArt(classifyFace,"CheckClassification",classifyLabel.transform);
        classStatus=Label(ui,"ClassificationFeedback","",
            new Vector2(405,-185),new Vector2(260,23),12,Gold);

        SetSelected(selected);
        SetPhase(phase);
        SetConductivity(tested,conducts);
    }

    public void SetSelected(int index)
    {
        selected=index;
        if(chosen!=null)chosen.text=index<0?"CHOOSE A SAMPLE":"SAMPLE "+(char)('A'+index);
        for(int i=0;i<3;i++)
        {
            if(sampleCards[i]==null)continue;
            if(UsingReferenceArt(sampleCards[i]))
            {
                sampleCards[i].color=Color.white;
                var frame=Find(sampleCards[i].transform.parent,"BorderImage")?.GetComponent<Image>();
                if(frame!=null)frame.color=i==index?Gold:Border;
            }
            else sampleCards[i].color=i==index?new Color32(10,179,180,255):SampleColors[i];
        }
    }
    public void SetPhase(int index)
    {
        phase=Mathf.Clamp(index,0,2);
        for(int i=0;i<3;i++)if(phaseCards[i]!=null)
        {
            if(UsingReferenceArt(phaseCards[i]))
            {
                phaseCards[i].color=Color.white;
                var frame=Find(phaseCards[i].transform.parent,"BorderImage")?.GetComponent<Image>();
                if(frame!=null)frame.color=i==phase?Gold:Border;
            }
            else phaseCards[i].color=i==phase?Teal:Off;
        }
        SetConductivity(false,false);
    }
    public void SetConductivity(bool hasTested,bool doesConduct)
    {
        tested=hasTested;conducts=doesConduct;
        if(resultPlate!=null)
        {
            string asset=!tested?"ConductivityNotTested":
                conducts?"ConductivityConducts":"ConductivityNo";
            var sprite=LoadButtonArt(asset);
            if(sprite!=null)
            {
                resultPlate.sprite=sprite;
                resultPlate.color=Color.white;
                if(resultText!=null)resultText.gameObject.SetActive(false);
            }
            else resultPlate.color=!tested?Off:
                conducts?new Color32(19,139,78,255):
                    new Color32(126,66,92,255);
        }
        if(resultText!=null && resultText.gameObject.activeSelf)
            resultText.text=!tested?"NOT TESTED":
                conducts?"CONDUCTS":"NO CONDUCTION";
        if(realBulb!=null)
        {
            var sprite=LoadButtonArt(tested&&conducts?"BulbOn":"BulbOff");
            if(sprite!=null){realBulb.sprite=sprite;realBulb.color=Color.white;}
            realBulb.preserveAspect=true;
        }
        if(realWire!=null)
        {
            var sprite=LoadButtonArt(tested&&conducts?"WireOn":"WireOff");
            if(sprite!=null){realWire.sprite=sprite;realWire.color=Color.white;}
        }
        if(bulbGlow!=null && !UsingReferenceArt(realBulb))
            bulbGlow.color=tested&&conducts?
                new Color32(255,220,62,255):Off;
    }
    public void SetNotebook(string a,string b,string c)
    {
        if(noteA!=null)noteA.text=Condense(a);
        if(noteB!=null)noteB.text=Condense(b);
        if(noteC!=null)noteC.text=Condense(c);
    }
    public void SetClassification(int sample,int bin)
    {
        if(classStatus!=null)
            classStatus.text="SAMPLE "+(char)('A'+sample)+" → "+
                (bin==0?"IONIC":bin==1?"COVALENT":"METALLIC");
        for(int i=0;i<3;i++)if(classCards[i]!=null)
        {
            if(UsingReferenceArt(classCards[i]))
            {
                classCards[i].color=Color.white;
                var frame=Find(classCards[i].transform.parent,"BorderImage")?.GetComponent<Image>();
                if(frame!=null)frame.color=i==bin?Gold:Border;
            }
            else classCards[i].color=i==bin?Teal:Off;
        }
    }
    private static string Condense(string s)
    {
        if(string.IsNullOrEmpty(s))return "S: --   M: --\nAq: N/A";
        return s.Replace("SOLID ","S: ").Replace("  MOLTEN ","   M: ")
            .Replace("\n     AQ ","\nAq: ").Replace("  Aq:","\nAq:");
    }

    private const string ButtonArtFolder="ChemQuestStage5Modular/";

    private static Sprite LoadButtonArt(string name)
    {
        var sprite=Resources.Load<Sprite>(ButtonArtFolder+name);
        if(sprite!=null)return sprite;
        // The pack works even when Unity imports the PNG as a Texture.
        var image=Resources.Load<Texture2D>(ButtonArtFolder+name);
        if(image==null)return null;
        return Sprite.Create(image,new Rect(0,0,image.width,image.height),
            new Vector2(.5f,.5f),100f);
    }

    private static bool UsingReferenceArt(Image face)
    {
        return face!=null && face.sprite!=null &&
            face.sprite.name!="Stage5Level3RoundedUI";
    }

    private static void InstallButtonArt(Image face,string spriteName,
        params Transform[] oldPlaceholderParts)
    {
        var sprite=LoadButtonArt(spriteName);
        if(sprite==null)return; // Keep functional fallback if pack is absent.
        face.sprite=sprite;
        face.color=Color.white;
        face.preserveAspect=false;
        face.type=Image.Type.Simple;
        face.raycastTarget=false; // Touch controller owns the actual clicks.
        // The PNG includes its own glossy border and silhouette; keeping the
        // procedurally generated frame caused the ugly gold rectangles.
        var outerFrame=Find(face.transform.parent,"BorderImage");
        if(outerFrame!=null)outerFrame.gameObject.SetActive(false);
        if(oldPlaceholderParts!=null)
            foreach(var part in oldPlaceholderParts)
                if(part!=null)part.gameObject.SetActive(false);
    }

    private static void InstallHeaderArt(Transform parent,string spriteName,
        Vector2 position,Vector2 size,Transform oldHeaderText)
    {
        var sprite=LoadButtonArt(spriteName);
        if(sprite==null)return;
        var header=EnsureImage(parent,spriteName+"_SeparateSprite");
        header.sprite=sprite;
        header.color=Color.white;
        header.preserveAspect=false;
        Place(header.rectTransform,position,size);
        var frame=Find(parent,"PanelBackground_Frame");
        if(frame!=null)frame.gameObject.SetActive(true);
        if(oldHeaderText!=null)oldHeaderText.gameObject.SetActive(false);
    }

    private static RectTransform Group(Transform parent,string name,Vector2 pos,Vector2 size)
    {
        var old=Find(parent,name) as RectTransform;
        if(old==null)
        {
            var obj=new GameObject(name,typeof(RectTransform));
            obj.transform.SetParent(parent,false);
            old=obj.GetComponent<RectTransform>();
        }
        old.gameObject.SetActive(true);
        Place(old,pos,size);
        return old;
    }
    private static Image PanelImage(Transform parent,string name,Vector2 pos,Vector2 size,
        Color fill,Color border)
    {
        // Every shape is a real Unity Image with an independently editable
        // RectTransform. A separate, slightly larger border sits behind it.
        var outer=Group(parent,name+"_Frame",pos,size);
        var frame=EnsureImage(outer,"BorderImage");
        frame.gameObject.SetActive(true);
        frame.color=border;
        Place(frame.rectTransform,Vector2.zero,size);
        var inner=EnsureImage(outer,name);
        inner.color=fill;
        Place(inner.rectTransform,Vector2.zero,size-new Vector2(4,4));
        return inner;
    }
    private static Image ButtonFace(Transform parent,string name,Vector2 pos,
        Vector2 size,string text,Color fill)
    {
        var item=Group(parent,name,pos,size);
        var result=PanelImage(item,"ButtonSurface",Vector2.zero,size,fill,Gold);
        Label(item,"ButtonText",text,Vector2.zero,size-new Vector2(8,8),17,Text);
        return result;
    }
    private static Image EnsureImage(Transform parent,string name)
    {
        var child=Find(parent,name);
        if(child==null)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(parent,false);
            child=go.transform;
        }
        var image=child.GetComponent<Image>();
        if(image==null)image=child.gameObject.AddComponent<Image>();
        image.raycastTarget=false;
        // Rounded corners are generated as a real Sprite, and the Stage 5
        // Bake utility persists it as an editable PNG asset in Resources.
        if(image.sprite==null)image.sprite=RoundedSprite();
        return image;
    }
    private static TMP_Text Label(Transform parent,string name,string content,
        Vector2 pos,Vector2 size,int font,Color tint)
    {
        var old=Find(parent,name);
        if(old==null)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
            go.transform.SetParent(parent,false);
            old=go.transform;
        }
        Place(old as RectTransform,pos,size);
        var label=old.GetComponent<TextMeshProUGUI>();
        label.text=content;label.fontSize=font;label.color=tint;
        label.alignment=TextAlignmentOptions.Center;
        label.enableAutoSizing=false;label.raycastTarget=false;
        label.textWrappingMode=TextWrappingModes.Normal;
        return label;
    }
    private static Sprite roundedSprite;
    private static Sprite RoundedSprite()
    {
        if(roundedSprite!=null)return roundedSprite;
        const int width=64;
        const int radius=12;
        var tex=new Texture2D(width,width,TextureFormat.RGBA32,false);
        tex.filterMode=FilterMode.Bilinear;
        var pixels=new Color32[width*width];
        for(int y=0;y<width;y++)
        for(int x=0;x<width;x++)
        {
            float dx=Mathf.Max(radius-x,0f)+Mathf.Max(x-(width-1-radius),0f);
            float dy=Mathf.Max(radius-y,0f)+Mathf.Max(y-(width-1-radius),0f);
            float a=Mathf.Clamp01(radius+0.5f-Mathf.Sqrt(dx*dx+dy*dy));
            pixels[y*width+x]=new Color(1f,1f,1f,a);
        }
        tex.SetPixels32(pixels);tex.Apply();
        roundedSprite=Sprite.Create(tex,new Rect(0,0,width,width),
            new Vector2(.5f,.5f),100f);
        roundedSprite.name="Stage5Level3RoundedUI";
        return roundedSprite;
    }

    private static void Place(RectTransform t,Vector2 pos,Vector2 size)
    {
        if(t==null)return;
        t.anchorMin=t.anchorMax=new Vector2(.5f,.5f);
        t.pivot=new Vector2(.5f,.5f);
        t.anchoredPosition=pos;t.sizeDelta=size;
        t.localScale=Vector3.one;
    }
    private static Transform Find(Transform parent,string name)
    {
        if(parent==null)return null;
        foreach(var child in parent.GetComponentsInChildren<Transform>(true))
            if(child.name==name)return child;
        return null;
    }
}
