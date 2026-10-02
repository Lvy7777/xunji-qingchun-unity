using System.Collections;
using UnityEngine;

public enum RedMemoryPickupKind { Fragment, ClueKey, MemoryGlow, HealingFlower }

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
        RedMemoryPlayerController player = chapter != null ? chapter.Player : null;
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
    private GoalMemoryGatherController gather;
    public void Configure(RedMemoryIntroController owner, GoalMemoryGatherController gatherController) { chapter = owner; gather = gatherController; }
    public GoalMemoryGatherController GatherController => gather;
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
    private float cameraOriginX;
    private float factor;
    public void Configure(Transform targetCamera, float movementFactor)
    {
        cameraTransform = targetCamera;
        factor = movementFactor;
        origin = transform.position;
        cameraOriginX = targetCamera != null ? targetCamera.position.x : 0f;
    }
    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        float cameraDelta = cameraTransform.position.x - cameraOriginX;
        transform.position = new Vector3(origin.x + cameraDelta * (1f - factor), origin.y, origin.z);
    }
}

public sealed class RedMemoryCameraFollow : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D targetBody;
    private Vector3 velocity;
    private Camera followCamera;
    private float defaultSize;
    private Transform focusTarget;
    private float focusUntil;
    public void Configure(Transform followTarget)
    {
        target = followTarget;
        targetBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
        followCamera = GetComponent<Camera>();
        defaultSize = followCamera != null ? followCamera.orthographicSize : 5.2f;
    }
    public void Snap() { if (target != null) transform.position = DesiredPosition(); }
    public void FocusOn(Transform focus, float duration) { focusTarget = focus; focusUntil = Time.unscaledTime + duration; }
    private void LateUpdate()
    {
        if (target == null) return;
        if (focusTarget != null && Time.unscaledTime >= focusUntil) focusTarget = null;
        Vector3 desired = focusTarget != null
            ? new Vector3(Mathf.Clamp(focusTarget.position.x, 9f, 139f), Mathf.Clamp(focusTarget.position.y + 1.2f, 1.25f, 3.4f), -10f)
            : DesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.24f);
        if (followCamera != null)
        {
            float targetSize = target.position.x > 65f && target.position.x < 111f ? defaultSize + 0.65f : defaultSize;
            followCamera.orthographicSize = Mathf.Lerp(followCamera.orthographicSize, targetSize, Time.deltaTime * 2.2f);
        }
    }
    private Vector3 DesiredPosition()
    {
        float horizontalVelocity = targetBody != null ? targetBody.velocity.x : 0f;
        float lookAhead = Mathf.Clamp(horizontalVelocity * 0.38f, -2.2f, 2.8f);
        float vertical = Mathf.Clamp(target.position.y + 1.8f, 1.25f, 3.4f);
        return new Vector3(Mathf.Clamp(target.position.x + lookAhead, 9f, 139f), vertical, -10f);
    }
}
