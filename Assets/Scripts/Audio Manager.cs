using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds; // Liste deiner Sounds
    public static AudioManager instance; // Das Singleton

    void Awake()
    {
        // Singleton Logik: Verhindert, dass beim Szenenwechsel ein zweiter Manager entsteht
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // Für jeden Sound eine eigene AudioSource-Komponente erstellen
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.spatialBlend = s.spatialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " nicht gefunden!");
            return;
        }
        s.source.Stop();
        s.source.Play();
    }

    public AudioSource PlayAttached(string name, Transform parent, bool restart = true)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"[AudioManager] Sound '{name}' nicht gefunden oder Clip fehlt.");
            return null;
        }

        // Eine Source direkt am Ballroom-Objekt erzeugen (oder wiederverwenden)
        AudioSource src = parent.GetComponent<AudioSource>();
        if (src == null) src = parent.gameObject.AddComponent<AudioSource>();

        src.clip = s.clip;
        src.volume = s.volume;
        src.pitch = s.pitch;
        src.loop = s.loop;

        src.spatialBlend = s.spatialBlend;   // muss > 0 sein für Distance
        src.minDistance = s.minDistance;
        src.maxDistance = s.maxDistance;
        src.rolloffMode = AudioRolloffMode.Linear;

        if (restart) src.Stop();
        if (!src.isPlaying) src.Play();

        return src;
    }
}