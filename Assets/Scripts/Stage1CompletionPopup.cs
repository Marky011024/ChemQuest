using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shared Stage 1 victory popup using the same centered dark-blue card,
// score formatting, and navigation pattern as Stage 2 Level 1.
// Called only AFTER the existing BOW assessment saves the result.
public sealed class Stage1CompletionPopup : MonoBehaviour
{
    private GameObject overlay;
    private TextMeshProUGUI headingText, descriptionText, scoreText;
    private Button nextButton, selectionButton;
    private int activeLevel;

    private static readonly Color32 Navy=new Color32(7,18,39,205);
    private static readonly Color32 Card=new Color32(24,49,80,255);
    private static readonly Color32 Gold=new Color32(255,195,74,255);
    private static readonly Color32 Teal=new Color32(17,137,148,255);
    private static readonly Color32 Slate=new Color32(76,103,147,255);

    public void Show(int level,int score,TMP_FontAsset preferredFont)
    {
        activeLevel=Mathf.Clamp(level,1,3);
        if(overlay==null)Build(preferredFont);
        if(overlay==null)
        {
            Debug.LogError("CHEMQUEST Stage 1: Cannot display completion popup.");
            return;
        }
        headingText.text="LEVEL COMPLETE!";
        descriptionText.text=activeLevel==1
            ?"Chemical change investigation successfully completed."
            :activeLevel==2
                ?"Thermal and color reaction successfully completed."
                :"Precipitate puzzle successfully completed.";
        scoreText.text="FINAL SCORE: "+Mathf.Clamp(score,0,100).ToString("D6");
        overlay.SetActive(true);
        // Ensure the completion canvas is above the game, the previous BOW,
        // and any other scene-specific UI canvases.
        var canvas=overlay.GetComponent<Canvas>();
        if(canvas!=null)canvas.sortingOrder=400;
    }

    private void Build(TMP_FontAsset font)
    {
        overlay=new GameObject("Stage1UnifiedLevelComplete",
            typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        // A Screen Space Overlay canvas must be a root GameObject.
        // Creating it in the active scene ensures it is removed on scene load.
        var canvas=overlay.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        canvas.sortingOrder=400;
        var scaler=overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=1f;
        var shade=Image(overlay.transform,"DimmedBackground",Navy,Vector2.zero,
            new Vector2(1280,720));
        shade.rectTransform.anchorMin=Vector2.zero;
        shade.rectTransform.anchorMax=Vector2.one;
        shade.rectTransform.offsetMin=Vector2.zero;
        shade.rectTransform.offsetMax=Vector2.zero;
        shade.raycastTarget=true;
        var card=Image(shade.transform,"CompleteCard",Card,Vector2.zero,
            new Vector2(700,400));
        card.raycastTarget=true;
        headingText=Label(card.transform,"Heading","LEVEL COMPLETE!",36,Gold,
            new Vector2(0,125),new Vector2(640,58),font);
        descriptionText=Label(card.transform,"Description","",21,Color.white,
            new Vector2(0,42),new Vector2(650,60),font);
        scoreText=Label(card.transform,"FinalScore","FINAL SCORE: 000100",25,Gold,
            new Vector2(0,-41),new Vector2(620,50),font);
        selectionButton=MakeButton(card.transform,"LevelSelectionButton",
            "LEVEL SELECTION",new Vector2(-149,-145),new Vector2(263,61),
            Slate,font,()=>Navigate("Stage1LevelSelection"));
        nextButton=MakeButton(card.transform,"NextLevelButton",
            "NEXT LEVEL",new Vector2(149,-145),new Vector2(263,61),
            Teal,font,()=>Navigate(activeLevel==3?"Stage2Level1":
                "Stage1Level"+(activeLevel+1)));
    }

    private static Image Image(Transform parent,string name,Color color,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;
        rect.sizeDelta=size;
        var image=go.GetComponent<Image>();
        image.color=color;
        return image;
    }

    private static TextMeshProUGUI Label(Transform parent,string name,string words,
        int fontSize,Color color,Vector2 pos,Vector2 size,TMP_FontAsset font)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;
        rect.sizeDelta=size;
        var text=go.GetComponent<TextMeshProUGUI>();
        text.font=font!=null?font:TMP_Settings.defaultFontAsset;
        text.text=words;
        text.color=color;
        text.fontSize=fontSize;
        text.alignment=TextAlignmentOptions.Center;
        text.textWrappingMode=TextWrappingModes.Normal;
        text.raycastTarget=false;
        return text;
    }

    private static Button MakeButton(Transform parent,string name,string words,
        Vector2 pos,Vector2 size,Color background,TMP_FontAsset font,UnityEngine.Events.UnityAction action)
    {
        var image=Image(parent,name,background,pos,size);
        var button=image.gameObject.AddComponent<Button>();
        button.targetGraphic=image;
        Label(image.transform,"Text",words,19,Color.white,Vector2.zero,
            new Vector2(size.x-12,size.y-8),font);
        button.onClick.AddListener(action);
        return button;
    }

    private static void Navigate(string name)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {
            Debug.LogError("CHEMQUEST: "+name+" is missing from Build Profiles.");
            return;
        }
        SceneManager.LoadScene(name);
    }
}
