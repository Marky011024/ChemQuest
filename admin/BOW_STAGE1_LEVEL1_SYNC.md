# Stage 1 Level 1 BOW → Unity approved snapshot (TEST FEATURE)

This branch includes **experimental** sync for Stage 1 Level 1's two formative BOW items.
It does NOT sync Pre-test, Post-test, or BOW in other stages/levels.
A published admin `questionBank` record is still admin-only and DOES NOT itself
update the Unity app. Admin must explicitly publish the two-item versioned set.

## Safety
- Back up the local Unity project before replacing scripts.
- No live assessment answers should be put in `syncPilot/demo`.
- Formative BOW answers in `publishedBow/stage1-level1` are readable to
  authenticated student clients; Firebase Anonymous auth is not confidentiality.
- Use approved teacher-reviewed BOW content and don't publish changes during
  data collection. Do not use this experimental feature for respondents before
  testing on Android.
- Existing gameplay logic, retry behavior and score callbacks are not changed.

## Firestore
1. Review and publish the updated `admin/firestore.rules` safely (preserving
   existing admins and questionBank permissions).
2. It adds only:
   `publishedBow/stage1-level1` authenticated document GET, when status is
   `published`, admin-only writes, and no list access.
3. Keep anonymous Authentication enabled if using the Unity pilot.

## Admin web
1. Deploy latest `admin/index.html` through the existing Firebase Hosting
   website. Hosting deployment does NOT deploy rules.
2. In the admin Question Bank, create **exactly two** BOW entries:
   stage 1, level 1, question order 1 and 2, A/B choices, correct A or B,
   nonempty hint and explanation. Leave C/D blank.
3. After teacher approval, publish both records individually.
4. Click **Publish approved Level 1 set to Unity** under Question Bank.
   Confirm to write a single document `publishedBow/stage1-level1` with
   status, version, payload and updatedAt. This is a *manual explicit*
   publish step, not automatic per-question publishing.

## Unity
1. The new local installer/updater places updated
   `Assets/Scripts/Stage1BowLearningCheck.cs` and
   `Assets/Scripts/ChemQuestQuestionSyncPilot.cs`, with .bak backups.
2. Existing `FirebaseSyncPilot` GameObject in Stage1Level1 must have
   `Run On Start` enabled and the Firebase API key set.
3. Run Stage1Level1 Play Mode and inspect Console:
   `CHEMQUEST BOW SNAPSHOT SYNC OK`.
   If no snapshot yet, a 404 warning is expected and original questions
   remain available.
4. Trigger the Level 1 BOW learning check, which will use a validated cached
   two-item snapshot. The snapshot is locked per attempt.
5. Test an edit-and-republish cycle. Exit Play/enter again to fetch the
   newest set. While offline, cached set remains; if no cache, hardcoded
   Level 1 questions remain. Other levels always use original questions.
6. Verify on Android APK separately before student use.

## Limitations
- This iteration checks for updates when the pilot starts, not continuously.
- A snapshot published after the last successful fetch won't be seen until
  another successful fetch. The cache is local PlayerPrefs, not encrypted.
- No rollback UI, expiry/revocation, or full pre/post-test version lock exists.
- No automated device test or Firestore deploy is executed by GitHub commits.
