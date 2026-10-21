using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Start()
    {
        // Make sure gameplay starts normally.
        Time.timeScale = 1f;

        // Hide the pause menu when the level starts.
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void GoToLevelSelection()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("Stage1LevelSelection");
    }

    public void GoToStageSelection()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("StageSelection");
    }

    private void OnDestroy()
    {
        // Safety check so another scene never remains paused.
        Time.timeScale = 1f;
    }
}