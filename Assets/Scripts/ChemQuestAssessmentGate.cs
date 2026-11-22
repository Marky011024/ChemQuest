using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Parallel 15-item chemistry assessments (three items per stage).
// No hints, no retries, no energy deduction and no score reveal per question.
// Each answer is persisted immediately so closing the Android app cannot
// silently restart or allow changing previously submitted responses.
// Content is a research draft: Grade 9 Science teacher approval is required.
public sealed class ChemQuestAssessmentGate : MonoBehaviour
{
    public const int ItemCount=15;
    public const string PreDone="CQ_PreTestDone";
    public const string PostDone="CQ_PostTestDone";
    public const string PreScore="CQ_PreTestScore";
    public const string PostScore="CQ_PostTestScore";
    public const string PreNext="CQ_PreTestNext";
    public const string PostNext="CQ_PostTestNext";

    private struct Item
    {
        public string prompt,a,b,c,d;
        public int key;
        public Item(string p,string a0,string b0,string c0,string d0,int answer)
        {prompt=p;a=a0;b=b0;c=c0;d=d0;key=answer;}
        public string Choice(int i)=>i==0?a:i==1?b:i==2?c:d;
    }

    // Stage 1: chemical changes; Stage 2: ionic; Stage 3: covalent;
    // Stage 4: metallic; Stage 5: comparative material properties.
    private static readonly Item[] PreItems={
        new Item("Zinc reacts with dilute hydrochloric acid. Which observation indicates chemical change?",
            "A new gas forms","The flask is moved","The metal is rearranged","The table is shaken",0),
        new Item("An experiment warms its surroundings. The reaction is best described as...",
            "Endothermic","Exothermic","A physical mixture","A change of state only",1),
        new Item("Why does limewater turn cloudy when carbon dioxide is introduced?",
            "Oxygen disappears","A metal melts","Calcium carbonate forms","Only water evaporates",2),
        new Item("When sodium forms Na+, it has...",
            "Gained a proton","Shared two neutrons","Gained an electron","Lost one electron",3),
        new Item("What holds Na+ and Cl- together in an ionic compound?",
            "Attraction of opposite charges","Free-floating neutrons","Sharing one electron pair","An external magnet",0),
        new Item("Why can molten sodium chloride conduct electricity?",
            "Its ions can move","Its particles stop moving","Its electrons disappear","It turns into a metal",0),
        new Item("In a covalent bond, atoms primarily...",
            "Transfer protons","Share electron pairs","Become positive ions only","Lose their nuclei",1),
        new Item("How many H-O covalent bonds are found in one H2O molecule?",
            "One","Three","Four","Two",3),
        new Item("In a typical ammonia (NH3) molecule, nitrogen forms how many N-H bonds?",
            "One","Two","Three","Four",2),
        new Item("What particles are mobile in the usual model of metallic bonding?",
            "Delocalized electrons","Fixed positive ions","Neutrons only","Whole atoms escaping",0),
        new Item("Why does a solid metal conduct electricity?",
            "Only its nuclei move","It has freely moving ions","Its electrons are mobile","It contains dissolved salt",2),
        new Item("A metal can be drawn into a thin wire. This property is...",
            "Brittleness","Solubility","Density","Ductility",3),
        new Item("A brittle sample shatters when struck. Which observation best supports this?",
            "It bends into a sheet","It fractures rather than deforms","It emits light","Its mass increases",1),
        new Item("Which state generally permits electrical conduction in a typical ionic compound?",
            "Solid crystal only","Dry powder only","Molten or dissolved","Frozen solid only",2),
        new Item("Can a heating test alone conclusively identify ionic, metallic, or covalent bonding?",
            "Yes, always","Yes, for all solids","Only by sample color","No, use additional evidence",3)
    };
    private static readonly Item[] PostItems={
        new Item("A flask containing zinc and acid produces bubbles. What do the bubbles suggest?",
            "New gas produced by a reaction","Only movement of the flask","The reaction stopped","No chemical change",0),
        new Item("A reaction causes a measured temperature rise in the mixture. What is the most likely energy classification?",
            "Endothermic, heat absorbed","Exothermic, heat released","Neither: only color changed","Evaporation without energy",1),
        new Item("A white precipitate makes CO2-treated limewater look milky. What is it?",
            "Sodium metal","Hydrogen gas","Calcium carbonate","Copper sulfate",2),
        new Item("A neutral chlorine atom becomes Cl-. What happened?",
            "One proton was removed","Its nucleus split","It lost one electron","It gained one electron",3),
        new Item("Why does an ionic crystal contain an ordered arrangement of positive and negative ions?",
            "Electrostatic attractions act between opposite ions","All ions repel each other","Electrons are shared in H-H bonds","Metallic atoms flow freely",0),
        new Item("Solid salt does not light a conductivity tester, but melted salt does. Why?",
            "Mobile ions carry charge in the melt","The melt has no ions","Only solid ions move","Melting creates metallic atoms",0),
        new Item("Two hydrogen atoms form H2. How is the bond formed?",
            "By exchanging neutrons","By sharing electrons","By losing both nuclei","By turning into ions in water",1),
        new Item("Which molecular formula has two O-H single bonds?",
            "CO2","NH3","NaCl","H2O",3),
        new Item("How many hydrogen atoms share electrons with nitrogen in NH3?",
            "One","Two","Three","Five",2),
        new Item("In metallic bonding, what helps positive metal ions remain attracted together?",
            "A sea of delocalized electrons","Fixed neutrons moving between ions","An external electric current","Water molecules",0),
        new Item("A copper wire carries electric current. What is the main mobile charge carrier?",
            "Copper nuclei","Stationary metal ions","Delocalized electrons","Neutrons",2),
        new Item("A sample stretches into wire without breaking. What characteristic is shown?",
            "Brittleness","Thermal insulation","Solubility","Ductility",3),
        new Item("Which result most clearly demonstrates brittleness in a strike test?",
            "The sample flattens","The sample breaks into pieces","The sample becomes magnetic","The sample dissolves",1),
        new Item("An ionic sample fails to conduct as a dry solid. Which test should also be tried?",
            "Only its color","Its shape alone","Conductivity when molten or in solution","Only its mass",2),
        new Item("For an unknown sample, which approach best supports identifying bonding type?",
            "Use just its melting appearance","Use only a heat trial","Use only one sample's color","Combine conductivity and mechanical evidence",3)
    };


    // Firestore snapshots contain only wording and A-D choices; built-in answer
    // keys remain unchanged. Lock the selected set before an attempt starts.
    [Serializable] private sealed class PublishedItem
    {
        public int order; public string prompt,a,b,c,d;
    }
    [Serializable] private sealed class PublishedTest
    {
        public int schema; public string version; public PublishedItem[] items;
    }
    private static string CloudKey(bool isPost)=>isPost?"CQ_PostApprovedSnapshotV1":"CQ_PreApprovedSnapshotV1";
    private static string LockedKey(bool isPost)=>isPost?"CQ_PostAttemptLockedSnapshotV1":"CQ_PreAttemptLockedSnapshotV1";
    private PublishedTest attemptSnapshot;
    private static PublishedTest Parse(string raw)
    {
        if(string.IsNullOrWhiteSpace(raw))return null;
        try
        {
            var data=JsonUtility.FromJson<PublishedTest>(raw);
            if(data==null||data.schema!=1||data.items==null||
               data.items.Length!=ItemCount||string.IsNullOrWhiteSpace(data.version))return null;
            for(int i=0;i<ItemCount;i++)
            {
                var q=data.items[i];
                if(q==null||q.order!=i+1||string.IsNullOrWhiteSpace(q.prompt)||
                   string.IsNullOrWhiteSpace(q.a)||string.IsNullOrWhiteSpace(q.b)||
                   string.IsNullOrWhiteSpace(q.c)||string.IsNullOrWhiteSpace(q.d))return null;
                var opts=new[]{q.a.Trim(),q.b.Trim(),q.c.Trim(),q.d.Trim()};
                for(int j=0;j<4;j++)for(int k=j+1;k<4;k++)
                    if(string.Equals(opts[j],opts[k],StringComparison.OrdinalIgnoreCase))return null;
            }
            return data;
        }
        catch(Exception){return null;}
    }
    public static bool TryCacheApprovedTest(string kind,string json)
    {
        bool isPost=kind=="post";
        if(!isPost&&kind!="pre")return false;
        if(Parse(json)==null)return false;
        PlayerPrefs.SetString(CloudKey(isPost),json);
        PlayerPrefs.Save();
        return true;
    }
    private void LockAttemptSnapshot()
    {
        // Once selected, even app restart will use the same assessment version.
        string key=LockedKey(post);
        if(PlayerPrefs.HasKey(key))
        {
            attemptSnapshot=Parse(PlayerPrefs.GetString(key,""));
            return;
        }
        string raw=PlayerPrefs.GetString(CloudKey(post),"");
        attemptSnapshot=Parse(raw);
        PlayerPrefs.SetString(key,attemptSnapshot==null?"":raw);
        PlayerPrefs.Save();
        if(attemptSnapshot!=null)
            Debug.Log("CHEMQUEST "+(post?"POST":"PRE")+"-TEST: locked approved snapshot "+attemptSnapshot.version);
    }
    private Item GetItem(int index)
    {
        var original=(post?PostItems:PreItems)[index];
        var q=attemptSnapshot!=null?attemptSnapshot.items[index]:null;
        return q==null?original:new Item(q.prompt,q.a,q.b,q.c,q.d,original.key);
    }

    private bool post;
    private GameObject overlay;
    private TMP_Text titleLabel, progressLabel, questionLabel, hintLabel;
    private readonly TMP_Text[] choices=new TMP_Text[4];
    private bool finished;

    public static bool PreCompleted=>PlayerPrefs.GetInt(PreDone,0)==1;
    public static bool PostCompleted=>PlayerPrefs.GetInt(PostDone,0)==1;

    private System.Action afterPreTest;

    public void OpenPreIfNeeded() => OpenPreIfNeeded(null);

    public void OpenPreIfNeeded(System.Action onComplete)
    {
        if (PreCompleted || !HasProfile() || overlay != null)
            return;

        afterPreTest = onComplete;
        Open(false);
    }

    public void OpenPostIfEligible()
    {
        if(ChemQuestCertificate.AllLevelsComplete() && !PostCompleted && HasProfile())Open(true);
    }

    private static bool HasProfile()
    {
        return !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentName","")) &&
               !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentSchool","")) &&
               !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentSection",""));
    }

    private void Open(bool isPost)
    {
        if(overlay!=null)return;
        post=isPost;
        finished=false;
        LockAttemptSnapshot();
        CreatePanel();
        DisplayNext();
    }

    private string ProgressKey=>post?PostNext:PreNext;
    private string ScoreKey=>post?PostScore:PreScore;
    private string DoneKey=>post?PostDone:PreDone;
    private string AnswerPrefix=>post?"CQ_PostTestAnswer":"CQ_PreTestAnswer";

    private void DisplayNext()
    {
        if(finished)return;
        int index=Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey,0),0,ItemCount);
        if(index>=ItemCount){Finish();return;}
        var item=GetItem(index);
        titleLabel.text=post?"CHEMQUEST  •  POST-TEST":"CHEMQUEST  •  PRE-TEST";
        progressLabel.text="QUESTION "+(index+1)+" / "+ItemCount+"     •     "+
            "STAGE "+(index/3+1)+" TOPIC";
        questionLabel.text=item.prompt;
        for(int i=0;i<4;i++)choices[i].text=((char)('A'+i))+".  "+item.Choice(i);
        hintLabel.text="Choose one answer. No hints, retries, or energy deductions.";
    }

    private void Answer(int selected)
    {
        if(finished)return;
        int index=PlayerPrefs.GetInt(ProgressKey,0);
        if(index<0||index>=ItemCount)return;
        var item=GetItem(index);
        PlayerPrefs.SetInt(AnswerPrefix+index,selected);
        if(selected==item.key)PlayerPrefs.SetInt(ScoreKey,
            PlayerPrefs.GetInt(ScoreKey,0)+1);
        PlayerPrefs.SetInt(ProgressKey,index+1);
        PlayerPrefs.Save();
        if(index+1==ItemCount)Finish();
        else DisplayNext();
    }

    private void Finish()
    {
        if(finished)return;
        finished=true;
        PlayerPrefs.SetInt(DoneKey,1);
        PlayerPrefs.SetString(post?"CQ_PostTestFinishedUtc":"CQ_PreTestFinishedUtc",
            DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
        Debug.Log("CHEMQUEST "+(post?"POST":"PRE")+"-TEST completed: "+
            PlayerPrefs.GetInt(ScoreKey,0)+"/"+ItemCount);
        if(overlay!=null)Destroy(overlay);
        if(post)
        {
            // Reopen to refresh the certificate, which now includes test scores.
            SceneManager.LoadScene("ChemQuestCertificate");
        }
        else
        {
            var completed = afterPreTest;
            afterPreTest = null;
            completed?.Invoke();
        }
    }

    private void CreatePanel()
    {
        overlay=new GameObject("ChemQuestAssessmentOverlay",
            typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas=overlay.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=650;
        var scaler=overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1f;
        EnsureEventSystem();
        var backdrop=Image(overlay.transform,"ScreenBlocker",
            new Color32(4,14,31,244),Vector2.zero,new Vector2(1280,720));
        backdrop.rectTransform.anchorMin=Vector2.zero;
        backdrop.rectTransform.anchorMax=Vector2.one;
        backdrop.rectTransform.offsetMin=Vector2.zero;
        backdrop.rectTransform.offsetMax=Vector2.zero;
        var card=Image(backdrop.transform,"AssessmentCard",
            new Color32(23,48,77,255),Vector2.zero,new Vector2(990,590));
        card.raycastTarget=true;
        titleLabel=Text(card.transform,"AssessmentTitle","",36,
            new Color32(255,199,88,255),new Vector2(0,239),new Vector2(905,60));
        progressLabel=Text(card.transform,"AssessmentProgress","",20,
            new Color32(102,221,239,255),new Vector2(0,174),new Vector2(905,45));
        questionLabel=Text(card.transform,"AssessmentQuestion","",26,Color.white,
            new Vector2(0,60),new Vector2(884,143));
        hintLabel=Text(card.transform,"AssessmentDirections","",16,
            new Color32(211,223,234,255),new Vector2(0,-224),new Vector2(895,49));
        for(int i=0;i<4;i++)
        {
            int choice=i;
            float x=i%2==0?-228:228;
            float y=i<2?-94:-171;
            var button=Image(card.transform,"Choice"+i,
                i%2==0?new Color32(24,132,149,255):new Color32(64,93,133,255),
                new Vector2(x,y),new Vector2(429,66));
            var b=button.gameObject.AddComponent<Button>();
            b.targetGraphic=button;
            choices[i]=Text(button.transform,"ChoiceText","",18,Color.white,
                Vector2.zero,new Vector2(405,62));
            b.onClick.AddListener(()=>Answer(choice));
        }
    }

    private static Image Image(Transform parent,string name,Color color,Vector2 position,Vector2 size)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        obj.transform.SetParent(parent,false);
        var rect=obj.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=position;
        rect.sizeDelta=size;
        var img=obj.GetComponent<Image>();
        img.color=color;
        return img;
    }

    private static TMP_Text Text(Transform parent,string name,string words,int size,
        Color color,Vector2 pos,Vector2 dims)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent,false);
        var rect=obj.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;
        rect.sizeDelta=dims;
        var text=obj.GetComponent<TextMeshProUGUI>();
        text.font=TMP_Settings.defaultFontAsset;
        text.fontSize=size;
        text.textWrappingMode=TextWrappingModes.Normal;
        text.overflowMode=TextOverflowModes.Ellipsis;
        text.color=color;
        text.text=words;
        text.alignment=TextAlignmentOptions.Center;
        text.raycastTarget=false;
        return text;
    }

    private static void EnsureEventSystem()
    {
        var es=FindFirstObjectByType<EventSystem>();
        if(es==null)new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
    }
}
