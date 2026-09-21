using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectionManager : MonoBehaviour
{
    public void OpenStage1()
    {
        SceneManager.LoadScene("Stage1");
    }
}