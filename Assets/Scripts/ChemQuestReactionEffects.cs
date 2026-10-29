using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Lightweight non-interactive reaction feedback. Child canvas uses no raycaster and never blocks dragging.</summary>
public class ChemQuestReactionEffects : MonoBehaviour
{
    public int Stage=2;
    public int Level=1;
    private Canvas canvas;
    private RectTransform layer;
    private static readonly Color32 Cyan=new Color32(76,213,248,255);
    private static readonly Color32 Gold=new Color32(255,211,91,255);

    public void BuildEditableUI()
    {
        if(transform.Find("ReactionEffectsCanvas")!=null)return;
        var root=new GameObject("ReactionEffectsCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        root.transform.SetParent(transform,false);
        canvas=root.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=80;
        var scale=root.GetComponent<CanvasScaler>();
        scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scale.referenceResolution=new Vector2(1280,720);
        scale.matchWidthOrHeight=1f;
        layer=root.GetComponent<RectTransform>();
        var title=CreateText(layer,"ReactionAnnouncement","",new Vector2(0,110),new Vector2(700,70),30);
        title.color=Gold;
        title.gameObject.SetActive(false);
    }
    private void Awake()
    {
        BuildEditableUI();
        canvas=transform.Find("ReactionEffectsCanvas").GetComponent<Canvas>();
        layer=canvas.GetComponent<RectTransform>();
    }
    private static Image Dot(Transform parent,string name,Vector2 pos,float diameter,Color color)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(Image));
        obj.transform.SetParent(parent,false);
        var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
        r.anchoredPosition=pos;r.sizeDelta=Vector2.one*diameter;
        var image=obj.GetComponent<Image>();image.color=color;image.raycastTarget=false;
        return image;
    }
    private static TMP_Text CreateText(Transform parent,string name,string value,Vector2 pos,Vector2 size,int font)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchoredPosition=pos;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.fontSize=font;
        t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;
        return t;
    }
    public void ElectronTransfer()
    {
        // A visual celebration after an accepted drop; no change to actual electron state.
        StartCoroutine(Flight(new Vector2(-265,55),new Vector2(280,55),Gold));
    }
    public void IonSnap()=>StartCoroutine(FlashAt(Vector2.zero,Cyan,"ION PLACED"));
    public void BondFormed(string formula)=>StartCoroutine(FlashAt(Vector2.zero,Gold,formula+"  •  BOND FORMED!"));
    public void ExperimentFeedback(string message)=>StartCoroutine(FlashAt(new Vector2(0,45),Cyan,message));
    private IEnumerator Flight(Vector2 from,Vector2 to,Color color)
    {
        var dot=Dot(layer,"ElectronTransferTrail",from,17,color);
        float time=0,seconds=.55f;
        while(time<seconds)
        {
            time+=Time.unscaledDeltaTime;
            float t=Mathf.Clamp01(time/seconds);
            float eased=t*t*(3-2*t);
            dot.rectTransform.anchoredPosition=Vector2.Lerp(from,to,eased)+
                Vector2.up*Mathf.Sin(t*Mathf.PI)*65;
            dot.color=new Color(color.r,color.g,color.b,1f-.2f*t);
            yield return null;
        }
        Destroy(dot.gameObject);
        yield return FlashAt(to,Cyan,"ELECTRON TRANSFERRED");
    }
    private IEnumerator FlashAt(Vector2 pos,Color color,string message)
    {
        var caption=layer.Find("ReactionAnnouncement")?.GetComponent<TMP_Text>();
        if(caption!=null)
        {
            caption.text=message;caption.color=color;caption.gameObject.SetActive(true);
        }
        var particles=new Image[9];
        for(int i=0;i<particles.Length;i++)
            particles[i]=Dot(layer,"ReactionSpark"+i,pos,9,color);
        float elapsed=0f;
        while(elapsed<.65f)
        {
            elapsed+=Time.unscaledDeltaTime;
            float t=Mathf.Clamp01(elapsed/.65f);
            for(int i=0;i<particles.Length;i++)
            {
                float angle=i*Mathf.PI*2/particles.Length;
                particles[i].rectTransform.anchoredPosition=pos+
                    new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*t*110;
                particles[i].color=new Color(color.r,color.g,color.b,1-t);
            }
            if(caption!=null)caption.alpha=1-t;
            yield return null;
        }
        foreach(var p in particles)if(p!=null)Destroy(p.gameObject);
        if(caption!=null){caption.gameObject.SetActive(false);caption.alpha=1;}
    }
}
