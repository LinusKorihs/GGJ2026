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
        s.source.Play();
    }

    public void PlayAtPosition(string name, Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;

        // Erstellt ein temporäres Objekt für den Sound an der Position
        GameObject tempGO = new GameObject("TempAudio_" + name);
        tempGO.transform.position = position;
        AudioSource source = tempGO.AddComponent<AudioSource>();

        // Einstellungen vom Sound-Objekt übernehmen
        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.spatialBlend = s.spatialBlend;
        source.minDistance = s.minDistance;
        source.maxDistance = s.maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear; // Gleichmäßiges Leiserwerden

        source.Play();

        // Objekt zerstören, wenn der Sound fertig ist
        Destroy(tempGO, s.clip.length);
    }
}