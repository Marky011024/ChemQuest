using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Stage4LevelSelectionManager : MonoBehaviour
{
    private static readonly Color Back=new Color32(10,22,47,255);
    private static readonly Color Panel=new Color32(23,44,76,255);
    private static readonly Color Blue=new Color32(56,189,248,255);

    private void Start()
    {
        var root=GameObject.Find("Stage4LevelSelectionCanvas");
        if(root==null)BuildEditorScene();
        Bind();
    }

    public void BuildEditorScene()
    {
        if(GameObject.Find("Stage4LevelSelectionCanvas")!=null)return;
        var go=new GameObject("Stage4LevelSelectionCanvas",typeof(RectTransform),typeof(Canvas),
            typeof(CanvasScaler),typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1;
        var background=CreatePanel(go.transform,"Background",Vector2.zero,new Vector2(1280,720),Back);
        var bgr=background.GetComponent<RectTransform>();
        bgr.anchorMin=Vector2.zero;bgr.anchorMax=Vector2.one;bgr.offsetMin=Vector2.zero;bgr.offsetMax=Vector2.zero;
        Text(go.transform,"Heading","STAGE 4 — ELECTRON OCEAN",32,
            new Vector2(0,267),new Vector2(850,70),Blue);
        Text(go.transform,"Subtitle","METALLIC BONDING AND THE SEA OF DELOCALIZED ELECTRONS",20,
            new Vector2(0,208),new Vector2(1050,48),Color.white);
        string[] names={"LEVEL 1  •  LATTICE & CLOUD","LEVEL 2  •  CONDUCTIVITY",
            "LEVEL 3  •  MALLEABILITY"};
        for(int i=0;i<3;i++)
        {
            var card=CreatePanel(go.transform,"Level"+(i+1)+"Card",
                new Vector2(-365+i*365,-25),new Vector2(333,320),Panel);
            Text(card.transform,"LevelTitle",names[i],20,new Vector2(0,90),
                new Vector2(318,72),Blue);
            Text(card.transform,"Description",i==0?"Drag Cu or Mg metal cores into the lattice and add electrons.":
                i==1?"Connect terminals and demonstrate electrical conductivity.":
                "Apply force and observe how metal layers slide.",
                17,new Vector2(0,-2),new Vector2(285,102),Color.white);
            var button=CreatePanel(card.transform,"Level"+(i+1)+"SelectButton",
                new Vector2(0,-110),new Vector2(220,60),new Color32(11,131,138,255));
            button.AddComponent<Button>();
            Text(button.transform,"ButtonLabel","SELECT",20,Vector2.zero,new Vector2(210,55),Color.white);
        }
        var back=CreatePanel(go.transform,"BackButton",new Vector2(-515,310),
            new Vector2(155,48),new Color32(68,86,117,255));
        back.AddComponent<Button>();
        Text(back.transform,"BackLabel","BACK",20,Vector2.zero,new Vector2(145,42),Color.white);
    }

    private GameObject CreatePanel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.anchoredPosition=pos;rect.sizeDelta=size;
        go.GetComponent<Image>().color=color;
        return go;
    }
    private void Text(Transform parent,string name,string content,int font,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        var label=go.GetComponent<TextMeshProUGUI>();
        label.text=content;label.fontSize=font;label.color=color;
        label.textWrappingMode=TextWrappingModes.Normal;
        label.alignment=TextAlignmentOptions.Center;
        label.raycastTarget=false;
    }

    private void Bind()
    {
        foreach(var button in FindObjectsOfType<Button>(true))
        {
            for(int i=1;i<=3;i++)
            {
                if(button.name!="Level"+i+"SelectButton")continue;
                int level=i;
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(()=>Open(level));
                button.interactable=i==1 ||
                    PlayerPrefs.GetInt("Stage4Level"+(i-1)+"Completed",0)==1;
                var tmp=button.GetComponentInChildren<TMP_Text>(true);
                if(tmp!=null)tmp.text=button.interactable?"SELECT":"LOCKED";
            }
            if(button.name=="BackButton")
            {
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(()=>Navigate("StageSelection"));
            }
        }
    }
    private void Open(int level)
    {
        if(level>1 && PlayerPrefs.GetInt("Stage4Level"+(level-1)+"Completed",0)!=1)return;
        Navigate("Stage4Level"+level);
    }
    private void Navigate(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {Debug.LogError("Stage 4: Scene not in Build Settings: "+scene);return;}
        SceneManager.LoadScene(scene);
    }
}
