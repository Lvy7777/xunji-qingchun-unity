using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class YouthCrumblePlatform : MonoBehaviour
{
    private Collider2D platformCollider;
    private SpriteRenderer platformRenderer;
    private SpriteRenderer[] visualRenderers;

    private Vector3 restingPosition;
    private Color restingColor;
    private float warningTime = 0.55f;
    private float respawnTime = 2.6f;
    private Coroutine crumbleRoutine;

private void Awake()
    {
        CacheReferences();
    }

private void CacheReferences()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider2D>();
        }

        if (platformRenderer == null)
        {
            platformRenderer = GetComponent<SpriteRenderer>();
            visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            restingPosition = transform.localPosition;
            restingColor = platformRenderer.color;
        }
    }


    public void Configure(float warningDuration, float respawnDuration)
    {
        CacheReferences();

        warningTime = Mathf.Max(0.1f, warningDuration);
        respawnTime = Mathf.Max(0.5f, respawnDuration);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (crumbleRoutine == null && collision.collider.GetComponentInParent<YouthPlatformerPlayer>() != null)
        {
            crumbleRoutine = StartCoroutine(CrumbleRoutine());
        }
    }

    private IEnumerator CrumbleRoutine()
    {
        float elapsed = 0f;
        while (elapsed < warningTime)
        {
            elapsed += Time.deltaTime;
            float pulse = 0.65f + Mathf.PingPong(elapsed * 6f, 0.35f);
            platformRenderer.color = new Color(restingColor.r, pulse * restingColor.g, pulse * restingColor.b, 1f);
            transform.localPosition = restingPosition + Vector3.right * Mathf.Sin(elapsed * 52f) * 0.045f;
            yield return null;
        }

        transform.localPosition = restingPosition;
        platformCollider.enabled = false;
        for (int i = 0; i < visualRenderers.Length; i++)
        {
            visualRenderers[i].enabled = false;
        }
        yield return new WaitForSeconds(respawnTime);
        ResetPlatform();
    }

    public void ResetPlatform()
    {
        CacheReferences();

        if (crumbleRoutine != null)
        {
            StopCoroutine(crumbleRoutine);
            crumbleRoutine = null;
        }

        transform.localPosition = restingPosition;
        platformRenderer.color = restingColor;
        for (int i = 0; i < visualRenderers.Length; i++)
        {
            visualRenderers[i].enabled = true;
        }

        platformCollider.enabled = true;
    }
}