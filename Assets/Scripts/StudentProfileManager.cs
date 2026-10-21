using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentProfileManager : MonoBehaviour
{
    [Header("Student Profile Fields")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField schoolInputField;
    [SerializeField] private TMP_InputField sectionInputField;

    public void ContinueGame()
    {
        string studentName = nameInputField.text.Trim();
        string studentSchool = schoolInputField.text.Trim();
        string studentSection = sectionInputField.text.Trim();

        // Require all profile information.
        if (string.IsNullOrEmpty(studentName))
        {
            Debug.Log("Please enter your name.");
            return;
        }

        if (string.IsNullOrEmpty(studentSchool))
        {
            Debug.Log("Please enter your school.");
            return;
        }

        if (string.IsNullOrEmpty(studentSection))
        {
            Debug.Log("Please enter your class / section.");
            return;
        }

        // Save the student's profile locally.
        PlayerPrefs.SetString("StudentName", studentName);
        PlayerPrefs.SetString("StudentSchool", studentSchool);
        PlayerPrefs.SetString("StudentSection", studentSection);
        PlayerPrefs.Save();

        Debug.Log("Student Profile Saved:");
        Debug.Log("Name: " + studentName);
        Debug.Log("School: " + studentSchool);
        Debug.Log("Class / Section: " + studentSection);

        // Continue to the main game menu.
        SceneManager.LoadScene("MainMenu");
    }
}   
