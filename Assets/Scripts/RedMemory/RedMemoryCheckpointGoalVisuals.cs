using System.Collections;
using UnityEngine;

public sealed class CheckpointVisualController : MonoBehaviour
{
    private SpriteRenderer sign;
    private SpriteRenderer lamp;
    private SpriteRenderer glow;
    private SpriteRenderer ring;
    private Vector3 lampBaseScale;
    private bool active;

    public void Configure(SpriteRenderer signRenderer, SpriteRenderer lampRenderer, SpriteRenderer glowRenderer, SpriteRenderer ringRenderer,
        Sprite idleSprite, Sprite activeSprite, Sprite glowSprite, Sprite effectSprite)
    {
        sign = signRenderer; lamp = lampRenderer; glow = glowRenderer; ring = ringRenderer;
        if (sign != null && idleSprite != null) sign.sprite = idleSprite;
        if (lamp != null && activeSprite != null) lamp.sprite = activeSprite;
        if (glow != null && glowSprite != null) glow.sprite = glowSprite;
        if (ring != null && effectSprite != null) ring.sprite = effectSprite;
        lampBaseScale = lamp != null ? lamp.transform.localScale : Vector3.one;
        if (glow != null) glow.color = new Color(1f, 0.55f, 0.12f, 0.12f);
        if (ring != null) ring.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (lamp == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * (active ? 3.5f : 1.8f)) * (active ? 0.045f : 0.018f);
        lamp.transform.localScale = lampBaseScale * pulse;
        if (active && glow != null) glow.color = new Color(1f, 0.48f, 0.08f, 0.26f + Mathf.Sin(Time.time * 3.5f) * 0.06f);
    }

    public void Activate()
    {
        if (active) return;
        active = true;
        if (sign != null) sign.color = new Color(0.95f, 0.54f, 0.20f, 1f);
        if (lamp != null) lamp.color = new Color(1f, 0.68f, 0.22f, 1f);
        if (ring != null) StartCoroutine(RingRoutine());
    }

    private IEnumerator RingRoutine()
    {
        ring.gameObject.SetActive(true);
        ring.transform.localScale = Vector3.one * 0.2f;
        Color color = ring.color;
        float elapsed = 0f;
        while (elapsed < 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.7f);
            ring.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 2.2f, t);
            ring.color = new Color(color.r, color.g, color.b, 1f - t);
            yield return null;
        }
        ring.gameObject.SetActive(false);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class CheckpointRespawnPoint : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private CheckpointVisualController visual;
    private Vector2 respawnPosition;
    private bool activated;
    public void Configure(RedMemoryIntroController owner, Vector2 position, CheckpointVisualController visualController)
    {
        chapter = owner; respawnPosition = position; visual = visualController;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        activated = true;
        if (visual != null) visual.Activate();
        chapter.ActivateCheckpoint(respawnPosition);
    }
}

public sealed class GoalMemoryGatherController : MonoBehaviour
{
    private Transform moteRoot;
    private SpriteRenderer core;
    private SpriteRenderer lockedSeal;
    private Vector3 coreScale;

    public void Configure(Transform particles, SpriteRenderer coreRenderer, SpriteRenderer sealRenderer)
    {
        moteRoot = particles; core = coreRenderer; lockedSeal = sealRenderer;
        coreScale = core != null ? core.transform.localScale : Vector3.one;
        SetLocked(true);
    }

    public void SetLocked(bool locked)
    {
        if (lockedSeal != null) lockedSeal.gameObject.SetActive(locked);
        if (core != null) core.color = locked ? new Color(0.58f, 0.49f, 0.43f, 0.88f) : Color.white;
    }

    public void ShowMissing(bool missingFragments, bool missingKey)
    {
        StopCoroutine(nameof(MissingPulseRoutine));
        StartCoroutine(MissingPulseRoutine(missingFragments, missingKey));
    }

    private IEnumerator MissingPulseRoutine(bool missingFragments, bool missingKey)
    {
        Color coreBase = new Color(0.58f, 0.49f, 0.43f, 0.88f);
        Color weakMemory = new Color(0.55f, 0.25f, 0.18f, 0.82f);
        Color lockedClue = new Color(1f, 0.58f, 0.16f, 1f);
        for (int i = 0; i < 6; i++)
        {
            if (core != null) core.color = missingFragments && i % 2 == 0 ? weakMemory : coreBase;
            if (lockedSeal != null)
            {
                lockedSeal.gameObject.SetActive(true);
                lockedSeal.color = missingKey && i % 2 == 0 ? lockedClue : Color.white;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (core != null) core.color = coreBase;
        if (lockedSeal != null) lockedSeal.color = Color.white;
    }

    public IEnumerator PlayGather(Transform player)
    {
        SetLocked(false);
        if (moteRoot != null) moteRoot.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < 1.05f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 1.05f);
            if (core != null) core.transform.localScale = coreScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.16f);
            if (moteRoot != null)
            {
                moteRoot.Rotate(0f, 0f, 95f * Time.unscaledDeltaTime);
                moteRoot.localScale = Vector3.one * Mathf.Lerp(1.4f, 0.12f, t);
                if (player != null) moteRoot.position = Vector3.Lerp(transform.position, player.position + Vector3.up * 0.45f, t);
            }
            yield return null;
        }
        if (core != null) core.transform.localScale = coreScale;
        if (moteRoot != null) moteRoot.gameObject.SetActive(false);
    }
}
