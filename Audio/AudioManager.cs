using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance;

    [SerializeField] private Sound[] musicSounds, sfxSounds;
    [SerializeField] private AudioSource musicSource, sfxSource;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        PlayMusic("Ambient1");
    }

    public void PlayMusic(string name) {
        Sound sound = Array.Find(musicSounds, x => x.name == name);

        if (sound == null) {
            Debug.Log("Sound not found");
        } else {
            musicSource.clip = sound.clips[0];
            musicSource.Play();
        }
    }

    public void PlaySFX(string name) {
        Sound sound = Array.Find(sfxSounds, x => x.name == name);

        if (sound == null) {
            Debug.Log("Sound not found");

        } else {
            sfxSource.PlayOneShot(sound.clips[0]);
        }
    }

    public void PlayRandomSFX(string name) {
        Sound sound = Array.Find(sfxSounds, x => x.name == name);

        if (sound == null) {
            Debug.Log("Sound not found");
            Debug.Log(name);

        } else {
            AudioClip[] clips = sound.clips;

            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            sfxSource.PlayOneShot(randomClip);
        }
    }
}
