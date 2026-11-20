using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// A modal educational hint shown on the first wrong answer.
// It blocks the level and assessment UI until the player presses TRY AGAIN.
// Existing wrong-attempt counters, scores, energy and stage-failure logic stay
// with the calling gameplay manager, preventing duplicated penalties.
public static class ChemQuestHintPopup
{
    private static GameObject current;
    public static bool IsOpen => current != null;

    public static void Show(string hint, Action onRetry = null)
    {
        if(current != null) return;
        var obj=new GameObject("ChemQuestConceptHintPopup",typeof(RectTransform),
            typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        current=obj;
        var canvas=obj.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting=true;
        // Above the Prof Atom tutorial canvas (32000), so the retry action stays clickable.
        canvas.sortingOrder=32050;
        var scaler=obj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=.5f;
        if(UnityEngine.Object.FindFirstObjectByType<EventSystem>()==null)
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        var background=MakeImage("Backdrop",obj.transform,new Color32(4,12,30,233));
        var bg=background.rectTransform;
        bg.anchorMin=Vector2.zero;
        bg.anchorMax=Vector2.one;
        bg.offsetMin=bg.offsetMax=Vector2.zero;

        var panel=MakeImage("ConceptHintCard",background.transform,new Color32(20,46,77,255));
        Position(panel.rectTransform,new Vector2(790,395),new Vector2(0,0));
        var gold=new Color32(255,205,100,255);
        MakeText("Header",panel.transform,"CONCEPT HINT",36,gold,new Vector2(0,138),new Vector2(735,65));
        MakeText("Subtitle",panel.transform,"Not quite! Review the chemistry concept before your second attempt.",
            19,new Color32(202,227,243,255),new Vector2(0,80),new Vector2(705,60));
        string clean=string.IsNullOrWhiteSpace(hint)
            ? "Review the lesson carefully and try a different answer."
            : hint.Trim();
        if(clean.StartsWith("HINT:",StringComparison.OrdinalIgnoreCase))
            clean=clean.Substring(5).Trim();
        MakeText("ConceptText",panel.transform,clean,24,Color.white,new Vector2(0,-12),new Vector2(692,135));

        var buttonImage=MakeImage("TryAgainButton",panel.transform,new Color32(13,140,153,255));
        Position(buttonImage.rectTransform,new Vector2(390,76),new Vector2(0,-144));
        var button=buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic=buttonImage;
        button.transition=Selectable.Transition.ColorTint;
        MakeText("ButtonLabel",buttonImage.transform,"TRY AGAIN",25,Color.white,Vector2.zero,new Vector2(375,72));
        button.onClick.AddListener(()=>{
            var toClose=current;
            current=null;
            if(toClose!=null)UnityEngine.Object.Destroy(toClose);
            onRetry?.Invoke();
        });
        obj.transform.SetAsLastSibling();
    }

    private static Image MakeImage(string name,Transform parent,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var image=go.GetComponent<Image>();
        image.color=color;
        image.raycastTarget=true;
        return image;
    }
    private static void Position(RectTransform rect,Vector2 size,Vector2 position)
    {
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=new Vector2(.5f,.5f);
        rect.sizeDelta=size;
        rect.anchoredPosition=position;
    }
    private static void MakeText(string name,Transform parent,string text,int fontSize,
        Color color,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        Position(go.GetComponent<RectTransform>(),size,position);
        var label=go.GetComponent<TextMeshProUGUI>();
        label.font=TMP_Settings.defaultFontAsset;
        label.text=text;
        label.fontSize=fontSize;
        label.color=color;
        label.alignment=TextAlignmentOptions.Center;
        label.textWrappingMode=TextWrappingModes.Normal;
        label.overflowMode=TextOverflowModes.Ellipsis;
        label.raycastTarget=false;
    }
}
