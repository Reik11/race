using UnityEngine;

namespace RacingGame
{
    /// <summary>
    /// Mengoptimalkan performa kamera dengan menerapkan Layer Cull Distances.
    /// Objek pada layer tertentu (seperti prop kecil/sedang) tidak akan dirender
    /// jika jaraknya melebihi batas yang ditentukan, mengurangi beban rendering GPU.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CullingOptimizer : MonoBehaviour
    {
        [Header("Camera Reference")]
        [Tooltip("Kamera yang akan dioptimalkan. Jika kosong, akan mengambil kamera pada objek ini.")]
        [SerializeField] private Camera targetCamera;

        [Header("Optimization Toggle")]
        [Tooltip("Aktifkan atau nonaktifkan Layer Cull Distances secara realtime untuk testing.")]
        [SerializeField] private bool enableLayerCulling = true;

        [Header("Cull Distances (Units)")]
        [Tooltip("Jarak render maksimal untuk objek kecil (misal: batu kecil, sampah, tiang tipis).")]
        [SerializeField] private float smallPropsDistance = 50f;

        [Tooltip("Jarak render maksimal untuk objek sedang (misal: semak, pohon kecil, pagar).")]
        [SerializeField] private float mediumPropsDistance = 150f;

        [Tooltip("Jarak render maksimal untuk detail terrain (misal: rumput detail, batu hiasan).")]
        [SerializeField] private float terrainDetailsDistance = 200f;

        private float[] defaultDistances = new float[32];
        private float[] customDistances = new float[32];

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            // Simpan jarak default kamera (biasanya 0, artinya dirender hingga Far Clip Plane)
            defaultDistances = targetCamera.layerCullDistances;

            // Terapkan konfigurasi culling jika diaktifkan
            ApplyCulling();
        }

        /// <summary>
        /// Menerapkan jarak cull kustom ke kamera target berdasarkan layer objek.
        /// </summary>
        public void ApplyCulling()
        {
            if (targetCamera == null) return;

            // Inisialisasi ulang array jarak kustom
            customDistances = new float[32];

            // Cari index layer berdasarkan namanya di Unity Project
            int smallPropsLayer = LayerMask.NameToLayer("SmallProps");
            int mediumPropsLayer = LayerMask.NameToLayer("MediumProps");
            int terrainDetailsLayer = LayerMask.NameToLayer("TerrainDetails");
            int roadLayer = LayerMask.NameToLayer("Road");

            // Terapkan jarak cull pada layer yang terdeteksi valid (index >= 0)
            if (smallPropsLayer >= 0) customDistances[smallPropsLayer] = smallPropsDistance;
            if (mediumPropsLayer >= 0) customDistances[mediumPropsLayer] = mediumPropsDistance;
            if (terrainDetailsLayer >= 0) customDistances[terrainDetailsLayer] = terrainDetailsDistance;

            // Layer "Road" sengaja dibiarkan bernilai 0 agar selalu visible hingga Far Clip Plane
            if (roadLayer >= 0) customDistances[roadLayer] = 0f;

            // Pasang array jarak kustom ke properti kamera
            targetCamera.layerCullDistances = customDistances;
            Debug.Log("[CullingOptimizer] Berhasil menerapkan kustom Layer Cull Distances pada kamera.");
        }

        /// <summary>
        /// Mengembalikan jarak render kamera ke nilai default.
        /// </summary>
        public void ResetCulling()
        {
            if (targetCamera == null) return;

            targetCamera.layerCullDistances = defaultDistances;
            Debug.Log("[CullingOptimizer] Mengembalikan jarak render kamera ke default.");
        }

        // Dipanggil otomatis saat nilai diubah di dalam Inspector Unity Editor (untuk mempermudah testing)
        private void OnValidate()
        {
            if (Application.isPlaying && targetCamera != null)
            {
                if (enableLayerCulling)
                {
                    ApplyCulling();
                }
                else
                {
                    ResetCulling();
                }
            }
        }
    }
}
