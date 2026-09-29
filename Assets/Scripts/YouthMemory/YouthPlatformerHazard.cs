using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class YouthPlatformerHazard : MonoBehaviour
{
    private YouthClayPlatformerManager manager;
    private Transform visualRoot;
    private Vector2 patrolStart;
    private Vector2 patrolEnd;
    private float patrolSpeed;
    private float phase;
    private bool configured;

    public void Configure(
        YouthClayPlatformerManager owner,
        Transform visuals,
        Vector2 start,
        Vector2 end,
        float speed,
        float phaseOffset)
    {
        manager = owner;
        visualRoot = visuals;
        patrolStart = start;
        patrolEnd = end;
        patrolSpeed = Mathf.Max(0f, speed);
        phase = Mathf.Repeat(phaseOffset, 1f);
        transform.position = patrolStart;
        configured = true;
    }

    private void Update()
    {
        if (!configured)
        {
            return;
        }

        float distance = Vector2.Distance(patrolStart, patrolEnd);
        if (distance > 0.01f && patrolSpeed > 0.01f)
        {
            float t = Mathf.PingPong(Time.time * patrolSpeed / distance + phase, 1f);
            transform.position = Vector2.Lerp(patrolStart, patrolEnd, t);
        }

        if (visualRoot != null)
        {
            visualRoot.Rotate(0f, 0f, 95f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (manager == null)
        {
            return;
        }

        YouthPlatformerPlayer target = other.GetComponent<YouthPlatformerPlayer>();
        if (target != null)
        {
            manager.HandleHazardHit(transform.position);
        }
    }
}