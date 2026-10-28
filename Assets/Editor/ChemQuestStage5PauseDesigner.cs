using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Preserve every existing experiment, but replace only Stage 5's generic
// MENU and PAUSE panel with the original Stage 1 prefabs and layout.
public static class ChemQuestStage5PauseDesigner
{
    [MenuItem("Tools/ChemQuest/Apply Stage 1 Menu to ALL Stage 5 Levels")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("ChemQuest","Exit Play Mode first.","OK");
            return;
        }
        var buttonPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseButton.prefab");
        var menuPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PauseMenuPanel.prefab");
        if(buttonPrefab==null || menuPrefab==null)
        {
            EditorUtility.DisplayDialog("ChemQuest",
                "Stage 1 PauseButton.prefab or PauseMenuPanel.prefab was not found. No scenes changed.","OK");
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        int updated=0;
        for(int level=1;level<=3;level++)
        {
            string path="Assets/Scenes/Stage5Level"+level+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                Debug.LogWarning("Stage 5 gameplay scene not built: "+path);
                continue;
            }
            Scene scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            var canvas=GameObject.Find("Stage5Level"+level+"Canvas");
            if(canvas==null)
            {
                Debug.LogWarning("Stage 5 canvas not found in "+path);
                continue;
            }
            var originalMenu=Find(canvas.transform,"MenuButton");
            if(originalMenu!=null)Object.DestroyImmediate(originalMenu.gameObject);
            var oldPanel=Find(canvas.transform,"PausePanel");
            if(oldPanel!=null)Object.DestroyImmediate(oldPanel.gameObject);

            var button=Find(canvas.transform,"PauseButton");
            if(button==null)
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab,scene);
                instance.name="PauseButton";
                instance.transform.SetParent(canvas.transform,false);
                button=instance.transform;
            }
            RectTransform bt=button.GetComponent<RectTransform>();
            if(bt!=null)
            {
                bt.anchorMin=bt.anchorMax=new Vector2(0,1);
                bt.pivot=new Vector2(.5f,.5f);
                bt.anchoredPosition=new Vector2(47,-54);
                bt.localScale=Vector3.one;
                bt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,56);
                bt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,56);
            }
            button.gameObject.SetActive(true);
            var oldMenu=Find(canvas.transform,"PauseMenuPanel");
            if(oldMenu==null)
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(menuPrefab,scene);
                instance.name="PauseMenuPanel";
                instance.transform.SetParent(canvas.transform,false);
                oldMenu=instance.transform;
            }
            var rect=oldMenu.GetComponent<RectTransform>();
            if(rect!=null)
            {
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
                rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                rect.localScale=Vector3.one;
            }
            foreach(var text in oldMenu.GetComponentsInChildren<TMP_Text>(true))
            {
                text.raycastTarget=false;
                if(text.transform.parent!=null && text.transform.parent.name=="StageSelectionButton")
                    text.text="STAGE SELECTION";
                if(text.transform.parent!=null && text.transform.parent.name=="LevelSelectionButton")
                    text.text="LEVEL SELECTION";
            }
            // Keep modal above the gameplay board when opened, while leaving
            // it inactive in edit mode so the puzzle is visible in Hierarchy.
            oldMenu.SetAsLastSibling();
            oldMenu.gameObject.SetActive(false);

            // Stage 1 places the menu at top left and the energy icons to its right.
            for(int i=0;i<5;i++)
            {
                var energy=Find(canvas.transform,"Energy"+(i+1));
                var e=energy?.GetComponent<RectTransform>();
                if(e!=null)e.anchoredPosition=new Vector2(-475+i*43,307);
            }
            var label=Find(canvas.transform,"EnergyLabel")?.GetComponent<RectTransform>();
            if(label!=null)label.anchoredPosition=new Vector2(-464,350);
            var count=Find(canvas.transform,"EnergyCount")?.GetComponent<RectTransform>();
            if(count!=null)count.anchoredPosition=new Vector2(-260,340);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updated++;
        }
        EditorUtility.DisplayDialog("ChemQuest",
            "Applied the original Stage 1 PauseButton and PauseMenuPanel to "+updated+
            " Stage 5 scenes. Buttons are wired by Stage5ElementCrucibleGame in Play Mode.","OK");
    }

    private static Transform Find(Transform root,string name)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
            if(t.name==name)return t;
        return null;
    }
}
