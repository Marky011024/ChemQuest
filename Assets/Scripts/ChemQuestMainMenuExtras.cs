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
        // Upgrade old serialized settings UI once during Editor installation.
        // The previous Slider's centered fill anchors caused oversize bars.
        if(panel!=null && panel.Find("SettingsCard")==null)
        {
            if(Application.isPlaying)Destroy(panel.gameObject);
            else DestroyImmediate(panel.gameObject);
            panel=null;
        }
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
        var card=panel.Find("SettingsCard");
        EnsureMuteStatus(card);
        UpdateMuteStatus(card);
        Bind(card,"CloseButton",()=>Settings(false));
        var music=card?.Find("MusicVolume")?.GetComponent<Slider>();
        var sfx=card?.Find("SFXVolume")?.GetComponent<Slider>();
        // Repair handles in already-installed scenes without requiring rebuild.
        if(music!=null && music.handleRect!=null)
        {
            var image=music.handleRect.GetComponent<Image>();
            if(image!=null)image.raycastTarget=true;
        }
        if(sfx!=null && sfx.handleRect!=null)
        {
            var image=sfx.handleRect.GetComponent<Image>();
            if(image!=null)image.raycastTarget=true;
        }
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
        Bind(card,"MuteButton",()=>{
            ChemQuestAudioSettings.ToggleMute();
            if(music!=null)music.SetValueWithoutNotify(ChemQuestAudioSettings.Music);
            if(sfx!=null)sfx.SetValueWithoutNotify(ChemQuestAudioSettings.Sfx);
            UpdateMuteStatus(card);
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
        var rect=Rect(parent,name,pos,new Vector2(500,46));
        // Unity Slider controls fill anchors. Use stretch children with zero
        // offsets, otherwise a 500px fill spills beyond the modal panel.
        var track=Panel(rect,"Background",Vector2.zero,
            new Vector2(480,14),new Color32(64,103,130,255));
        var fillArea=Rect(rect,"Fill Area",Vector2.zero,new Vector2(452,14));
        var fill=Panel(fillArea,"Fill",Vector2.zero,Vector2.zero,Teal);
        fill.rectTransform.anchorMin=Vector2.zero;
        fill.rectTransform.anchorMax=Vector2.one;
        fill.rectTransform.offsetMin=Vector2.zero;
        fill.rectTransform.offsetMax=Vector2.zero;
        var handleArea=Rect(rect,"Handle Slide Area",Vector2.zero,new Vector2(452,37));
        var handle=Panel(handleArea,"Handle",Vector2.zero,new Vector2(20,30),Gold);
        // GraphicRaycaster needs a raycastable target on the handle to begin dragging.
        handle.raycastTarget=true;
        var slider=rect.gameObject.AddComponent<Slider>();
        slider.fillRect=fill.rectTransform;
        slider.handleRect=handle.rectTransform;
        slider.targetGraphic=handle;
        slider.direction=Slider.Direction.LeftToRight;
        slider.minValue=0f;
        slider.maxValue=1f;
        slider.SetValueWithoutNotify(Mathf.Clamp01(initial));
        return slider;
    }
    private static GameObject CreateSettingsPanel(Transform parent)
    {
        // Fullscreen raycast-blocking scrim keeps the main menu from
        // visually competing with the settings modal.
        var overlay=Panel(parent,"ChemQuestSettingsPanel",Vector2.zero,
            new Vector2(1280,720),new Color32(3,12,27,207));
        overlay.raycastTarget=true;
        var card=Panel(overlay.transform,"SettingsCard",Vector2.zero,
            new Vector2(690,460),Navy);
        card.raycastTarget=true;
        Panel(card.transform,"GoldAccent",new Vector2(0,212),
            new Vector2(640,5),Gold);
        Label(card.transform,"SettingsHeader","AUDIO SETTINGS",
            new Vector2(0,157),new Vector2(620,56),32,Gold);
        Label(card.transform,"SettingsHint","Customize your ChemQuest experience",
            new Vector2(0,120),new Vector2(600,30),17,Color.white);
        Label(card.transform,"MusicCaption","BACKGROUND MUSIC",
            new Vector2(0,65),new Vector2(530,34),21,Color.white);
        VolumeSlider(card.transform,"MusicVolume",new Vector2(0,22),
            ChemQuestAudioSettings.Music);
        Label(card.transform,"SfxCaption","SOUND EFFECTS",
            new Vector2(0,-39),new Vector2(530,34),21,Color.white);
        VolumeSlider(card.transform,"SFXVolume",new Vector2(0,-82),
            ChemQuestAudioSettings.Sfx);
        AddButton(card.transform,"MuteButton","MUTE / UNMUTE",
            new Vector2(-157,-167),new Vector2(255,51),Teal);
        AddButton(card.transform,"CloseButton","BACK",
            new Vector2(157,-167),new Vector2(255,51),MidNavy);
        return overlay.gameObject;
    }
    private static void EnsureMuteStatus(Transform card)
    {
        if(card==null || card.Find("MuteStatus")!=null)return;
        Label(card,"MuteStatus","",new Vector2(0,-123),
            new Vector2(590,26),17,Gold);
    }
    private static void UpdateMuteStatus(Transform card)
    {
        if(card==null)return;
        EnsureMuteStatus(card);
        bool muted=ChemQuestAudioSettings.Muted;
        var indicator=card.Find("MuteStatus")?.GetComponent<TMP_Text>();
        if(indicator!=null)
        {
            indicator.text=muted?"SOUND MUTED":"SOUND ON";
            indicator.color=muted?new Color32(255,167,104,255):new Color32(110,227,202,255);
        }
        var label=card.Find("MuteButton/Label")?.GetComponent<TMP_Text>();
        if(label!=null)label.text=muted?"UNMUTE SOUND":"MUTE SOUND";
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
        if(visible)
        {
            settingsPanel.transform.SetAsLastSibling();
            UpdateMuteStatus(settingsPanel.transform.Find("SettingsCard"));
        }
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
        // Old MainMenu.unity still contains PlayButton and WelcomeText;
        // these are the source of the visible overlap in the screenshot.
        var mainCanvas=GameObject.Find("Canvas");
        if(mainCanvas==null)return;
        foreach(var t in mainCanvas.GetComponentsInChildren<Transform>(true))
        {
            if(t==mainCanvas.transform)continue;
            if(t.name=="PlayButton" || t.name=="WelcomeText")
                t.gameObject.SetActive(false);
        }
    }
}
