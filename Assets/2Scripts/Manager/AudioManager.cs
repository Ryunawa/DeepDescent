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

        private System.Random random = new System.Random();

        /// <summary>
        /// Play a music (will loop if the sound is set to loop)
        /// </summary>
        /// <param name="pName">The name associated to the music to play</param>
        public void PlayMusic(string pName, float volume = 0.1f)
        {
            Sound s = Array.Find(musicSounds, x => x.name == pName);

            if (s == null || s.clips.Length == 0)
                return;

            AudioClip clip = s.clips[random.Next(s.clips.Length)];
            musicSource.clip = clip;
            musicSource.volume = volume;
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
        /// Play a sound once
        /// </summary>
        /// <param name="pSoundName">The name associated to the sound to play</param>
        public void PlaySfx(string pSoundName, float volume = 0.5f)
        {
            Sound s = Array.Find(sfxSounds, x => x.name == pSoundName);

            if (s == null || s.clips.Length == 0)
            {
                Debug.LogWarning("no sounds in list or sound '" + pSoundName + "' has not been found.");
                return;
            }

            AudioClip clip = s.clips[random.Next(s.clips.Length)];
            sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// To play a spatial sound
        /// </summary>
        /// <param name="pSoundName">Name of the sound to play</param>
        /// <param name="pScript">The origin from where to play the sound</param>
        /// <param name="pMinDistance">Min distance where the sound is at his max volume</param>
        /// <param name="pMaxDistance">Max distance where the sound can be heard</param>
        /// EXEMPLE
        /// Footstep sounds: minDistance = 1, maxDistance = 5
        /// Weapon sounds: minDistance = 2, maxDistance = 10
        /// Explosions: minDistance = 5, maxDistance = 50
        public void PlaySfx(string pSoundName, MonoBehaviour pScript, float pMinDistance, float pMaxDistance, float volume = 0.5f)
        {
            Sound s = Array.Find(sfxSounds, x => x.name == pSoundName);

            if (s == null || s.clips.Length == 0)
            {
                Debug.LogWarning($"Sound {pSoundName} not found or has no clips.");
                return;
            }

            AudioClip clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
            AudioSource objectAudioSource = pScript.GetComponent<AudioSource>();

            if (objectAudioSource == null)
            {
                Debug.Log($"No AudioSource found on {pScript.gameObject.name}. Adding one.");
                objectAudioSource = pScript.gameObject.AddComponent<AudioSource>();
            }

            objectAudioSource.clip = clip;
            objectAudioSource.spatialBlend = 1;
            objectAudioSource.minDistance = pMinDistance;
            objectAudioSource.maxDistance = pMaxDistance;

            Debug.Log($"Playing sound {pSoundName} on {pScript.gameObject.name}");
            objectAudioSource.PlayOneShot(clip, volume);
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
        /// Load an audio setting depending on the name
        /// </summary>
        /// <param name="pKeyName">value get from the saved parameter in the player preferences</

        /// <returns></returns>
        public float LoadAudioSetting(string pKeyName)
        {
            return PlayerPrefs.GetFloat(pKeyName);
        }
    }
}
