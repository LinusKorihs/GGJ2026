using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(.1f, 3f)] public float pitch = 1f;

    public bool loop;

    [HideInInspector] public AudioSource source;

    [Range(0f, 1f)] public float spatialBlend = 1f; // Default lieber 2D
    public float minDistance = 1f;
    public float maxDistance = 20f;
}
