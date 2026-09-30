using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
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
    [SerializeField] private float fallLimit = -6f;
    [Header("Optional animation interface")]
    [SerializeField] private Animator animator;

    private Rigidbody2D body;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer[] renderers;
    private RedMemoryIntroController chapter;
    private Transform visualRoot;
    private float coyoteTimer;
    private float bufferTimer;
    private bool inputEnabled;
    private bool jumpCutRequested;
    private bool respawning;
    private bool wasGrounded;

    public bool IsGrounded { get; private set; }
    public bool InputEnabled => inputEnabled;
    public Rigidbody2D Body => body;

    public void Configure(RedMemoryIntroController owner, Transform visuals)
    {
        chapter = owner;
        visualRoot = visuals;
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.gravityScale = 3.2f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bodyCollider = GetComponent<BoxCollider2D>();
        PhysicsMaterial2D material = new PhysicsMaterial2D("RedMemoryPlayerMaterial") { friction = 0f, bounciness = 0f };
        body.sharedMaterial = material;
    }

    private void Update()
    {
        CheckGrounded();
        coyoteTimer = IsGrounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        bufferTimer = Mathf.Max(0f, bufferTimer - Time.deltaTime);

        if (inputEnabled && Input.GetKeyDown(KeyCode.Space)) bufferTimer = jumpBuffer;
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
        }

        if (!wasGrounded && IsGrounded && Mathf.Abs(body.velocity.y) < 0.4f) chapter.PlayLandSfx();
        wasGrounded = IsGrounded;
    }

    private void FixedUpdate()
    {
        if (!inputEnabled) return;

        float axis = Input.GetAxisRaw("Horizontal");
        float target = axis * moveSpeed;
        float rate = Mathf.Abs(axis) > 0.01f ? acceleration : deceleration;
        Vector2 velocity = body.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, target, rate * Time.fixedDeltaTime);

        if (bufferTimer > 0f && coyoteTimer > 0f)
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
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(axis);
            visualRoot.localScale = scale;
        }
    }

    private void CheckGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 size = new Vector2(bounds.size.x * 0.78f, 0.08f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance * 0.5f), size, 0f);
        IsGrounded = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != bodyCollider && !hits[i].isTrigger)
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
        body.velocity = new Vector2(direction * 5.2f, 5.2f);
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Hurt");
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        float elapsed = 0f;
        while (elapsed < 0.9f)
        {
            elapsed += 0.1f;
            bool visible = Mathf.FloorToInt(elapsed * 10f) % 2 == 0;
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = visible;
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = true;
    }

    public void Respawn(Vector2 position)
    {
        transform.position = position;
        body.velocity = Vector2.zero;
        coyoteTimer = coyoteTime;
        bufferTimer = 0f;
        respawning = false;
        inputEnabled = true;
    }
}
