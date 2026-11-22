using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectionManager : MonoBehaviour
{
    private void Start()
    {
        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.name == "BackButton")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ReturnToMainMenu);
                button.interactable = true;
                continue;
            }

            for (int stage = 1; stage <= 5; stage++)
            {
                if (button.name == "Stage" + stage + "SelectButton")
                {
                    int selectedStage = stage;
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(() => OpenStage(selectedStage));
                    button.interactable = ChemQuestStudentProgress.IsStageUnlocked(stage);

                    var label = button.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                        label.text = button.interactable ? "SELECT" : "LOCKED";

                    break;
                }

                if (button.name == "Stage" + stage + "Button")
                {
                    button.interactable = ChemQuestStudentProgress.IsStageUnlocked(stage);
                    var lockedText = button.transform.Find("LockedText");
                    if (lockedText != null)
                        lockedText.gameObject.SetActive(!button.interactable);
                    break;
                }
            }
        }
    }

    private void OpenStage(int stage)
    {
        if (ChemQuestStudentProgress.IsStageUnlocked(stage))
            OpenScene("Stage" + stage + "LevelSelection");
    }

    private void OpenScene(string sceneName)
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("ChemQuest: " + sceneName + " is not enabled in Build Settings.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMainMenu() { OpenScene("MainMenu"); }
    public void OpenStage1() { OpenStage(1); }
    public void OpenStage2() { OpenStage(2); }
    public void OpenStage3() { OpenStage(3); }
    public void OpenStage4() { OpenStage(4); }
    public void OpenStage5() { OpenStage(5); }
}
