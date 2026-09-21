using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage1Manager : MonoBehaviour
{
    public void StartLevel1()
    {
        SceneManager.LoadScene("Stage1Level1");
    }
}