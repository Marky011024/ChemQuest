using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Stage3LevelSelectionManager : MonoBehaviour
{
    [Header("Stage 3 Level Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("Stage 3 Level Labels")]
    [SerializeField] private TMP_Text level1ButtonText;
    [SerializeField] private TMP_Text level2ButtonText;
    [SerializeField] private TMP_Text level3ButtonText;

    private void Awake()
    {
        // The copied Stage 3 scene still contains Stage1LevelSelectionManager,
        // which incorrectly applies Stage 1 completion flags to these buttons.
        foreach (var old in FindObjectsOfType<Stage1LevelSelectionManager>(true))
            old.enabled = false;

        BindSceneButtons();
    }

    private void Start() { UpdateLevelLocks(); }
    private void OnEnable() { UpdateLevelLocks(); }

    private void BindSceneButtons()
    {
        // Use serialized assignments, with name-based fallback for older scenes.
        if (level1Button == null) level1Button = FindButton("Level1SelectButton");
        if (level2Button == null) level2Button = FindButton("Level2SelectButton");
        if (level3Button == null) level3Button = FindButton("Level3SelectButton");

        if (level1Button != null)
        {
            level1Button.onClick = new Button.ButtonClickedEvent();
            level1Button.onClick.AddListener(OpenLevel1);
        }
        if (level2Button != null)
        {
            level2Button.onClick = new Button.ButtonClickedEvent();
            level2Button.onClick.AddListener(OpenLevel2);
        }
        if (level3Button != null)
        {
            level3Button.onClick = new Button.ButtonClickedEvent();
            level3Button.onClick.AddListener(OpenLevel3);
        }
        if (level1ButtonText == null && level1Button != null)
            level1ButtonText = level1Button.GetComponentInChildren<TMP_Text>(true);
        if (level2ButtonText == null && level2Button != null)
            level2ButtonText = level2Button.GetComponentInChildren<TMP_Text>(true);
        if (level3ButtonText == null && level3Button != null)
            level3ButtonText = level3Button.GetComponentInChildren<TMP_Text>(true);
    }

    private Button FindButton(string objectName)
    {
        foreach (var button in FindObjectsOfType<Button>(true))
            if (button.name == objectName) return button;
        return null;
    }

    public void UpdateLevelLocks()
    {
        bool level2Unlocked = PlayerPrefs.GetInt("Stage3Level1Completed", 0) == 1;
        bool level3Unlocked = PlayerPrefs.GetInt("Stage3Level2Completed", 0) == 1;
        SetLevelState(level1Button, level1ButtonText, true);
        SetLevelState(level2Button, level2ButtonText, level2Unlocked);
        SetLevelState(level3Button, level3ButtonText, level3Unlocked);
    }

    private void SetLevelState(Button button, TMP_Text text, bool unlocked)
    {
        if (button != null) button.interactable = unlocked;
        if (text != null) text.text = unlocked ? "SELECT" : "LOCKED";
    }

    private void Navigate(string scene)
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("Stage 3 Level Selection: " + scene + " is not in Build Settings.");
            return;
        }
        SceneManager.LoadScene(scene);
    }

    public void OpenLevel1() { Navigate("Stage3Level1"); }

    public void OpenLevel2()
    {
        if (PlayerPrefs.GetInt("Stage3Level1Completed", 0) != 1)
        {
            Debug.Log("Stage 3 Level 2 is locked: complete Stage 3 Level 1 first.");
            UpdateLevelLocks();
            return;
        }
        Navigate("Stage3Level2");
    }

    public void OpenLevel3()
    {
        if (PlayerPrefs.GetInt("Stage3Level2Completed", 0) != 1)
        {
            Debug.Log("Stage 3 Level 3 is locked: complete Stage 3 Level 2 first.");
            UpdateLevelLocks();
            return;
        }
        Navigate("Stage3Level3");
    }
}
