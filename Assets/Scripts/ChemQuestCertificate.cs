using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

// In-game, offline completion certificate. Certificate results come from the
// actual fifteen completed-level keys; no demo/pretend achievements.
public class ChemQuestCertificate : MonoBehaviour
{
    private const string IssuedAtKey="ChemQuestCertificateIssuedUtc";
    private const string CodeKey="ChemQuestCertificateId";
    private Transform page;
    private GameObject controls;
    private TMP_Text status;
    private static readonly Color32 Navy=new Color32(11,26,48,255);
    private static readonly Color32 Gold=new Color32(228,183,85,255);
    private static readonly Color32 Blue=new Color32(46,133,178,255);

    public static bool AllLevelsComplete()
    {
        for(int stage=1;stage<=5;stage++)
            for(int level=1;level<=3;level++)
                if(PlayerPrefs.GetInt("Stage"+stage+"Level"+level+"Completed",0)!=1)return false;
        return true;
    }
    public static void RecordIfEligible()
    {
        if(!AllLevelsComplete() || PlayerPrefs.HasKey(IssuedAtKey))return;
        var now=DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        var name=PlayerPrefs.GetString("StudentName","").Trim();
        var school=PlayerPrefs.GetString("StudentSchool","").Trim();
        var section=PlayerPrefs.GetString("StudentSection","").Trim();
        if(string.IsNullOrEmpty(name)||string.IsNullOrEmpty(school)||string.IsNullOrEmpty(section))
        {
            Debug.LogWarning("ChemQuest certificate: student profile is incomplete.");
            return;
        }
        string payload=name+"|"+school+"|"+section+"|"+now+"|CHEMQUEST";
        byte[] hashed;
        using(var sha=SHA256.Create())hashed=sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var id=BitConverter.ToString(hashed,0,10).Replace("-","");
        PlayerPrefs.SetString(IssuedAtKey,now);
        PlayerPrefs.SetString(CodeKey,"CQ-"+id);
        PlayerPrefs.Save();
    }
    private static int Best(int stage,int level)=>Mathf.Clamp(
        PlayerPrefs.GetInt("Stage"+stage+"Level"+level+"Score",0),0,100);
    private static int Total()
    {
        int sum=0;
        for(int s=1;s<=5;s++)for(int l=1;l<=3;l++)sum+=Best(s,l);
        return sum;
    }
    private static string StageStars(int stage)
    {
        int sum=0;
        for(int l=1;l<=3;l++)sum+=Best(stage,l);
        // Scores alone cannot prove that no earlier failed run occurred.
        // Therefore show performance bands, not invented attempt-history awards.
        if(sum==300)return "3-star score band";
        if(sum>=225)return "2-star score band";
        return "Completion recorded";
    }
    private void Start()
    {
        Time.timeScale=1f;
        if(transform.Find("CertificateCanvas")==null)BuildEditableUI();
        EnsureInputSystem();
        EnsureLockedUI();
        ApplyGameCertificateDesign();
        EnsurePerformanceTable();
        Bind();
        Refresh();
    }
    public void BuildEditableUI()
    {
        if(transform.Find("CertificateCanvas")!=null)return;
        var root=new GameObject("CertificateCanvas",typeof(RectTransform),typeof(Canvas),
            typeof(CanvasScaler),typeof(GraphicRaycaster));
        root.transform.SetParent(transform,false);
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.matchWidthOrHeight=1f;
        var bg=Image(root.transform,"Background",Vector2.zero,new Vector2(1280,720),Navy);
        bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;
        bg.rectTransform.offsetMin=bg.rectTransform.offsetMax=Vector2.zero;
        var cert=Image(root.transform,"CertificatePage",new Vector2(0,41),
            new Vector2(1100,605),new Color32(245,246,237,255));
        cert.raycastTarget=false;
        page=cert.transform;
        var border=Image(page,"GoldBorder",Vector2.zero,new Vector2(1068,571),Gold);
        border.raycastTarget=false;
        var inner=Image(page,"IvoryPaper",Vector2.zero,new Vector2(1057,560),
            new Color32(250,249,240,255));inner.raycastTarget=false;
        var ink=new Color32(24,48,77,255);
        Txt(page,"Brand","CHEMQUEST",30,new Vector2(0,231),new Vector2(900,48),ink);
        Txt(page,"DocumentTitle","CERTIFICATE OF CHEMICAL MASTERY",34,
            new Vector2(0,183),new Vector2(995,60),ink);
        Txt(page,"RecipientLabel","Presented to",18,new Vector2(0,131),
            new Vector2(900,38),ink);
        Txt(page,"RecipientName","STUDENT NAME",37,new Vector2(0,88),new Vector2(970,58),ink);
        Txt(page,"Identity","School  •  Class / Section",17,new Vector2(0,42),
            new Vector2(980,38),ink);
        Txt(page,"Achievement","For successfully completing all 15 levels across five chemistry stages.",
            19,new Vector2(0,1),new Vector2(1020,44),ink);
        Txt(page,"Score","OVERALL GAME SCORE  0%",24,new Vector2(0,-45),
            new Vector2(920,45),ink);
        Txt(page,"Stages","STAGE 1  |  STAGE 2  |  STAGE 3  |  STAGE 4  |  STAGE 5",
            17,new Vector2(0,-96),new Vector2(1000,75),ink);
        Txt(page,"Transcript","Level results appear here",15,new Vector2(0,-174),
            new Vector2(980,104),ink);
        Txt(page,"CertificateMetadata","Issue date  •  Certificate ID",14,
            new Vector2(0,-244),new Vector2(985,40),ink);
        controls=new GameObject("Controls",typeof(RectTransform));
        controls.transform.SetParent(root.transform,false);
        Btn(controls.transform,"ExportPNG","SAVE PNG",new Vector2(-365,-331),
            new Vector2(188,42),Blue);
        Btn(controls.transform,"ExportPDF","SAVE PDF",new Vector2(-137,-331),
            new Vector2(188,42),Blue);
        Btn(controls.transform,"BackButton","STAGE SELECTION",new Vector2(140,-331),
            new Vector2(244,42),new Color32(68,91,118,255));
        status=Txt(controls.transform,"Status","",14,new Vector2(0,-290),
            new Vector2(1050,31),Color.white);
    }

    // Landscape adaptation of the approved navy/cyan/gold game certificate.
    // Everything is a native editable Unity UI object, not flattened artwork.
    public void ApplyGameCertificateDesign()
    {
        var certificate=Find("CertificatePage");
        if(certificate==null)return;
        if(Find("GameDesignHeader")==null)
        {
            var dark=new Color32(10,43,70,255);
            var deep=new Color32(5,25,45,255);
            var teal=new Color32(18,114,136,255);
            var gold=new Color32(236,188,76,255);
            var cream=new Color32(255,250,236,255);
            var cyan=new Color32(92,221,238,255);

            var header=Image(certificate,"GameDesignHeader",new Vector2(0,218),
                new Vector2(1040,123),dark);header.raycastTarget=false;
            var accent=Image(certificate,"GameDesignHeaderAccent",new Vector2(0,151),
                new Vector2(1000,4),gold);accent.raycastTarget=false;
            var topMark=Image(certificate,"ChemistryEmblem",new Vector2(-452,218),
                new Vector2(68,68),teal);topMark.raycastTarget=false;
            Txt(topMark.transform,"ChemistryEmblemText","H2O",23,Vector2.zero,
                new Vector2(67,65),cream);
            var rightMark=Image(certificate,"AtomEmblem",new Vector2(451,218),
                new Vector2(68,68),teal);rightMark.raycastTarget=false;
            Txt(rightMark.transform,"AtomEmblemText","Na+",21,Vector2.zero,
                new Vector2(67,65),cream);

            var identity=Image(certificate,"StudentIdentityPanel",new Vector2(-225,45),
                new Vector2(584,150),new Color32(233,244,240,255));
            identity.raycastTarget=false;
            var stripe=Image(certificate,"StudentIdentityAccent",new Vector2(-511,45),
                new Vector2(7,135),teal);stripe.raycastTarget=false;
            var badge=Image(certificate,"AccuracyBadgePanel",new Vector2(374,45),
                new Vector2(245,150),deep);badge.raycastTarget=false;
            var badgeStripe=Image(certificate,"AccuracyBadgeAccent",new Vector2(374,117),
                new Vector2(231,6),gold);badgeStripe.raycastTarget=false;
            Txt(certificate,"BadgeLabel","CHEMISTRY MASTERY",16,
                new Vector2(373,-18),new Vector2(235,32),teal);
            var bar=Image(certificate,"PerformanceHeader",new Vector2(0,-63),
                new Vector2(1020,40),dark);bar.raycastTarget=false;
            var barStripe=Image(certificate,"PerformanceHeaderAccent",
                new Vector2(-496,-63),new Vector2(8,37),cyan);barStripe.raycastTarget=false;

            for(int i=0;i<5;i++)
            {
                var line=Image(certificate,"TranscriptRow"+(i+1),
                    new Vector2(0,-111-i*26),new Vector2(1000,24),
                    i%2==0?new Color32(232,243,241,255):new Color32(244,247,238,255));
                line.raycastTarget=false;
            }
            var foot=Image(certificate,"CongratulationsBanner",new Vector2(0,-255),
                new Vector2(1028,61),deep);foot.raycastTarget=false;
            Txt(foot.transform,"CongratsLabel","CONGRATULATIONS!",21,
                new Vector2(-190,8),new Vector2(620,34),gold);
            Txt(foot.transform,"CongratsDetail","FIVE STAGES  •  FIFTEEN LEVELS  •  CHEMICAL MASTERY",
                13,new Vector2(-190,-17),new Vector2(620,25),cream);
            Txt(foot.transform,"AchievementTrophy","CQ",32,
                new Vector2(399,0),new Vector2(85,54),gold);
        }
        // Update previously serialized TMP objects even if the scene was built before this change.
        var formula=Find("ChemistryEmblemText")?.GetComponent<TMP_Text>();
        if(formula!=null)formula.text="H2O";
        var ionic=Find("AtomEmblemText")?.GetComponent<TMP_Text>();
        if(ionic!=null)ionic.text="Na+";
        // Correct glyphs already saved in existing scene objects too.
        var trophy=Find("AchievementTrophy")?.GetComponent<TMP_Text>();
        if(trophy!=null)trophy.text="CQ";
        var congrats=Find("CongratsLabel")?.GetComponent<TMP_Text>();
        if(congrats!=null)congrats.text="CONGRATULATIONS!";
        // Existing dynamic text remains connected to its original fields.
        PositionText("Brand",new Vector2(0,250),new Vector2(600,42),26,new Color32(92,221,238,255));
        PositionText("DocumentTitle",new Vector2(0,202),new Vector2(900,63),30,new Color32(249,205,106,255));
        PositionText("Achievement",new Vector2(0,136),new Vector2(990,34),16,new Color32(21,55,76,255));
        PositionText("RecipientLabel",new Vector2(-228,106),new Vector2(550,31),17,new Color32(17,105,126,255));
        PositionText("RecipientName",new Vector2(-225,61),new Vector2(547,54),31,new Color32(9,36,62,255));
        PositionText("Identity",new Vector2(-225,17),new Vector2(550,62),16,new Color32(33,69,87,255));
        PositionText("Score",new Vector2(373,48),new Vector2(226,100),22,new Color32(247,213,118,255));
        PositionText("Stages",new Vector2(0,-29),new Vector2(960,33),20,Color.white);
        PositionText("Transcript",new Vector2(0,-167),new Vector2(984,134),17,new Color32(17,50,73,255));
        PositionText("CertificateMetadata",new Vector2(0,-270),new Vector2(1000,18),12,new Color32(25,68,83,255));
        foreach(var key in new[]{"Brand","DocumentTitle","Achievement","RecipientLabel",
            "RecipientName","Identity","Score","Stages","Transcript","CertificateMetadata"})
        {
            var t=Find(key);
            if(t!=null)t.SetAsLastSibling();
        }

        var identityPanel=Find("StudentIdentityPanel")?.GetComponent<RectTransform>();
        if(identityPanel!=null){identityPanel.anchoredPosition=new Vector2(-225,62);identityPanel.sizeDelta=new Vector2(584,123);}
        var identityStripe=Find("StudentIdentityAccent")?.GetComponent<RectTransform>();
        if(identityStripe!=null){identityStripe.anchoredPosition=new Vector2(-511,62);identityStripe.sizeDelta=new Vector2(7,113);}
        var accuracyPanel=Find("AccuracyBadgePanel")?.GetComponent<RectTransform>();
        if(accuracyPanel!=null){accuracyPanel.anchoredPosition=new Vector2(374,62);accuracyPanel.sizeDelta=new Vector2(245,123);}
        var accuracyStripe=Find("AccuracyBadgeAccent")?.GetComponent<RectTransform>();
        if(accuracyStripe!=null)accuracyStripe.anchoredPosition=new Vector2(374,119);
        var labelBadge=Find("BadgeLabel")?.GetComponent<RectTransform>();
        if(labelBadge!=null)labelBadge.anchoredPosition=new Vector2(374,10);
        var performanceHeaderRect=Find("PerformanceHeader")?.GetComponent<RectTransform>();
        if(performanceHeaderRect!=null){performanceHeaderRect.anchoredPosition=new Vector2(0,-29);performanceHeaderRect.sizeDelta=new Vector2(1020,36);}
        var barAccent=Find("PerformanceHeaderAccent")?.GetComponent<RectTransform>();
        if(barAccent!=null){barAccent.anchoredPosition=new Vector2(-496,-29);barAccent.sizeDelta=new Vector2(8,35);}
        PositionText("RecipientLabel",new Vector2(-228,107),new Vector2(550,25),17,new Color32(17,105,126,255));
        PositionText("RecipientName",new Vector2(-225,72),new Vector2(550,42),29,new Color32(9,36,62,255));
        PositionText("Identity",new Vector2(-225,36),new Vector2(550,27),16,new Color32(33,69,87,255));
        PositionText("Score",new Vector2(374,69),new Vector2(230,78),23,new Color32(247,213,118,255));
        // Remove legacy row strips that sat on top of the new table.
        for(int i=1;i<=5;i++)
        {
            var strip=Find("TranscriptRow"+i);
            if(strip!=null)strip.gameObject.SetActive(false);
        }
        var footer=Find("CongratulationsBanner")?.GetComponent<RectTransform>();
        if(footer!=null){footer.anchoredPosition=new Vector2(0,-233);footer.sizeDelta=new Vector2(1024,39);}
        var footerHeading=Find("CongratsLabel")?.GetComponent<RectTransform>();
        if(footerHeading!=null){footerHeading.anchoredPosition=new Vector2(-195,6);footerHeading.sizeDelta=new Vector2(610,23);}
        var footerCaption=Find("CongratsDetail")?.GetComponent<RectTransform>();
        if(footerCaption!=null){footerCaption.anchoredPosition=new Vector2(-195,-12);footerCaption.sizeDelta=new Vector2(610,14);}
        var trophyMark=Find("AchievementTrophy")?.GetComponent<RectTransform>();
        if(trophyMark!=null){trophyMark.anchoredPosition=new Vector2(401,0);trophyMark.sizeDelta=new Vector2(90,32);}
        var footerText=Find("CongratsLabel")?.GetComponent<TMP_Text>();
        if(footerText!=null){footerText.fontSize=19;footerText.enableAutoSizing=false;}
        var footerNote=Find("CongratsDetail")?.GetComponent<TMP_Text>();
        if(footerNote!=null){footerNote.fontSize=11;footerNote.enableAutoSizing=false;}
        // A locked certificate is not a fake award. Hide the victory footer at runtime.
    }

    // Two-column 15-level table fits a landscape Unity Game View without
    // inventing old retry data. All rows are editable TextMeshPro elements.
    public void EnsurePerformanceTable()
    {
        var cert=Find("CertificatePage");
        if(cert==null)return;
        var old=Find("Transcript");
        if(old!=null)old.gameObject.SetActive(false);
        // Re-create only the display table when applying the design in Edit Mode.
        // This never touches gameplay data, scores, or retry keys.
        var previous=Find("CertificatePerformanceTable");
        if(previous!=null){if(Application.isPlaying)return;DestroyImmediate(previous.gameObject);}

        var navy=new Color32(8,40,66,255);
        var ink=new Color32(20,52,71,255);
        var aqua=new Color32(18,113,136,255);
        var cream=new Color32(243,249,246,255);
        var table=new GameObject("CertificatePerformanceTable",typeof(RectTransform));
        table.transform.SetParent(cert,false);
        var rt=table.GetComponent<RectTransform>();
        rt.anchoredPosition=new Vector2(0,-128);
        rt.sizeDelta=new Vector2(1010,161);
        string[] headings={"LEVEL","SCORE","RETRIES","RESULT"};
        for(int side=0;side<2;side++)
        {
            float origin=side==0?-251f:251f;
            var panel=Image(table.transform,"TablePanel"+side,
                new Vector2(origin,0),new Vector2(491,161),cream);
            panel.raycastTarget=false;
            var head=Image(panel.transform,"Header",new Vector2(0,68),
                new Vector2(486,24),navy);
            head.raycastTarget=false;
            float[] x={-180,-56,40,157};
            float[] w={116,100,91,141};
            for(int j=0;j<4;j++)
                Txt(head.transform,"Heading"+j,headings[j],17,
                    new Vector2(x[j],0),new Vector2(w[j],23),Color.white);
            for(int row=0;row<8;row++)
            {
                int index=side*8+row;
                if(index>=15)break;
                int stage=index/3+1,level=index%3+1;
                string id="S"+stage+"L"+level;
                float y=43-row*16.3f;
                if(row%2==1)
                {
                    var stripe=Image(panel.transform,"RowStripe"+row,
                        new Vector2(0,y),new Vector2(487,16),new Color32(225,240,239,255));
                    stripe.raycastTarget=false;
                }
                Txt(panel.transform,id+"Level",id,17,new Vector2(x[0],y),
                    new Vector2(w[0],16),ink);
                Txt(panel.transform,id+"Score","--",17,new Vector2(x[1],y),
                    new Vector2(w[1],16),aqua);
                Txt(panel.transform,id+"Retries","--",17,new Vector2(x[2],y),
                    new Vector2(w[2],16),ink);
                Txt(panel.transform,id+"Result","--",16,new Vector2(x[3],y),
                    new Vector2(w[3],16),ink);
            }
        }
        table.transform.SetAsLastSibling();
    }
    private void SetPerformanceTableVisible(bool visible)
    {
        var t=Find("CertificatePerformanceTable");
        if(t!=null)t.gameObject.SetActive(visible);
        var old=Find("Transcript");
        if(old!=null)old.gameObject.SetActive(false);
    }
    private void PopulatePerformanceTable()
    {
        for(int s=1;s<=5;s++)
        {
            for(int l=1;l<=3;l++)
            {
                string id="S"+s+"L"+l;
                int score=Best(s,l);
                string retryKey="Stage"+s+"Level"+l+"RetryCount";
                bool known=PlayerPrefs.HasKey("Stage"+s+"Level"+l+"RetryTrackingStarted") ||
                    PlayerPrefs.HasKey(retryKey);
                int retries=PlayerPrefs.GetInt(retryKey,0);
                Set(id+"Score",score+"/100");
                Set(id+"Retries",known?retries.ToString():"--");
                Set(id+"Result",!known?
                    (score==100?"Perfect":score==75?"After hint":"Completed"):
                    (retries>0?"Retried":score==75?"After hint":"No retry"));
            }
        }
    }
    private void PositionText(string name,Vector2 pos,Vector2 size,int font,Color color)
    {
        var t=Find(name)?.GetComponent<TMP_Text>();
        if(t==null)return;
        var rt=t.rectTransform;rt.anchoredPosition=pos;rt.sizeDelta=size;
        t.color=color;t.fontSize=font;t.fontSizeMax=font;
    }

    // The certificate scene can lack an EventSystem; buttons then show but cannot click.
    public static void EnsureInputSystem()
    {
        var existing=UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if(existing==null)
        {
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            Debug.Log("ChemQuest certificate: created missing EventSystem.");
        }
        else if(existing.GetComponent<BaseInputModule>()==null)
            existing.gameObject.AddComponent<InputSystemUIInputModule>();
    }
    public void EnsureLockedUI()
    {
        var certificate=Find("CertificatePage");
        if(certificate==null || Find("LockedProgressText")!=null)return;
        var ink=new Color32(29,62,91,255);
        var lockBox=Image(certificate,"LockedBadge",new Vector2(0,-59),
            new Vector2(195,44),new Color32(238,225,189,255));
        lockBox.raycastTarget=false;
        Txt(lockBox.transform,"LockedBadgeText","LOCKED",21,Vector2.zero,
            new Vector2(185,36),ink);
        Txt(certificate,"LockedProgressText","0 / 15 LEVELS COMPLETE",23,
            new Vector2(0,-113),new Vector2(800,44),ink);
        var track=Image(certificate,"LockedProgressTrack",new Vector2(0,-161),
            new Vector2(620,20),new Color32(220,228,232,255));
        track.raycastTarget=false;
        var fill=Image(track.transform,"LockedProgressFill",Vector2.zero,
            new Vector2(0,20),new Color32(20,146,156,255));
        var rect=fill.rectTransform;
        rect.anchorMin=new Vector2(0,.5f);
        rect.anchorMax=new Vector2(0,.5f);
        rect.pivot=new Vector2(0,.5f);
        rect.anchoredPosition=Vector2.zero;
        fill.raycastTarget=false;
        Txt(certificate,"LockedGuidance","Finish your remaining chemistry puzzles to earn this certificate.",
            18,new Vector2(0,-204),new Vector2(950,49),ink);
        foreach(var name in new[]{"LockedBadge","LockedProgressText","LockedProgressTrack","LockedGuidance"})
        {
            var t=Find(name);
            if(t!=null)t.gameObject.SetActive(false);
        }
    }
    private void SetLockedVisuals(bool locked,int completed)
    {
        foreach(var name in new[]{"LockedBadge","LockedProgressText","LockedProgressTrack","LockedGuidance"})
        {
            var t=Find(name);
            if(t!=null)t.gameObject.SetActive(locked);
        }
        var banner=Find("CongratulationsBanner");
        if(banner!=null)banner.gameObject.SetActive(!locked);
        foreach(var name in new[]{"PerformanceHeader","PerformanceHeaderAccent","TranscriptRow1",
            "TranscriptRow2","TranscriptRow3","TranscriptRow4","TranscriptRow5",
            "AccuracyBadgePanel","AccuracyBadgeAccent","BadgeLabel"})
        {
            var visual=Find(name);
            if(visual!=null)visual.gameObject.SetActive(!locked);
        }
        if(locked)
        {
            PositionText("RecipientLabel",new Vector2(0,102),new Vector2(800,30),18,
                new Color32(17,105,126,255));
            PositionText("RecipientName",new Vector2(0,57),new Vector2(970,48),28,
                new Color32(9,36,62,255));
            PositionText("Identity",new Vector2(0,17),new Vector2(900,34),15,
                new Color32(33,69,87,255));
            PositionText("Achievement",new Vector2(0,136),new Vector2(990,34),16,
                new Color32(21,55,76,255));
            var progress=Find("LockedProgressText")?.GetComponent<RectTransform>();
            if(progress!=null)progress.anchoredPosition=new Vector2(0,-112);
            var guidance=Find("LockedGuidance")?.GetComponent<RectTransform>();
            if(guidance!=null)guidance.anchoredPosition=new Vector2(0,-213);
        }
        else
        {
            PositionText("RecipientLabel",new Vector2(-228,106),new Vector2(550,31),17,
                new Color32(17,105,126,255));
            PositionText("RecipientName",new Vector2(-225,61),new Vector2(547,54),31,
                new Color32(9,36,62,255));
            PositionText("Identity",new Vector2(-225,17),new Vector2(550,62),16,
                new Color32(33,69,87,255));
        }
        if(!locked)return;
        Set("LockedProgressText",completed+" / 15 LEVELS COMPLETE");
        Set("LockedGuidance",completed==14?
            "Only 1 level remaining! Finish it to unlock your certificate.":
            "Complete the remaining "+(15-completed)+" levels to earn your certificate.");
        var rect=Find("LockedProgressFill")?.GetComponent<RectTransform>();
        if(rect!=null)rect.sizeDelta=new Vector2(620f*completed/15f,20);
    }
    private static int CompletedCount()
    {
        int count=0;
        for(int stage=1;stage<=5;stage++)
            for(int level=1;level<=3;level++)
                if(PlayerPrefs.GetInt("Stage"+stage+"Level"+level+"Completed",0)==1)count++;
        return count;
    }
    public void ReturnToStageSelection()
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded("StageSelection"))
        {
            Debug.LogError("ChemQuest certificate: StageSelection is missing or disabled in Build Settings.");
            if(status!=null)status.text="Stage Selection scene is missing from Build Settings.";
            return;
        }
        SceneManager.LoadScene("StageSelection");
    }
    private static Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchoredPosition=pos;rt.sizeDelta=size;
        var img=go.GetComponent<Image>();img.color=color;return img;
    }
    private static TMP_Text Txt(Transform parent,string name,string label,int font,
        Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.anchoredPosition=pos;rt.sizeDelta=size;
        var txt=go.GetComponent<TextMeshProUGUI>();txt.text=label;txt.fontSize=font;
        txt.alignment=TextAlignmentOptions.Center;txt.color=color;txt.raycastTarget=false;
        txt.enableAutoSizing=true;txt.fontSizeMin=12;txt.fontSizeMax=font;
        return txt;
    }
    private static void Btn(Transform parent,string name,string label,Vector2 pos,
        Vector2 size,Color color)
    {
        var image=Image(parent,name,pos,size,color);
        var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
        Txt(image.transform,"Label",label,18,Vector2.zero,size,Color.white);
    }
    private Transform Find(string name)
    {
        foreach(var t in GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
    private void Bind()
    {
        page=Find("CertificatePage");
        controls=Find("Controls")?.gameObject;
        status=Find("Status")?.GetComponent<TMP_Text>();
        BindButton("ExportPNG",()=>StartCoroutine(Export(false)));
        BindButton("ExportPDF",()=>StartCoroutine(Export(true)));
        BindButton("BackButton",ReturnToStageSelection);
    }
    private void BindButton(string name,UnityEngine.Events.UnityAction action)
    {
        var b=Find(name)?.GetComponent<Button>();
        if(b==null)return;
        b.onClick.RemoveAllListeners();b.onClick.AddListener(action);
    }
    private void Set(string name,string value)
    {
        var text=Find(name)?.GetComponent<TMP_Text>();
        if(text!=null)text.text=value;
    }
    private void Refresh()
    {
        RecordIfEligible();
        SetLockedVisuals(!AllLevelsComplete() || !PlayerPrefs.HasKey(IssuedAtKey),CompletedCount());
        if(!AllLevelsComplete() || !PlayerPrefs.HasKey(IssuedAtKey))
        {
            Set("DocumentTitle","CERTIFICATE LOCKED");
            Set("RecipientLabel","PROGRESS TOWARDS CHEMICAL MASTERY");
            Set("Identity","Your progress is saved across all five stages.");
            Set("RecipientName","COMPLETE ALL 15 LEVELS");
            Set("Achievement","Complete all fifteen chemistry puzzle levels to earn your award.");
            Set("Score","");
            Set("Stages","");
            Set("Transcript","");
            SetPerformanceTableVisible(false);
            Set("CertificateMetadata","");
            if(status!=null)status.text="Certificate not yet available.";
            var p=Find("ExportPNG")?.GetComponent<Button>();if(p!=null)p.interactable=false;
            p=Find("ExportPDF")?.GetComponent<Button>();if(p!=null)p.interactable=false;
            return;
        }
        Set("DocumentTitle","CERTIFICATE OF CHEMICAL MASTERY");
        Set("RecipientLabel","Presented to");
        Set("Achievement","For successfully completing all 15 levels across five chemistry stages.");
        Set("RecipientName",PlayerPrefs.GetString("StudentName","STUDENT"));
        Set("Identity",PlayerPrefs.GetString("StudentSchool","")+
            "  •  "+PlayerPrefs.GetString("StudentSection",""));
        Set("Score",(Total()*100f/1500f).ToString("0.0")+"%\nACCURACY");
        var stages=new StringBuilder();
        for(int s=1;s<=5;s++)
        {
            if(s>1)stages.Append("     ");
            stages.Append("S").Append(s).Append(": ").Append(StageStars(s));
        }
        Set("Stages","PERFORMANCE TRANSCRIPT  |  15 LEVEL RESULTS");
        var transcript=new StringBuilder();
        for(int s=1;s<=5;s++)
        {
            transcript.Append(new[]{"","CATALYST CHRONICLES","ION FORGE","ORBITAL OVERLAP","ELECTRON OCEAN","ELEMENT CRUCIBLE"}[s]).Append("    |    ");
            for(int l=1;l<=3;l++)
            {
                transcript.Append("L").Append(l).Append(" ").Append(Best(s,l)).Append("/100");
                if(l<3)transcript.Append("   |   ");
            }
            if(s<5)transcript.Append("\n");
        }
        Set("Transcript",transcript.ToString());
        SetPerformanceTableVisible(true);
        PopulatePerformanceTable();
        Set("CertificateMetadata","ISSUED: "+PlayerPrefs.GetString(IssuedAtKey)+
            "    |    ID: "+PlayerPrefs.GetString(CodeKey)+
            "    |    -- = RETRIES NOT RECORDED");
    }
    private IEnumerator Export(bool pdf)
    {
        if(!AllLevelsComplete() || !PlayerPrefs.HasKey(IssuedAtKey))yield break;
        // Capture the certificate itself, not the Game View background or buttons.
        yield return new WaitForEndOfFrame();
        var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
        if(screenshot==null)yield break;
        Texture2D cropped=null;
        try
        {
            var rect=page.GetComponent<RectTransform>();
            var corners=new Vector3[4];
            rect.GetWorldCorners(corners);
            float sx=(float)screenshot.width/Screen.width;
            float sy=(float)screenshot.height/Screen.height;
            int x=Mathf.Clamp(Mathf.FloorToInt(corners[0].x*sx),0,screenshot.width-1);
            int y=Mathf.Clamp(Mathf.FloorToInt(corners[0].y*sy),0,screenshot.height-1);
            int right=Mathf.Clamp(Mathf.CeilToInt(corners[2].x*sx),x+1,screenshot.width);
            int top=Mathf.Clamp(Mathf.CeilToInt(corners[2].y*sy),y+1,screenshot.height);
            cropped=new Texture2D(right-x,top-y,TextureFormat.RGB24,false);
            cropped.SetPixels(screenshot.GetPixels(x,y,right-x,top-y));
            cropped.Apply();
            var bytes=pdf?EncodePdf(cropped.EncodeToJPG(95),cropped.width,cropped.height):
                cropped.EncodeToPNG();
            string file="ChemQuest_"+PlayerPrefs.GetString(CodeKey,"Certificate").Replace("-","_")+
                (pdf?".pdf":".png");
            string savedLocation;
#if UNITY_ANDROID && !UNITY_EDITOR
            if(GetAndroidApiLevel()>=29)
            {
                // Android 10+: MediaStore publishes the file in the phone's
                // public Downloads folder (no storage permission required).
                savedLocation=SaveInAndroidDownloads(file,bytes,pdf);
            }
            else
            {
                // Older Android versions may require runtime storage permission
                // for public Downloads. Never falsely claim the file is there.
                string privatePath=Path.Combine(Application.persistentDataPath,file);
                File.WriteAllBytes(privatePath,bytes);
                savedLocation="private app storage (Android 9 or earlier): "+privatePath;
            }
#else
            savedLocation=Path.Combine(Application.persistentDataPath,file);
            File.WriteAllBytes(savedLocation,bytes);
#endif
            if(status!=null)status.text="Certificate saved: "+savedLocation;
            Debug.Log("ChemQuest certificate exported: "+savedLocation);
        }
        catch(Exception e)
        {
            if(status!=null)status.text="Export failed: "+e.Message;
            Debug.LogException(e);
        }
        finally
        {
            if(screenshot!=null)Destroy(screenshot);
            if(cropped!=null)Destroy(cropped);
        }
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private static int GetAndroidApiLevel()
    {
        using(var version=new AndroidJavaClass("android.os.Build$VERSION"))
            return version.GetStatic<int>("SDK_INT");
    }

    // Saves into Files > Downloads > ChemQuest on Android 10+.
    // MediaStore is the supported Android scoped-storage API for creating
    // public PDF and PNG documents without special storage permissions.
    private static string SaveInAndroidDownloads(string file,byte[] bytes,bool pdf)
    {
        using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
        using(var resolver=activity.Call<AndroidJavaObject>("getContentResolver"))
        using(var downloads=new AndroidJavaClass("android.provider.MediaStore$Downloads"))
        using(var collection=downloads.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"))
        using(var values=new AndroidJavaObject("android.content.ContentValues"))
        {
            values.Call("put","_display_name",file);
            values.Call("put","mime_type",pdf?"application/pdf":"image/png");
            values.Call("put","relative_path","Download/ChemQuest/");
            using(var pending=new AndroidJavaObject("java.lang.Integer",1))
                values.Call("put","is_pending",pending);
            AndroidJavaObject uri=null;
            bool finished=false;
            try
            {
                uri=resolver.Call<AndroidJavaObject>("insert",collection,values);
                if(uri==null)
                    throw new IOException("Android Downloads is not available.");
                using(var stream=resolver.Call<AndroidJavaObject>("openOutputStream",uri))
                {
                    if(stream==null)
                        throw new IOException("Could not open the Downloads file.");
                    // JNI forwards C# byte[] as a Java byte[].
                    stream.Call("write",bytes);
                    stream.Call("flush");
                }
                values.Call("clear");
                using(var published=new AndroidJavaObject("java.lang.Integer",0))
                    values.Call("put","is_pending",published);
                resolver.Call<int>("update",uri,values,null,null);
                finished=true;
                return "Downloads/ChemQuest/"+file;
            }
            finally
            {
                if(uri!=null)
                {
                    if(!finished)
                    {
                        try { resolver.Call<int>("delete",uri,null,null); }
                        catch(Exception cleanupError)
                        { Debug.LogWarning("Could not remove incomplete certificate: "+cleanupError.Message); }
                    }
                    uri.Dispose();
                }
            }
        }
    }
#endif

    // Minimal standards-compliant one-page PDF with a JPEG image embedded.
    private static byte[] EncodePdf(byte[] jpg,int width,int height)
    {
        using(var stream=new MemoryStream())
        {
            var offsets=new List<long>{0};
            Action<string> write=s=>{var data=Encoding.ASCII.GetBytes(s);stream.Write(data,0,data.Length);};
            write("%PDF-1.4\n");
            Action<int,string> obj=(id,contents)=>{
                offsets.Add(stream.Position);write(id+" 0 obj\n"+contents+"\nendobj\n");
            };
            obj(1,"<< /Type /Catalog /Pages 2 0 R >>");
            obj(2,"<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
            obj(3,"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 463] /Resources << /XObject << /Im0 5 0 R >> >> /Contents 4 0 R >>");
            string content="q 842 0 0 463 0 0 cm /Im0 Do Q\n";
            obj(4,"<< /Length "+Encoding.ASCII.GetByteCount(content)+" >>\nstream\n"+content+"endstream");
            offsets.Add(stream.Position);
            write("5 0 obj\n<< /Type /XObject /Subtype /Image /Width "+width+
                " /Height "+height+" /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length "+
                jpg.Length+" >>\nstream\n");
            stream.Write(jpg,0,jpg.Length);
            write("\nendstream\nendobj\n");
            long xref=stream.Position;
            write("xref\n0 6\n0000000000 65535 f \n");
            for(int i=1;i<=5;i++)write(offsets[i].ToString("D10")+" 00000 n \n");
            write("trailer\n<< /Root 1 0 R /Size 6 >>\nstartxref\n"+xref+"\n%%EOF");
            return stream.ToArray();
        }
    }
}
