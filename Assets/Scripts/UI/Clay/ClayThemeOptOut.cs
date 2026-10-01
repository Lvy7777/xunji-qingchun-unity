using UnityEngine;

/// <summary>
/// Marks a canvas that owns a scene-specific presentation system and should not
/// be restyled by the shared clay theme maintenance pass.
/// </summary>
[DisallowMultipleComponent]
public sealed class ClayThemeOptOut : MonoBehaviour
{
}
