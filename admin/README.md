# ChemQuest Admin Question Bank — Initial Development

This is the first **working source-code foundation** of the web admin tool.
It is intentionally on a separate branch and does not modify Unity gameplay.

## Current functionality
- Firebase Email/Password sign-in
- An `admins/{uid}` active-role check
- Draft question creation/editing for Pre-test, BOW (Stage 1–5 Level 1–3), and Post-test
- Search/filter and explicit Publish action
- Protected Firestore questionBank collection; no unauthenticated access

## Setup before running
1. Create a Firebase project through the Firebase Console.
2. Enable Authentication > Sign-in method > Email/Password.
3. Register a Firebase **Web** app and copy its config.
4. Copy `firebase-config.example.js` to `firebase-config.js` and insert your Web app config.
5. Create an Email/Password user in Authentication.
6. In Firestore Console, create `admins/{USER_UID}` with Boolean `active: true`. Do this only for the real authorized admin.
7. Create a Firestore database and deploy the contents of `firestore.rules` as its security rules. Review rules before applying any changes to an existing production project.
8. Use a local HTTP server from the `admin/` directory (for example `python -m http.server 8080`); open `http://localhost:8080`. The app uses ES modules and should not be opened from a file:// URL.
9. Only deploy publicly after verifying Authentication, Firestore security rules, and admin authorization.

## Pending before student use
- Import and reconcile **existing** Unity pre/post/BOW questions into an approved versioned seed set.
- Build a separate immutable published snapshot with version IDs and approved question counts.
- Unity offline JSON bundle + online update check + atomic download and version lock while a student is answering.
- Test the entire flow using test accounts and devices; avoid editing question content during collection of actual research assessments.
- Production hosting and first-admin provisioning; deployment has NOT been completed.

**Important:** Current Publish marks individual questions as approved within the ADMIN-ONLY Firestore collection. It does not yet trigger updates in the Unity app. No working online question synchronization or hosting is claimed.
