using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class YouthPlatformerPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7.2f;
    [SerializeField] private float acceleration = 44f;
    [SerializeField] private float jumpSpeed = 12.8f;
    [SerializeField] private float fallLimit = -8f;

    public event Action Fell;

    private Rigidbody2D body;
    private YouthGroundSensor groundSensor;
    private Transform visualRoot;
    private float touchDirection;
    private bool jumpReleaseRequested;
    private float coyoteDuration = 0.13f;
    private float jumpBufferDuration = 0.15f;
    private float jumpReleaseMultiplier = 0.48f;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float windAcceleration;
    private Vector2 platformMotion;
    private bool inputEnabled = true;
    private float walkPhase;

    public Rigidbody2D Body => body;

public void Configure(YouthGroundSensor sensor, Transform visuals, YouthPlatformerDifficultyProfile profile)
    {
        EnsureBody();

        groundSensor = sensor;
        visualRoot = visuals;

        if (profile != null)
        {
            moveSpeed = profile.moveSpeed;
            acceleration = profile.acceleration;
            jumpSpeed = profile.jumpSpeed;
            body.gravityScale = profile.gravityScale;
            coyoteDuration = profile.coyoteTime;
            jumpBufferDuration = profile.jumpBufferTime;
            jumpReleaseMultiplier = profile.jumpReleaseMultiplier;
        }
    }

private void Awake()
    {
        EnsureBody();
    }

private void EnsureBody()
    {
        if (body != null)
        {
            return;
        }

        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 3.6f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        PhysicsMaterial2D material = new PhysicsMaterial2D("YouthPlayerMaterial");
        material.friction = 0f;
        material.bounciness = 0f;
        body.sharedMaterial = material;
    }


private void Update()
    {
        if (!inputEnabled)
        {
            return;
        }

        bool grounded = groundSensor != null && groundSensor.IsGrounded;
        coyoteTimer = grounded ? coyoteDuration : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            RequestJump();
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
        {
            ReleaseJump();
        }

        if (transform.position.y < fallLimit)
        {
            inputEnabled = false;
            Fell?.Invoke();
        }
    }

private void FixedUpdate()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (platformMotion.sqrMagnitude > 0f)
        {
            body.position += platformMotion;
            platformMotion = Vector2.zero;
        }

        float keyboard = Input.GetAxisRaw("Horizontal");
        float direction = Mathf.Clamp(keyboard + touchDirection, -1f, 1f);
        Vector2 velocity = body.velocity;
        float targetSpeed = direction * moveSpeed;
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        velocity.x += windAcceleration * Time.fixedDeltaTime;
        velocity.x = Mathf.Clamp(velocity.x, -moveSpeed * 1.28f, moveSpeed * 1.28f);

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = jumpSpeed;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            AudioManager.Instance?.PlayUi(UISound.Correct);
        }

        if (jumpReleaseRequested && velocity.y > 0f)
        {
            velocity.y *= jumpReleaseMultiplier;
        }

        jumpReleaseRequested = false;
        body.velocity = velocity;

        if (visualRoot != null)
        {
            if (Mathf.Abs(direction) > 0.05f)
            {
                walkPhase += Time.fixedDeltaTime * 11f;
                visualRoot.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(walkPhase)) * 0.05f, 0f);
                visualRoot.localScale = new Vector3(direction < 0f ? -1f : 1f, 1f, 1f);
            }
            else
            {
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.zero, 0.24f);
            }
        }
    }

    public void SetTouchDirection(float direction)
    {
        touchDirection = direction;
    }

public void RequestJump()
    {
        if (inputEnabled)
        {
            jumpBufferTimer = jumpBufferDuration;
        }
    }

public void ReleaseJump()
    {
        if (inputEnabled)
        {
            jumpReleaseRequested = true;
        }
    }

    public void AddPlatformMotion(Vector2 delta)
    {
        if (inputEnabled)
        {
            platformMotion += delta;
        }
    }

    public void SetWindAcceleration(float value)
    {
        windAcceleration = inputEnabled ? value : 0f;
    }


public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled && body != null)
        {
            body.velocity = Vector2.zero;
            platformMotion = Vector2.zero;
            windAcceleration = 0f;
        }
    }

public void Respawn(Vector2 position)
    {
        transform.position = position;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        inputEnabled = true;
        jumpBufferTimer = 0f;
        coyoteTimer = coyoteDuration;
        jumpReleaseRequested = false;
        platformMotion = Vector2.zero;
        windAcceleration = 0f;
    }
}
