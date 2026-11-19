using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Pilot only. Does not modify ChemQuestAssessmentGate or Stage1BowLearningCheck.
// Requires Firebase Authentication > Anonymous enabled, and Firestore rules deployed
// as described in admin/UNITY_SYNC_PILOT.md.
// Attach to a temporary scene GameObject and fill in Firebase Web API key in Inspector.
public sealed class ChemQuestQuestionSyncPilot : MonoBehaviour
{
    [Header("Firebase project (Web app API key is not a secret)")]
    [SerializeField] private string projectId = "chemquest-admin";
    [SerializeField] private string webApiKey = "";
    [Header("Safe pilot")]
    [SerializeField] private bool runOnStart = false;

    [Serializable] private class AuthReply { public string idToken; public string error; }
    [Serializable] private class FirestoreString { public string stringValue; }
    [Serializable] private class FirestoreInteger { public string integerValue; }
    [Serializable] private class Fields
    {
        public FirestoreString status;
        public FirestoreString prompt;
        public FirestoreString kind;
        public FirestoreString version;
        public FirestoreInteger stage;
        public FirestoreInteger order;
    }
    [Serializable] private class DocumentReply { public string name; public Fields fields; }
    public string LastDownloadedQuestion { get; private set; }

    private void Start() { if(runOnStart) StartPilot(); }

    [ContextMenu("Test Firestore Published Question Sync")]
    public void StartPilot()
    {
        if(!Application.isPlaying)
        {
            Debug.LogWarning("Run the pilot in Play Mode.");
            return;
        }
        StartCoroutine(RunPilot());
    }

    private IEnumerator RunPilot()
    {
        if(string.IsNullOrWhiteSpace(webApiKey) || string.IsNullOrWhiteSpace(projectId))
        {
            Debug.LogError("ChemQuest sync pilot: set projectId and Firebase Web API key in Inspector.");
            yield break;
        }

        // Anonymous accounts have only the narrow pilot read permission, not questionBank access.
        string authUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" +
                         UnityWebRequest.EscapeURL(webApiKey);
        using(var req = new UnityWebRequest(authUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 20;
            yield return req.SendWebRequest();
            if(req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ChemQuest sync pilot: Firebase anonymous sign-in failed (" +
                    req.responseCode + "). Check Authentication > Sign-in method > Anonymous. " +
                    SafeError(req.downloadHandler.text));
                yield break;
            }

            var auth = JsonUtility.FromJson<AuthReply>(req.downloadHandler.text);
            if(auth == null || string.IsNullOrEmpty(auth.idToken))
            {
                Debug.LogError("ChemQuest sync pilot: missing Firebase ID token.");
                yield break;
            }

            string url = "https://firestore.googleapis.com/v1/projects/" +
                UnityWebRequest.EscapeURL(projectId) +
                "/databases/(default)/documents/syncPilot/demo";
            using(var docReq = UnityWebRequest.Get(url))
            {
                docReq.SetRequestHeader("Authorization", "Bearer " + auth.idToken);
                docReq.timeout = 20;
                yield return docReq.SendWebRequest();
                if(docReq.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("ChemQuest sync pilot: Firestore read failed (" +
                        docReq.responseCode + "). Check syncPilot/demo and deployed rules. " +
                        SafeError(docReq.downloadHandler.text));
                    yield break;
                }

                DocumentReply doc;
                try { doc = JsonUtility.FromJson<DocumentReply>(docReq.downloadHandler.text); }
                catch(Exception e)
                {
                    Debug.LogError("ChemQuest sync pilot: cannot decode Firestore document. " + e.Message);
                    yield break;
                }
                if(doc?.fields?.status?.stringValue != "published" ||
                   string.IsNullOrWhiteSpace(doc.fields.prompt?.stringValue))
                {
                    Debug.LogWarning("ChemQuest sync pilot: document is not a valid published test question.");
                    yield break;
                }

                LastDownloadedQuestion = doc.fields.prompt.stringValue;
                Debug.Log("CHEMQUEST SYNC PILOT OK | version=" +
                    (doc.fields.version?.stringValue ?? "unspecified") +
                    " | kind=" + (doc.fields.kind?.stringValue ?? "unspecified") +
                    " | question=" + LastDownloadedQuestion +
                    " | No live assessment was changed.");
            }
        }
    }

    // Avoid dumping response bodies or credentials into a screen shared during a defense.
    private static string SafeError(string body)
    {
        if(string.IsNullOrEmpty(body)) return "";
        return "See Firebase Console for more details.";
    }
}
