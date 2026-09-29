using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GlobalPresentationBootstrap : MonoBehaviour
{
    private static GlobalPresentationBootstrap instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null)
        {
            return;
        }

        GameObject root = new GameObject("GlobalPresentation");
        instance = root.AddComponent<GlobalPresentationBootstrap>();
        DontDestroyOnLoad(root);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (GetComponent<AudioManager>() == null) gameObject.AddComponent<AudioManager>();
        if (GetComponent<BGMController>() == null) gameObject.AddComponent<BGMController>();
        if (GetComponent<CursorEffectController>() == null) gameObject.AddComponent<CursorEffectController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ApplyToScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

    private static void ApplyToScene(Scene scene)
    {
        PortraitDisplayController.ApplyToLoadedScene(scene);
        HandbookCoverController.ApplyToLoadedScene(scene);

        Button[] buttons = Object.FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (scene.name == "Prologue" &&
                buttons[i].gameObject.scene == scene &&
                buttons[i].GetComponent<UIButtonAnimator>() == null)
            {
                buttons[i].gameObject.AddComponent<UIButtonAnimator>();
            }
        }

        TMP_FontAsset bodyFont = Resources.Load<TMP_FontAsset>("Fonts/HYAoJiaoTiJian/HYAoJiaoTiJian");
        if (bodyFont == null)
        {
            return;
        }

        TMP_Text[] texts = Object.FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].gameObject.scene == scene && !texts[i].name.Contains("Title"))
            {
                texts[i].font = bodyFont;
            }
        }
    }
}
