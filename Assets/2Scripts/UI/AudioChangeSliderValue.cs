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
            GameManager.GetManager<AudioManager>().SaveAudioSettings();
        }

        public void ToggleMusic()
        {
            GameManager.GetManager<AudioManager>().ToggleMusic();
        }

        public void ToggleSfx()
        {
            GameManager.GetManager<AudioManager>().ToggleSfx();
        }

        public void MusicVolume()
        {
            GameManager.GetManager<AudioManager>().MusicVolume(MusicSlider.value);
            musicSliderValueText.text = $"{MusicSlider.value * 100 :0}%";

        }

        public void SfxVolume()
        {
            GameManager.GetManager<AudioManager>().SfxVolume(SfxSlider.value);
            sfxSliderValueText.text = $"{SfxSlider.value * 100 :0}%";
        }

        private void SetLoadedVolumeValue()
        {
            float musicVolumeValue = GameManager.GetManager<AudioManager>().LoadAudioSetting("musicVolume");
            float sfxVolumeValue = GameManager.GetManager<AudioManager>().LoadAudioSetting("sfxVolume");
            
            GameManager.GetManager<AudioManager>().MusicVolume(musicVolumeValue);
            MusicSlider.value = musicVolumeValue;
            musicSliderValueText.text = $"{musicVolumeValue * 100 :0}%";
            
            GameManager.GetManager<AudioManager>().SfxVolume(sfxVolumeValue);
            SfxSlider.value = sfxVolumeValue;
            sfxSliderValueText.text = $"{sfxVolumeValue * 100 :0}%";
        }
    }
}
