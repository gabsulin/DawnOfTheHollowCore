using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle musicMuteToggle;
    public Toggle sfxMuteToggle;

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        musicSlider.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());

        if (musicMuteToggle != null)
            musicMuteToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsMusicMuted());

        if (sfxMuteToggle != null)
            sfxMuteToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsSFXMuted());
    }
}