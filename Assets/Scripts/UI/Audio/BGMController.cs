using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public sealed class SceneBgmEntry
{
    public string sceneName;
    public AudioClip clip;
    [TextArea] public string direction;
}

[DisallowMultipleComponent]
public sealed class BGMController : MonoBehaviour
{
    [SerializeField] private SceneBgmEntry[] entries = Array.Empty<SceneBgmEntry>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayFor(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayFor(scene.name);
    }

    private void PlayFor(string sceneName)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] != null && entries[i].sceneName == sceneName)
            {
                AudioManager.Instance.PlayBgm(entries[i].clip);
                return;
            }
        }
    }

    /*
    Local replacement plan:
    Prologue: warm piano + acoustic guitar + soft strings.
    RedMemory: dignified piano/strings; exploration gains a clearer pulse.
    HometownMemory: guzheng, bamboo flute, courtyard wind and birds.
    YouthMemory: fresh piano/guitar; rhythmic minigame is brighter.
    VillageMemory: warm rural life; quiz playful, poster-making relaxed.
    Ending: piano and strings with a gradual emotional resolution.
    */
}