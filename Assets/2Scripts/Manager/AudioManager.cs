using System;
using _2Scripts.Audio;
using _2Scripts.Interfaces;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class AudioManager : GameManagerSync<AudioManager>
    {
        [SerializeField] private Sound[] musicSounds, sfxSounds;
        [SerializeField] private AudioSource musicSource, sfxSource;

        private const string MusicValueSettingName = "musicVolume", SfxValueSettingName = "sfxVolume";

        /// <summary>
        /// Play a music (will loop if the sound is set to loop)
        /// </summary>
        /// <param name="pName">The name associated to the music to play</param>
        public void PlayMusic(string pName)
        {
            Sound s = Array.Find(musicSounds, x => x.name == pName);

            if (s == null) 
                return;
            
            musicSource.clip = s.clip;
            musicSource.Play();
        }

        /// <summary>
        /// Stop playing the current clip
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
    ///     Play a sound once
        /// </summary>
        /// <param name="pName">The name associated to the sound to play</param>
        public void PlaySfx(string pName)
        {
            Sound s = Array.Find(sfxSounds, x => x.name == pName);

            if (s == null) 
                return;
            
            sfxSource.clip = s.clip;
            sfxSource.PlayOneShot(s.clip);
        }

        /// <summary>
        /// Useful to toggle on/off the music volume
        /// </summary>
        public void ToggleMusic()
        {
            musicSource.mute = !musicSource.mute;
        }
        
        /// <summary>
        /// Useful to toggle on/off the sound volume
        /// </summary>
        public void ToggleSfx()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        /// <summary>
        /// Allow to increase or decrease the music volume
        /// </summary>
        /// <param name="pVolume"></param>
        public void MusicVolume(float pVolume)
        {
            musicSource.volume = pVolume;
        }
        
        /// <summary>
        /// Allow to increase or decrease the sound volume
        /// </summary>
        /// <param name="pVolume"></param>
        public void SfxVolume(float pVolume)
        {
            sfxSource.volume = pVolume;
        }

        /// <summary>
        /// Save all the audio settings in the player preferences
        /// </summary>
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat(MusicValueSettingName, musicSource.volume);
            PlayerPrefs.SetFloat(SfxValueSettingName, sfxSource.volume);
        }
        
        /// <summary>
        /// Load a audio setting depending on the name
        /// </summary>
        /// <param name="pKeyName">value get from the saved parameter in the player preferences</param>
        /// <returns></returns>
        public float LoadAudioSetting(string pKeyName)
        {
            return PlayerPrefs.GetFloat(pKeyName);
        }
    }
}
