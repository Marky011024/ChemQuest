using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// First-play, skippable guided introduction for ChemQuest levels.
/// UI is editable in the scene before Play, no gameplay scripts replaced.
/// Each level is introduced once per install/player profile (PlayerPrefs).
/// </summary>
public class ChemQuestFirstTimeTutorial : MonoBehaviour
{
    public int Stage=1;
    public int Level=1;
    public string TutorialVersion="v1";
    public bool ShowOnFirstPlay=true;
    private const string Prefix="ChemQuest_Tutorial_";
    private GameObject overlay;
    private GameObject practiceBanner;
    private TMP_Text title,body,counter,nextLabel,practiceLabel;
    private Button backButton,nextButton,skipButton,gotItButton;
    private int index;
    private bool practice;
    private static readonly Color32 Deep=new Color32(5,15,31,239);
    private static readonly Color32 Card=new Color32(22,48,82,255);
    private static readonly Color32 Cyan=new Color32(65,193,232,255);
    private static readonly Color32 Gold=new Color32(255,214,104,255);

    private string Key=>Prefix+"S"+Stage+"_L"+Level+"_"+TutorialVersion;
    private string[] Titles => new[]{"WELCOME TO CHEMQUEST","HOW TO PLAY","READY TO EXPERIMENT?"};
    private string[] Messages
    {
        get
        {
            if(Stage==1)
                return new[]{
                    "You're a chemistry investigator! Complete the experiment to discover changes in matter.",
                    Level==1?"Follow the lab instructions, add zinc to hydrochloric acid, and observe the hydrogen gas reaction.":
                        Level==2?"Use the experiment controls to observe how temperature and color change.":
                        "Arrange and test the substances to observe the precipitate puzzle.",
                    "Watch for feedback and hints. One incorrect attempt gives a hint; the second ends the run and uses one energy."
                };
            if(Stage==2)
                return new[]{
                    "Explore ionic bonding using electron transfer and the octet rule.",
                    Level==1?"Choose Na or K. DRAG the donor's valence electron into Cl's OUTER SHELL. Complete both NaCl and KCl.":
                        Level==2?"Drag Mg's TWO valence electrons to O, or ONE each to two Cl atoms. Complete MgO and MgCl2.":
                        "DRAG Na+ and Cl- ions into alternating positions in the ionic crystal lattice.",
                    "When the octet or lattice is complete, press FORM BOND. RESET lets you restart a puzzle without using energy."
                };
            if(Stage==3)
                return new[]{
                    "Build covalent bonds by sharing electrons in the correct bonding zones.",
                    Level==1?"DRAG electrons to form H2, then complete both O-H bonds in H2O.":
                        Level==2?"Drag the correct electrons into nitrogen's shared pairs to build NH3, respecting its lone pair.":
                        "Drag electron pairs into the correct bond zones to form the required double bonds.",
                    "Check the molecule and follow the on-screen instructions. Wrong attempts give one hint before a failed run."
                };
            if(Stage==4)
                return new[]{
                    "Investigate metallic bonding and the sea of delocalized electrons.",
                    Level==1?"Choose Cu or Mg. DRAG metal ions into the lattice and electrons into the electron cloud.":
                        Level==2?"Connect the correct pieces to model why metals conduct electricity.":
                        "Arrange the metallic layers and observe what happens during deformation.",
                    "Follow each experiment's task, then press CHECK. RESET lets you try the arrangement again."
                };
            return new[]{
                "Welcome to Element Crucible! Test the properties of unknown materials A, B and C.",
                Level==1?"DRAG a sample to the heating bench. Adjust heat and press APPLY HEAT to record observations.":
                    Level==2?"DRAG a sample to the mechanical bench. Test both STRIKE and PRESS for each sample.":
                    "DRAG samples to the station, compare SOLID, MOLTEN and AQUEOUS conductivity, then classify using the combined evidence.",
                "Compare all the observations before submitting. First mistake gives a hint; a second mistaken submission costs one energy."
            };
        }
    }
    private string PracticeMessage
    {
        get
        {
            if(Stage==2)return Level==3?
                "YOUR TURN  •  Drag Na+ and Cl- into alternating lattice slots.":
                "YOUR TURN  •  Drag electrons from the donor's OUTER SHELL to the receiving shell.";
            if(Stage==3)return "YOUR TURN  •  Drag electrons into the correct covalent bond zones.";
            if(Stage==4)return "YOUR TURN  •  Drag and arrange the particles as instructed.";
            if(Stage==5)return "YOUR TURN  •  Drag sample A onto the experiment station.";
            return "YOUR TURN  •  Follow the laboratory instructions and start the experiment.";
        }
    }

    private void Awake()
    {
        // The installer makes editable UI, but runtime creation also covers
        // older scenes if only this component was added.
        if(transform.Find("ChemQuestTutorialCanvas")==null)BuildEditableUI();
        Cache();
    }
    private IEnumerator Start()
    {
        if(overlay!=null)overlay.SetActive(false);
        if(practiceBanner!=null)practiceBanner.SetActive(false);
        // Wait for each scene's gameplay controller to initialize its own HUD.
        yield return null;
        yield return null;
        if(ShowOnFirstPlay && PlayerPrefs.GetInt(Key,0)!=1)
        {
            index=0;
            practice=false;
            Show();
        }
    }
    public void BuildEditableUI()
    {
        if(transform.Find("ChemQuestTutorialCanvas")!=null)return;
        var go=new GameObject("ChemQuestTutorialCanvas",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        go.transform.SetParent(transform,false);
        var canvas=go.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=200;
        var scaler=go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.matchWidthOrHeight=1f;

        var shade=CreateImage(go.transform,"TutorialOverlay",Vector2.zero,
            new Vector2(1280,720),Deep);
        stretch(shade.rectTransform);
        var card=CreateImage(shade.transform,"TutorialCard",new Vector2(0,-5),
            new Vector2(750,465),Card);
        card.raycastTarget=false;
        var outline=card.gameObject.AddComponent<Outline>();
        outline.effectColor=new Color32(72,155,204,215);outline.effectDistance=new Vector2(3,3);
        CreateText(shade.transform,"Eyebrow","CHEMQUEST  •  FIRST-TIME GUIDE",17,
            new Vector2(0,165),new Vector2(700,33),Cyan);
        CreateText(shade.transform,"TutorialTitle","WELCOME TO CHEMQUEST",33,
            new Vector2(0,102),new Vector2(695,64),Gold);
        CreateText(shade.transform,"TutorialBody","Learn the controls before starting.",23,
            new Vector2(0,2),new Vector2(646,144),Color.white);
        CreateText(shade.transform,"PageCounter","STEP 1 / 3",15,
            new Vector2(0,-106),new Vector2(390,35),Cyan);
        CreateButton(shade.transform,"BackButton","BACK",new Vector2(-221,-165),
            new Vector2(175,58),new Color32(52,79,119,255));
        CreateButton(shade.transform,"NextButton","NEXT",new Vector2(0,-165),
            new Vector2(214,58),new Color32(9,140,147,255));
        CreateButton(shade.transform,"SkipButton","SKIP TUTORIAL",new Vector2(223,-165),
            new Vector2(195,58),new Color32(63,85,114,255));

        var banner=CreateImage(go.transform,"PracticeGuide",new Vector2(0,260),
            new Vector2(900,94),new Color32(14,55,83,247));
        var bannerOutline=banner.gameObject.AddComponent<Outline>();
        bannerOutline.effectColor=Cyan;bannerOutline.effectDistance=new Vector2(2,2);
        CreateText(banner.transform,"PracticeText","YOUR TURN",18,new Vector2(-45,0),
            new Vector2(780,82),Color.white);
        CreateButton(banner.transform,"GotItButton","GOT IT",new Vector2(376,0),
            new Vector2(115,53),new Color32(13,135,145,255));
        banner.gameObject.SetActive(false);
        shade.gameObject.SetActive(false);
    }
    private static void stretch(RectTransform r)
    {
        r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
        r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
    }
    private static Image CreateImage(Transform p,string name,Vector2 pos,Vector2 size,Color tint)
    {
        var o=new GameObject(name,typeof(RectTransform),typeof(Image));
        o.transform.SetParent(p,false);
        var r=o.GetComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;
        var image=o.GetComponent<Image>();image.color=tint;
        return image;
    }
    private static TMP_Text CreateText(Transform p,string name,string value,int font,
        Vector2 pos,Vector2 size,Color tint)
    {
        var o=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        o.transform.SetParent(p,false);
        var r=o.GetComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;
        var text=o.GetComponent<TextMeshProUGUI>();
        text.text=value;text.fontSize=font;text.color=tint;
        text.alignment=TextAlignmentOptions.Center;
        text.textWrappingMode=TextWrappingModes.Normal;
        text.raycastTarget=false;return text;
    }
    private static Button CreateButton(Transform p,string name,string label,
        Vector2 pos,Vector2 size,Color tint)
    {
        var image=CreateImage(p,name,pos,size,tint);
        var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
        CreateText(image.transform,"Caption",label,18,Vector2.zero,size,Color.white);
        return button;
    }
    private Transform Find(string name)
    {
        foreach(var t in GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
    private void Cache()
    {
        overlay=Find("TutorialOverlay")?.gameObject;
        practiceBanner=Find("PracticeGuide")?.gameObject;
        title=Find("TutorialTitle")?.GetComponent<TMP_Text>();
        body=Find("TutorialBody")?.GetComponent<TMP_Text>();
        counter=Find("PageCounter")?.GetComponent<TMP_Text>();
        nextLabel=Find("NextButton")?.GetComponentInChildren<TMP_Text>(true);
        practiceLabel=Find("PracticeText")?.GetComponent<TMP_Text>();
        backButton=Find("BackButton")?.GetComponent<Button>();
        nextButton=Find("NextButton")?.GetComponent<Button>();
        skipButton=Find("SkipButton")?.GetComponent<Button>();
        gotItButton=Find("GotItButton")?.GetComponent<Button>();
        if(backButton!=null){backButton.onClick.RemoveAllListeners();backButton.onClick.AddListener(Back);}
        if(nextButton!=null){nextButton.onClick.RemoveAllListeners();nextButton.onClick.AddListener(Next);}
        if(skipButton!=null){skipButton.onClick.RemoveAllListeners();skipButton.onClick.AddListener(Skip);}
        if(gotItButton!=null){gotItButton.onClick.RemoveAllListeners();gotItButton.onClick.AddListener(EndPractice);}
    }
    private void Show()
    {
        if(overlay==null)return;
        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
        if(practiceBanner!=null)practiceBanner.SetActive(false);
        title.text=Titles[index];
        body.text=Messages[index];
        counter.text="STEP "+(index+1)+" / "+Titles.Length;
        if(backButton!=null)backButton.interactable=index>0;
        if(nextLabel!=null)nextLabel.text=index==Titles.Length-1?"TRY IT!":"NEXT";
    }
    private void Next()
    {
        if(index<Titles.Length-1){index++;Show();return;}
        FinishGuide(false);
    }
    private void Back(){if(index>0){index--;Show();}}
    private void Skip(){FinishGuide(true);}
    private void FinishGuide(bool skip)
    {
        PlayerPrefs.SetInt(Key,1);
        PlayerPrefs.Save();
        if(overlay!=null)overlay.SetActive(false);
        practice=!skip;
        if(practiceBanner!=null)practiceBanner.SetActive(practice);
        if(practiceLabel!=null)practiceLabel.text=PracticeMessage;
        Debug.Log("ChemQuest tutorial "+(skip?"skipped":"completed")+" for Stage "+Stage+" Level "+Level);
    }
    private void EndPractice(){practice=false;if(practiceBanner!=null)practiceBanner.SetActive(false);}
    /// <summary>Call from a successful first drag to dismiss the guide automatically.</summary>
    public void NotifyFirstInteraction()
    {
        if(practice)EndPractice();
    }
    public void ReplayTutorial()
    {
        if(!isActiveAndEnabled)return;
        index=0;practice=false;Show();
    }
}
