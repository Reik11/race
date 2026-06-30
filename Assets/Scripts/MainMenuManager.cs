using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RacingGame
{
    /// <summary>
    /// Mengelola menu utama versi sederhana (Simple Main Menu).
    /// Mengatur perpindahan scene, membuka/menutup panel pengaturan, dan keluar dari game.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Main Buttons")]
        [Tooltip("Tombol untuk memulai permainan.")]
        [SerializeField] private Button playButton;

        [Tooltip("Tombol untuk membuka panel pengaturan.")]
        [SerializeField] private Button settingsButton;

        [Tooltip("Tombol untuk keluar dari permainan.")]
        [SerializeField] private Button quitButton;

        [Header("Settings Panel")]
        [Tooltip("Objek Panel Pengaturan (Settings Panel).")]
        [SerializeField] private GameObject settingsPanel;

        [Tooltip("Tombol untuk menutup panel pengaturan.")]
        [SerializeField] private Button closeSettingsButton;

        [Tooltip("Slider untuk mengatur volume suara dasar.")]
        [SerializeField] private Slider simpleVolumeSlider;

        private void Start()
        {
            // Daftarkan listener klik untuk tombol-tombol utama
            if (playButton != null) playButton.onClick.AddListener(PlayGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            // Daftarkan listener untuk panel pengaturan
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
            if (simpleVolumeSlider != null) simpleVolumeSlider.onValueChanged.AddListener(SetSimpleVolume);

            // Pastikan panel pengaturan tertutup saat awal masuk menu
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Inisialisasi nilai slider volume dari AudioListener global
            if (simpleVolumeSlider != null)
            {
                simpleVolumeSlider.value = AudioListener.volume;
            }
        }

        /// <summary>
        /// Memulai permainan dengan memuat scene level pertama.
        /// </summary>
        public void PlayGame()
        {
            SceneManager.LoadScene("level1");
        }

        /// <summary>
        /// Membuka panel pengaturan.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Menutup panel pengaturan.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Mengatur volume suara secara global (sederhana).
        /// </summary>
        /// <param name="value">Nilai volume dari slider (0.0 sampai 1.0)</param>
        public void SetSimpleVolume(float value)
        {
            AudioListener.volume = value;
        }

        /// <summary>
        /// Keluar dari aplikasi permainan.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Keluar dari game...");
            Application.Quit();

            // Jika dijalankan di dalam Unity Editor, matikan mode Play
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
