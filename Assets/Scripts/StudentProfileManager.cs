using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentProfileManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public void ContinueGame()
    {
        string studentName = nameInputField.text.Trim();

        // Do not continue if the student did not enter a name.
        if (string.IsNullOrEmpty(studentName))
        {
            Debug.Log("Please enter your name.");
            return;
        }

        // Temporarily save the student's name.
        PlayerPrefs.SetString("StudentName", studentName);
        PlayerPrefs.Save();

        Debug.Log("Student Name Saved: " + studentName);

        // Open the Main Menu.
        SceneManager.LoadScene("MainMenu");
    }
}