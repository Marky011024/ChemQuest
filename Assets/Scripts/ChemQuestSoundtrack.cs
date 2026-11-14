using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Audio assets are loaded from Assets/Resources/ChemQuestAudio.
// This controller starts automatically in editor Play Mode and Android builds.
// It does not modify gameplay scores, energy, or level unlocks.
public sealed class ChemQuestSoundtrack : MonoBehaviour
{
    private static ChemQuestSoundtrack instance;
    private AudioSource musicSource, effectsSource;
    private readonly Dictionary<string, AudioClip> sounds=new Dictionary<string, AudioClip>();
    private readonly HashSet<Button> wiredButtons=new HashSet<Button>();
    private string currentMusic="";
    private int completedCount, lastEnergy=-1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if(instance!=null)return;
        var go=new GameObject("ChemQuestSoundtrack");
        DontDestroyOnLoad(go);
        instance=go.AddComponent<ChemQuestSoundtrack>();
    }

    private void Awake()
    {
        if(instance!=null && instance!=this){Destroy(gameObject);return;}
        instance=this;
        DontDestroyOnLoad(gameObject);
        var musicObject=new GameObject("BackgroundMusic");
        musicObject.transform.SetParent(transform,false);
        musicSource=musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake=false;musicSource.loop=true;musicSource.volume=1f;
        musicObject.AddComponent<ChemQuestMusicSource>();
        var effectsObject=new GameObject("SoundEffects");
        effectsObject.transform.SetParent(transform,false);
        effectsSource=effectsObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake=false;effectsSource.loop=false;effectsSource.volume=1f;
        effectsSource.spatialBlend=0;
        SceneManager.sceneLoaded+=OnSceneLoaded;
        completedCount=CountCompletedLevels();
        lastEnergy=PlayerPrefs.GetInt("PlayerEnergy",5);
        OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded-=OnSceneLoaded;
        if(instance==this)instance=null;
    }

    private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        wiredButtons.Clear();
        completedCount=CountCompletedLevels();
        lastEnergy=PlayerPrefs.GetInt("PlayerEnergy",5);
        var sceneName=scene.name;
        string target="stage1";
        for(int stage=1;stage<=5;stage++)
            if(sceneName.StartsWith("Stage"+stage,StringComparison.OrdinalIgnoreCase))
            {target="stage"+stage;break;}
        if(sceneName=="ChemQuestCertificate")target="stage5";
        ChangeMusic(target);
        StopAllCoroutines();
        StartCoroutine(WireInteractiveButtons());
        ChemQuestAudioSettings.Apply();
    }

    private void ChangeMusic(string name)
    {
        if(name==currentMusic && musicSource.isPlaying)return;
        currentMusic=name;
        var clip=Resources.Load<AudioClip>("ChemQuestAudio/Music/"+name);
        if(clip==null)
        {
            Debug.LogWarning("CHEMQUEST AUDIO: Missing Assets/Resources/ChemQuestAudio/Music/"+name);
            musicSource.Stop();return;
        }
        musicSource.Stop();
        musicSource.clip=clip;
        musicSource.loop=true;
        musicSource.Play();
        ChemQuestAudioSettings.Apply();
    }

    private IEnumerator WireInteractiveButtons()
    {
        // Some levels generate their UI in Start(), so rescan for new buttons.
        while(true)
        {
            foreach(var button in FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(button==null || !button.gameObject.scene.IsValid())continue;
                if(!wiredButtons.Add(button))continue;
                string sound=ClassifyButton(button.gameObject.name);
                button.onClick.AddListener(()=>Play(sound));
            }
            yield return new WaitForSecondsRealtime(1.5f);
        }
    }

    private static string ClassifyButton(string name)
    {
        string n=name.ToLowerInvariant();
        if(n.Contains("hint"))return "hint";
        if(n.Contains("bubble") || n.Contains("reaction"))return "bubble";
        if(n.Contains("probe") || n.Contains("electric"))return "electric";
        if(n.Contains("bond") || n.Contains("crystal"))return "crystal";
        if(n.Contains("drop") || n.Contains("place"))return "drop";
        return "click";
    }

    private static int CountCompletedLevels()
    {
        int count=0;
        for(int stage=1;stage<=5;stage++)
        for(int level=1;level<=3;level++)
            if(PlayerPrefs.GetInt("Stage"+stage+"Level"+level+"Completed",0)==1)count++;
        return count;
    }

    private void Update()
    {
        // Detect a new level reward even if the scene is not changed.
        int n=CountCompletedLevels();
        if(n>completedCount)Play("complete");
        completedCount=n;
        int energy=PlayerPrefs.GetInt("PlayerEnergy",5);
        if(lastEnergy>=0 && energy<lastEnergy)Play("energy");
        lastEnergy=energy;
    }

    private AudioClip GetEffect(string key)
    {
        AudioClip clip;
        if(sounds.TryGetValue(key,out clip))return clip;
        clip=Resources.Load<AudioClip>("ChemQuestAudio/Sfx/"+key);
        sounds[key]=clip;
        return clip;
    }

    public static void Play(string key)
    {
        if(instance==null || ChemQuestAudioSettings.Muted)return;
        var clip=instance.GetEffect(key);
        if(clip==null)return;
        // Uses the same saved settings as the main menu Music / SFX sliders.
        instance.effectsSource.PlayOneShot(clip,1f);
    }

    public static void Correct()=>Play("correct");
    public static void Wrong()=>Play("wrong");
    public static void Hint()=>Play("hint");
    public static void Drop()=>Play("drop");
    public static void Bubble()=>Play("bubble");
    public static void Crystal()=>Play("crystal");
    public static void Electric()=>Play("electric");
    public static void Certificate()=>Play("certificate");
}
