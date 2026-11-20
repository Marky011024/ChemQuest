using UnityEngine;

// One shared sprite source for Stage 5 Levels 1-3. The same material cube
// follows the finger/mouse and appears in the heater, mechanical bench and
// conductivity cell. Optional polished PNGs can override these safe defaults:
// Assets/Resources/ChemQuestStage5MaterialCubes/ParaffinWax.png,
// SodiumChloride.png, Copper.png (Sprite 2D/UI).
public static class Stage5MaterialCubeSprites
{
    private static readonly string[] Names = { "ParaffinWax", "SodiumChloride", "Copper" };
    private static readonly Sprite[] Cached = new Sprite[3];

    public static Sprite Get(int index)
    {
        if(index < 0 || index >= Cached.Length) return null;
        if(Cached[index] != null) return Cached[index];

        var path = "ChemQuestStage5MaterialCubes/" + Names[index];
        var imported = Resources.Load<Sprite>(path);
        if(imported != null) return Cached[index] = imported;

        // Do not depend on old atlas sub-sprite rects or raw texture loading,
        // which previously caused VirtualArtifacts / import errors.
        return Cached[index] = GenerateCube(index);
    }

    private static Sprite GenerateCube(int index)
    {
        const int size=96;
        var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
        tex.name="Stage5_"+Names[index]+"_Cube";
        tex.filterMode=FilterMode.Bilinear;
        tex.wrapMode=TextureWrapMode.Clamp;
        Color baseColor=index==0?new Color(.94f,.88f,.74f):
                        index==1?new Color(.84f,.76f,.98f):
                                 new Color(.83f,.38f,.18f);
        Color top=Color.Lerp(baseColor,Color.white,index==2?.24f:.50f);
        Color left=Color.Lerp(baseColor,Color.white,.12f);
        Color right=Color.Lerp(baseColor,Color.black,index==2?.33f:.22f);
        for(int y=0;y<size;y++)
        for(int x=0;x<size;x++)
        {
            float u=(x+.5f)/size,v=(y+.5f)/size;
            Color pixel=Color.clear;
            // Isometric cube with distinct top, left and right faces.
            // Shape vertices (image-space, y increases up):
            // top diamond: (0.50,.91) (.84,.74) (.50,.59) (.16,.74)
            // bottom cube: left (.16,.74),(.50,.59),(.50,.17),(.16,.34)
            // right (.50,.59),(.84,.74),(.84,.34),(.50,.17)
            bool topFace = v>=.59f && v<=.91f &&
                Mathf.Abs(u-.5f)<=.34f*(1f-Mathf.Abs(v-.75f)/.16f);
            bool leftFace = u>=.16f && u<=.50f &&
                v>=.34f-.5f*(u-.16f) && v<=.74f-.5f*(u-.16f);
            bool rightFace = u>.50f && u<=.84f &&
                v>=.17f+.5f*(u-.50f) && v<=.59f+.5f*(u-.50f);
            if(topFace)pixel=top;
            else if(leftFace)pixel=left;
            else if(rightFace)pixel=right;
            if(pixel.a>0)
            {
                float edge= (topFace && Mathf.Abs(v-.75f)>.122f) ? .10f:0f;
                pixel=Color.Lerp(pixel,Color.black,edge);
                pixel.a=1;
            }
            tex.SetPixel(x,y,pixel);
        }
        tex.Apply(false,false);
        return Sprite.Create(tex,new Rect(0,0,size,size),
            new Vector2(.5f,.5f),100f);
    }
}
