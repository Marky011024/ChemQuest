using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectionManager : MonoBehaviour
{
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
