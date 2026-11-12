using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shared formative BOW learning check for Stage 1 Levels 1 and 2.
// The level is only recorded as completed after both questions are answered.
public sealed class Stage1BowLearningCheck : MonoBehaviour
{
    private readonly Color navy = new Color32(24, 45, 77, 255);
    private readonly Color dark = new Color32(10, 30, 56, 255);
    private readonly Color teal = new Color32(15, 139, 149, 255);
    private readonly Color muted = new Color32(69, 94, 136, 255);
    private readonly Color gold = new Color32(255, 191, 74, 255);

    private GameObject overlay;
    private TextMeshProUGUI questionLabel, tipLabel;
    private TextMeshProUGUI[] answerLabels;
    private Action<bool> onFinished;
    private int stageLevel, questionIndex, mistakes;
    private bool activeCheck;
    private TMP_FontAsset fallbackFont;

    private struct Question
    {
        public string prompt, optionA, optionB, hint, explanation;
        public int correct;
        public Question(string p, string a, string b, int c, string h, string e)
        {
            prompt=p; optionA=a; optionB=b; correct=c; hint=h; explanation=e;
        }
    }

    private static readonly Question[] level1Questions =
    {
        new Question(
            "In the zinc and hydrochloric acid experiment, what shows that a chemical reaction happened?",
            "Gas bubbles formed", "The container changed position", 0,
            "HINT: Look for evidence that a new substance was produced.",
            "Gas formation is evidence that a chemical reaction occurred."),
        new Question(
            "What gas is produced when zinc reacts with hydrochloric acid?",
            "Oxygen gas", "Hydrogen gas", 1,
            "HINT: A reactive metal and an acid often release this gas.",
            "Zinc and hydrochloric acid produce hydrogen gas and zinc chloride.")
    };
    private static readonly Question[] level2Questions =
    {
        new Question(
            "The temperature rises during an experiment. What type of energy change occurred?",
            "Exothermic: heat released", "Endothermic: heat absorbed", 0,
            "HINT: A temperature increase indicates heat release.",
            "An exothermic reaction releases heat to its surroundings."),
        new Question(
            "What observation indicates a new substance formed when copper sulfate reacts with sodium hydroxide?",
            "Only the liquid was stirred", "A blue precipitate formed", 1,
            "HINT: A precipitate is a new insoluble solid.",
            "The blue copper(II) hydroxide precipitate is evidence of a chemical reaction.")
    };

    public void Begin(int level, TMP_FontAsset font, Action<bool> finished)
    {
        if(activeCheck)return;
        stageLevel=level==1?1:2;
        fallbackFont=font!=null?font:TMP_Settings.defaultFontAsset;
        onFinished=finished;
        questionIndex=0;
        mistakes=0;
        EnsureUI();
        if(overlay==null)
        {
            Debug.LogError("Stage 1 BOW check could not find a Canvas; completion was not saved.");
            return;
        }
        activeCheck=true;
        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
        ShowQuestion();
    }

    private void EnsureUI()
    {
        if(overlay!=null)return;
        var canvas=GetComponentInParent<Canvas>();
        if(canvas==null) canvas=FindFirstObjectByType<Canvas>();
        if(canvas==null)return;
        overlay=MakeImage("Stage1BowLearningCheck",canvas.transform,
            new Color32(3,12,28,225)).gameObject;
        var full=overlay.GetComponent<RectTransform>();
        full.anchorMin=Vector2.zero;full.anchorMax=Vector2.one;
        full.offsetMin=Vector2.zero;full.offsetMax=Vector2.zero;
        var panel=MakeImage("BowCard",overlay.transform,navy);
        SetRect(panel.rectTransform,Vector2.zero,new Vector2(810,430));
        MakeText("Title",panel.transform,"BOW LEARNING CHECK",31,gold,
            new Vector2(0,153),new Vector2(765,52));
        MakeText("Description",panel.transform,
            "Answer two questions about the chemistry you just explored.",17,Color.white,
            new Vector2(0,105),new Vector2(770,35));
        questionLabel=MakeText("Question",panel.transform,"",22,Color.white,
            new Vector2(0,13),new Vector2(744,102));
        tipLabel=MakeText("LearningHint",panel.transform,"",15,gold,
            new Vector2(0,-73),new Vector2(750,47));
        answerLabels=new TextMeshProUGUI[2];
        MakeAnswer(panel.transform,0,new Vector2(-188,-146),teal);
        MakeAnswer(panel.transform,1,new Vector2(188,-146),muted);
        overlay.SetActive(false);
    }

    private Image MakeImage(string name,Transform parent,Color color)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        obj.transform.SetParent(parent,false);
        var image=obj.GetComponent<Image>();
        image.color=color;
        image.raycastTarget=true;
        return image;
    }

    private static void SetRect(RectTransform rect,Vector2 center,Vector2 size)
    {
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=center;
        rect.sizeDelta=size;
    }

    private TextMeshProUGUI MakeText(string name,Transform parent,string text,int size,
        Color color,Vector2 center,Vector2 dimensions)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent,false);
        SetRect(obj.GetComponent<RectTransform>(),center,dimensions);
        var label=obj.GetComponent<TextMeshProUGUI>();
        label.font=fallbackFont;
        label.text=text;
        label.color=color;
        label.fontSize=size;
        label.enableAutoSizing=false;
        label.textWrappingMode=TextWrappingModes.Normal;
        label.alignment=TextAlignmentOptions.Center;
        label.overflowMode=TextOverflowModes.Ellipsis;
        label.raycastTarget=false;
        return label;
    }

    private void MakeAnswer(Transform parent,int index,Vector2 center,Color color)
    {
        var image=MakeImage("BowAnswer"+index,parent,color);
        SetRect(image.rectTransform,center,new Vector2(354,65));
        var btn=image.gameObject.AddComponent<Button>();
        int choice=index;
        btn.onClick.AddListener(()=>Answer(choice));
        answerLabels[index]=MakeText("Label",image.transform,"",17,Color.white,
            Vector2.zero,new Vector2(336,59));
    }

    private Question Current => (stageLevel==1?level1Questions:level2Questions)[questionIndex];

    private void ShowQuestion()
    {
        var q=Current;
        questionLabel.text=q.prompt;
        answerLabels[0].text=q.optionA;
        answerLabels[1].text=q.optionB;
        tipLabel.text="";
    }

    private void Answer(int choice)
    {
        if(!activeCheck || overlay==null || !overlay.activeSelf)return;
        var q=Current;
        if(choice!=q.correct)
        {
            mistakes++;
            tipLabel.text=mistakes==1?q.hint:q.explanation+" Choose the correct answer to continue.";
            return;
        }
        questionIndex++;
        if(questionIndex<(stageLevel==1?level1Questions.Length:level2Questions.Length))
        {
            ShowQuestion();
            return;
        }
        activeCheck=false;
        overlay.SetActive(false);
        var finished=onFinished;
        onFinished=null;
        if(finished!=null)finished(mistakes>0);
    }
}
