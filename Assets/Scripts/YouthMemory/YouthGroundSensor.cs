using System.Collections.Generic;
using UnityEngine;

public sealed class YouthGroundSensor : MonoBehaviour
{
    private readonly HashSet<Collider2D> contacts = new HashSet<Collider2D>();
    private int platformLayer;

    public bool IsGrounded => contacts.Count > 0;

    private void Awake()
    {
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == platformLayer)
        {
            contacts.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        contacts.Remove(other);
    }

    private void OnDisable()
    {
        contacts.Clear();
    }
}
