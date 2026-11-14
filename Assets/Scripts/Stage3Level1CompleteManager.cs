
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stage3Level1CompleteManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Level Complete UI")]
    [SerializeField] private GameObject finalCompletePanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text levelUnlockedText;

    [Header("Failed Game UI")]
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Energy System")]
    [SerializeField] private Stage3Level1EnergyManager energyManager;

    private bool hydrogenCompleted = false;
    private bool waterCompleted = false;
    private bool levelCompleted = false;
    private bool runFailed = false;

    private int wrongAttempts = 0;
    private int score = 0;
    private int bowQuestionIndex = -1;
    private GameObject bowPanel;
    private GameObject polishedCompletePanel;
    private TMP_Text bowQuestionText, bowHintText, polishedScoreText;

    private void Start()
    {
        if (finalCompletePanel != null)
            finalCompletePanel.SetActive(false);

        if (failedPanel != null)
            failedPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        BuildLearningPanels();
        UpdateScoreDisplay();
    }

    public void RegisterWrongAttempt()
    {
        if (levelCompleted || runFailed)
            return;

        wrongAttempts++;

        Debug.Log("Wrong attempts: " + wrongAttempts);

        if (wrongAttempts == 1)
        {
            ShowFeedback(
                "HINT: Share electrons in the correct bonding zones.",
                Color.yellow
            );
        }
        else
        {
            runFailed = true;

            if (energyManager != null)
                energyManager.FailRun();

            // The full-screen failure panel owns the failure message.
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);

            if (failedPanel != null)
            {
                failedPanel.SetActive(true);
                failedPanel.transform.SetAsLastSibling();
            }
        }
    }

    public void HydrogenPuzzleCompleted()
    {
        if (hydrogenCompleted || runFailed)
            return;

        hydrogenCompleted = true;

        Debug.Log("H2 Puzzle Completed!");

        CheckLevelCompletion();
    }

    public void WaterPuzzleCompleted()
    {
        if (waterCompleted || runFailed)
            return;

        waterCompleted = true;

        Debug.Log("H2O Puzzle Completed!");

        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (levelCompleted || runFailed)
            return;

        if (hydrogenCompleted && waterCompleted)
        {
            if (bowQuestionIndex < 0)
            {
                bowQuestionIndex = 0;
                ShowBowQuestion();
                return;
            }
            if (bowQuestionIndex < 2) return;
            levelCompleted = true;

            score = wrongAttempts == 0 ? 100 : 75;

            UpdateScoreDisplay();

            PlayerPrefs.SetInt("Stage3Level1Completed", 1);
            PlayerPrefs.SetInt("Stage3Level1Score", score);
            PlayerPrefs.SetInt("Stage3Level2Unlocked", 1);
            PlayerPrefs.Save();

            if (finalScoreText != null)
                finalScoreText.text = score.ToString("D6");

            if (levelUnlockedText != null)
                levelUnlockedText.text = "LEVEL 2 UNLOCKED!";

            if (polishedCompletePanel != null)
            {
                if (polishedScoreText != null) polishedScoreText.text = "FINAL SCORE: " + score.ToString("D6");
                polishedCompletePanel.SetActive(true);
                polishedCompletePanel.transform.SetAsLastSibling();
            }
            else if (finalCompletePanel != null)
            {
                finalCompletePanel.SetActive(true);
                finalCompletePanel.transform.SetAsLastSibling();
            }

            Debug.Log(
                "STAGE 3 LEVEL 1 COMPLETED! Score: " + score
            );
        }
    }

    // A separate top-sorted Canvas avoids modifying or covering the serialized
    // puzzle UI, and provides a properly shaded completion dialog.
    private void BuildLearningPanels()
    {
        var canvasGo = new GameObject("Stage3Level1LearningCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 140;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        bowPanel = Overlay(canvasGo.transform, "Stage3BowLearningCheck");
        var bowCard = MakeBox(bowPanel.transform, "BowCard", new Vector2(870,420),
            Vector2.zero, new Color32(24,49,80,255));
        MakeText(bowCard.transform,"Title","BOW LEARNING CHECK",31,
            new Vector2(0,145),new Vector2(790,52),new Color32(255,202,84,255));
        MakeText(bowCard.transform,"Info","Complete two covalent-bonding questions to finish Level 1.",18,
            new Vector2(0,92),new Vector2(790,40),new Color32(81,208,244,255));
        bowQuestionText = MakeText(bowCard.transform,"Question","",23,
            new Vector2(0,20),new Vector2(785,72),Color.white);
        bowHintText = MakeText(bowCard.transform,"Hint","",16,
            new Vector2(0,-57),new Vector2(770,36),new Color32(255,207,112,255));
        MakeButton(bowCard.transform,"OptionA","OPTION A",new Vector2(-201,-136),
            new Vector2(370,62),()=>AnswerBow(0));
        MakeButton(bowCard.transform,"OptionB","OPTION B",new Vector2(201,-136),
            new Vector2(370,62),()=>AnswerBow(1));
        bowPanel.SetActive(false);

        polishedCompletePanel = Overlay(canvasGo.transform,"Stage3PolishedComplete");
        var doneCard = MakeBox(polishedCompletePanel.transform,"CompleteCard",
            new Vector2(710,420),Vector2.zero,new Color32(24,49,80,255));
        MakeText(doneCard.transform,"Title","LEVEL 1 COMPLETE!",39,
            new Vector2(0,132),new Vector2(675,70),new Color32(255,202,84,255));
        MakeText(doneCard.transform,"Info",
            "H2 and H2O covalent bonding challenges completed.",21,
            new Vector2(0,48),new Vector2(675,62),Color.white);
        polishedScoreText = MakeText(doneCard.transform,"Score","FINAL SCORE: 000000",27,
            new Vector2(0,-21),new Vector2(660,48),new Color32(255,202,84,255));
        MakeText(doneCard.transform,"Unlocked","LEVEL 2 UNLOCKED!",21,
            new Vector2(0,-76),new Vector2(640,40),new Color32(98,241,160,255));
        MakeButton(doneCard.transform,"Select","LEVEL SELECTION",new Vector2(-150,-148),
            new Vector2(265,55),()=>Navigate("Stage3LevelSelection"));
        MakeButton(doneCard.transform,"Next","NEXT LEVEL",new Vector2(150,-148),
            new Vector2(265,55),()=>Navigate("Stage3Level2"));
        polishedCompletePanel.SetActive(false);
        if (finalCompletePanel != null) finalCompletePanel.SetActive(false);
    }

    private GameObject Overlay(Transform parent,string name)
    {
        var overlay = MakeBox(parent,name,new Vector2(1280,720),Vector2.zero,
            new Color32(4,12,27,235));
        var r = overlay.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return overlay;
    }
    private GameObject MakeBox(Transform parent,string name,Vector2 size,Vector2 pos,Color color)
    {
        var o=new GameObject(name,typeof(RectTransform),typeof(Image));
        o.transform.SetParent(parent,false);
        var r=o.GetComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;
        o.GetComponent<Image>().color=color;
        return o;
    }
    private TMP_Text MakeText(Transform parent,string name,string content,int font,Vector2 pos,
        Vector2 size,Color color)
    {
        var o=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        o.transform.SetParent(parent,false);
        var r=o.GetComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;
        var t=o.GetComponent<TextMeshProUGUI>();
        t.text=content;t.fontSize=font;t.color=color;
        t.alignment=TextAlignmentOptions.Center;
        t.textWrappingMode=TextWrappingModes.Normal;
        t.raycastTarget=false;
        return t;
    }
    private void MakeButton(Transform parent,string name,string label,Vector2 pos,
        Vector2 size,UnityEngine.Events.UnityAction clicked)
    {
        var o=MakeBox(parent,name,size,pos,new Color32(19,136,147,255));
        var button=o.AddComponent<Button>();
        button.onClick.AddListener(clicked);
        MakeText(o.transform,"Text",label,18,Vector2.zero,size,Color.white);
    }
    private void ShowBowQuestion()
    {
        if(bowPanel==null)
        {
            // Do not permanently block older scenes if UI creation fails.
            bowQuestionIndex=2;
            CheckLevelCompletion();
            return;
        }
        string prompt=bowQuestionIndex==0?
            "How do two hydrogen atoms form the H2 covalent bond?":
            "What is the formula and common name of the molecule formed by oxygen and two hydrogen atoms?";
        string left=bowQuestionIndex==0?
            "They share one pair of electrons":"H2O - water";
        string right=bowQuestionIndex==0?
            "One atom transfers an electron":"HO2 - water";
        bowQuestionText.text=prompt;
        bowHintText.text="";
        SetOption("OptionA",left);
        SetOption("OptionB",right);
        bowPanel.SetActive(true);
        bowPanel.transform.SetAsLastSibling();
    }
    private void SetOption(string name,string label)
    {
        foreach(var button in bowPanel.GetComponentsInChildren<Button>(true))
        {
            if(button.gameObject.name!=name)continue;
            var text=button.GetComponentInChildren<TMP_Text>(true);
            if(text!=null)text.text=label;
        }
    }
    private void AnswerBow(int answer)
    {
        if(runFailed || levelCompleted || bowQuestionIndex<0 || bowQuestionIndex>=2)return;
        if(answer!=0)
        {
            ChemQuestSoundtrack.Wrong();
            RegisterWrongAttempt();
            if(runFailed)
            {
                bowPanel.SetActive(false);
                if(failedPanel!=null)failedPanel.transform.SetAsLastSibling();
            }
            else
                bowHintText.text="HINT: Covalent bonds share electrons. Water has two H atoms and one O atom.";
            return;
        }
        ChemQuestSoundtrack.Correct();
        bowQuestionIndex++;
        if(bowQuestionIndex<2)ShowBowQuestion();
        else
        {
            bowPanel.SetActive(false);
            CheckLevelCompletion();
        }
    }
    private void Navigate(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("Stage 3 Level 1: "+scene+" is not in Build Settings.");
            return;
        }
        SceneManager.LoadScene(scene);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "SCORE\n" + score.ToString("D6");
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
    }

    public void AssignFailurePanel(GameObject panel)
    {
        if(panel == null) return;
        if(failedPanel != null && failedPanel != panel) failedPanel.SetActive(false);
        failedPanel = panel;
        failedPanel.SetActive(runFailed);
        if(runFailed) failedPanel.transform.SetAsLastSibling();
    }

    public void AssignFeedbackText(TMP_Text hint)
    {
        feedbackText=hint;
        if(feedbackText!=null)feedbackText.gameObject.SetActive(false);
    }

    public bool CanInteract()
    {
        return Time.timeScale > 0f && !levelCompleted &&
               !runFailed &&
               (energyManager == null ||
                !energyManager.IsOutOfEnergy);
    }
}
