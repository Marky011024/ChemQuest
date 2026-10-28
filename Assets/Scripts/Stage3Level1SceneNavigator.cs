
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage3Level1SceneNavigator : MonoBehaviour
{
    public void GoToLevelSelection()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage3LevelSelection");
    }

    public void GoToNextLevel()
    {
        if (PlayerPrefs.GetInt("Stage3Level1Completed", 0) == 1)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Stage3Level2");
        }
        else
        {
            Debug.LogWarning("Complete Stage 3 Level 1 first!");
        }
    }
}
