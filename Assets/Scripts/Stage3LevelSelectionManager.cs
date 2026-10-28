using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Stage3LevelSelectionManager : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [Header("Level Button Labels")]
    [SerializeField] private TMP_Text level1ButtonText;
    [SerializeField] private TMP_Text level2ButtonText;
    [SerializeField] private TMP_Text level3ButtonText;
    private void Start() { UpdateLevelLocks(); }
    public void UpdateLevelLocks()
    {
        bool l2=PlayerPrefs.GetInt("Stage3Level1Completed",0)==1;
        bool l3=PlayerPrefs.GetInt("Stage3Level2Completed",0)==1;
        SetLevelState(level1Button,level1ButtonText,true);
        SetLevelState(level2Button,level2ButtonText,l2);
        SetLevelState(level3Button,level3ButtonText,l3);
    }
    private void SetLevelState(Button b,TMP_Text t,bool unlocked)
    {
        if(b!=null)b.interactable=unlocked;
        if(t!=null)t.text=unlocked?"SELECT":"LOCKED";
    }
    public void OpenLevel1(){SceneManager.LoadScene("Stage3Level1");}
    public void OpenLevel2()
    {
        if(PlayerPrefs.GetInt("Stage3Level1Completed",0)==1)
            SceneManager.LoadScene("Stage3Level2");
    }
    public void OpenLevel3()
    {
        if(PlayerPrefs.GetInt("Stage3Level2Completed",0)==1)
            SceneManager.LoadScene("Stage3Level3");
    }
}
