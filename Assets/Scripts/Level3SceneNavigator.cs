
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3SceneNavigator : MonoBehaviour
{
    public void ContinueToStage2Level1()
    {
        if (PlayerPrefs.GetInt("Stage1Level3Completed", 0) != 1)
        {
            Debug.LogWarning("Stage 1 Level 3 is not yet completed!");
            return;
        }

        PlayerPrefs.SetInt("Stage2Level1Unlocked", 1);
        PlayerPrefs.Save();

        Debug.Log("Opening Stage 2 Level 1...");
        SceneManager.LoadScene("Stage2Level1");
    }

    public void ReturnToLevelSelection()
    {
        Debug.Log("Opening Stage 1 Level Selection...");
        SceneManager.LoadScene("Stage1LevelSelection");
    }
}
