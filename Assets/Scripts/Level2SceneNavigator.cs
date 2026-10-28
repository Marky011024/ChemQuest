
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2SceneNavigator : MonoBehaviour
{
    public void ContinueToLevel3()
    {
        if (PlayerPrefs.GetInt("Stage1Level2Completed", 0) != 1)
        {
            Debug.LogWarning("Level 2 is not yet completed!");
            return;
        }

        Debug.Log("Opening Stage 1 Level 3...");
        SceneManager.LoadScene("Stage1Level3");
    }

    public void ReturnToLevelSelection()
    {
        Debug.Log("Opening Stage 1 Level Selection...");
        SceneManager.LoadScene("Stage1LevelSelection");
    }
}
