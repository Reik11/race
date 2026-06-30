using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace RacingGame
{
    /// <summary>
    /// Mengelola menu utama versi animasi (Animated Main Menu).
    /// Menangani efek fade-in judul game, tombol muncul satu per satu,
    /// pemilihan level (Level Select), dan pengaturan volume via Unity AudioMixer.
    /// </summary>
    public class MainMenuAnimated : MonoBehaviour
    {
        [Header("Canvas Groups (For Animation)")]
        [Tooltip("CanvasGroup dari Judul Game untuk efek fade-in.")]
        [SerializeField] private CanvasGroup gameTitleGroup;

        [Tooltip("Daftar CanvasGroup dari tombol-tombol menu untuk dimunculkan satu per satu.")]
        [SerializeField] private CanvasGroup[] menuButtonGroups;

        [Header("Main Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [Tooltip("Panel Pemilihan Level.")]
        [SerializeField] private GameObject levelSelectPanel;

        [Tooltip("Panel Pengaturan Volume.")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Close Buttons")]
        [SerializeField] private Button closeLevelSelectButton;
        [SerializeField] private Button closeSettingsButton;

        [Header("Audio Mixer & Sliders")]
        [Tooltip("Referensi ke AudioMixer utama game.")]
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Animation Settings")]
        [SerializeField] private float titleFadeDuration = 1.0f;
        [SerializeField] private float buttonFadeDuration = 0.3f;
        [SerializeField] private float delayBetweenButtons = 0.2f;

        private void Start()
        {
            // Sembunyikan panel-panel tambahan di awal
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            // Mendaftarkan event listener untuk tombol-tombol
            if (playButton != null) playButton.onClick.AddListener(PlayDefaultLevel);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OpenLevelSelect);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            if (closeLevelSelectButton != null) closeLevelSelectButton.onClick.AddListener(CloseLevelSelect);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);

            // Mendaftarkan event listener untuk slider volume
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(val => SetMixerVolume("MasterVolume", val));
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(val => SetMixerVolume("MusicVolume", val));
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(val => SetMixerVolume("SFXVolume", val));

            // Setup nilai awal slider dari AudioMixer jika memungkinkan
            InitSliderValues();

            // Jalankan coroutine animasi menu utama saat scene dimuat
            StartCoroutine(AnimateMainMenuIntro());
        }

        // Memuat nilai volume saat ini ke UI Slider
        private void InitSliderValues()
        {
            if (audioMixer != null)
            {
                if (masterVolumeSlider != null) masterVolumeSlider.value = GetMixerVolumePercent("MasterVolume");
                if (musicVolumeSlider != null) musicVolumeSlider.value = GetMixerVolumePercent("MusicVolume");
                if (sfxVolumeSlider != null) sfxVolumeSlider.value = GetMixerVolumePercent("SFXVolume");
            }
        }

        private float GetMixerVolumePercent(string parameterName)
        {
            if (audioMixer.GetFloat(parameterName, out float value))
            {
                // Konversi dari dB (-80 sampai 20) kembali ke nilai persen/slider (0.0001 sampai 1.0)
                return Mathf.Pow(10, value / 20f);
            }
            return 0.75f; // Nilai default jika gagal membaca
        }

        private void SetMixerVolume(string parameterName, float sliderValue)
        {
            if (audioMixer != null)
            {
                // Hindari nilai 0 mutlak untuk menghindari error logaritma
                float volumeDb = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
                audioMixer.SetFloat(parameterName, volumeDb);
            }
        }

        private IEnumerator AnimateMainMenuIntro()
        {
            // 1. Inisialisasi awal: Sembunyikan judul dan semua tombol
            if (gameTitleGroup != null)
            {
                gameTitleGroup.alpha = 0f;
            }

            foreach (var btnGroup in menuButtonGroups)
            {
                if (btnGroup != null)
                {
                    btnGroup.alpha = 0f;
                    btnGroup.interactable = false;
                    btnGroup.blocksRaycasts = false;
                }
            }

            yield return new WaitForSeconds(0.2f); // Delay awal kecil sebelum animasi dimulai

            // 2. Animasi fade-in Judul Game secara halus
            if (gameTitleGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < titleFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    gameTitleGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / titleFadeDuration);
                    yield return null;
                }
                gameTitleGroup.alpha = 1f;
            }

            yield return new WaitForSeconds(0.1f); // Delay kecil sebelum tombol mulai muncul

            // 3. Memunculkan tombol satu per satu dengan delay vertikal
            for (int i = 0; i < menuButtonGroups.Length; i++)
            {
                if (menuButtonGroups[i] != null)
                {
                    StartCoroutine(FadeInButton(menuButtonGroups[i]));
                    yield return new WaitForSeconds(delayBetweenButtons);
                }
            }
        }

        private IEnumerator FadeInButton(CanvasGroup btnGroup)
        {
            float elapsed = 0f;
            while (elapsed < buttonFadeDuration)
            {
                elapsed += Time.deltaTime;
                btnGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeDuration);
                yield return null;
            }
            btnGroup.alpha = 1f;
            btnGroup.interactable = true;
            btnGroup.blocksRaycasts = true;
        }

        // Fungsionalitas Tombol

        public void PlayDefaultLevel()
        {
            SceneManager.LoadScene("level1");
        }

        /// <summary>
        /// Memuat level tertentu berdasarkan nama scenenya (untuk dipanggil dari tombol list level).
        /// </summary>
        /// <param name="levelName">Nama scene level yang ingin dimuat.</param>
        public void LoadSpecificLevel(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }

        public void OpenLevelSelect()
        {
            if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        }

        public void CloseLevelSelect()
        {
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("Keluar dari game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
