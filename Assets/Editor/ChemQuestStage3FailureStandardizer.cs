using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class ChemQuestStage3FailureStandardizer
{
    [MenuItem("Tools/ChemQuest/Fix Stage 3 Levels 2 and 3 Failed Screens")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        RegisterScenes();
        int count=0;
        for(int level=2;level<=3;level++)
        {
            string path="Assets/Scenes/Stage3Level"+level+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogError("Missing scene: "+path);continue;
            }
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            var root=GameObject.Find(level==2?"Stage3Level2GeneratedCanvas":"Stage3Level3Canvas");
            if(root==null)
            {
                Debug.LogError("Missing editable UI canvas in "+path+
                    ". Build that level first, then run this fix.");
                continue;
            }
            string panelName=level==2?"Level2FailedPanel":"FailedPanel";
            Transform failed=Find(root.transform,panelName);
            if(failed==null)
            {
                Debug.LogError("Missing "+panelName+" in "+path);
                continue;
            }
            StandardizeFailed(failed,level);
            // Ensure modal displays above all puzzle visuals in the same canvas.
            // Only change sibling order, not the game logic or energy logic.
            failed.SetAsLastSibling();
            failed.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            count++;
        }
        EditorUtility.DisplayDialog("ChemQuest",
            "Updated and saved "+count+" Stage 3 failed screens. Test two wrong attempts, Retry, and Level Selection in each level.",
            "OK");
    }

    private static void StandardizeFailed(Transform modal,int level)
    {
        var backdrop=modal.GetComponent<Image>();
        if(backdrop==null)backdrop=modal.gameObject.AddComponent<Image>();
        backdrop.color=new Color32(5,12,27,237);
        backdrop.raycastTarget=true;
        var rect=modal.GetComponent<RectTransform>();
        if(rect!=null)
        {
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.pivot=new Vector2(.5f,.5f);
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            rect.anchoredPosition=Vector2.zero;
        }
        Transform card=Find(modal,"Panel");
        if(card==null)card=Find(modal,"ModalCard");
        if(card==null)
        {
            var go=new GameObject("FailureCard",typeof(RectTransform),typeof(Image));
            go.transform.SetParent(modal,false);
            card=go.transform;
        }
        var cRect=card.GetComponent<RectTransform>();
        if(cRect!=null)
        {
            cRect.anchorMin=cRect.anchorMax=new Vector2(.5f,.5f);
            cRect.anchoredPosition=Vector2.zero;
            cRect.sizeDelta=new Vector2(670,400);
        }
        var cardImage=card.GetComponent<Image>();
        if(cardImage==null)cardImage=card.gameObject.AddComponent<Image>();
        cardImage.color=new Color32(20,43,75,255);
        cardImage.raycastTarget=false;
        var shadow=card.GetComponent<Shadow>();
        if(shadow==null)shadow=card.gameObject.AddComponent<Shadow>();
        shadow.effectColor=new Color(0,0,.07f,.75f);
        shadow.effectDistance=new Vector2(0,-7);

        Transform title=Find(modal,level==2?"Heading":"Title");
        if(title!=null)
        {
            var txt=title.GetComponent<TMP_Text>();
            if(txt!=null)
            {
                txt.text="STAGE FAILED!";
                txt.color=new Color32(255,95,99,255);
                txt.fontSize=40;
                txt.fontStyle=FontStyles.Bold;
                txt.raycastTarget=false;
            }
            Place(title,new Vector2(0,116),new Vector2(640,68));
            title.SetAsLastSibling();
        }
        Transform desc=Find(modal,level==2?"Description":"Message");
        if(desc!=null)
        {
            var txt=desc.GetComponent<TMP_Text>();
            if(txt!=null)
            {
                txt.text="Maximum wrong attempts reached.\\nOne energy has been deducted.";
                txt.color=Color.white;
                txt.fontSize=22;
                txt.raycastTarget=false;
            }
            Place(desc,new Vector2(0,32),new Vector2(630,87));
            desc.SetAsLastSibling();
        }
        Transform retry=Find(modal,"RetryButton");
        Transform selection=Find(modal,level==2?"ReturnButton":"FailedLevelSelectionButton");
        SetupButton(retry,"RETRY",new Vector2(-154,-111),new Color32(24,93,137,255));
        SetupButton(selection,"LEVEL SELECTION",new Vector2(154,-111),new Color32(40,75,116,255));

        // These button objects already have their OnClick listeners configured
        // by Stage3Level2AmmoniaGame / Stage3Level3Game in Start/BindScene.
        // Preserve their exact names and components so wiring is unaffected.
    }

    private static void SetupButton(Transform button,string label,Vector2 pos,Color color)
    {
        if(button==null)
        {
            Debug.LogWarning("Stage 3 failed panel missing "+label+" button");
            return;
        }
        Place(button,pos,new Vector2(280,65));
        var image=button.GetComponent<Image>();
        if(image!=null){image.color=color;image.raycastTarget=true;}
        var component=button.GetComponent<Button>();
        if(component!=null)
        {
            component.interactable=true;
            if(component.targetGraphic==null && image!=null)
                component.targetGraphic=image;
        }
        var caption=button.GetComponentInChildren<TMP_Text>(true);
        if(caption!=null)
        {
            caption.text=label;
            caption.fontStyle=FontStyles.Bold;
            caption.enableAutoSizing=true;
            caption.fontSizeMin=14;
            caption.fontSizeMax=20;
            caption.raycastTarget=false;
        }
        button.SetAsLastSibling();
    }
    private static void Place(Transform t,Vector2 pos,Vector2 size)
    {
        var rect=t.GetComponent<RectTransform>();
        if(rect==null)return;
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.anchoredPosition=pos;
        rect.sizeDelta=size;
    }
    private static Transform Find(Transform t,string name)
    {
        foreach(var x in t.GetComponentsInChildren<Transform>(true))
            if(x.name==name)return x;
        return null;
    }
    private static void RegisterScenes()
    {
        var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach(var scene in new[]{"StageSelection","Stage3LevelSelection",
            "Stage3Level1","Stage3Level2","Stage3Level3"})
        {
            string path="Assets/Scenes/"+scene+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)continue;
            int idx=list.FindIndex(x=>x.path==path);
            if(idx<0)list.Add(new EditorBuildSettingsScene(path,true));
            else if(!list[idx].enabled)
                list[idx]=new EditorBuildSettingsScene(path,true);
        }
        EditorBuildSettings.scenes=list.ToArray();
    }
}
