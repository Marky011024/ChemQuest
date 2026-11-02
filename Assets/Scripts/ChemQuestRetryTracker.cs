using UnityEngine;
using UnityEngine.SceneManagement;

// Observes actual shared-energy deductions during gameplay. A failed run costs
// one energy. This is not the same as one hint or one wrong click.
// Existing historical attempts cannot be reconstructed and remain unknown.
public sealed class ChemQuestRetryTracker : MonoBehaviour
{
    private EnergyManager connected;
    private int previousEnergy;
    private string activeLevel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if(FindFirstObjectByType<ChemQuestRetryTracker>()!=null)return;
        var go=new GameObject("ChemQuestRetryTracker");
        DontDestroyOnLoad(go);
        go.AddComponent<ChemQuestRetryTracker>();
    }
    private void OnEnable()
    {
        SceneManager.activeSceneChanged+=SceneChanged;
        RefreshLevel();
    }
    private void OnDisable()
    {
        SceneManager.activeSceneChanged-=SceneChanged;
        if(connected!=null)connected.OnEnergyChanged-=EnergyChanged;
    }
    private void Update()
    {
        // EnergyManager may spawn later than this tracker on initial scene load.
        if(EnergyManager.Instance!=connected)
        {
            if(connected!=null)connected.OnEnergyChanged-=EnergyChanged;
            connected=EnergyManager.Instance;
            if(connected!=null)
            {
                previousEnergy=connected.CurrentEnergy;
                connected.OnEnergyChanged+=EnergyChanged;
            }
        }
    }
    private void SceneChanged(Scene from,Scene to)
    {
        RefreshLevel();
        if(connected!=null)previousEnergy=connected.CurrentEnergy;
    }
    private void RefreshLevel()
    {
        string scene=SceneManager.GetActiveScene().name;
        activeLevel=null;
        // Only accept exact StageNLevelM scene names with 1..5 / 1..3.
        for(int stage=1;stage<=5;stage++)
            for(int level=1;level<=3;level++)
                if(scene=="Stage"+stage+"Level"+level)
                    activeLevel="Stage"+stage+"Level"+level;
        if(activeLevel!=null)
        {
            // A previously-completed level without recorded history stays
            // historical/unknown until the student replays it.
            PlayerPrefs.SetInt(activeLevel+"RetryTrackingStarted",1);
            PlayerPrefs.Save();
        }
    }
    private void EnergyChanged()
    {
        if(connected==null)return;
        int now=connected.CurrentEnergy;
        if(activeLevel!=null && now<previousEnergy)
        {
            string key=activeLevel+"RetryCount";
            int deduction=previousEnergy-now;
            PlayerPrefs.SetInt(key,PlayerPrefs.GetInt(key,0)+deduction);
            PlayerPrefs.Save();
            Debug.Log("ChemQuest retry tracker: "+activeLevel+
                " failed run(s) recorded: "+deduction);
        }
        previousEnergy=now;
    }
}
