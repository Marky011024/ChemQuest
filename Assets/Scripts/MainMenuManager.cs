using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text welcomeText;

    void Start()
    {
        string studentName = PlayerPrefs.GetString("StudentName", "Student");

        if(welcomeText!=null)welcomeText.text = "WELCOME TO CHEMQUEST\n" + studentName.ToUpper();
        if(GetComponent<ChemQuestMainMenuExtras>()==null)
            gameObject.AddComponent<ChemQuestMainMenuExtras>();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("StageSelection");
    }
}