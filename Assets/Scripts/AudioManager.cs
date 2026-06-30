using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace RacingGame
{
    /// <summary>
    /// AudioManager mengelola semua pemutaran musik (BGM) dan efek suara (SFX) di dalam game.
    /// Dibuat menggunakan pola Singleton agar tetap persis di semua scene (DontDestroyOnLoad).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // Instance tunggal dari AudioManager
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [Tooltip("Referensi ke AudioMixer Utama.")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Audio Sources")]
        [Tooltip("AudioSource khusus untuk memutar musik latar (BGM).")]
        [SerializeField] private AudioSource musicSource;

        [Tooltip("AudioSource khusus untuk memutar efek suara (SFX).")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips Database")]
        [SerializeField] private List<SFXClipInfo> sfxClips;
        [SerializeField] private AudioClip defaultBGM;

        // Menyimpan daftar SFX agar mudah dicari menggunakan string nama
        private Dictionary<string, AudioClip> sfxDatabase = new Dictionary<string, AudioClip>();

        // Menyimpan volume musik saat ini (sebelum di-ducking saat pause)
        private float currentMusicVolumePercent = 0.75f;
        private bool isDucked = false;

        [Serializable]
        public struct SFXClipInfo
        {
            public string name;
            public AudioClip clip;
        }

        private void Awake()
        {
            // Konfigurasi pola Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Putar BGM default jika diset dan scene saat ini bukan scene gameplay dengan tutorial
            // (Nanti diatur di scene MainMenu BGM berputar langsung, di scene level1 menunggu instruksi StartRace)
            if (defaultBGM != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
            {
                PlayMusic(defaultBGM);
            }
        }

        private void InitializeDatabase()
        {
            sfxDatabase.Clear();
            foreach (var sfx in sfxClips)
            {
                if (!string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
                {
                    sfxDatabase[sfx.name.ToLower()] = sfx.clip;
                }
            }
        }

        /// <summary>
        /// Memutar efek suara (SFX) sekali bunyi berdasarkan nama string.
        /// </summary>
        /// <param name="name">Nama SFX yang terdaftar di database (case-insensitive).</param>
        public void PlaySFX(string name)
        {
            string key = name.ToLower();
            if (sfxDatabase.ContainsKey(key))
            {
                if (sfxSource != null)
                {
                    sfxSource.PlayOneShot(sfxDatabase[key]);
                }
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX dengan nama '{name}' tidak ditemukan di database!");
            }
        }

        /// <summary>
        /// Memutar musik latar (BGM) baru secara berulang (loop).
        /// </summary>
        /// <param name="clip">AudioClip musik yang akan diputar.</param>
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;

            // Jika musik yang sama sedang berputar, jangan di-restart
            if (musicSource.isPlaying && musicSource.clip == clip) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Menghentikan pemutaran musik latar.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Mengatur volume suara untuk group tertentu di AudioMixer.
        /// </summary>
        /// <param name="groupName">Nama parameter exposed di AudioMixer ("MasterVolume", "MusicVolume", "SFXVolume").</param>
        /// <param name="value">Nilai volume linear (0.0001 sampai 1.0).</param>
        public void SetVolume(string groupName, float value)
        {
            if (audioMixer == null) return;

            // Simpan volume musik untuk kebutuhan ducking
            if (groupName.Equals("MusicVolume", StringComparison.OrdinalIgnoreCase) && !isDucked)
            {
                currentMusicVolumePercent = value;
            }

            // Konversi nilai linear ke skala desibel (dB)
            float volumeDb = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixer.SetFloat(groupName, volumeDb);
        }

        /// <summary>
        /// Mengaktifkan atau menonaktifkan efek ducking (meredupkan musik) saat game di-pause.
        /// </summary>
        /// <param name="duck">True untuk meredupkan musik, False untuk mengembalikannya ke normal.</param>
        public void SetPauseDucking(bool duck)
        {
            if (audioMixer == null) return;

            isDucked = duck;
            if (duck)
            {
                // Turunkan volume musik secara drastis saat game di-jeda (dikurangi 15dB dari volume linear saat ini)
                float duckedVolume = currentMusicVolumePercent * 0.15f; 
                float volumeDb = Mathf.Log10(Mathf.Max(duckedVolume, 0.0001f)) * 20f;
                audioMixer.SetFloat("MusicVolume", volumeDb);
            }
            else
            {
                // Kembalikan volume musik ke nilai aslinya sebelum jeda
                float volumeDb = Mathf.Log10(Mathf.Max(currentMusicVolumePercent, 0.0001f)) * 20f;
                audioMixer.SetFloat("MusicVolume", volumeDb);
            }
        }
    }
}
