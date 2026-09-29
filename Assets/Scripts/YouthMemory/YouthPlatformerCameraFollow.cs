using UnityEngine;

public sealed class YouthPlatformerCameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.20f;
    [SerializeField] private float minimumX = 8.8f;
    [SerializeField] private float maximumX = 39.2f;

    private Transform target;
    private Vector3 velocity;

    public void Configure(Transform followTarget, float minX, float maxX)
    {
        target = followTarget;
        minimumX = minX;
        maximumX = maxX;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float targetX = Mathf.Clamp(target.position.x + 1.4f, minimumX, maximumX);
        Vector3 desired = new Vector3(targetX, 0.15f, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = new Vector3(Mathf.Clamp(target.position.x + 1.4f, minimumX, maximumX), 0.15f, transform.position.z);
        velocity = Vector3.zero;
    }
}
