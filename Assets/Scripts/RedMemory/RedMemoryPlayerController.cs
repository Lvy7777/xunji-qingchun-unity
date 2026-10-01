using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(RedMemoryPlayerCombat))]
public sealed class RedMemoryPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.5f;
    [SerializeField] private float acceleration = 36f;
    [SerializeField] private float deceleration = 44f;
    [SerializeField] private float jumpHeight = 3.1f;
    [SerializeField, Range(0.08f, 0.12f)] private float coyoteTime = 0.1f;
    [SerializeField, Range(0.08f, 0.12f)] private float jumpBuffer = 0.1f;
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private float fallLimit = -7f;
    [Header("Optional animation interface")]
    [SerializeField] private Animator animator;

    private readonly Collider2D[] groundHits = new Collider2D[8];
    private Rigidbody2D body;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer[] renderers;
    private RedMemoryIntroController chapter;
    private Transform visualRoot;
    private RedMemoryPlayerVisual playerVisual;
    private RedMemoryPlayerCombat combat;
    private float coyoteTimer;
    private float bufferTimer;
    private float hurtControlLock;
    private bool inputEnabled;
    private bool jumpCutRequested;
    private bool respawning;
    private bool wasGrounded;

    public bool IsGrounded { get; private set; }
    public bool InputEnabled => inputEnabled;
    public bool IsControlLocked => hurtControlLock > 0f || combat.IsStomping;
    public Rigidbody2D Body => body;
    public Bounds ColliderBounds => bodyCollider.bounds;
    public RedMemoryPlayerCombat Combat => combat;

    public void Configure(RedMemoryIntroController owner, Transform visuals)
    {
        chapter = owner;
        visualRoot = visuals;
        playerVisual = visuals != null ? visuals.GetComponent<RedMemoryPlayerVisual>() : null;
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        combat.Configure(this, owner);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.gravityScale = 3.2f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bodyCollider = GetComponent<BoxCollider2D>();
        combat = GetComponent<RedMemoryPlayerCombat>();
        PhysicsMaterial2D material = new PhysicsMaterial2D("RedMemoryPlayerMaterial") { friction = 0f, bounciness = 0f };
        body.sharedMaterial = material;
    }

    private void Update()
    {
        CheckGrounded();
        hurtControlLock = Mathf.Max(0f, hurtControlLock - Time.deltaTime);
        coyoteTimer = IsGrounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        bufferTimer = Mathf.Max(0f, bufferTimer - Time.deltaTime);
        if (inputEnabled && !IsControlLocked && Input.GetKeyDown(KeyCode.Space)) bufferTimer = jumpBuffer;
        if (inputEnabled && Input.GetKeyUp(KeyCode.Space)) jumpCutRequested = true;
        if (inputEnabled && transform.position.y < fallLimit && !respawning)
        {
            respawning = true;
            chapter.HandleFall();
        }
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(body.velocity.x));
            animator.SetBool("IsGrounded", IsGrounded);
            animator.SetFloat("VerticalVelocity", body.velocity.y);
            animator.SetBool("IsHurt", hurtControlLock > 0f);
            animator.SetBool("IsStomping", combat.IsStomping);
        }
        if (!wasGrounded && IsGrounded)
        {
            chapter.PlayLandSfx();
            combat.NotifyLanded();
        }
        wasGrounded = IsGrounded;
    }

    private void FixedUpdate()
    {
        if (!inputEnabled || combat.IsStomping) return;
        float axis = hurtControlLock > 0f ? 0f : Input.GetAxisRaw("Horizontal");
        float target = axis * moveSpeed;
        float rate = Mathf.Abs(axis) > 0.01f ? acceleration : deceleration;
        Vector2 velocity = body.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, target, rate * Time.fixedDeltaTime);
        if (hurtControlLock <= 0f && bufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y * body.gravityScale) * jumpHeight);
            bufferTimer = 0f;
            coyoteTimer = 0f;
            chapter.PlayJumpSfx();
        }
        if (jumpCutRequested && velocity.y > 0f) velocity.y *= 0.5f;
        jumpCutRequested = false;
        body.velocity = velocity;
        if (visualRoot != null && Mathf.Abs(axis) > 0.05f)
        {
            if (playerVisual != null) playerVisual.SetFacing(axis);
            else
            {
                Vector3 scale = visualRoot.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(axis);
                visualRoot.localScale = scale;
            }
        }
    }

    private void CheckGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 size = new Vector2(bounds.size.x * 0.72f, 0.08f);
        int count = Physics2D.OverlapBoxNonAlloc(new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance * 0.5f), size, 0f, groundHits);
        IsGrounded = false;
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = groundHits[i];
            if (hit != null && hit != bodyCollider && !hit.isTrigger && hit.GetComponentInParent<EnemyBase>() == null)
            {
                IsGrounded = true;
                break;
            }
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled) body.velocity = Vector2.zero;
    }

    public void Knockback(Vector2 source)
    {
        float direction = transform.position.x >= source.x ? 1f : -1f;
        body.velocity = new Vector2(direction * 4.8f, 4.5f);
        hurtControlLock = 0.28f;
        if (playerVisual != null) playerVisual.PlayHurt();
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Hurt");
        StartCoroutine(Flash());
    }

    public void BounceFromStomp(float force = 7.2f)
    {
        body.velocity = new Vector2(body.velocity.x, force);
        combat.CancelStomp();
    }

    public void PlayCheer()
    {
        if (playerVisual != null) playerVisual.PlayCheer();
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetBool("Celebrate", true);
    }

    public void PlayGroundStomp()
    {
        if (playerVisual != null) playerVisual.PlayGroundStomp();
    }

    public void PlayDefeat()
    {
        if (playerVisual != null) playerVisual.PlayDefeat();
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetBool("IsDead", true);
    }

    private IEnumerator Flash()
    {
        float elapsed = 0f;
        while (elapsed < 0.95f)
        {
            elapsed += 0.1f;
            float alpha = Mathf.FloorToInt(elapsed * 10f) % 2 == 0 ? 0.35f : 1f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = renderers[i].color;
                color.a = alpha;
                renderers[i].color = color;
            }
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = renderers[i].color;
            color.a = 1f;
            renderers[i].color = color;
        }
    }

    public void Respawn(Vector2 position)
    {
        transform.position = position;
        body.velocity = Vector2.zero;
        coyoteTimer = coyoteTime;
        bufferTimer = 0f;
        respawning = false;
        combat.CancelStomp();
        inputEnabled = true;
    }
}
