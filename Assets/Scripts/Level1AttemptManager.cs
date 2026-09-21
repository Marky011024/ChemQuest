using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1AttemptManager : MonoBehaviour
{
    [SerializeField] private GameObject mistakeHintText;
    [SerializeField] private GameObject[] energyIcons;
    [SerializeField] private GameObject levelFailedPanel;

    private int mistakeCount = 0;
    private int currentEnergy;

    public int MistakeCount
    {
        get { return mistakeCount; }
    }

    public bool HasMadeMistake
    {
        get { return mistakeCount > 0; }
    }


        private void Start()
{
    currentEnergy = PlayerPrefs.GetInt("PlayerEnergy", 5);

    for (int i = 0; i < energyIcons.Length; i++)
    {
        if (energyIcons[i] != null)
        {
            energyIcons[i].SetActive(i < currentEnergy);
        }
    }
}

    public void RegisterMistake()
    {
        mistakeCount++;

        Debug.Log("Level 1 mistake count: " + mistakeCount);

        if (mistakeCount == 1)
        {
            // First mistake: show the conceptual hint.
            if (mistakeHintText != null)
            {
                mistakeHintText.SetActive(true);
            }

            Debug.Log("First mistake: Conceptual hint shown.");
        }
        else if (mistakeCount >= 2)
{
    if (currentEnergy > 0)
    {
        currentEnergy--;

        PlayerPrefs.SetInt("PlayerEnergy", currentEnergy);
        PlayerPrefs.Save();

        if (currentEnergy < energyIcons.Length &&
            energyIcons[currentEnergy] != null)
        {
            energyIcons[currentEnergy].SetActive(false);
        }

        Debug.Log("Energy lost! Current energy: " + currentEnergy);
    }
    if (levelFailedPanel != null)
    {
    levelFailedPanel.SetActive(true);
    }
    Debug.Log("Second mistake: Lose 1 energy and retry Level 1.");
    }
}
    public void HideMistakeHint()
{
    if (mistakeHintText != null)
    {
        mistakeHintText.SetActive(false);
    }
}
public void RetryLevel()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
[ContextMenu("Reset Energy To 5")]
public void ResetEnergyToFive()
{
    PlayerPrefs.SetInt("PlayerEnergy", 5);
    PlayerPrefs.Save();

    Debug.Log("Player energy reset to 5.");
}
}