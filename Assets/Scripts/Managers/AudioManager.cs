using UnityEngine;
using System;

public static class AudioManager
{
    public static event Action<Vector3,float> OnNoise;

    public static void MakeNoise(Vector3 position, float loudness) //loudness determines projection
    {
        OnNoise?.Invoke(position,loudness);
    }

//AudioManager.MakeNoise(transform.position, loudness)
}
