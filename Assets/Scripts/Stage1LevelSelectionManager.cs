using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage1LevelSelectionManager : MonoBehaviour
{
    [Header("Level 2")]
    [SerializeField] private Button level2SelectButton;
    [SerializeField] private TMP_Text level2ButtonText;

    [Header("Level 3")]
    [SerializeField] private Button level3SelectButton;
    [SerializeField] private TMP_Text level3ButtonText;

    private void Start()
    {
        UpdateLevelLocks();
    }

    private void UpdateLevelLocks()
    {
        // LEVEL 2
        // Unlock Level 2 after Level 1 has been completed.
        bool level1Completed =
            PlayerPrefs.GetInt("Stage1Level1Completed", 0) == 1;

        if (level2SelectButton != null)
        {
            level2SelectButton.interactable = level1Completed;
        }

        if (level2ButtonText != null)
        {
            level2ButtonText.text = level1Completed ? "SELECT" : "LOCKED";
        }

        // LEVEL 3
        // Unlock Level 3 after Level 2 has been completed.
        bool level2Completed =
            PlayerPrefs.GetInt("Stage1Level2Completed", 0) == 1;

        if (level3SelectButton != null)
        {
            level3SelectButton.interactable = level2Completed;
        }

        if (level3ButtonText != null)
        {
            level3ButtonText.text = level2Completed ? "SELECT" : "LOCKED";
        }
    }

    public void BackToStageSelection()
    {
        SceneManager.LoadScene("StageSelection");
    }

    public void OpenLevel1()
    {
        SceneManager.LoadScene("Stage1Level1");
    }

    public void OpenLevel2()
    {
        // Extra protection so Level 2 cannot be opened while locked.
        if (PlayerPrefs.GetInt("Stage1Level1Completed", 0) == 1)
        {
            SceneManager.LoadScene("Stage1Level2");
        }
        else
        {
            Debug.Log("Level 2 is locked. Complete Level 1 first.");
        }
    }

    public void OpenLevel3()
    {
        // Extra protection so Level 3 cannot be opened while locked.
        if (PlayerPrefs.GetInt("Stage1Level2Completed", 0) == 1)
        {
            SceneManager.LoadScene("Stage1Level3");
        }
        else
        {
            Debug.Log("Level 3 is locked. Complete Level 2 first.");
        }
    }
}