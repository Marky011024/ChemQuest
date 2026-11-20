using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps the six selection-screen ENERGY headers synced with the shared
// saved/regenerating EnergyManager instead of the scene's static "ENERGY 5/5".
public sealed class ChemQuestSelectionEnergyHUD : MonoBehaviour
{
    private TMP_Text energyLabel;
    private EnergyManager energyManager;
    private int lastDisplayed=-1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if(FindFirstObjectByType<ChemQuestSelectionEnergyHUD>()!=null)return;
        var go=new GameObject("ChemQuestSelectionEnergyHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<ChemQuestSelectionEnergyHUD>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded+=OnSceneLoaded;
        FindSelectionEnergyLabel();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded-=OnSceneLoaded;
        DetachEnergy();
    }

    private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        DetachEnergy();
        FindSelectionEnergyLabel();
    }

    private static bool IsSelectionScene(string scene)
    {
        if(scene=="StageSelection")return true;
        for(int stage=1;stage<=5;stage++)
            if(scene=="Stage"+stage+"LevelSelection")return true;
        return false;
    }

    private void FindSelectionEnergyLabel()
    {
        energyLabel=null;
        lastDisplayed=-1;
        if(!IsSelectionScene(SceneManager.GetActiveScene().name))return;

        // All six existing scenes name their TMP header "EnergyText".
        foreach(var text in FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(text!=null && text.name=="EnergyText" && text.gameObject.scene.IsValid())
            {
                energyLabel=text;
                break;
            }
        }
        if(energyLabel==null)
        {
            Debug.LogWarning("CHEMQUEST: EnergyText header was not found on selection screen.");
            return;
        }

        // The EnergyManager may first be created in a gameplay scene. Create
        // the normal shared manager for selection screens if it is missing,
        // so its saved 15-minute regeneration clock is also respected.
        if(EnergyManager.Instance==null)
            new GameObject("SharedEnergyManager",typeof(EnergyManager));
        AttachEnergy();
        RefreshHeader();
    }

    private void AttachEnergy()
    {
        if(energyManager==EnergyManager.Instance)return;
        DetachEnergy();
        energyManager=EnergyManager.Instance;
        if(energyManager!=null)
            energyManager.OnEnergyChanged+=RefreshHeader;
    }

    private void DetachEnergy()
    {
        if(energyManager!=null)
            energyManager.OnEnergyChanged-=RefreshHeader;
        energyManager=null;
    }

    private void Update()
    {
        if(energyLabel==null)return;
        if(energyManager!=EnergyManager.Instance)AttachEnergy();
        // Update only when the number changes; avoid excessive TMP rebuilds.
        RefreshHeader();
    }

    private void RefreshHeader()
    {
        if(energyLabel==null)return;
        if(energyManager!=null)
        {
            energyManager.RefreshEnergy();
            int current=Mathf.Clamp(energyManager.CurrentEnergy,0,EnergyManager.MaxEnergy);
            if(current==lastDisplayed)return;
            lastDisplayed=current;
            energyLabel.text="ENERGY  "+current+"/"+EnergyManager.MaxEnergy;
        }
        else
        {
            int saved=Mathf.Clamp(PlayerPrefs.GetInt("PlayerEnergy",EnergyManager.MaxEnergy),
                0,EnergyManager.MaxEnergy);
            if(saved==lastDisplayed)return;
            lastDisplayed=saved;
            energyLabel.text="ENERGY  "+saved+"/"+EnergyManager.MaxEnergy;
        }
    }
}
