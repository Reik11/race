using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace RacingGame
{
    /// <summary>
    /// Mengelola antarmuka tutorial (Tutorial UI) di awal permainan.
    /// Menghentikan permainan sementara di awal, menampilkan petunjuk kontrol dan tujuan,
    /// serta berintegrasi dengan PauseMenuManager untuk mencegah jeda game saat tutorial aktif.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("CanvasGroup dari panel tutorial untuk menangani transisi memudar.")]
        [SerializeField] private CanvasGroup tutorialPanelGroup;

        [Tooltip("Text component untuk menampilkan nama level secara dinamis.")]
        [SerializeField] private TextMeshProUGUI levelTitleText;

        [Tooltip("Text component untuk menampilkan instruksi/tujuan.")]
        [SerializeField] private TextMeshProUGUI instructionsText;

        [Tooltip("Tombol untuk memulai balapan.")]
        [SerializeField] private Button startButton;

        [Header("Settings")]
        [Tooltip("Durasi animasi fade-out panel tutorial (detik).")]
        [SerializeField] private float fadeDuration = 0.3f;

        [Tooltip("Musik BGM yang akan diputar setelah tutorial selesai.")]
        [SerializeField] private AudioClip sirkuitBGM;

        [Header("References")]
        [Tooltip("Referensi ke PauseMenuManager untuk mengontrol akses jeda game.")]
        [SerializeField] private PauseMenuManager pauseManager;

        [Tooltip("Referensi ke mobil pemain untuk mematikan input sementara.")]
        [SerializeField] private PrometeoCarController playerCar;

        private Coroutine fadeCoroutine;

        private void Start()
        {
            // Pastikan panel tutorial aktif sepenuhnya di awal
            if (tutorialPanelGroup != null)
            {
                tutorialPanelGroup.alpha = 1f;
                tutorialPanelGroup.interactable = true;
                tutorialPanelGroup.blocksRaycasts = true;
            }

            // Integrasi referensi otomatis jika belum di-assign di Inspector
            if (pauseManager == null)
            {
                pauseManager = FindFirstObjectByType<PauseMenuManager>();
            }

            if (playerCar == null)
            {
                playerCar = FindFirstObjectByType<PrometeoCarController>();
            }

            // Nonaktifkan menu pause agar tidak bisa dibuka selama tutorial berlangsung
            if (pauseManager != null)
            {
                pauseManager.CanPause = false;
                pauseManager.SetPauseButtonActive(false); // Sembunyikan tombol pause di layar saat tutorial
            }

            // Nonaktifkan input mobil pemain sementara
            if (playerCar != null)
            {
                playerCar.enabled = false;
            }

            // Daftarkan listener pada tombol start
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartRace);
            }

            // Atur judul level berdasarkan nama Scene yang sedang aktif
            if (levelTitleText != null)
            {
                levelTitleText.text = "LEVEL: " + SceneManager.GetActiveScene().name.ToUpper();
            }

            // Tulis instruksi tutorial secara terstruktur dengan bullet point untuk HP (Mobile)
            if (instructionsText != null)
            {
                instructionsText.text = 
                    "<b>KONTROL HP (SENTUH):</b>\n" +
                    "• Gunakan tombol <b>Panah Kiri / Kanan</b> di layar kiri untuk berbelok.\n" +
                    "• Sentuh tombol <b>Pedal Kanan (Gas)</b> untuk melaju.\n" +
                    "• Sentuh tombol <b>Pedal Kiri (Rem/Mundur)</b> untuk melambat.\n" +
                    "• Sentuh tombol <b>Handbrake (Rem Tangan)</b> untuk melakukan drift.\n" +
                    "• Ketuk tombol <b>Pause (||)</b> di pojok kanan atas untuk menjeda game.\n\n" +
                    "<b>MISI BALAPAN:</b>\n" +
                    "• Lewati semua Checkpoint (0 sampai 3) secara berurutan.\n" +
                    "• Capai <b>Finish Line</b> untuk menyelesaikan balapan!";
            }

            // Hentikan waktu permainan (jeda di awal)
            Time.timeScale = 0f;

            // Aktifkan kursor mouse agar bisa klik tombol "Mulai!"
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Dipanggil saat tombol "Mulai!" ditekan.
        /// Memulai permainan, menyembunyikan panel tutorial, dan mengembalikan kontrol ke pemain.
        /// </summary>
        public void StartRace()
        {
            // Jalankan animasi memudarkan panel tutorial
            TriggerFadeOut();

            // Aktifkan kembali waktu permainan
            Time.timeScale = 1f;

            // Sembunyikan dan kunci kursor kembali ke tengah layar
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Aktifkan kembali input mobil pemain
            if (playerCar != null)
            {
                playerCar.enabled = true;
            }

            // Perbolehkan menu pause dibuka sekarang
            if (pauseManager != null)
            {
                pauseManager.CanPause = true;
                pauseManager.SetPauseButtonActive(true); // Tampilkan kembali tombol pause di layar game
            }

            // Mainkan musik latar (BGM) sirkuit
            if (AudioManager.Instance != null && sirkuitBGM != null)
            {
                AudioManager.Instance.PlayMusic(sirkuitBGM);
            }
        }

        private void TriggerFadeOut()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOutPanel());
        }

        private IEnumerator FadeOutPanel()
        {
            float elapsedTime = 0f;
            float startAlpha = tutorialPanelGroup != null ? tutorialPanelGroup.alpha : 1f;

            if (tutorialPanelGroup != null)
            {
                // Nonaktifkan interaksi tombol agar tidak bisa diklik dua kali
                tutorialPanelGroup.interactable = false;
                tutorialPanelGroup.blocksRaycasts = false;

                while (elapsedTime < fadeDuration)
                {
                    // Menggunakan unscaledDeltaTime karena di awal timeScale = 0
                    elapsedTime += Time.unscaledDeltaTime;
                    tutorialPanelGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
                    yield return null;
                }

                tutorialPanelGroup.alpha = 0f;
            }
        }
    }
}
