using System;
using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

namespace _2Scripts.UI
{
    public class AudioChangeSliderValue : MonoBehaviour
    { 
        [SerializeField] private Slider MusicSlider, SfxSlider;
        [SerializeField] private TextMeshProUGUI musicSliderValueText, sfxSliderValueText;

        private void OnEnable()
        {
            SetLoadedVolumeValue();
        }

        private void OnDisable()
        {
            AudioManager.instance.SaveAudioSettings();
        }

        public void ToggleMusic()
        {
            AudioManager.instance.ToggleMusic();
        }

        public void ToggleSfx()
        {
            AudioManager.instance.ToggleSfx();
        }

        public void MusicVolume()
        {
            AudioManager.instance.MusicVolume(MusicSlider.value);
            musicSliderValueText.text = $"{MusicSlider.value * 100 :0}%";

        }

        public void SfxVolume()
        {
            AudioManager.instance.SfxVolume(SfxSlider.value);
            sfxSliderValueText.text = $"{SfxSlider.value * 100 :0}%";
        }

        private void SetLoadedVolumeValue()
        {
            float musicVolumeValue = AudioManager.instance.LoadAudioSetting("musicVolume");
            float sfxVolumeValue = AudioManager.instance.LoadAudioSetting("sfxVolume");
            
            AudioManager.instance.MusicVolume(musicVolumeValue);
            MusicSlider.value = musicVolumeValue;
            musicSliderValueText.text = $"{musicVolumeValue * 100 :0}%";
            
            AudioManager.instance.SfxVolume(sfxVolumeValue);
            SfxSlider.value = sfxVolumeValue;
            sfxSliderValueText.text = $"{sfxVolumeValue * 100 :0}%";
        }
    }
}
