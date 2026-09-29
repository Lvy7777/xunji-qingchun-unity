using UnityEngine;
using UnityEngine.UI;

public sealed class HometownSceneAmbience : MonoBehaviour
{
    [SerializeField] private float hotspotPulseSpeed = 2.2f;
    [SerializeField] private float moteDriftSpeed = 0.65f;

    private RectTransform[] hotspotGlows;
    private CanvasGroup[] hotspotGroups;
    private Vector2[] hotspotPositions;
    private RectTransform[] motes;
    private Vector2[] motePositions;

    private void Awake()
    {
        Transform interactables = transform.Find("Interactables");
        int hotspotCount = interactables != null ? interactables.childCount : 0;
        hotspotGlows = new RectTransform[hotspotCount];
        hotspotGroups = new CanvasGroup[hotspotCount];
        hotspotPositions = new Vector2[hotspotCount];

        for (int i = 0; i < hotspotCount; i++)
        {
            Transform hotspot = interactables.GetChild(i);
            Transform glow = hotspot.Find("ExplorationGlow");
            if (glow == null)
            {
                continue;
            }

            hotspotGlows[i] = glow as RectTransform;
            hotspotPositions[i] = hotspotGlows[i].anchoredPosition;
            hotspotGroups[i] = glow.GetComponent<CanvasGroup>();

            if (hotspotGroups[i] == null)
            {
                hotspotGroups[i] = glow.gameObject.AddComponent<CanvasGroup>();
            }
        }

        Transform visualLayers = transform.Find("Environment/VisualLayers");
        int moteCount = visualLayers != null ? 4 : 0;
        motes = new RectTransform[moteCount];
        motePositions = new Vector2[moteCount];

        for (int i = 0; i < moteCount; i++)
        {
            Transform mote = visualLayers.Find("Mote_0" + (i + 1));
            motes[i] = mote as RectTransform;

            if (motes[i] != null)
            {
                motePositions[i] = motes[i].anchoredPosition;
            }
        }
    }

    private void Update()
    {
        float time = Time.unscaledTime;

        for (int i = 0; i < hotspotGlows.Length; i++)
        {
            if (hotspotGlows[i] == null)
            {
                continue;
            }

            float phase = time * hotspotPulseSpeed + i * 0.8f;
            float pulse = 0.5f + 0.5f * Mathf.Sin(phase);
            hotspotGroups[i].alpha = Mathf.Lerp(0.38f, 0.9f, pulse);
            hotspotGlows[i].localScale = Vector3.one * Mathf.Lerp(0.97f, 1.06f, pulse);
        }

        for (int i = 0; i < motes.Length; i++)
        {
            if (motes[i] == null)
            {
                continue;
            }

            float phase = time * moteDriftSpeed + i * 1.4f;
            motes[i].anchoredPosition = motePositions[i] + new Vector2(
                Mathf.Sin(phase) * 8f,
                Mathf.Cos(phase * 1.4f) * 11f);
        }
    }
}
