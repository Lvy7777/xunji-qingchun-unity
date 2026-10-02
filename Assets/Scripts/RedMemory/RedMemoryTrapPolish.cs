using System.Collections;
using UnityEngine;

public sealed class TrapSequenceController : MonoBehaviour
{
    [SerializeField] private string sequenceId;
    [SerializeField] private string readablePattern;
    public string SequenceId => sequenceId;
    public void Configure(string id, string pattern) { sequenceId = id; readablePattern = pattern; }
}

public sealed class TrapTelegraphController : MonoBehaviour
{
    private SpriteRenderer indicator;
    private Vector3 baseScale;
    public void Configure(SpriteRenderer warningIndicator)
    {
        indicator = warningIndicator;
        if (indicator != null) baseScale = indicator.transform.localScale;
    }
    public IEnumerator Pulse(float duration)
    {
        if (indicator == null) yield break;
        indicator.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.12f;
            indicator.transform.localScale = baseScale * pulse;
            yield return null;
        }
        indicator.transform.localScale = baseScale;
        indicator.gameObject.SetActive(false);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class EnvironmentalSpikeTrap : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private float nextHit;
    public void Configure(RedMemoryIntroController owner) => chapter = owner;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHit || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        nextHit = Time.time + 0.35f;
        chapter.HandleHazard(transform.position);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class SwingingHazard : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private Transform pivot;
    private float amplitude;
    private float frequency;
    private float phase;
    private float nextHit;
    public void Configure(RedMemoryIntroController owner, Transform swingPivot, float degrees, float cyclesPerSecond, float phaseOffset)
    {
        chapter = owner; pivot = swingPivot; amplitude = degrees; frequency = cyclesPerSecond; phase = phaseOffset;
    }
    private void FixedUpdate()
    {
        if (pivot != null) pivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin((Time.time + phase) * frequency * Mathf.PI * 2f) * amplitude);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHit || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        nextHit = Time.time + 0.3f;
        chapter.HandleHazard(transform.position);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class MemoryFractureFloor : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private Collider2D floorCollider;
    private SpriteRenderer floorRenderer;
    private bool cycling;
    public void Configure(RedMemoryIntroController owner)
    {
        chapter = owner;
        floorCollider = GetComponent<Collider2D>();
        floorRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!cycling && collision.collider.GetComponent<RedMemoryPlayerController>() != null) StartCoroutine(FractureRoutine());
    }
    private IEnumerator FractureRoutine()
    {
        cycling = true;
        chapter.NotifyCrumbleWarning(transform.position);
        for (int i = 0; i < 7; i++)
        {
            if (floorRenderer != null) floorRenderer.color = i % 2 == 0 ? new Color(1f, 0.35f, 0.18f, 0.5f) : Color.white;
            yield return new WaitForSeconds(0.09f);
        }
        floorCollider.enabled = false;
        if (floorRenderer != null) floorRenderer.enabled = false;
        chapter.NotifyPlatformBroken(transform.position);
        yield return new WaitForSeconds(2.4f);
        floorCollider.enabled = true;
        if (floorRenderer != null) { floorRenderer.enabled = true; floorRenderer.color = Color.white; }
        cycling = false;
    }
}
