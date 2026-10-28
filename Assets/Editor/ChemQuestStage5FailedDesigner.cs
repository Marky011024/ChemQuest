using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class ChemQuestStage5FailedDesigner
{
    [MenuItem("Tools/ChemQuest/Polish Failed Screens - ALL Stage 5 Levels")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        int updated=0;
        for(int level=1;level<=3;level++)
        {
            string path="Assets/Scenes/Stage5Level"+level+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogWarning("Stage 5 scene has not been built: "+path);
                continue;
            }
            Scene scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            GameObject canvas=GameObject.Find("Stage5Level"+level+"Canvas");
            if(canvas==null)
            {
                Debug.LogWarning("Stage 5 canvas missing: "+path);
                continue;
            }
            var overlay=Find(canvas.transform,"FailedPanel");
            if(overlay==null)
            {
                Debug.LogWarning("Stage 5 FailedPanel missing: "+path);
                continue;
            }
            var rect=overlay.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=Vector2.zero;
                rect.anchorMax=Vector2.one;
                rect.offsetMin=Vector2.zero;
                rect.offsetMax=Vector2.zero;
                rect.localScale=Vector3.one;
            }
            var image=overlay.GetComponent<Image>();
            if(image!=null)
            {
                image.color=new Color32(4,11,25,232);
                image.raycastTarget=true;
            }
            var card=Find(overlay,"ModalCard");
            if(card!=null)
            {
                var r=card.GetComponent<RectTransform>();
                if(r!=null)
                {
                    r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
                    r.anchoredPosition=Vector2.zero;
                    r.sizeDelta=new Vector2(670,400);
                }
                var background=card.GetComponent<Image>();
                if(background!=null)
                {
                    background.color=new Color32(24,46,79,255);
                    background.raycastTarget=false;
                }
                var shadow=card.GetComponent<Shadow>();
                if(shadow==null)shadow=card.gameObject.AddComponent<Shadow>();
                shadow.effectColor=new Color(0,.02f,.06f,.65f);
                shadow.effectDistance=new Vector2(0,-7);
            }
            FormatText(Find(overlay,"Title"),"STAGE FAILED!",
                new Vector2(0,113),new Vector2(630,65),37,
                new Color32(255,100,108,255),true);
            FormatText(Find(overlay,"Description"),
                "Maximum wrong attempts reached.\nOne energy has been deducted.",
                new Vector2(0,32),new Vector2(610,90),20,Color.white,false);
            FormatButton(Find(overlay,"RetryButton"),"RETRY",new Vector2(-153,-110),
                new Color32(24,96,144,255));
            FormatButton(Find(overlay,"FailLevelButton"),"LEVEL SELECTION",
                new Vector2(153,-110),new Color32(42,79,119,255));

            // Important: keep the existing component names and navigation logic.
            overlay.SetAsLastSibling();
            overlay.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updated++;
        }
        EditorUtility.DisplayDialog("ChemQuest",
            "Polished "+updated+" Stage 5 Failed Panels to match the Stage 3 Level 1 full-screen overlay. Retry and Level Selection remain connected by the gameplay controller.",
            "OK");
    }

    private static void FormatButton(Transform target,string caption,Vector2 pos,Color color)
    {
        if(target==null)return;
        var rect=target.GetComponent<RectTransform>();
        if(rect!=null)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=pos;
            rect.sizeDelta=new Vector2(278,65);
        }
        var img=target.GetComponent<Image>();
        if(img!=null){img.color=color;img.raycastTarget=true;}
        var button=target.GetComponent<Button>();
        if(button!=null)
        {
            button.interactable=true;
            if(button.targetGraphic==null)button.targetGraphic=img;
        }
        var label=target.GetComponentInChildren<TMP_Text>(true);
        if(label!=null)
        {
            label.text=caption;
            label.fontStyle=FontStyles.Bold;
            label.enableAutoSizing=true;
            label.fontSizeMin=13;label.fontSizeMax=19;
            label.color=Color.white;
            label.raycastTarget=false;
        }
        target.SetAsLastSibling();
    }
    private static void FormatText(Transform target,string content,Vector2 pos,
        Vector2 size,int fontsize,Color color,bool bold)
    {
        if(target==null)return;
        var rect=target.GetComponent<RectTransform>();
        if(rect!=null)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
        }
        var text=target.GetComponent<TMP_Text>();
        if(text!=null)
        {
            text.text=content;
            text.fontSize=fontsize;
            text.color=color;
            text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
            text.alignment=TextAlignmentOptions.Center;
            text.raycastTarget=false;
        }
        target.SetAsLastSibling();
    }
    private static Transform Find(Transform root,string name)
    {
        foreach(var child in root.GetComponentsInChildren<Transform>(true))
            if(child.name==name)return child;
        return null;
    }
}
