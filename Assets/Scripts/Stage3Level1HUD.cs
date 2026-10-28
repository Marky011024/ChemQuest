using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class Stage3Level1HUD : MonoBehaviour
{
    [Header("Reuse Stage 1 UI Prefabs")]
    [SerializeField] private GameObject pauseButtonPrefab;
    [SerializeField] private GameObject pauseMenuPrefab;
    [Header("Stage 3 Level 1")]
    [SerializeField] private Stage3Level1CompleteManager completeManager;
    [Header("Editable Scene UI (created by Finisher)")]
    [SerializeField] private Button scenePauseButton;
    [SerializeField] private GameObject scenePausePanel;
    [SerializeField] private GameObject sceneOutOfEnergyPanel;
    [SerializeField] private TMP_Text sceneTimerText;
    [SerializeField] private TMP_Text sceneHintText;
    [SerializeField] private GameObject sceneFailedPanel;
    private GameObject menuPanel, noEnergyPanel;
    private TMP_Text timerLabel;
    private float nextRefresh;
    private bool paused;

    private void Start()
    {
        EnsureEventSystem();
        DisableOriginalPauseButton();
        if (completeManager == null)
            completeManager = FindObjectOfType<Stage3Level1CompleteManager>();

        if(scenePauseButton != null && scenePausePanel != null &&
           sceneOutOfEnergyPanel != null && sceneHintText != null)
        {
            // The HUD now exists as editable scene objects.
            scenePauseButton.onClick.RemoveAllListeners();
            scenePauseButton.onClick.AddListener(PauseGame);
            menuPanel=scenePausePanel;
            noEnergyPanel=sceneOutOfEnergyPanel;
            timerLabel=sceneTimerText;
            ConnectMenuButton(noEnergyPanel,"BackToSelectionButton",
                ()=>GoTo("Stage3LevelSelection"));
            ConnectMenuButton(menuPanel,"ContinueButton",ResumeGame);
            ConnectMenuButton(menuPanel,"LevelSelectionButton",
                ()=>GoTo("Stage3LevelSelection"));
            ConnectMenuButton(menuPanel,"StageSelectionButton",
                ()=>GoTo("StageSelection"));
            foreach(var b in menuPanel.GetComponentsInChildren<Button>(true))
                if(b.gameObject.name=="StageSelectionButton")
                {
                    var t=b.GetComponentInChildren<TMP_Text>(true);
                    if(t!=null)t.text="STAGE SELECTION";
                }
            menuPanel.SetActive(false);
            noEnergyPanel.SetActive(false);
            if(completeManager!=null)
            {
                completeManager.AssignFeedbackText(sceneHintText);
                if(sceneFailedPanel!=null) completeManager.AssignFailurePanel(sceneFailedPanel);
            }
            if(sceneFailedPanel!=null)
            {
                ConnectMenuButton(sceneFailedPanel,"LevelSelectionButton",()=>GoTo("Stage3LevelSelection"));
                ConnectMenuButton(sceneFailedPanel,"RetryButton",RetryFailedLevel);
            }
            UpdateEnergyUI();
            return;
        }

        var canvasGO = new GameObject("Stage3Level1HUDCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        // Leave the molecule in the existing Stage3Level1 canvas. Only add HUD.
        var hint = Text(canvasGO.transform,"Stage3Level1HintText","",21,
            new Vector2(0,-295),new Vector2(850,56),new Color32(255,215,92,255));
        hint.gameObject.SetActive(false);
        if(completeManager!=null)completeManager.AssignFeedbackText(hint);

        BuildPauseMenu(canvasGO.transform);
        BuildOutOfEnergy(canvasGO.transform);
        UpdateEnergyUI();
    }

    // The original Stage3Level1Canvas already contains a Stage 1 PauseButton.
    // Hide only that older button; keep all game objects and other UI intact.
    private void DisableOriginalPauseButton()
    {
        var originalCanvas=GameObject.Find("Stage3Level1Canvas");
        if(originalCanvas==null)return;

        foreach(var button in originalCanvas.GetComponentsInChildren<Button>(true))
        {
            if(button.gameObject.name=="PauseButton")
                button.gameObject.SetActive(false);
        }
    }

    private void EnsureEventSystem()
    {
        var system=FindObjectOfType<EventSystem>();
        if(system==null)system=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var old in system.GetComponents<StandaloneInputModule>())
            old.enabled=false;
        if(system.GetComponent<InputSystemUIInputModule>()==null)
        {
            var module=system.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    private TextMeshProUGUI Text(Transform parent,string name,string value,int size,
        Vector2 position,Vector2 dimensions,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();
        r.anchoredPosition=position;
        r.sizeDelta=dimensions;
        var t=go.GetComponent<TextMeshProUGUI>();
        t.text=value; t.fontSize=size; t.color=color;
        t.alignment=TextAlignmentOptions.Center;
        t.textWrappingMode=TextWrappingModes.Normal;
        t.raycastTarget=false;
        return t;
    }

    private GameObject Panel(Transform parent,string name,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        go.GetComponent<RectTransform>().sizeDelta=size;
        go.GetComponent<Image>().color=color;
        return go;
    }

    private Button Button(Transform parent,string name,string label,Vector2 pos,
        Vector2 size,Action callback)
    {
        var go=Panel(parent,name,size,new Color32(24,93,137,255));
        go.GetComponent<RectTransform>().anchoredPosition=pos;
        var b=go.AddComponent<Button>();
        b.onClick.AddListener(()=>callback());
        Text(go.transform,"Text",label,22,Vector2.zero,size,Color.white);
        return b;
    }

    private void ConnectMenuButton(GameObject prefab,string objectName,Action callback)
    {
        bool found=false;
        foreach(var button in prefab.GetComponentsInChildren<Button>(true))
        {
            if(button.gameObject.name!=objectName)continue;
            button.onClick=new Button.ButtonClickedEvent();
            button.onClick.AddListener(()=>callback());
            found=true;
        }
        if(!found)Debug.LogWarning("Stage3Level1: missing menu button "+objectName);
    }

    private void BuildPauseMenu(Transform parent)
    {
        if(pauseButtonPrefab!=null)
        {
            var go=Instantiate(pauseButtonPrefab,parent,false);
            go.name="Stage3Level1PauseButton";
            var rect=go.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=new Vector2(0,1); rect.anchorMax=new Vector2(0,1);
                rect.pivot=new Vector2(0.5f,0.5f);
                rect.anchoredPosition=new Vector2(56,-59);
            }
            var button=go.GetComponent<Button>();
            if(button!=null)
            {
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(PauseGame);
            }
        }
        else Button(parent,"Stage3Level1PauseButton","MENU",
            new Vector2(-505,288),new Vector2(145,52),PauseGame);

        if(pauseMenuPrefab!=null)
        {
            menuPanel=Instantiate(pauseMenuPrefab,parent,false);
            menuPanel.name="Stage3Level1PauseMenu";
            var rect=menuPanel.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
                rect.offsetMin=Vector2.zero; rect.offsetMax=Vector2.zero;
            }
            ConnectMenuButton(menuPanel,"ContinueButton",ResumeGame);
            ConnectMenuButton(menuPanel,"LevelSelectionButton",
                ()=>GoTo("Stage3LevelSelection"));
            ConnectMenuButton(menuPanel,"StageSelectionButton",()=>GoTo("StageSelection"));
            foreach(var b in menuPanel.GetComponentsInChildren<Button>(true))
                if(b.gameObject.name=="StageSelectionButton")
                {
                    var label=b.GetComponentInChildren<TMP_Text>(true);
                    if(label!=null)label.text="STAGE SELECTION";
                }
        }
        else
        {
            menuPanel=Panel(parent,"Stage3Level1PauseMenu",new Vector2(1280,720),
                new Color32(8,26,49,235));
            Text(menuPanel.transform,"Heading","GAME PAUSED",38,
                new Vector2(0,140),new Vector2(640,80),Color.white);
            Button(menuPanel.transform,"ContinueButton","CONTINUE",
                new Vector2(0,25),new Vector2(360,65),ResumeGame);
            Button(menuPanel.transform,"LevelSelectionButton","LEVEL SELECTION",
                new Vector2(0,-70),new Vector2(360,65),
                ()=>GoTo("Stage3LevelSelection"));
            Button(menuPanel.transform,"StageSelectionButton","STAGE SELECTION",
                new Vector2(0,-165),new Vector2(360,65),
                ()=>GoTo("StageSelection"));
        }
        menuPanel.SetActive(false);
    }

    private void BuildOutOfEnergy(Transform parent)
    {
        noEnergyPanel=Panel(parent,"Stage3Level1OutOfEnergyPanel",
            new Vector2(1280,720),new Color32(5,14,29,245));
        var card=Panel(noEnergyPanel.transform,"OutOfEnergyCard",
            new Vector2(700,400),new Color32(22,44,76,255));
        Text(card.transform,"OutOfEnergyTitle","OUT OF ENERGY!",38,
            new Vector2(0,113),new Vector2(650,80),
            new Color32(255,206,90,255));
        Text(card.transform,"Description","Your shared energy is empty.",22,
            new Vector2(0,45),new Vector2(650,55),Color.white);
        timerLabel=Text(card.transform,"EnergyTimerText","Next Energy In: 15:00",25,
            new Vector2(0,-25),new Vector2(640,60),
            new Color32(255,206,90,255));
        Button(card.transform,"BackToSelectionButton","BACK TO LEVEL SELECTION",
            new Vector2(0,-125),new Vector2(350,65),
            ()=>GoTo("Stage3LevelSelection"));
        noEnergyPanel.SetActive(false);
    }

    private void Update()
    {
        if(Time.unscaledTime<nextRefresh)return;
        nextRefresh=Time.unscaledTime+0.25f;
        UpdateEnergyUI();
    }

    private void UpdateEnergyUI()
    {
        if(noEnergyPanel==null || EnergyManager.Instance==null)return;
        bool empty=!EnergyManager.Instance.HasEnergy;
        if(empty)
        {
            if(!noEnergyPanel.activeSelf)noEnergyPanel.SetActive(true);
            noEnergyPanel.transform.SetAsLastSibling();
            int seconds=EnergyManager.Instance.SecondsUntilNextEnergy();
            if(timerLabel!=null)timerLabel.text="Next Energy In: "+
                (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
        }
        else if(noEnergyPanel.activeSelf)noEnergyPanel.SetActive(false);
    }

    private void RetryFailedLevel()
    {
        if(EnergyManager.Instance==null || !EnergyManager.Instance.HasEnergy)return;
        GoTo("Stage3Level1");
    }

    public void PauseGame()
    {
        if(paused || (completeManager!=null && !completeManager.CanInteract()))return;
        paused=true;
        menuPanel.SetActive(true);
        Time.timeScale=0f;
    }
    public void ResumeGame()
    {
        paused=false;
        Time.timeScale=1f;
        menuPanel.SetActive(false);
    }
    private void GoTo(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("Stage 3 Level 1: "+scene+" missing from Build Settings.");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
    private void OnDestroy(){Time.timeScale=1f;}
}
