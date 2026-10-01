using System.Collections;
using UnityEngine;

public sealed class MovingPlatform : MonoBehaviour
{
    private Vector3 pointA;
    private Vector3 pointB;
    private float speed;
    private float progress;
    private int direction = 1;
    public void Configure(Vector3 offset, float moveSpeed)
    {
        pointA = transform.position;
        pointB = pointA + offset;
        speed = moveSpeed;
    }
    private void FixedUpdate()
    {
        float distance = Mathf.Max(0.01f, Vector3.Distance(pointA, pointB));
        progress += direction * speed * Time.fixedDeltaTime / distance;
        if (progress >= 1f) { progress = 1f; direction = -1; }
        else if (progress <= 0f) { progress = 0f; direction = 1; }
        transform.position = Vector3.Lerp(pointA, pointB, Mathf.SmoothStep(0f, 1f, progress));
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<RedMemoryPlayerController>() != null) collision.transform.SetParent(transform, true);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<RedMemoryPlayerController>() != null) collision.transform.SetParent(transform.parent, true);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class CrumblingPlatform : MonoBehaviour
{
    [SerializeField] private float warningDuration = 0.75f;
    [SerializeField] private float respawnDelay = 3f;
    private Collider2D platformCollider;
    private SpriteRenderer sprite;
    private Vector3 basePosition;
    private bool running;
    private RedMemoryIntroController chapter;
    public void Configure(RedMemoryIntroController owner) => chapter = owner;
    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        basePosition = transform.position;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!running && collision.collider.GetComponent<RedMemoryPlayerController>() != null) StartCoroutine(BreakRoutine());
    }
    public void BreakImmediately()
    {
        if (!running) StartCoroutine(BreakRoutine(0.08f));
    }
    private IEnumerator BreakRoutine(float delay = -1f)
    {
        running = true;
        chapter.NotifyCrumbleWarning(transform.position);
        float duration = delay >= 0f ? delay : warningDuration;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = basePosition + (Vector3)Random.insideUnitCircle * Mathf.Lerp(0.015f, 0.07f, elapsed / duration);
            if (sprite != null) sprite.color = Color.Lerp(Color.white, new Color(0.85f, 0.48f, 0.25f), elapsed / duration);
            yield return null;
        }
        platformCollider.enabled = false;
        if (sprite != null) sprite.enabled = false;
        transform.position = basePosition;
        chapter.NotifyPlatformBroken(basePosition);
        yield return new WaitForSeconds(respawnDelay);
        platformCollider.enabled = true;
        if (sprite != null) { sprite.enabled = true; sprite.color = Color.white; }
        running = false;
    }
}

public sealed class FallingRockTrap : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private RedMemoryPlayerController player;
    private Transform rock;
    private SpriteRenderer shadow;
    private Vector3 rockStart;
    private float cooldown = 2.5f;
    private float nextReady;
    private bool active;
    public void Configure(RedMemoryIntroController owner, RedMemoryPlayerController target, Transform rockTransform, SpriteRenderer warningShadow)
    {
        chapter = owner; player = target; rock = rockTransform; shadow = warningShadow; rockStart = rock.localPosition;
        rock.gameObject.SetActive(false); shadow.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active && Time.time >= nextReady && other.GetComponentInParent<RedMemoryPlayerController>() != null) StartCoroutine(DropRoutine());
    }
    private IEnumerator DropRoutine()
    {
        active = true;
        shadow.gameObject.SetActive(true);
        chapter.NotifyRockWarning(transform.position);
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            shadow.transform.localScale = Vector3.one * Mathf.Lerp(0.25f, 1.1f, elapsed / 0.6f);
            yield return null;
        }
        rock.gameObject.SetActive(true);
        rock.localPosition = rockStart;
        Vector3 end = new Vector3(rockStart.x, -0.2f, rockStart.z);
        elapsed = 0f;
        while (elapsed < 0.42f)
        {
            elapsed += Time.deltaTime;
            rock.localPosition = Vector3.Lerp(rockStart, end, elapsed / 0.42f);
            yield return null;
        }
        if (Vector2.Distance(player.transform.position, rock.position) < 1.15f) chapter.HandleHazard(rock.position);
        chapter.NotifyRockImpact(rock.position);
        rock.gameObject.SetActive(false);
        shadow.gameObject.SetActive(false);
        nextReady = Time.time + cooldown;
        active = false;
    }
}

public sealed class RollingBoulder : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private Transform boulder;
    private Vector3 start;
    private float speed;
    private bool rolling;
    public void Configure(RedMemoryIntroController owner, Transform visual, float rollSpeed)
    {
        chapter = owner; boulder = visual; start = visual.position; speed = rollSpeed; visual.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!rolling && other.GetComponentInParent<RedMemoryPlayerController>() != null) StartCoroutine(RollRoutine());
    }
    private IEnumerator RollRoutine()
    {
        rolling = true;
        boulder.position = start;
        boulder.gameObject.SetActive(true);
        chapter.NotifyBoulderStarted();
        float elapsed = 0f;
        while (elapsed < 9f)
        {
            elapsed += Time.deltaTime;
            boulder.position += Vector3.right * (speed * Time.deltaTime);
            boulder.Rotate(0f, 0f, -speed * 55f * Time.deltaTime);
            if (Vector2.Distance(chapter.Player.transform.position, boulder.position) < 1.25f) chapter.HandleHazard(boulder.position);
            yield return null;
        }
        boulder.gameObject.SetActive(false);
        rolling = false;
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class MemoryRepairPoint : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private GameObject bridge;
    private SpriteRenderer marker;
    private bool repaired;
    public void Configure(RedMemoryIntroController owner, GameObject repairedBridge)
    {
        chapter = owner; bridge = repairedBridge; marker = GetComponentInChildren<SpriteRenderer>(); bridge.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!repaired && other.GetComponentInParent<RedMemoryPlayerController>() != null) chapter.ShowRepairTutorial();
    }
    public void TryRepair()
    {
        if (repaired) return;
        repaired = true;
        StartCoroutine(RepairRoutine());
    }
    private IEnumerator RepairRoutine()
    {
        chapter.NotifyRepairStarted();
        float elapsed = 0f;
        Vector3 targetScale = bridge.transform.localScale;
        Vector3 from = targetScale * 0.05f;
        bridge.SetActive(true);
        bridge.transform.localScale = from;
        while (elapsed < 0.65f)
        {
            elapsed += Time.deltaTime;
            bridge.transform.localScale = Vector3.Lerp(from, targetScale, Mathf.SmoothStep(0f, 1f, elapsed / 0.65f));
            if (marker != null) marker.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.2f, 0f), elapsed / 0.65f);
            yield return null;
        }
        bridge.transform.localScale = targetScale;
        chapter.NotifyRepairCompleted();
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemoryCheckpoint : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private Vector2 respawnPosition;
    private SpriteRenderer marker;
    private bool activated;
    public void Configure(RedMemoryIntroController owner, Vector2 position)
    {
        chapter = owner; respawnPosition = position; marker = GetComponentInChildren<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        activated = true;
        if (marker != null) marker.color = new Color(1f, 0.55f, 0.16f, 1f);
        chapter.ActivateCheckpoint(respawnPosition);
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class RedMemoryDiscoveryZone : MonoBehaviour
{
    private RedMemoryIntroController chapter;
    private bool discovered;
    public void Configure(RedMemoryIntroController owner) => chapter = owner;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (discovered || other.GetComponentInParent<RedMemoryPlayerController>() == null) return;
        discovered = true;
        chapter.DiscoverHiddenArea();
    }
}
