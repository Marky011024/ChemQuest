using UnityEngine;

// Saved placement metadata for the original Stage 1 Level 1 microscope button.
// Contains no gameplay logic. Helps prevent repeated Editor installs moving UI again.
[DisallowMultipleComponent]
public sealed class ChemQuestMicroscopeLayoutMarker : MonoBehaviour
{
    [HideInInspector] public bool applied;
    [HideInInspector] public Vector2 originalButtonPosition;
    [HideInInspector] public Vector2 originalFlaskPosition;
}
