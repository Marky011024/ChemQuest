# ChemQuest Unity ↔ Firestore sync pilot (non-production)

This is a deliberately isolated connectivity test. It DOES NOT replace the game's
hardcoded questions in `ChemQuestAssessmentGate.cs` or `Stage1BowLearningCheck.cs`.
It does not automatically sync the admin-only `questionBank` collection.

## What already exists in the code branch

- `Assets/Scripts/ChemQuestQuestionSyncPilot.cs` signs in anonymously with Firebase
  Authentication and reads exactly `syncPilot/demo` via Firestore REST.
- `admin/firestore.rules` includes a pilot-only rule: authenticated users may
  **get** `syncPilot/demo` only when status is `published`; only authorized admins
  may write it. No list access. Admin-only questionBank remains unchanged.
- Neither Firestore rules nor Unity scenes are deployed/modified automatically.

## Set up in Firebase Console (do this before running)

1. **Authentication > Sign-in method > Anonymous > Enable.**
   Email/Password admin login stays enabled.
2. Open the **ChemQuest Admin** project Firestore Rules. Safely merge and publish
   the new `syncPilot` block from `admin/firestore.rules` into the existing
   production rules. Keep the `admins` and `questionBank` restrictions in place.
   Alternatively deploy from a properly configured local Firebase project using
   `firebase deploy --only firestore:rules --project chemquest-admin`.
   **Note:** `firebase deploy --only hosting` does NOT deploy Firestore rules.
3. Firestore > Data > **Start collection**: `syncPilot`; document ID `demo`.
   Add fields:
   - `status` (string): `published`
   - `version` (string): `pilot-v1`
   - `kind` (string): `bow`
   - `stage` (number): `1`
   - `order` (number): `1`
   - `prompt` (string): `TEST ONLY: Which observation can indicate a chemical reaction?`
   This is intentionally a harmless sample, **not** a graded pre/post-test item.
   No answer key should be included in a public-readable test document.

## Unity (backup only)

1. Keep a backup of `D:\ChemQuest_AndroidBuild` before pulling changes.
2. Copy/pull `ChemQuestQuestionSyncPilot.cs` into the same `Assets/Scripts`
   location in the backup Unity project; this GitHub commit does **not** change
   already installed files on your computer.
3. Open Unity 6000.6.10f1. Use an existing temporary test scene or GameObject,
   attach `ChemQuestQuestionSyncPilot`.
4. Find the Firebase **Web API key** under Firebase Console > Project settings
   > Your apps > Web app configuration. Put it into the script's Inspector
   `Web Api Key` field (do not paste passwords or service-account private keys).
5. Check `Run On Start` or enter Play mode and invoke
   `Test Firestore Published Question Sync` from the component context menu.
6. The Console should display:
   `CHEMQUEST SYNC PILOT OK | version=pilot-v1 ...`
7. Change the test document prompt in Firestore and repeat Play to verify
   the new value appears. Switching status to `draft` should result in a
   Firestore read denied error. This verifies the access control.

## Interpreting errors

- `400/OPERATION_NOT_ALLOWED`: Anonymous sign-in not enabled.
- `403/PERMISSION_DENIED`: pilot rules not deployed, wrong project, wrong path,
  or document not published.
- `404`: `syncPilot/demo` missing.
- If Play works but Android doesn't: confirm internet access and Android
  Internet permission in Unity Player Settings.

## After pilot success, BEFORE real student use

- Create teacher-approved immutable versioned question-set snapshots.
- Require approved question counts and clear stage/level mapping.
- Integrate the Unity pre/post/BOW screen data source with a version lock
  while an attempt is in progress.
- Implement safe offline fallback, rollback, and test on an Android device.
- Do not expose graded assessment answer keys in a publicly readable
  Firestore document. Firebase anonymous authentication does not prevent
  a student from inspecting content available to the app.
- Do not modify or redeploy research assessments mid-testing.
