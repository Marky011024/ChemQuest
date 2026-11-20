using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Visual only. The drop-zone script remains responsible for accepting reagents,
// starting the chemical reaction, temperature, scores and questions.
public sealed class ChemQuestReagentPourEffect : MonoBehaviour
{
    [SerializeField] float duration = 1.2f;
    [SerializeField] int particleCount = 15;

    public IEnumerator Play(Sprite sprite, Vector3 sourceWorld, bool sodium)
    {
        var canvas = GetComponentInParent<Canvas>();
        var target = GetComponent<RectTransform>();
        if (canvas == null || target == null || sprite == null) yield break;
        var canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 a, b;
        var source = RectTransformUtility.WorldToScreenPoint(cam, sourceWorld);
        var destination = RectTransformUtility.WorldToScreenPoint(cam, target.TransformPoint(target.rect.center));
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, source, cam, out a) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, destination, cam, out b)) yield break;

        var layer = new GameObject("PourEffect_Runtime", typeof(RectTransform));
        layer.transform.SetParent(canvas.transform, false);
        layer.transform.SetAsLastSibling();
        var body = new GameObject("TiltingReagentBottle",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        body.transform.SetParent(layer.transform, false);
        var rt = (RectTransform)body.transform;
        rt.sizeDelta = new Vector2(100,140);
        var img = body.GetComponent<Image>();
        img.sprite=sprite; img.preserveAspect=true; img.raycastTarget=false;

        var particles = new RectTransform[particleCount];
        var graphics = new Image[particleCount];
        for(int i=0; i<particleCount; i++)
        {
            var g = new GameObject("FallingReagent_"+(i+1),typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            g.transform.SetParent(layer.transform,false);
            particles[i]=(RectTransform)g.transform;
            particles[i].sizeDelta = new Vector2(7,8);
            graphics[i]=g.GetComponent<Image>();
            graphics[i].raycastTarget=false;
        }
        float elapsed=0;
        Vector2 hover=b+new Vector2(sodium?82:-82,106);
        while(elapsed<duration)
        {
            elapsed+=Time.deltaTime;
            float t=Mathf.Clamp01(elapsed/Mathf.Max(.1f,duration));
            rt.anchoredPosition=Vector2.Lerp(a,hover,Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.33f)));
            float tilt=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.26f)/.23f));
            rt.localRotation=Quaternion.Euler(0,0,(sodium?1:-1)*62*tilt);
            img.color=new Color(1,1,1,t>.86f?(1-t)/.14f:1);
            for(int i=0;i<particleCount;i++)
            {
                float start=.39f+i*.025f;
                float p=Mathf.Clamp01((t-start)/.31f);
                graphics[i].color=sodium?new Color(.95f,.96f,1f,t>=start&&p<1?1:0):
                    new Color(.18f,.84f,1f,t>=start&&p<1?1:0);
                if(t<start||p>=1)continue;
                var mouth=hover+new Vector2(sodium?-34:34,10);
                var end=b+new Vector2(((i*17)%19-9)*2,0);
                particles[i].anchoredPosition=Vector2.Lerp(mouth,end,p)+Vector2.up*24*p*(1-p);
            }
            yield return null;
        }
        Destroy(layer);
    }
}
