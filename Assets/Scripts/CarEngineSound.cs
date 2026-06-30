using UnityEngine;

namespace RacingGame
{
    /// <summary>
    /// Mengelola pemutaran dan modulasi pitch suara mesin mobil pemain secara dinamis.
    /// Pitch akan naik/turun menyesuaikan kecepatan aktual fisik (velocity magnitude) mobil.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CarEngineSound : MonoBehaviour
    {
        [Header("Audio Setup")]
        [Tooltip("AudioSource mesin mobil. Jika kosong, akan mengambil komponen di objek ini.")]
        [SerializeField] private AudioSource engineSource;

        [Header("Pitch Settings")]
        [Tooltip("Pitch mesin saat mobil diam.")]
        [SerializeField] private float minPitch = 0.8f;

        [Tooltip("Pitch mesin maksimal saat kecepatan penuh.")]
        [SerializeField] private float maxPitch = 2.0f;

        [Tooltip("Faktor pembagi untuk kecepatan mobil. Semakin besar nilainya, pitch naik lebih lambat.")]
        [SerializeField] private float speedScaleFactor = 30f;

        private Rigidbody carRigidbody;

        private void Start()
        {
            // Ambil komponen AudioSource di objek ini jika belum di-assign
            if (engineSource == null)
            {
                engineSource = GetComponent<AudioSource>();
            }

            // Dapatkan komponen Rigidbody dari mobil pemain
            carRigidbody = GetComponentInParent<Rigidbody>();

            if (carRigidbody == null)
            {
                carRigidbody = GetComponent<Rigidbody>();
            }

            // Pastikan AudioSource diatur untuk looping musik mesin
            if (engineSource != null)
            {
                engineSource.loop = true;
                if (!engineSource.isPlaying)
                {
                    engineSource.Play();
                }
            }
        }

        private void Update()
        {
            if (engineSource == null || carRigidbody == null) return;

            // Membaca kecepatan linear mobil pemain
            float speed = carRigidbody.linearVelocity.magnitude;

            // Hitung target pitch secara linear berdasarkan kecepatan mobil
            // Pitch berkisar dari minPitch (0.8) hingga maxPitch (2.0)
            float targetPitch = minPitch + (speed / speedScaleFactor);
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

            // Interpolasi pitch agar transisi perubahan suara mesin terdengar halus (smooth lerp)
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);
        }
    }
}
