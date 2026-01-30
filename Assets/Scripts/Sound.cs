using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;     // Der Name, mit dem wir den Sound aufrufen
    public AudioClip clip;  // Die eigentliche Audio-Datei

    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    [HideInInspector]
    public AudioSource source; // Wird vom Manager automatisch zugewiesen

    [Range(0f, 1f)]
    public float spatialBlend = 1f; // 0 = 2D (überall gleich), 1 = 3D (ortsabhängig)
    public float minDistance = 1f;  // Ab hier wird es leiser
    public float maxDistance = 20f; // Ab hier hört man gar nichts mehr
}