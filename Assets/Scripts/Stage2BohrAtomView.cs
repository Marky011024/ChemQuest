using UnityEngine;
using UnityEngine.UI;

// A lightweight, editable Unity UI Bohr diagram. No external sprite required.
// Shows occupied shells and valence changes during ionic electron transfer.
[RequireComponent(typeof(RectTransform),typeof(CanvasRenderer))]
public class Stage2BohrAtomView : MaskableGraphic
{
    [SerializeField] private int[] electronsPerShell = {2,8,1};
    private const int Steps=56;
    private const float Width=1.1f;
    public void SetShells(params int[] shells)
    {
        electronsPerShell=shells;
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if(electronsPerShell==null)return;
        float radiusStep=15.5f;
        int count=electronsPerShell.Length;
        var rings=new Color32(120,190,230,205);
        var electron=new Color32(255,207,85,255);
        for(int s=0;s<count;s++)
        {
            float radius=(s+1)*radiusStep;
            for(int i=0;i<Steps;i++)
            {
                float a=i*2*Mathf.PI/Steps;
                float b=(i+1)*2*Mathf.PI/Steps;
                var directionA=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                var directionB=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                AddQuad(vh,directionA*(radius-Width),directionA*(radius+Width),
                    directionB*(radius+Width),directionB*(radius-Width),rings);
            }
            int e=Mathf.Clamp(electronsPerShell[s],0,32);
            for(int j=0;j<e;j++)
            {
                float a=(j+0.5f)*2*Mathf.PI/e + Mathf.PI*.5f;
                AddCircle(vh,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,3.3f,electron);
            }
        }
    }
    private static void AddQuad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color32 tint)
    {
        int n=vh.currentVertCount;
        vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);
        vh.AddVert(c,tint,Vector2.zero);vh.AddVert(d,tint,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
    private static void AddCircle(VertexHelper vh,Vector2 center,float radius,Color32 tint)
    {
        const int sides=12;
        int n=vh.currentVertCount;
        vh.AddVert(center,tint,Vector2.zero);
        for(int k=0;k<=sides;k++)
        {
            float a=k*2*Mathf.PI/sides;
            vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,tint,Vector2.zero);
        }
        for(int k=0;k<sides;k++)vh.AddTriangle(n,n+1+k,n+2+k);
    }
}
