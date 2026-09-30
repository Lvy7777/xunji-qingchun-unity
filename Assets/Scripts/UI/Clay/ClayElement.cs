using UnityEngine;

[DisallowMultipleComponent]
public sealed class ClayElement : MonoBehaviour
{
    [SerializeField] private ClayRole role = ClayRole.Auto;
    [SerializeField] private ClayTone tone = ClayTone.Auto;
    [SerializeField] private bool keepSprite;
    [SerializeField] private bool keepColor;
    [SerializeField] private bool disableMotion;

    public ClayRole Role => role;
    public ClayTone Tone => tone;
    public bool KeepSprite => keepSprite;
    public bool KeepColor => keepColor;
    public bool DisableMotion => disableMotion;
}
