using System;
using UnityEngine;

// Read-only, versioned, per-level BOW content. The caller captures the returned
// object at the start of a BOW attempt, preventing updates mid-assessment.
// Returns null if no valid teacher-approved snapshot is cached.
public static class ChemQuestBowData
{
    [Serializable] public sealed class Question
    {
        public int order;
        public string prompt;
        public string a;
        public string b;
        public int correct;
        public string hint;
        public string explanation;
    }
    [Serializable] public sealed class Snapshot
    {
        public int schema;
        public string version;
        public Question[] items;
        public Question At(int index) =>
            items!=null && index>=0 && index<items.Length ? items[index] : null;
    }
    public static Snapshot Load(int stage,int level)
    {
        if(stage<1 || stage>5 || level<1 || level>3)return null;
        string key="ChemQuestBowS"+stage+"L"+level+"SnapshotV1";
        string raw=PlayerPrefs.GetString(key,"");
        if(string.IsNullOrWhiteSpace(raw))return null;
        try
        {
            var s=JsonUtility.FromJson<Snapshot>(raw);
            if(s==null || s.schema!=1 || s.items==null || s.items.Length!=2 ||
               string.IsNullOrWhiteSpace(s.version))return null;
            for(int i=0;i<2;i++)
            {
                var q=s.items[i];
                if(q==null || q.order!=i+1 || (q.correct!=0 && q.correct!=1) ||
                   string.IsNullOrWhiteSpace(q.prompt) ||
                   string.IsNullOrWhiteSpace(q.a) || string.IsNullOrWhiteSpace(q.b) ||
                   string.IsNullOrWhiteSpace(q.hint) || string.IsNullOrWhiteSpace(q.explanation) ||
                   string.Equals(q.a.Trim(),q.b.Trim(),StringComparison.OrdinalIgnoreCase))
                    return null;
            }
            Debug.Log("ChemQuest BOW: stage "+stage+" level "+level+
                " using approved cached snapshot "+s.version);
            return s;
        }
        catch(Exception ex)
        {
            Debug.LogWarning("ChemQuest BOW cache invalid for stage "+stage+
                " level "+level+": "+ex.Message);
            return null;
        }
    }
}