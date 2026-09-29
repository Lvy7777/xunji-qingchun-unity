using UnityEngine;

public static class SFXController
{
    public static void Play(UISound cue)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUi(cue);
        }
    }
}