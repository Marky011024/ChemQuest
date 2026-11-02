using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

// Matches the ChemQuest manuscript Title Screen & Main Menu wireframe:
// profile badge at top-left, certificate trophy top-right,
// game logo and three stacked buttons centered on the Academy artwork.
// All assets below remain editable native Unity UI elements.
public class ChemQuestMainMenuExtras : MonoBehaviour
{
    private static readonly Color Navy = new Color32(9,31,58,246);
    private static readonly Color MidNavy = new Color32(23,52,84,245);
    private static readonly Color Teal = new Color32(23,130,151,255);
    private static readonly Color Gold = new Color32(248,200,105,255);
    private GameObject settingsPanel;
    private TMP_Text feedback;

    private void Start()
    {
        Build();
        HideLegacyButtonsForPlay();
        ChemQuestAudioSettings.Apply();
    }

    public void Build()
    {
        var overlay=GameObject.Find("ChemQuestMenuOverlay");
        if(overlay==null)
        {
            overlay=new GameObject("ChemQuestMenuOverlay",typeof(RectTransform),
                typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        }
        var canvas=overlay.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=200;
        var scaler=overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.matchWidthOrHeight=1f;
        EnsureEventSystem();

        Transform root=overlay.transform.Find("ChemQuestManuscriptMenu");
        if(root==null)
        {
            root=Panel(overlay.transform,"ChemQuestManuscriptMenu",Vector2.zero,
                new Vector2(1280,720),new Color32(0,0,0,0)).transform;
            // Decorative center plaque leaves the illustrated Academy visible.
            Panel(root,"LogoBackdrop",new Vector2(0,154),new Vector2(670,133),Navy);
            Label(root,"ChemQuestLogo","CHEMQUEST",new Vector2(0,181),
                new Vector2(620,58),47,Gold);
            Label(root,"Tagline","EXPLORE  |  LEARN  |  DISCOVER",new Vector2(0,131),
                new Vector2(620,36),20,Color.white);
            Panel(root,"MenuBackdrop",new Vector2(0,-83),new Vector2(470,276),
                new Color32(11,34,59,234));
            AddButton(root,"StartButton","START  /  ENTER ACADEMY",
                new Vector2(0,4),new Vector2(420,68),Teal);
            AddButton(root,"StageSelectionButton","STAGE SELECT",
                new Vector2(0,-81),new Vector2(420,68),MidNavy);
            AddButton(root,"SettingsButton","SETTINGS",
                new Vector2(0,-166),new Vector2(420,68),MidNavy);
            Panel(root,"PlayerBadge",new Vector2(-484,296),
                new Vector2(270,64),Navy);
            Label(root,"PlayerName","PLAYER",new Vector2(-484,304),
                new Vector2(258,32),17,Gold);
            Label(root,"ProgressBadge","CHEMISTRY EXPLORER",
                new Vector2(-484,281),new Vector2(258,22),13,Color.white);
            AddButton(root,"CertificateButton","AWARD  /  CERTIFICATE",
                new Vector2(469,296),new Vector2(296,64),MidNavy);
            Label(root,"FooterText","CHEMQUEST  |  CHEMICAL BONDING ADVENTURE",
                new Vector2(0,-328),new Vector2(860,25),15,Color.white);
        }

        Transform panel=overlay.transform.Find("ChemQuestSettingsPanel");
        if(panel==null)panel=CreateSettingsPanel(overlay.transform).transform;
        settingsPanel=panel.gameObject;
        Transform message=overlay.transform.Find("ChemQuestNavigationFeedback");
        if(message==null)message=Label(overlay.transform,"ChemQuestNavigationFeedback","",
            new Vector2(0,-269),new Vector2(900,32),16,Gold).transform;
        feedback=message.GetComponent<TMP_Text>();

        // Handles buttons generated in Edit Mode where runtime click events cannot persist.
        Bind(root,"StartButton",()=>Open("StageSelection"));
        Bind(root,"StageSelectionButton",()=>Open("StageSelection"));
        Bind(root,"SettingsButton",()=>Settings(true));
        Bind(root,"CertificateButton",()=>Open("ChemQuestCertificate"));
        Bind(panel,"CloseButton",()=>Settings(false));
        var music=panel.Find("MusicVolume")?.GetComponent<Slider>();
        var sfx=panel.Find("SFXVolume")?.GetComponent<Slider>();
        if(music!=null)
        {
            music.onValueChanged.RemoveAllListeners();
            music.SetValueWithoutNotify(ChemQuestAudioSettings.Music);
            music.onValueChanged.AddListener(ChemQuestAudioSettings.SetMusic);
        }
        if(sfx!=null)
        {
            sfx.onValueChanged.RemoveAllListeners();
            sfx.SetValueWithoutNotify(ChemQuestAudioSettings.Sfx);
            sfx.onValueChanged.AddListener(ChemQuestAudioSettings.SetSfx);
        }
        Bind(panel,"MuteButton",()=>{
            ChemQuestAudioSettings.ToggleMute();
            if(music!=null)music.SetValueWithoutNotify(ChemQuestAudioSettings.Music);
            if(sfx!=null)sfx.SetValueWithoutNotify(ChemQuestAudioSettings.Sfx);
        });
        var nameText=root.Find("PlayerName")?.GetComponent<TMP_Text>();
        if(nameText!=null)nameText.text="PLAYER: "+PlayerPrefs.GetString("StudentName","STUDENT").ToUpperInvariant();
        settingsPanel.SetActive(false);
        // Old quick-nav layout is replaced by the manuscript-centered design.
        var legacy=overlay.transform.Find("ChemQuestQuickNavigation");
        if(legacy!=null)legacy.gameObject.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        var es=FindFirstObjectByType<EventSystem>();
        if(es==null)
        {
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        }
        else if(es.GetComponent<BaseInputModule>()==null)
        {
            es.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();
        r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
        r.anchoredPosition=pos;
        r.sizeDelta=size;
        return r;
    }
    private static Image Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var r=Rect(parent,name,pos,size);
        var img=r.gameObject.AddComponent<Image>();
        img.color=color;
        img.raycastTarget=false;
        return img;
    }
    private static TMP_Text Label(Transform parent,string name,string value,
        Vector2 pos,Vector2 size,int font,Color color)
    {
        var r=Rect(parent,name,pos,size);
        var text=r.gameObject.AddComponent<TextMeshProUGUI>();
        text.text=value;
        text.color=color;
        text.fontSize=font;
        text.fontSizeMin=13;
        text.fontSizeMax=font;
        text.enableAutoSizing=true;
        text.alignment=TextAlignmentOptions.Center;
        text.raycastTarget=false;
        return text;
    }
    private static void AddButton(Transform parent,string name,string title,Vector2 pos,
        Vector2 size,Color color)
    {
        var r=Rect(parent,name,pos,size);
        var img=r.gameObject.AddComponent<Image>();img.color=color;
        var button=r.gameObject.AddComponent<Button>();button.targetGraphic=img;
        Label(r,"Label",title,Vector2.zero,size,23,Gold);
    }
    private static Slider VolumeSlider(Transform parent,string name,Vector2 pos,float initial)
    {
        var rect=Rect(parent,name,pos,new Vector2(550,42));
        var track=Panel(rect,"Background",Vector2.zero,
            new Vector2(520,13),new Color32(67,98,124,255));
        var fillArea=Rect(rect,"Fill Area",Vector2.zero,new Vector2(500,13));
        var fill=Panel(fillArea,"Fill",Vector2.zero,new Vector2(500,13),Gold);
        var handleArea=Rect(rect,"Handle Slide Area",Vector2.zero,new Vector2(500,32));
        var handle=Panel(handleArea,"Handle",Vector2.zero,new Vector2(24,32),Color.white);
        var slider=rect.gameObject.AddComponent<Slider>();
        slider.fillRect=fill.rectTransform;
        slider.handleRect=handle.rectTransform;
        slider.targetGraphic=handle;
        slider.minValue=0f;slider.maxValue=1f;
        slider.SetValueWithoutNotify(initial);
        return slider;
    }
    private static GameObject CreateSettingsPanel(Transform parent)
    {
        var panel=Panel(parent,"ChemQuestSettingsPanel",Vector2.zero,
            new Vector2(750,500),Navy);
        Panel(panel.transform,"GoldAccent",new Vector2(0,234),
            new Vector2(710,6),Gold);
        Label(panel.transform,"SettingsHeader","AUDIO SETTINGS",new Vector2(0,175),
            new Vector2(680,64),34,Gold);
        Label(panel.transform,"MusicCaption","BACKGROUND MUSIC",new Vector2(0,91),
            new Vector2(600,40),22,Color.white);
        VolumeSlider(panel.transform,"MusicVolume",new Vector2(0,52),ChemQuestAudioSettings.Music);
        Label(panel.transform,"SfxCaption","SOUND EFFECTS",new Vector2(0,-16),
            new Vector2(600,40),22,Color.white);
        VolumeSlider(panel.transform,"SFXVolume",new Vector2(0,-56),ChemQuestAudioSettings.Sfx);
        AddButton(panel.transform,"MuteButton","MUTE / UNMUTE",new Vector2(-172,-156),
            new Vector2(290,58),Teal);
        AddButton(panel.transform,"CloseButton","BACK",new Vector2(172,-156),
            new Vector2(290,58),MidNavy);
        return panel.gameObject;
    }
    private static void Bind(Transform parent,string name,UnityEngine.Events.UnityAction action)
    {
        var button=parent?.Find(name)?.GetComponent<Button>();
        if(button==null)return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }
    private void Settings(bool visible)
    {
        if(settingsPanel==null)return;
        settingsPanel.SetActive(visible);
        if(visible)settingsPanel.transform.SetAsLastSibling();
    }
    private void Open(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {
            if(feedback!=null)feedback.text=scene+" is not enabled in Build Settings.";
            Debug.LogError("ChemQuest: "+scene+" missing or disabled in Build Settings.");
            return;
        }
        SceneManager.LoadScene(scene);
    }
    private static void HideLegacyButtonsForPlay()
    {
        // Hide only old center buttons/text, never the existing artwork.
        foreach(var button in FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(button==null || button.GetComponentInParent<ChemQuestMainMenuExtras>()!=null)continue;
            if(button.gameObject.scene.name!="MainMenu")continue;
            var text=button.GetComponentInChildren<TMP_Text>(true);
            if(text!=null && (text.text.Contains("ENTER ACADEMY") || text.text=="START"))
                button.gameObject.SetActive(false);
        }
        foreach(var t in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(t.gameObject.scene.name!="MainMenu")continue;
            if(t.text.StartsWith("WELCOME TO CHEMQUEST"))
                t.gameObject.SetActive(false);
        }
    }
}
