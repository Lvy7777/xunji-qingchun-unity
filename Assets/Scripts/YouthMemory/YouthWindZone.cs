using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class YouthWindZone : MonoBehaviour
{
    private float horizontalAcceleration;

    public void Configure(float acceleration)
    {
        horizontalAcceleration = acceleration;
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        YouthPlatformerPlayer player = other.GetComponentInParent<YouthPlatformerPlayer>();
        if (player != null)
        {
            player.SetWindAcceleration(horizontalAcceleration);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        YouthPlatformerPlayer player = other.GetComponent<YouthPlatformerPlayer>();
        if (player != null)
        {
            player.SetWindAcceleration(0f);
        }
    }
}