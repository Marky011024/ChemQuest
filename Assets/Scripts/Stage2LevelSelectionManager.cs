using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Stage2LevelSelectionManager : MonoBehaviour
{
    private void Start()
    {
        foreach(var button in FindObjectsOfType<Button>(true))
        {
            for(int level=1;level<=3;level++)
            {
                if(button.name!="Level"+level+"SelectButton")continue;
                int selected=level;
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(()=>Open(selected));
                button.interactable=level==1||
                    PlayerPrefs.GetInt("Stage2Level"+(level-1)+"Completed",0)==1;
                var label=button.GetComponentInChildren<TMP_Text>(true);
                if(label!=null)label.text=button.interactable?"SELECT":"LOCKED";
            }
            if(button.name=="BackButton")
            {
                button.onClick=new Button.ButtonClickedEvent();
                button.onClick.AddListener(()=>Navigate("StageSelection"));
            }
        }
    }
    private void Open(int level)
    {
        if(level>1&&PlayerPrefs.GetInt("Stage2Level"+(level-1)+"Completed",0)!=1)return;
        Navigate("Stage2Level"+level);
    }
    private void Navigate(string name)
    {
        Time.timeScale=1;
        if(!Application.CanStreamedLevelBeLoaded(name))
        {Debug.LogError("Stage2 Level Selection scene missing from Build Settings: "+name);return;}
        SceneManager.LoadScene(name);
    }
}