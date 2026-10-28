using UnityEngine;
using UnityEngine.UI;

// Editable Bohr diagram matching ChemQuest Stage 2 manuscript:
// thin concentric orbits, consistent electron spacing, filled nucleus.
[RequireComponent(typeof(RectTransform),typeof(CanvasRenderer))]
public class Stage2BohrAtomView : MaskableGraphic
{
    [SerializeField] private int[] electronsPerShell = {2,8,1};
    private const int Segments = 96;
    private static readonly Color32 OrbitColor = new Color32(127,182,225,225);
    private static readonly Color32 ElectronColor = new Color32(255,219,107,255);
    private static readonly Color32 CoreColor = new Color32(26,72,112,255);
    private static readonly Color32 GlowColor = new Color32(68,178,220,42);

    public void SetShells(params int[] shells)
    {
        electronsPerShell = shells;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if(electronsPerShell == null || electronsPerShell.Length==0)return;
        int count=electronsPerShell.Length;
        // Both three- and four-shell atoms remain within a 170x170 drawing.
        float step=count>=4?19f:24.5f;
        float core=14f;
        Disc(vh,Vector2.zero,core+3f,GlowColor,48);
        Disc(vh,Vector2.zero,core,CoreColor,48);
        for(int shell=0;shell<count;shell++)
        {
            float r=(shell+1)*step;
            Ring(vh,r,1.05f,OrbitColor);
            int n=Mathf.Clamp(electronsPerShell[shell],0,32);
            // Shell with no electrons is omitted visually: no empty 4th orbit
            // when Na/K/Mg has donated its outer electron(s).
            if(n==0)continue;
            float phase=shell%2==0?Mathf.PI/2f:Mathf.PI/2f+0.22f;
            for(int j=0;j<n;j++)
            {
                float angle=2*Mathf.PI*j/n + phase;
                Vector2 center=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;
                Disc(vh,center,5.7f,CoreColor,20);
                Disc(vh,center,3.75f,ElectronColor,20);
            }
        }
    }

    private static void Ring(VertexHelper vh,float radius,float thickness,Color32 color)
    {
        for(int j=0;j<Segments;j++)
        {
            float a=j*2*Mathf.PI/Segments,b=(j+1)*2*Mathf.PI/Segments;
            Vector2 va=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
            Vector2 vb=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
            Quad(vh,va*(radius-thickness),va*(radius+thickness),
                vb*(radius+thickness),vb*(radius-thickness),color);
        }
    }
    private static void Disc(VertexHelper vh,Vector2 center,float radius,Color32 color,int steps)
    {
        int start=vh.currentVertCount;
        vh.AddVert(center,color,Vector2.zero);
        for(int i=0;i<=steps;i++)
        {
            float a=2*Mathf.PI*i/steps;
            vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);
        }
        for(int i=0;i<steps;i++)vh.AddTriangle(start,start+i+1,start+i+2);
    }
    private static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 color)
    {
        int start=vh.currentVertCount;
        vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);
        vh.AddVert(c,color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);
        vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
    }
}
