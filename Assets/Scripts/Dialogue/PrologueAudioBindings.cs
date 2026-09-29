using UnityEngine;

[DisallowMultipleComponent]
public sealed class PrologueAudioBindings : MonoBehaviour
{
    public static PrologueAudioBindings Instance { get; private set; }

    [Header("P-001 replaceable audio slots")]
    [SerializeField] private AudioClip prologueBgm;
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip dialogueAdvance;
    [SerializeField] private AudioClip bookOpen;
    [SerializeField] private AudioClip questPopup;
    [SerializeField] private AudioClip memoryReward;

    private void Awake()
    {
        Instance = this;
        if (prologueBgm != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgm(prologueBgm);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void PlayUiHover() => Play(Instance != null ? Instance.uiHover : null);
    public static void PlayUiClick() => Play(Instance != null ? Instance.uiClick : null);
    public static void PlayDialogueAdvance() => Play(Instance != null ? Instance.dialogueAdvance : null);
    public static void PlayBookOpen() => Play(Instance != null ? Instance.bookOpen : null);
    public static void PlayQuestPopup() => Play(Instance != null ? Instance.questPopup : null);
    public static void PlayMemoryReward() => Play(Instance != null ? Instance.memoryReward : null);

    private static void Play(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(clip);
        }
    }
}
