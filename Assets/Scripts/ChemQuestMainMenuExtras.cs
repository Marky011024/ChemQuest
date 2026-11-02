using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Non-destructive MainMenu augmentation. Keeps ENTER ACADEMY and existing scene art.
public class ChemQuestMainMenuExtras : MonoBehaviour
{
    private static readonly Color Navy=new Color32(13,38,64,244);
    private static readonly Color Cyan=new Color32(30,141,163,255);
    private static readonly Color Gold=new Color32(248,201,102,255);
    private GameObject settingsPanel;
    private TMP_Text feedback;

    private void Start()
    {
        Build();
        ChemQuestAudioSettings.Apply();
    }
    public void Build()
    {
        // Dedicated overlay prevents UI from being hidden behind the illustrated menu.
        var overlay=GameObject.Find("ChemQuestMenuOverlay");
        if(overlay==null)
        {
            overlay=new GameObject("ChemQuestMenuOverlay",typeof(RectTransform),
                typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var overlayCanvas=overlay.GetComponent<Canvas>();
            overlayCanvas.renderMode=RenderMode.ScreenSpaceOverlay;
            overlayCanvas.overrideSorting=true;
            overlayCanvas.sortingOrder=200;
            var scaler=overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);
            scaler.matchWidthOrHeight=1f;
        }
        if(overlay.transform.Find("ChemQuestQuickNavigation")!=null)
        {
            BindExisting(overlay.transform);
            return;
        }
        var root=RectObject(overlay.transform,"ChemQuestQuickNavigation",new Vector2(0,45),
            new Vector2(1050,64));
        root.anchorMin=new Vector2(.5f,0);
        root.anchorMax=new Vector2(.5f,0);
        root.anchoredPosition=new Vector2(0,43);
        // Keep the existing ENTER ACADEMY button: quick actions are separate.
        AddButton(root.transform,"StageSelectionQuickButton","STAGE SELECTION",new Vector2(-336,0),
            new Vector2(280,60),()=>Open("StageSelection"));
        AddButton(root.transform,"CertificateQuickButton","MY CERTIFICATE",new Vector2(0,0),
            new Vector2(280,60),()=>Open("ChemQuestCertificate"));
        AddButton(root.transform,"SettingsQuickButton","SETTINGS",new Vector2(336,0),
            new Vector2(280,60),()=>SetSettings(true));

        var panel=RectObject(overlay.transform,"ChemQuestSettingsPanel",Vector2.zero,new Vector2(760,490));
        settingsPanel=panel.gameObject;
        var background=panel.gameObject.AddComponent<Image>();background.color=Navy;
        var outline=RectObject(panel.transform,"GoldTop",new Vector2(0,225),new Vector2(720,6));
        var trim=outline.gameObject.AddComponent<Image>();trim.color=Gold;trim.raycastTarget=false;
        Text(panel.transform,"SettingsTitle","CHEMQUEST  |  AUDIO SETTINGS",new Vector2(0,165),
            new Vector2(700,65),32,Gold);
        Text(panel.transform,"MusicLabel","BACKGROUND MUSIC",new Vector2(0,85),
            new Vector2(650,42),23,Color.white);
        var music=MakeSlider(panel.transform,"MusicVolume",new Vector2(0,45),
            ChemQuestAudioSettings.Music, v=>ChemQuestAudioSettings.SetMusic(v));
        Text(panel.transform,"SFXLabel","SOUND EFFECTS",new Vector2(0,-18),
            new Vector2(650,42),23,Color.white);
        var sfx=MakeSlider(panel.transform,"SFXVolume",new Vector2(0,-59),
            ChemQuestAudioSettings.Sfx, v=>ChemQuestAudioSettings.SetSfx(v));
        AddButton(panel.transform,"MuteButton","MUTE / UNMUTE",new Vector2(-175,-145),
            new Vector2(287,57),()=>{ChemQuestAudioSettings.ToggleMute();music.value=ChemQuestAudioSettings.Music;sfx.value=ChemQuestAudioSettings.Sfx;});
        AddButton(panel.transform,"CloseButton","BACK",new Vector2(175,-145),
            new Vector2(287,57),()=>SetSettings(false));
        settingsPanel.SetActive(false);
        feedback=Text(overlay.transform,"ChemQuestNavigationFeedback","",new Vector2(0,-242),
            new Vector2(950,38),18,Gold);
    }
    private static RectTransform RectObject(Transform parent,string name,Vector2 pos,Vector2 size)
    {
        var obj=new GameObject(name,typeof(RectTransform));
        obj.transform.SetParent(parent,false);
        var rect=obj.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;rect.sizeDelta=size;
        return rect;
    }
    private static TMP_Text Text(Transform parent,string name,string value,Vector2 pos,
        Vector2 size,int font,Color color)
    {
        var rect=RectObject(parent,name,pos,size);
        var txt=rect.gameObject.AddComponent<TextMeshProUGUI>();
        txt.text=value;txt.color=color;txt.fontSize=font;
        txt.alignment=TextAlignmentOptions.Center;
        txt.enableAutoSizing=true;txt.fontSizeMin=13;txt.fontSizeMax=font;
        txt.raycastTarget=false;
        return txt;
    }
    private static void AddButton(Transform parent,string name,string caption,Vector2 pos,
        Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var rect=RectObject(parent,name,pos,size);
        var img=rect.gameObject.AddComponent<Image>();img.color=Cyan;
        var button=rect.gameObject.AddComponent<Button>();
        button.targetGraphic=img;
        button.onClick.AddListener(action);
        Text(rect,"Text",caption,Vector2.zero,size,23,Gold);
    }
    private static Slider MakeSlider(Transform parent,string name,Vector2 pos,
        float value,UnityEngine.Events.UnityAction<float> changed)
    {
        var rect=RectObject(parent,name,pos,new Vector2(580,44));
        var background=RectObject(rect,"Track",Vector2.zero,new Vector2(550,14));
        var trackImg=background.gameObject.AddComponent<Image>();
        trackImg.color=new Color32(76,105,128,255);
        var fillArea=RectObject(rect,"Fill Area",Vector2.zero,new Vector2(520,14));
        var fill=RectObject(fillArea,"Fill",Vector2.zero,new Vector2(520,14));
        var fillImage=fill.gameObject.AddComponent<Image>();fillImage.color=Gold;
        var handleArea=RectObject(rect,"Handle Slide Area",Vector2.zero,new Vector2(530,36));
        var handle=RectObject(handleArea,"Handle",Vector2.zero,new Vector2(26,34));
        var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=Color.white;
        var slider=rect.gameObject.AddComponent<Slider>();
        slider.fillRect=fill;slider.handleRect=handle;
        slider.targetGraphic=handleImage;
        slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false;
        slider.value=value;slider.onValueChanged.AddListener(changed);
        return slider;
    }
    private void BindExisting(Transform canvas)
    {
        var nav=canvas.Find("ChemQuestQuickNavigation");
        var p=canvas.Find("ChemQuestSettingsPanel");
        settingsPanel=p==null?null:p.gameObject;
        feedback=canvas.Find("ChemQuestNavigationFeedback")?.GetComponent<TMP_Text>();
        BindClick(nav,"StageSelectionQuickButton",()=>Open("StageSelection"));
        BindClick(nav,"CertificateQuickButton",()=>Open("ChemQuestCertificate"));
        BindClick(nav,"SettingsQuickButton",()=>SetSettings(true));
        BindClick(p,"CloseButton",()=>SetSettings(false));
        var music=p?.Find("MusicVolume")?.GetComponent<Slider>();
        var sfx=p?.Find("SFXVolume")?.GetComponent<Slider>();
        if(music!=null)
        {
            music.onValueChanged.RemoveAllListeners();
            music.SetValueWithoutNotify(ChemQuestAudioSettings.Music);
            music.onValueChanged.AddListener(v=>ChemQuestAudioSettings.SetMusic(v));
        }
        if(sfx!=null)
        {
            sfx.onValueChanged.RemoveAllListeners();
            sfx.SetValueWithoutNotify(ChemQuestAudioSettings.Sfx);
            sfx.onValueChanged.AddListener(v=>ChemQuestAudioSettings.SetSfx(v));
        }
        BindClick(p,"MuteButton",()=>{
            ChemQuestAudioSettings.ToggleMute();
            if(music!=null)music.SetValueWithoutNotify(ChemQuestAudioSettings.Music);
            if(sfx!=null)sfx.SetValueWithoutNotify(ChemQuestAudioSettings.Sfx);
        });
        if(settingsPanel!=null)settingsPanel.SetActive(false);
    }
    private static void BindClick(Transform parent,string name,UnityEngine.Events.UnityAction callback)
    {
        var button=parent?.Find(name)?.GetComponent<Button>();
        if(button==null)return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }
    private void SetSettings(bool open)
    {
        if(settingsPanel!=null){settingsPanel.SetActive(open);settingsPanel.transform.SetAsLastSibling();}
    }
    private void Open(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {
            string message=scene+" is missing or disabled in Build Settings.";
            if(feedback!=null)feedback.text=message;
            Debug.LogError("ChemQuest: "+message);
            return;
        }
        SceneManager.LoadScene(scene);
    }
}
