using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Persist separate music and sound effect volume sliders across scenes.
// Music sources should be tagged with ChemQuestMusicSource to classify precisely.
// Untagged audio sources are classified as SFX unless they loop.
public static class ChemQuestAudioSettings
{
    private const string MusicKey="ChemQuestMusicVolume";
    private const string SfxKey="ChemQuestSfxVolume";
    private const string MutedKey="ChemQuestSoundMuted";
    public static float Music=>PlayerPrefs.GetFloat(MusicKey,.25f);
    public static float Sfx=>PlayerPrefs.GetFloat(SfxKey,.65f);
    public static bool Muted=>PlayerPrefs.GetInt(MutedKey,0)==1;
    private static readonly Dictionary<AudioSource,float> BaseLevels=new Dictionary<AudioSource,float>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if(Object.FindFirstObjectByType<ChemQuestAudioSettingsRunner>()!=null)return;
        var go=new GameObject("ChemQuestAudioSettings");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChemQuestAudioSettingsRunner>();
    }
    public static void SetMusic(float volume)
    {
        PlayerPrefs.SetFloat(MusicKey,Mathf.Clamp01(volume));PlayerPrefs.Save();Apply();
    }
    public static void SetSfx(float volume)
    {
        PlayerPrefs.SetFloat(SfxKey,Mathf.Clamp01(volume));PlayerPrefs.Save();Apply();
    }
    public static void ToggleMute()
    {
        PlayerPrefs.SetInt(MutedKey,Muted?0:1);PlayerPrefs.Save();Apply();
    }
    public static void Apply()
    {
        foreach(var src in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(src==null)continue;
            if(!BaseLevels.ContainsKey(src))BaseLevels[src]=src.volume;
            float original=BaseLevels[src];
            bool music=src.GetComponent<ChemQuestMusicSource>()!=null || src.loop;
            src.volume=original*(Muted?0f:(music?Music:Sfx));
        }
    }
}

public sealed class ChemQuestAudioSettingsRunner:MonoBehaviour
{
    private void OnEnable(){SceneManager.sceneLoaded+=Loaded;}
    private void OnDisable(){SceneManager.sceneLoaded-=Loaded;}
    private void Loaded(Scene scene,LoadSceneMode mode){ChemQuestAudioSettings.Apply();}
    private void Start(){ChemQuestAudioSettings.Apply();}
}

// Attach this optional marker to a non-looped music AudioSource.
public sealed class ChemQuestMusicSource:MonoBehaviour {}
