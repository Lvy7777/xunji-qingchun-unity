using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class YouthMovingPlatform : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private Vector2 frameDelta;
    private float cycleSeconds;
    private float phaseOffset;
    private bool configured;

private void Awake()
    {
        EnsureBody();
    }

private void EnsureBody()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }


public void Configure(Vector2 start, Vector2 end, float cycle, float phase)
    {
        EnsureBody();
        startPoint = start;
        endPoint = end;
        cycleSeconds = Mathf.Max(0.5f, cycle);
        phaseOffset = Mathf.Repeat(phase, 1f);
        body.position = startPoint;
        configured = true;
    }

    private void FixedUpdate()
    {
        if (!configured)
        {
            return;
        }

        float raw = Mathf.PingPong(Time.fixedTime / cycleSeconds + phaseOffset, 1f);
        float eased = raw * raw * (3f - 2f * raw);
        Vector2 nextPosition = Vector2.Lerp(startPoint, endPoint, eased);
        frameDelta = nextPosition - body.position;
        body.MovePosition(nextPosition);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        YouthPlatformerPlayer passenger = collision.collider.GetComponentInParent<YouthPlatformerPlayer>();
        if (passenger != null)
        {
            passenger.AddPlatformMotion(frameDelta);
        }
    }
}