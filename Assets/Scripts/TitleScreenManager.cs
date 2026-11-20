using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    // The title screen is the first enabled scene in the Android build.
    // Returning students with a complete locally saved profile go straight
    // to MainMenu; first-time students still enter their details once.
    private void Start()
    {
        if (HasSavedStudentProfile())
        {
            Debug.Log("CHEMQUEST: Returning student detected. Opening Main Menu.");
            OpenScene("MainMenu");
        }
    }

    private static bool HasSavedStudentProfile()
    {
        return !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentName", "")) &&
               !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentSchool", "")) &&
               !string.IsNullOrWhiteSpace(PlayerPrefs.GetString("StudentSection", ""));
    }

    // Called by the existing START GAME button on first launch.
    public void StartGame()
    {
        OpenScene(HasSavedStudentProfile() ? "MainMenu" : "StudentProfile");
    }

    private static void OpenScene(string scene)
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("CHEMQUEST startup scene missing from Build Profiles: " + scene);
            return;
        }
        SceneManager.LoadScene(scene);
    }
}
