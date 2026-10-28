using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectionManager : MonoBehaviour
{
    private void Start()
    {
        // StageSelection.unity has a visible Stage3SelectButton with empty OnClick.
        // Connect it directly, without touching other stage navigation.
        foreach (var button in FindObjectsOfType<Button>(true))
        {
            if (button.gameObject.name != "Stage3SelectButton") continue;
            button.onClick.AddListener(OpenStage3);
            button.interactable = true;
        }
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

    public void OpenStage1() { OpenScene("Stage1LevelSelection"); }
    public void OpenStage2() { OpenScene("Stage2Level1"); }
    public void OpenStage3() { OpenScene("Stage3LevelSelection"); }
}
