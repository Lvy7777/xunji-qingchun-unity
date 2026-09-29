using UnityEngine;

public sealed class YouthPlatformerCollectible : MonoBehaviour
{
    private YouthClayPlatformerManager manager;
    private int collectibleIndex;
    private bool collected;
    private Vector3 initialScale;
    private Vector3 baseLocalPosition;

    private float phase;

    public void Initialize(YouthClayPlatformerManager owner, int index)
    {
        manager = owner;
        collectibleIndex = index;
        collected = false;
        initialScale = transform.localScale;
        baseLocalPosition = transform.localPosition;

        phase = index * 0.7f;
    }

private void Update()
    {
        if (collected)
        {
            return;
        }

        transform.localPosition = baseLocalPosition + Vector3.up * (Mathf.Sin(Time.unscaledTime * 2.4f + phase) * 0.08f);
        transform.localScale = initialScale * (1f + Mathf.Sin(Time.unscaledTime * 3f + phase) * 0.035f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<YouthPlatformerPlayer>() == null)
        {
            return;
        }

        collected = true;
        manager.Collect(collectibleIndex, gameObject);
    }

public void ResetCollectible()
    {
        collected = false;
        transform.localPosition = baseLocalPosition;
        transform.localScale = initialScale;
        gameObject.SetActive(true);
    }
}
