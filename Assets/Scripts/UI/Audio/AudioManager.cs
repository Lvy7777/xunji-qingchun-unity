using System;
using UnityEngine;

public enum UISound
{
    Hover,
    Click,
    PageTurn,
    DialogueAdvance,
    ChapterTitle,
    Collect,
    Reward,
    Correct,
    Incorrect,
    Complete
}

[Serializable]
public sealed class AudioCueEntry
{
    public UISound cue;
    public AudioClip clip;
}

[DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Assign local clips here after importing into Assets/Audio")]
    [SerializeField] private AudioCueEntry[] uiCues = Array.Empty<AudioCueEntry>();
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();
    }

    public void PlayUi(UISound cue)
    {
        for (int i = 0; i < uiCues.Length; i++)
        {
            if (uiCues[i] != null && uiCues[i].cue == cue && uiCues[i].clip != null)
            {
                sfxSource.PlayOneShot(uiCues[i].clip, sfxVolume);
                return;
            }
        }
    }

    public void PlayBgm(AudioClip clip, float fadeSeconds = 0.8f)
    {
        if (clip == null || bgmSource.clip == clip)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    private void EnsureSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }
}