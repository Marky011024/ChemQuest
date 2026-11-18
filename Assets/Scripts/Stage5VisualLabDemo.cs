using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Visual-only, lightweight animated workbench for Stage 5 Levels 1-3.
// Displays scientific observations using Unity UI primitives in the existing
// drop station. No scene reserialization, resources, physics or answer changes.
public sealed class Stage5VisualLabDemo : MonoBehaviour
{
    private int level, sample = -1, phase;
    private float elapsed, animation, heatSetting;
    private bool heatTested, mechanicallyTested, pressed, probed, conducting;
    private RectTransform station, layer;
    private Image sampleSolid, sampleLiquid, burner, flame, tool, lamp, bulbHalo, beaker, liquid, leftProbe, rightProbe;
    private Image[] shards, ions, waves;
    private TMP_Text status;
    private Sprite disc;
    private static readonly Color Cyan = new Color(.25f,.83f,1f,1f);
    private static readonly Color Gold = new Color(1f,.75f,.31f,1f);
    private static readonly Color Soft = new Color(.73f,.82f,.95f,1f);
    private static readonly Color Slate = new Color(.30f,.42f,.62f,1f);
    private static readonly Color[] SampleColors = {
        new Color(.66f,.84f,.98f,1f), new Color(.79f,.70f,.97f,1f),
        new Color(.59f,.90f,.84f,1f)
    };
    private bool ready;

    public void Initialize(int sceneLevel, RectTransform testStation)
    {
        if (testStation == null) return;
        level = Mathf.Clamp(sceneLevel,1,3);
        station = testStation;
        Build();
        SelectSample(-1);
        ready = true;
    }

    private Sprite CircleSprite()
    {
        var tex = new Texture2D(64,64,TextureFormat.RGBA32,false);
        tex.filterMode=FilterMode.Bilinear;
        for(int y=0;y<64;y++)
        for(int x=0;x<64;x++)
        {
            float distance = Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));
            float a=Mathf.Clamp01((31f-distance)*.85f);
            tex.SetPixel(x,y,new Color(1,1,1,a));
        }
        tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100f);
    }

    private Image Shape(string name, Transform parent, Vector2 pos, Vector2 size, Color color, bool round=false)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform;
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        var img=go.GetComponent<Image>();
        img.color=color;img.raycastTarget=false;
        if(round)img.sprite=disc;
        return img;
    }

    private TMP_Text Text(string name, Transform parent, Vector2 pos, Vector2 size, int font, Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform;
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        var label=go.GetComponent<TextMeshProUGUI>();
        label.text="";label.color=color;label.fontSize=font;
        label.alignment=TextAlignmentOptions.Center;
        label.textWrappingMode=TextWrappingModes.Normal;
        label.raycastTarget=false;
        return label;
    }

    private void Build()
    {
        disc=CircleSprite();
        layer=Shape("AnimatedExperimentDemonstration",station,Vector2.zero,
            new Vector2(430,225),new Color(0,0,0,0)).rectTransform;
        // Scale the animation to the station size across all three saved scenes.
        float factor=Mathf.Min(1f, Mathf.Min(station.rect.width / 430f,
            station.rect.height / 260f));
        layer.localScale=Vector3.one * Mathf.Max(.54f,factor);
        status=Text("LiveVisualState",layer,new Vector2(0,89),new Vector2(390,30),17,Gold);

        // The central test bench serves as a physical reference in all levels.
        burner=Shape("HeaterOrTestBench",layer,new Vector2(0,-62),new Vector2(192,17),Slate);
        Shape("BenchTrim",layer,new Vector2(0,-55),new Vector2(194,4),Cyan);
        flame=Shape("HeaterGlow",layer,new Vector2(0,-73),new Vector2(140,22),
            new Color(1f,.35f,.08f,.7f),true);
        sampleSolid=Shape("SolidMaterialSample",layer,new Vector2(0,-9),
            new Vector2(90,72),Soft);
        sampleLiquid=Shape("MeltingMaterialPuddle",layer,new Vector2(0,-45),
            new Vector2(120,23),new Color(.89f,.95f,1f,.94f),true);
        tool=Shape("MechanicalTool",layer,new Vector2(0,74),
            new Vector2(124,12),new Color(.84f,.88f,.95f,1f));
        shards=new Image[7];
        for(int i=0;i<shards.Length;i++)
            shards[i]=Shape("ImpactFragment"+i,layer,new Vector2(0,-15),
                new Vector2(22,20),SampleColors[1]);

        waves=new Image[3];
        for(int i=0;i<waves.Length;i++)
            waves[i]=Shape("HeatWave"+i,layer,new Vector2(-44+i*44,27),
                new Vector2(12,37),new Color(1f,.56f,.15f,.3f),true);

        // Conductivity model: beaker, phase, two electrodes, bulb and moving ions.
        beaker=Shape("GlassBeakerOutline",layer,new Vector2(0,-24),
            new Vector2(178,94),new Color(.28f,.72f,1f,.45f));
        liquid=Shape("TestMedium",layer,new Vector2(0,-41),
            new Vector2(163,54),new Color(.12f,.66f,.90f,.75f));
        leftProbe=Shape("LeftProbe",layer,new Vector2(-48,0),
            new Vector2(10,96),new Color(.92f,.36f,.26f,1f));
        rightProbe=Shape("RightProbe",layer,new Vector2(48,0),
            new Vector2(10,96),new Color(.36f,.70f,1f,1f));
        Shape("WireLeft",layer,new Vector2(-55,58),
            new Vector2(85,6),new Color(.92f,.54f,.34f,1));
        Shape("WireRight",layer,new Vector2(55,58),
            new Vector2(85,6),new Color(.53f,.79f,1f,1));
        bulbHalo=Shape("BulbLight",layer,new Vector2(0,70),
            new Vector2(66,66),new Color(1f,.84f,.2f,.20f),true);
        lamp=Shape("ConductivityLamp",layer,new Vector2(0,70),
            new Vector2(34,34),new Color(.38f,.46f,.57f,1f),true);
        Shape("LampBase",layer,new Vector2(0,47),
            new Vector2(25,11),Slate);
        ions=new Image[9];
        for(int i=0;i<ions.Length;i++)
            ions[i]=Shape("MovingCharge"+i,layer,new Vector2(-61+(i%3)*61,-60+(i/3)*20),
                new Vector2(18,18),i%2==0?new Color(.36f,1f,.51f,1f):new Color(.82f,.43f,1f,1f),true);
        ArrangeLevel();
    }

    private void ArrangeLevel()
    {
        bool thermal=level==1,mechanical=level==2,electric=level==3;
        burner.gameObject.SetActive(!electric);
        flame.gameObject.SetActive(thermal);
        sampleSolid.gameObject.SetActive(!electric && sample>=0);
        sampleLiquid.gameObject.SetActive(false);
        tool.gameObject.SetActive(mechanical);
        foreach(var shard in shards) shard.gameObject.SetActive(false);
        foreach(var wave in waves) wave.gameObject.SetActive(thermal && sample>=0);
        foreach(var img in new[]{beaker,liquid,leftProbe,rightProbe,bulbHalo,lamp})
            img.gameObject.SetActive(electric);
        foreach(var ion in ions) ion.gameObject.SetActive(false);
        if(electric)
        {
            lamp.color=Slate;bulbHalo.color=new Color(1f,.86f,.25f,0);
            liquid.color=new Color(.24f,.60f,.81f,.30f);
        }
    }

    public void SelectSample(int index)
    {
        sample=index;
        animation=0;
        heatTested=mechanicallyTested=probed=false;
        heatSetting=0;
        pressed=false;
        conducting=false;
        ArrangeLevel();
        if(sampleSolid!=null && index>=0)
        {
            sampleSolid.color=SampleColors[Mathf.Clamp(index,0,2)];
            sampleSolid.rectTransform.sizeDelta=new Vector2(86,70);
            sampleSolid.rectTransform.anchoredPosition=new Vector2(0,-9);
        }
        if(status!=null)
            status.text=index<0?"DRAG A SAMPLE TO START":"SAMPLE "+(char)('A'+index)+" - READY";
    }

    public void SetHeat(float sliderValue)
    {
        heatSetting=Mathf.Clamp01(sliderValue/100f);
        if(level!=1 || sample<0 || heatTested) return;
        flame.color=new Color(1f,.30f+heatSetting*.35f,.05f,.1f+heatSetting*.9f);
        status.text="HEAT POWER  "+Mathf.RoundToInt(sliderValue)+"%";
    }

    public void HeatTest(float sliderValue)
    {
        if(!ready||level!=1||sample<0)return;
        heatSetting=Mathf.Clamp01(sliderValue/100f);
        animation=0;
        heatTested=true;
        status.text=sample==0?"HEATING: SAMPLE SOFTENS...":"HEATING: SOLID REMAINS INTACT";
    }

    public void MechanicalTest(bool usePress)
    {
        if(!ready||level!=2||sample<0)return;
        pressed=usePress;
        mechanicallyTested=true;
        animation=0;
        status.text=usePress?"HYDRAULIC PRESS TEST":"IMPACT / STRIKE TEST";
    }

    public void SetPhase(int nextPhase)
    {
        if(!ready||level!=3)return;
        phase=Mathf.Clamp(nextPhase,0,2);
        probed=false;conducting=false;animation=0;
        status.text=phase==0?"SOLID STATE - TEST THE PROBE":
            phase==1?"MOLTEN STATE - TEST THE PROBE":"AQUEOUS STATE - TEST THE PROBE";
        RefreshElectricPhase();
    }

    public void ProbeTest(bool isConducting)
    {
        if(!ready||level!=3||sample<0)return;
        probed=true;conducting=isConducting;animation=0;
        status.text=conducting?"BULB ON  -  CHARGES MOVE":"BULB OFF  -  NO CHARGE FLOW";
        RefreshElectricPhase();
    }

    private void RefreshElectricPhase()
    {
        if(level!=3||lamp==null)return;
        bool available=sample>=0;
        liquid.enabled=available;
        liquid.color=phase==0?
            new Color(.29f,.67f,.94f,.30f):
            phase==1?new Color(.96f,.61f,.23f,.66f):
            new Color(.20f,.72f,.94f,.65f);
        lamp.color=probed&&conducting?Gold:Slate;
        bulbHalo.color=probed&&conducting?new Color(1f,.83f,.18f,.37f):new Color(1f,.83f,.18f,0f);
        foreach(var ion in ions)
            ion.gameObject.SetActive(available && probed && conducting && phase!=0 && sample==1);
    }

    private void Update()
    {
        if(!ready||station==null||sample<0)return;
        elapsed+=Time.deltaTime;
        animation+=Time.deltaTime;
        if(level==1)AnimateThermal();
        else if(level==2)AnimateMechanical();
        else AnimateElectric();
    }

    private void AnimateThermal()
    {
        float amount=heatTested?Mathf.SmoothStep(0,1,Mathf.Clamp01(animation/2.2f)):0f;
        flame.color=new Color(1f,.31f+.45f*heatSetting,.08f,
            heatTested?.75f+.2f*Mathf.Sin(elapsed*7f):.16f+heatSetting*.65f);
        flame.rectTransform.sizeDelta=new Vector2(135+Mathf.Sin(elapsed*6f)*8,20+heatSetting*8);
        for(int i=0;i<waves.Length;i++)
        {
            var r=waves[i].rectTransform;
            r.anchoredPosition=new Vector2(-45+i*45+Mathf.Sin(elapsed*3f+i)*8,
                25+Mathf.PingPong(elapsed*18+i*12,27));
            waves[i].color=new Color(1f,.57f,.2f,.18f+heatSetting*.30f);
        }
        if(heatTested && sample==0)
        {
            sampleSolid.rectTransform.sizeDelta=new Vector2(88+amount*6,70*(1-amount)+3);
            sampleSolid.rectTransform.anchoredPosition=new Vector2(0,-11-amount*31);
            sampleSolid.color=Color.Lerp(SampleColors[0],new Color(.92f,.96f,1f,.45f),amount);
            sampleLiquid.gameObject.SetActive(true);
            sampleLiquid.rectTransform.sizeDelta=new Vector2(20+amount*156,10+amount*18);
            sampleLiquid.color=new Color(.82f,.94f,1f,.20f+amount*.72f);
            if(amount>=.99f)status.text="SAMPLE A: MELTED INTO LIQUID";
        }
        else if(heatTested)
        {
            sampleSolid.color=Color.Lerp(SampleColors[sample],new Color(1f,.72f,.45f,1f),
                Mathf.Sin(elapsed*4f)*.10f+.18f*amount);
            if(amount>=.99f)status.text="SAMPLE "+(char)('A'+sample)+": STILL SOLID";
        }
    }

    private void AnimateMechanical()
    {
        if(!mechanicallyTested)
        {
            sampleSolid.rectTransform.sizeDelta=new Vector2(86,70);
            tool.rectTransform.anchoredPosition=new Vector2(0,76);
            return;
        }
        float t=Mathf.Clamp01(animation/.55f);
        tool.rectTransform.sizeDelta=pressed?new Vector2(140,25):new Vector2(103,15);
        tool.rectTransform.anchoredPosition=new Vector2(0,76-(pressed?82:55)*Mathf.Sin(t*Mathf.PI*.5f));
        if(animation>1.0f)tool.rectTransform.anchoredPosition=new Vector2(0,72);
        if(sample==1 && !pressed) // ionic teaching sample: brittle fracture
        {
            float f=Mathf.Clamp01((animation-.38f)/.75f);
            sampleSolid.rectTransform.sizeDelta=new Vector2(86*(1-f),70*(1-f));
            for(int i=0;i<shards.Length;i++)
            {
                shards[i].gameObject.SetActive(f>.01f);
                float angle=i*Mathf.PI*2/shards.Length;
                var r=shards[i].rectTransform;
                r.anchoredPosition=new Vector2(Mathf.Cos(angle)*f*75,-18+Mathf.Sin(angle)*f*35);
                r.localEulerAngles=new Vector3(0,0,angle*Mathf.Rad2Deg+elapsed*30f);
                shards[i].color=SampleColors[1];
            }
            if(animation>1.2f)status.text="CRACKS AND SHATTERS - BRITTLE";
        }
        else if(sample==2 && pressed) // metallic teaching sample: malleable
        {
            float f=Mathf.SmoothStep(0,1,Mathf.Clamp01((animation-.20f)/.85f));
            sampleSolid.rectTransform.sizeDelta=new Vector2(86+f*99,70-f*49);
            sampleSolid.rectTransform.anchoredPosition=new Vector2(0,-9-f*26);
            if(animation>1.2f)status.text="FLATTENS WITHOUT CRACKING";
        }
        else if(sample==0 && pressed)
        {
            float f=Mathf.SmoothStep(0,1,Mathf.Clamp01((animation-.2f)/.85f));
            sampleSolid.rectTransform.sizeDelta=new Vector2(86+f*35,70-f*44);
            sampleSolid.rectTransform.anchoredPosition=new Vector2(0,-9-f*21);
            if(animation>1.2f)status.text="SOFT MATERIAL COMPRESSES";
        }
        else if(animation>1.2f)
            status.text=sample==1?"RESISTS PRESSING":"NO SHATTERING OBSERVED";
    }

    private void AnimateElectric()
    {
        if(lamp==null)return;
        if(probed&&conducting)
        {
            bulbHalo.color=new Color(1f,.81f,.12f,.25f+.18f*(.5f+.5f*Mathf.Sin(elapsed*6f)));
            lamp.color=Color.Lerp(Gold,Color.white,.20f+.15f*Mathf.Sin(elapsed*5f));
        }
        if(probed&&conducting&&sample==1&&phase!=0)
        {
            for(int i=0;i<ions.Length;i++)
            {
                var r=ions[i].rectTransform;
                float x=-62+Mathf.Repeat(elapsed*22+i*22,124);
                r.anchoredPosition=new Vector2(x,-59+(i%3)*19+
                    Mathf.Sin(elapsed*2f+i)*4);
            }
        }
    }

    private void OnDestroy()
    {
        if(disc!=null&&disc.texture!=null)Destroy(disc.texture);
        if(disc!=null)Destroy(disc);
    }
}
