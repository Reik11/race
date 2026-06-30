using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RacingGame
{
    /// <summary>
    /// Mengelola menu jeda (Pause Menu) di dalam game.
    /// Mengontrol waktu permainan, visibilitas kursor, status kunci kursor,
    /// animasi transisi menu, dan penonaktifkan input player.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("CanvasGroup dari panel pause untuk menangani efek fade-in/fade-out.")]
        [SerializeField] private CanvasGroup pausePanelGroup;
        
        [Tooltip("Tombol untuk melanjutkan permainan.")]
        [SerializeField] private Button resumeButton;
        
        [Tooltip("Tombol untuk mengulang level saat ini.")]
        [SerializeField] private Button restartButton;
        
        [Tooltip("Tombol untuk kembali ke menu utama.")]
        [SerializeField] private Button mainMenuButton;

        [Tooltip("Tombol Pause kecil yang selalu tampil di layar saat gameplay berjalan (untuk Mobile/Mouse).")]
        [SerializeField] private Button pauseButtonInGame;

        [Header("Settings")]
        [Tooltip("Durasi animasi fade-in/fade-out (detik).")]
        [SerializeField] private float fadeDuration = 0.25f;

        [Header("References (Optional)")]
        [Tooltip("Referensi ke script mobil pemain. Jika dikosongkan, akan dicari otomatis di scene.")]
        [SerializeField] private PrometeoCarController playerCar;

        private bool isPaused = false;
        private Coroutine fadeCoroutine;

        /// <summary>
        /// Apakah game saat ini sedang dalam kondisi jeda (pause)?
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Mengatur apakah menu pause diperbolehkan untuk dibuka.
        /// Dapat digunakan oleh sistem lain seperti TutorialManager untuk menonaktifkan pause.
        /// </summary>
        public bool CanPause { get; set; } = true;

        private void Start()
        {
            // Memastikan panel pause tersembunyi sepenuhnya di awal game
            if (pausePanelGroup != null)
            {
                pausePanelGroup.alpha = 0f;
                pausePanelGroup.interactable = false;
                pausePanelGroup.blocksRaycasts = false;
            }

            // Jika referensi mobil pemain belum di-assign, cari secara otomatis
            if (playerCar == null)
            {
                playerCar = FindFirstObjectByType<PrometeoCarController>();
            }

            // Mendaftarkan event listener pada tombol UI
            // Mendaftarkan event listener pada tombol UI
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
            if (pauseButtonInGame != null) pauseButtonInGame.onClick.AddListener(PauseGame);

            // Sembunyikan kursor di awal permainan hanya jika tidak ada TutorialManager yang aktif
            TutorialManager tutorial = FindFirstObjectByType<TutorialManager>();
            if (tutorial == null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void Update()
        {
            // Deteksi penekanan tombol Escape atau P untuk toggle pause
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && CanPause)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        /// <summary>
        /// Menjeda jalannya permainan.
        /// </summary>
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Menghentikan fisika dan update berbasis waktu normal

            // Konfigurasi kursor agar aktif dan bebas bergerak untuk berinteraksi dengan UI
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Sembunyikan tombol pause kecil di layar gameplay
            if (pauseButtonInGame != null)
            {
                pauseButtonInGame.gameObject.SetActive(false);
            }

            // Meredupkan musik latar (Ducking BGM)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetPauseDucking(true);
            }

            // Menonaktifkan kontrol mobil pemain
            if (playerCar != null)
            {
                playerCar.enabled = false;
            }

            // Jalankan animasi memudarkan masuk panel pause
            TriggerFade(true);
        }

        /// <summary>
        /// Melanjutkan kembali permainan dari kondisi jeda.
        /// </summary>
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Mengembalikan kecepatan waktu normal

            // Menyembunyikan dan mengunci kursor kembali ke tengah layar
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Tampilkan kembali tombol pause kecil di layar gameplay
            if (pauseButtonInGame != null)
            {
                pauseButtonInGame.gameObject.SetActive(true);
            }

            // Mengembalikan musik latar ke volume normal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetPauseDucking(false);
            }

            // Mengaktifkan kembali kontrol mobil pemain
            if (playerCar != null)
            {
                playerCar.enabled = true;
            }

            // Jalankan animasi memudarkan keluar panel pause
            TriggerFade(false);
        }

        /// <summary>
        /// Mengulang level yang sedang aktif.
        /// </summary>
        public void RestartLevel()
        {
            // Kembalikan timeScale ke normal agar scene baru tidak termuat dalam kondisi beku
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Memuat scene Menu Utama.
        /// </summary>
        public void LoadMainMenu()
        {
            // Kembalikan timeScale ke normal agar scene MainMenu tidak termuat dalam kondisi beku
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Menyalakan atau mematikan tombol pause in-game di layar secara eksternal.
        /// </summary>
        public void SetPauseButtonActive(bool active)
        {
            if (pauseButtonInGame != null)
            {
                pauseButtonInGame.gameObject.SetActive(active);
            }
        }

        // Mengatur coroutine fade agar tidak bertabrakan jika dipanggil berturut-turut
        private void TriggerFade(bool fadeIn)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadePanel(fadeIn));
        }

        // Coroutine untuk transisi fade in/out menggunakan CanvasGroup.alpha
        private IEnumerator FadePanel(bool fadeIn)
        {
            float targetAlpha = fadeIn ? 1f : 0f;
            float startAlpha = pausePanelGroup != null ? pausePanelGroup.alpha : 0f;
            float elapsedTime = 0f;

            if (pausePanelGroup != null)
            {
                // Jika fade-in, blocksRaycasts diaktifkan agar input UI terdaftar
                if (fadeIn)
                {
                    pausePanelGroup.blocksRaycasts = true;
                }
                else
                {
                    pausePanelGroup.interactable = false;
                    pausePanelGroup.blocksRaycasts = false;
                }

                while (elapsedTime < fadeDuration)
                {
                    // Menggunakan unscaledDeltaTime agar animasi tetap berjalan halus saat Time.timeScale = 0
                    elapsedTime += Time.unscaledDeltaTime; 
                    pausePanelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                    yield return null;
                }

                pausePanelGroup.alpha = targetAlpha;

                // Jika selesai fade-in, aktifkan interaksi tombol
                if (fadeIn)
                {
                    pausePanelGroup.interactable = true;
                }
            }
        }
    }
}
