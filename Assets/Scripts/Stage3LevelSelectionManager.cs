
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

    private void Start()
    {
        UpdateLevelLocks();
    }

    // UPDATE LEVEL LOCK STATUS
    public void UpdateLevelLocks()
    {
        bool level2Unlocked =
            PlayerPrefs.GetInt("Stage3Level1Completed", 0) == 1;

        bool level3Unlocked =
            PlayerPrefs.GetInt("Stage3Level2Completed", 0) == 1;

        SetLevelState(level1Button, level1ButtonText, true);
        SetLevelState(level2Button, level2ButtonText, level2Unlocked);
        SetLevelState(level3Button, level3ButtonText, level3Unlocked);
    }

    // CHANGE BUTTON STATUS AND LABEL
    private void SetLevelState(
        Button button,
        TMP_Text buttonText,
        bool unlocked)
    {
        if (button != null)
        {
            button.interactable = unlocked;
        }

        if (buttonText != null)
        {
            buttonText.text = unlocked ? "SELECT" : "LOCKED";
        }
    }

    // OPEN STAGE 3 LEVEL 1
    public void OpenLevel1()
    {
        SceneManager.LoadScene("Stage3Level1");
    }
}
