using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class Stage5LevelSelectionManager : MonoBehaviour
{
    private void Start()
    {
        foreach(var button in FindObjectsOfType<Button>(true))
        {
            if(button.name=="Stage5BackButton")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(()=>Go("StageSelection"));
            }
            for(int level=1;level<=3;level++)
            {
                if(button.name!="Stage5Level"+level+"Button")continue;
                int next=level;
                bool unlocked=level==1||PlayerPrefs.GetInt("Stage5Level"+(level-1)+"Completed",0)==1;
                button.interactable=unlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(()=>Go("Stage5Level"+next));
                var label=button.GetComponentInChildren<TMP_Text>(true);
                if(label!=null)label.text=unlocked?"SELECT":"LOCKED";
            }
        }
    }
    private static void Go(string scene)
    {
        Time.timeScale=1f;
        if(!Application.CanStreamedLevelBeLoaded(scene))
        {Debug.LogError("Stage 5 scene not registered: "+scene);return;}
        SceneManager.LoadScene(scene);
    }
}