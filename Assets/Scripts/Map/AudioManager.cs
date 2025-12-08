using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] music, sfx;
    public AudioSource musicSource, loopSource, sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic("Menu", false);
    }

    public void PlayMusic(string name, bool hasIntro)
    {
        Sound s = Array.Find(music, x => x.name == name);

        if (s == null || s.clips.Length == 0)
        {
            Debug.Log("Music not found or no clips assigned: " + name);
            return;
        }

        if (hasIntro && s.clips.Length >= 2)
        {
            AudioClip intro = s.clips[0];
            AudioClip loop = s.clips[1];

            musicSource.clip = intro;
            musicSource.Play();
            musicSource.loop = false;

            double introEndTime = AudioSettings.dspTime + intro.length;
            loopSource.clip = loop;
            loopSource.loop = true;
            loopSource.PlayScheduled(introEndTime);

            Debug.Log("Intro and Loop scheduled successfully.");
        }
        else
        {
            musicSource.clip = s.GetRandomClip();
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfx, x => x.name == name);

        if (s == null)
        {
            Debug.Log("SFX not found: " + name);
            return;
        }

        AudioClip clip = s.GetRandomClip();
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        loopSource.mute = !loopSource.mute;
    }

    public void ToggleSFX() => sfxSource.mute = !sfxSource.mute;
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
        loopSource.volume = volume;
    }

    public void SFXVolume(float volume) => sfxSource.volume = volume;
}
