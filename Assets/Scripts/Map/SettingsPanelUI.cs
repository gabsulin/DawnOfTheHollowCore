using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Mute Buttons")]
    public Button musicMuteButton;
    public Button sfxMuteButton;

    [Header("Mute Button Sprites")]
    public Sprite musicUnmutedSprite;
    public Sprite musicMutedSprite;
    public Sprite sfxUnmutedSprite;
    public Sprite sfxMutedSprite;

    private Image musicButtonImage;
    private Image sfxButtonImage;

    private void Awake()
    {
        musicButtonImage = musicMuteButton.GetComponent<Image>();
        sfxButtonImage = sfxMuteButton.GetComponent<Image>();
    }

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        musicSlider.value = AudioManager.Instance.musicSource.volume;
        sfxSlider.value = AudioManager.Instance.sfxSource.volume;

        RefreshMusicButton();
        RefreshSFXButton();
    }

    public void OnMusicSliderChanged()
    {
        AudioManager.Instance?.MusicVolume(musicSlider.value);
    }

    public void OnSFXSliderChanged()
    {
        AudioManager.Instance?.SFXVolume(sfxSlider.value);
    }

    public void OnMusicMuteClicked()
    {
        AudioManager.Instance.ToggleMusic();
        RefreshMusicButton();
    }

    public void OnSFXMuteClicked()
    {
        AudioManager.Instance.ToggleSFX();
        RefreshSFXButton();
    }

    private void RefreshMusicButton()
    {
        if (musicButtonImage == null) return;
        musicButtonImage.sprite = AudioManager.Instance.IsMusicMuted()
            ? musicMutedSprite
            : musicUnmutedSprite;
    }

    private void RefreshSFXButton()
    {
        if (sfxButtonImage == null) return;
        sfxButtonImage.sprite = AudioManager.Instance.IsSFXMuted()
            ? sfxMutedSprite
            : sfxUnmutedSprite;
    }
}