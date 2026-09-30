using System.Collections;
using UnityEngine;

public enum RedMemoryPickupKind { Fragment, ClueKey }

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemoryPickup : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private RedMemoryPickupKind kind;
    private int index;
    private bool collected;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private SpriteRenderer sprite;

    public void Configure(RedMemoryIntroController owner, RedMemoryPickupKind pickupKind, int pickupIndex)
    {
        chapter = owner;
        kind = pickupKind;
        index = pickupIndex;
        basePosition = transform.position;
        baseScale = transform.localScale;
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (collected) return;
        float phase = index * 0.83f;
        transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * 2.2f + phase) * 0.12f);
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * 2.8f + phase) * 0.035f);
        RedMemoryPlayerController player = chapter.Player;
        if (sprite != null && player != null)
        {
            float proximity = 1f - Mathf.Clamp01(Vector2.Distance(player.transform.position, transform.position) / 2.5f);
            sprite.color = Color.Lerp(Color.white, new Color(1f, 0.92f, 0.58f, 1f), proximity * 0.45f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        collected = true;
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        chapter.Collect(kind);
        Vector3 from = transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.12f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, from * 1.2f, elapsed / 0.12f);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from * 1.2f, Vector3.zero, elapsed / 0.18f);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemoryHazard : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    public void Configure(RedMemoryIntroController owner) => chapter = owner;
    private void OnTriggerEnter2D(Collider2D other)
    {
        RedMemoryPlayerController player = other.GetComponentInParent<RedMemoryPlayerController>();
        if (player != null) chapter.HandleHazard(transform.position);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemoryGoal : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    public void Configure(RedMemoryIntroController owner) => chapter = owner;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<RedMemoryPlayerController>() != null) chapter.TryReachGoal();
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemorySafePoint : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private Vector2 respawnPosition;
    public void Configure(RedMemoryIntroController owner, Vector2 position) { chapter = owner; respawnPosition = position; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<RedMemoryPlayerController>() != null) chapter.SetSafePosition(respawnPosition);
    }
}

public sealed class RedMemoryParallaxLayer : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 origin;
    private float factor;
    public void Configure(Transform targetCamera, float movementFactor) { cameraTransform = targetCamera; factor = movementFactor; origin = transform.position; }
    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        transform.position = new Vector3(origin.x + cameraTransform.position.x * factor, origin.y, origin.z);
    }
}

public sealed class RedMemoryCameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 velocity;
    public void Configure(Transform followTarget) => target = followTarget;
    public void Snap() { if (target != null) transform.position = new Vector3(Mathf.Clamp(target.position.x + 2f, 9f, 49f), 1.5f, -10f); }
    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(Mathf.Clamp(target.position.x + 2f, 9f, 49f), 1.5f, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.22f);
    }
}
