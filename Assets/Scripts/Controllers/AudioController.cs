using UnityEngine;
using System.Collections.Generic;

namespace GameSystems
{
    /// <summary>
    /// Singleton controller for managing audio playback including music and sound effects.
    /// Handles volume control, audio pooling, and persistent audio across scenes.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] musicTracks;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip towerPlaceSound;
        [SerializeField] private AudioClip towerUpgradeSound;
        [SerializeField] private AudioClip towerSellSound;
        [SerializeField] private AudioClip enemyDeathSound;
        [SerializeField] private AudioClip projectileFireSound;

        [Header("Settings")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private int sfxPoolSize = 10;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentTrackIndex = 0;

        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistAcrossScenes)
                DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            InitializeSFXPool();
        }

        void Start()
        {
            ApplyVolumeSettings();
            
            if (musicTracks.Length > 0)
                PlayMusic(0);
        }

        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Create SFX source if not assigned
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void InitializeSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject poolObj = new GameObject($"SFXPool_{i}");
                poolObj.transform.SetParent(transform);
                AudioSource source = poolObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                sfxPool.Add(source);
            }
        }

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;

            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;

            foreach (var source in sfxPool)
            {
                if (source != null)
                    source.volume = masterVolume * sfxVolume;
            }
        }

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;

        #endregion

        #region Music Control

        public void PlayMusic(int trackIndex)
        {
            if (musicTracks == null || trackIndex < 0 || trackIndex >= musicTracks.Length)
                return;

            currentTrackIndex = trackIndex;
            musicSource.clip = musicTracks[trackIndex];
            musicSource.Play();
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null) return;
            
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayNextTrack()
        {
            if (musicTracks == null || musicTracks.Length == 0) return;
            
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
            PlayMusic(currentTrackIndex);
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (musicSource != null)
                musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (musicSource != null)
                musicSource.UnPause();
        }

        #endregion

        #region Sound Effects

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null) return;

            // Try to find an available source from the pool
            AudioSource availableSource = sfxPool.Find(s => !s.isPlaying);

            if (availableSource != null)
            {
                availableSource.volume = masterVolume * sfxVolume * volumeMultiplier;
                availableSource.PlayOneShot(clip);
            }
            else
            {
                // Fallback to main SFX source if pool is exhausted
                sfxSource.PlayOneShot(clip, volumeMultiplier);
            }
        }

        public void PlaySFX(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, masterVolume * sfxVolume * volumeMultiplier);
        }

        // Convenience methods for common sounds
        public void PlayButtonClick() => PlaySFX(buttonClickSound);
        public void PlayTowerPlace() => PlaySFX(towerPlaceSound);
        public void PlayTowerUpgrade() => PlaySFX(towerUpgradeSound);
        public void PlayTowerSell() => PlaySFX(towerSellSound);
        public void PlayEnemyDeath() => PlaySFX(enemyDeathSound);
        public void PlayProjectileFire() => PlaySFX(projectileFireSound, 0.5f);

        // Static method for inspector assignment on prefabs
        public static void PlayButtonClickStatic()
        {
            if (Instance != null)
                Instance.PlayButtonClick();
        }

        #endregion

        #region Utility

        public void StopAllSFX()
        {
            if (sfxSource != null)
                sfxSource.Stop();

            foreach (var source in sfxPool)
            {
                if (source != null && source.isPlaying)
                    source.Stop();
            }
        }

        public void MuteAll(bool mute)
        {
            if (musicSource != null)
                musicSource.mute = mute;

            if (sfxSource != null)
                sfxSource.mute = mute;

            foreach (var source in sfxPool)
            {
                if (source != null)
                    source.mute = mute;
            }
        }

        #endregion
    }
}
