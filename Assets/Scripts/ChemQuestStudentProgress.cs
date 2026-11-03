using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Local-only profile separation for the legacy PlayerPrefs-based ChemQuest game.
// Existing progress is archived before switching to another entered profile.
public static class ChemQuestStudentProgress
{
    private const string ActiveProfileKey="CQ_ActiveProfile_v1";
    private const string SnapshotPrefix="CQ_Profile_v1_";
    private static readonly string[] GlobalIntKeys={
        "Stage1Completed","Stage2Completed","Stage3Completed","Stage4Completed","Stage5Completed",
        "Stage2Unlocked","Stage3Unlocked","Stage4Unlocked","Stage5Unlocked",
        "PlayerEnergy"
    };
    private static readonly string[] GlobalStringKeys={
        "EnergyLastUpdateUTC","ChemQuestCertificateIssuedUtc","ChemQuestCertificateId"
    };
    private static IEnumerable<string> ProgressIntKeys()
    {
        foreach(var key in GlobalIntKeys)yield return key;
        for(int stage=1;stage<=5;stage++)
        for(int level=1;level<=3;level++)
        {
            string prefix="Stage"+stage+"Level"+level;
            yield return prefix+"Completed";
            yield return prefix+"Score";
            yield return prefix+"Unlocked";
            yield return prefix+"RetryCount";
            yield return prefix+"RetryTrackingStarted";
            // Level 1 original and later stage game variants
            yield return prefix+"Attempts";
        }
    }
    private static string ProfileID(string name,string school,string section)
    {
        string normalized=(name.Trim()+"|"+school.Trim()+"|"+section.Trim()).ToUpperInvariant();
        using(var sha=SHA256.Create())
        {
            var hash=sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
            return BitConverter.ToString(hash,0,12).Replace("-","");
        }
    }
    private static string Slot(string profile,string key)=>SnapshotPrefix+profile+"_"+key;
    private static void SaveSnapshot(string profile)
    {
        foreach(string key in ProgressIntKeys())
        {
            string slot=Slot(profile,key);
            bool exists=PlayerPrefs.HasKey(key);
            PlayerPrefs.SetInt(slot+"_Exists",exists?1:0);
            if(exists)PlayerPrefs.SetInt(slot,PlayerPrefs.GetInt(key));
        }
        foreach(string key in GlobalStringKeys)
        {
            string slot=Slot(profile,key);
            bool exists=PlayerPrefs.HasKey(key);
            PlayerPrefs.SetInt(slot+"_Exists",exists?1:0);
            if(exists)PlayerPrefs.SetString(slot,PlayerPrefs.GetString(key));
        }
        PlayerPrefs.SetInt(SnapshotPrefix+profile+"_Saved",1);
        PlayerPrefs.Save();
    }
    private static void LoadSnapshot(string profile)
    {
        bool saved=PlayerPrefs.GetInt(SnapshotPrefix+profile+"_Saved",0)==1;
        foreach(string key in ProgressIntKeys())
        {
            string slot=Slot(profile,key);
            if(saved && PlayerPrefs.GetInt(slot+"_Exists",0)==1)
                PlayerPrefs.SetInt(key,PlayerPrefs.GetInt(slot));
            else PlayerPrefs.DeleteKey(key);
        }
        foreach(string key in GlobalStringKeys)
        {
            string slot=Slot(profile,key);
            if(saved && PlayerPrefs.GetInt(slot+"_Exists",0)==1)
                PlayerPrefs.SetString(key,PlayerPrefs.GetString(slot));
            else PlayerPrefs.DeleteKey(key);
        }
        if(!saved)
        {
            PlayerPrefs.SetInt("PlayerEnergy",5);
            PlayerPrefs.SetString("EnergyLastUpdateUTC",DateTime.UtcNow.ToString("o"));
        }
        PlayerPrefs.Save();
    }
    // Called only when a student has entered all three required profile fields.
    public static void SelectProfile(string name,string school,string section)
    {
        string next=ProfileID(name,school,section);
        string previous=PlayerPrefs.GetString(ActiveProfileKey,"");
        if(previous==next)return;
        // On first install of this feature, the existing visible profile owns the
        // pre-existing legacy progress. That means no migration is destructive.
        if(string.IsNullOrEmpty(previous))
        {
            string priorName=PlayerPrefs.GetString("StudentName","");
            string priorSchool=PlayerPrefs.GetString("StudentSchool","");
            string priorSection=PlayerPrefs.GetString("StudentSection","");
            if(!string.IsNullOrEmpty(priorName)&&
               !string.IsNullOrEmpty(priorSchool)&&
               !string.IsNullOrEmpty(priorSection))
            {
                previous=ProfileID(priorName,priorSchool,priorSection);
            }
        }
        if(!string.IsNullOrEmpty(previous))
        {
            SaveSnapshot(previous);
            // This code runs on the profile selection scene, where shared
            // EnergyManager should not be alive. The archived energy value is safe.
        }
        else
        {
            // First student of an existing legacy installation gets its progress.
            // No previous identity can be inferred safely, so we retain it.
            PlayerPrefs.SetString(ActiveProfileKey,next);
            PlayerPrefs.Save();
            return;
        }
        LoadSnapshot(next);
        PlayerPrefs.SetString(ActiveProfileKey,next);
        PlayerPrefs.Save();
        Debug.Log("ChemQuest: student profile switched; local progress separated.");
    }
}
