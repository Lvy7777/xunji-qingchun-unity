using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RedMemoryPlayerCombat : MonoBehaviour
{
    [SerializeField] private float stompPause = 0.08f;
    [SerializeField] private float stompSpeed = 18f;
    [SerializeField] private float impactRadius = 1.15f;
    private readonly Collider2D[] impactHits = new Collider2D[16];
    private RedMemoryPlayerController player;
    private RedMemoryIntroController chapter;
    private Rigidbody2D body;
    private Coroutine stompRoutine;
    private bool impactPending;
    public bool IsStomping { get; private set; }

    public void Configure(RedMemoryPlayerController owner, RedMemoryIntroController game)
    {
        player = owner;
        chapter = game;
        body = owner.Body;
    }

    private void Update()
    {
        if (player == null || !player.InputEnabled || player.IsGrounded || IsStomping) return;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) stompRoutine = StartCoroutine(GroundStompRoutine());
    }

    private IEnumerator GroundStompRoutine()
    {
        IsStomping = true;
        impactPending = true;
        body.velocity = Vector2.zero;
        body.gravityScale = 0f;
        player.PlayGroundStomp();
        chapter.NotifyGroundStompStarted();
        yield return new WaitForSeconds(stompPause);
        body.gravityScale = 3.2f;
        body.velocity = Vector2.down * stompSpeed;
        stompRoutine = null;
    }

    public void NotifyLanded()
    {
        if (!impactPending) return;
        impactPending = false;
        IsStomping = false;
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, impactRadius, impactHits);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = impactHits[i];
            if (hit == null) continue;
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy != null) enemy.TakeHit(2, true);
            CrumblingPlatform crumble = hit.GetComponentInParent<CrumblingPlatform>();
            if (crumble != null) crumble.BreakImmediately();
            MemoryRepairPoint repair = hit.GetComponentInParent<MemoryRepairPoint>();
            if (repair != null) repair.TryRepair();
        }
        chapter.NotifyGroundStompImpact(transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHurtbox hurtbox = other.GetComponent<EnemyHurtbox>();
        if (hurtbox == null || body.velocity.y >= -0.05f) return;
        float feet = player.ColliderBounds.min.y;
        bool clearlyAbove = feet >= hurtbox.WorldTop - 0.28f && transform.position.y > hurtbox.transform.position.y + 0.35f;
        if (!clearlyAbove) return;
        int damage = IsStomping ? 2 : 1;
        if (hurtbox.Owner.TakeHit(damage, IsStomping))
        {
            chapter.NotifyEnemyStomp(hurtbox.transform.position);
            player.BounceFromStomp(IsStomping ? 8.2f : 7.2f);
        }
    }

    public void CancelStomp()
    {
        if (stompRoutine != null) StopCoroutine(stompRoutine);
        stompRoutine = null;
        impactPending = false;
        IsStomping = false;
        if (body != null) body.gravityScale = 3.2f;
    }
}
