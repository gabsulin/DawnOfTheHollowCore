using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] music, sfx;
    public AudioSource musicSource, loopSource, sfxSource;

    [Header("Crossfade")]
    public float fadeDuration = 1.5f;

    private const string KeyMusicVolume = "MusicVolume";
    private const string KeySFXVolume = "SFXVolume";
    private const string KeyMusicMuted = "MusicMuted";
    private const string KeySFXMuted = "SFXMuted";

    private float targetMusicVolume = 1f;
    private Coroutine activeFade;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
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

    private void LoadSettings()
    {
        targetMusicVolume = PlayerPrefs.GetFloat(KeyMusicVolume, 1f);
        float sfxVol = PlayerPrefs.GetFloat(KeySFXVolume, 1f);
        bool musicMuted = PlayerPrefs.GetInt(KeyMusicMuted, 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt(KeySFXMuted, 0) == 1;

        musicSource.volume = targetMusicVolume;
        loopSource.volume = targetMusicVolume;
        sfxSource.volume = sfxVol;

        musicSource.mute = musicMuted;
        loopSource.mute = musicMuted;
        sfxSource.mute = sfxMuted;
    }

    public float GetMusicVolume() => targetMusicVolume;
    public float GetSFXVolume() => sfxSource.volume;
    public bool IsMusicMuted() => musicSource.mute;
    public bool IsSFXMuted() => sfxSource.mute;

    public void PlayMusic(string name, bool hasIntro)
    {
        Sound s = Array.Find(music, x => x.name == name);
        if (s == null || s.clips.Length == 0)
        {
            Debug.Log("Music not found or no clips assigned: " + name);
            return;
        }

        if (activeFade != null)
            StopCoroutine(activeFade);

        activeFade = StartCoroutine(CrossfadeTo(s, hasIntro));
    }

    private IEnumerator CrossfadeTo(Sound s, bool hasIntro)
    {
        float startVolume = musicSource.volume;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            loopSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        musicSource.Stop();
        loopSource.Stop();
        musicSource.volume = 0f;
        loopSource.volume = 0f;

        if (hasIntro && s.clips.Length >= 2)
        {
            AudioClip intro = s.clips[0];
            AudioClip loop = s.clips[1];

            musicSource.clip = intro;
            musicSource.loop = false;
            musicSource.Play();

            double introEndTime = AudioSettings.dspTime + intro.length;
            loopSource.clip = loop;
            loopSource.loop = true;
            loopSource.PlayScheduled(introEndTime);
        }
        else
        {
            musicSource.clip = s.GetRandomClip();
            musicSource.loop = true;
            musicSource.Play();
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            musicSource.volume = Mathf.Lerp(0f, targetMusicVolume, t);
            loopSource.volume = Mathf.Lerp(0f, targetMusicVolume, t);
            yield return null;
        }

        musicSource.volume = targetMusicVolume;
        loopSource.volume = targetMusicVolume;
        activeFade = null;
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
            sfxSource.PlayOneShot(clip);
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        loopSource.mute = !loopSource.mute;
        PlayerPrefs.SetInt(KeyMusicMuted, musicSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
        PlayerPrefs.SetInt(KeySFXMuted, sfxSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void MusicVolume(float volume)
    {
        targetMusicVolume = volume;
        if (activeFade == null)
        {
            musicSource.volume = volume;
            loopSource.volume = volume;
        }
        PlayerPrefs.SetFloat(KeyMusicVolume, volume);
        PlayerPrefs.Save();
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(KeySFXVolume, volume);
        PlayerPrefs.Save();
    }
}