using UnityEngine;
using UnityEditor;

namespace RacingGame
{
    /// <summary>
    /// Script Editor untuk mengotomatiskan pembuatan LOD (Level of Detail) pada sirkuit jalan raya.
    /// Membagi visual objek jalan menjadi 3 tingkatan (LOD0 detail penuh, LOD1 jalan utama saja, LOD2 flat/tersembunyi)
    /// untuk menghemat performa rendering di jarak jauh.
    /// </summary>
    public class RoadLODSetup : EditorWindow
    {
        [MenuItem("Tools/RacingGame/Setup Road LOD")]
        public static void SetupRoadLOD()
        {
            // Cari semua objek jalan raya
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int lodGroupCreated = 0;

            foreach (GameObject obj in allObjects)
            {
                // Optimasi khusus segmen jalan utama (biasanya memiliki MeshRenderer)
                if ((obj.CompareTag("Road") || obj.name.ToLower().Contains("road") || obj.name.ToLower().Contains("asphalt")) 
                    && obj.GetComponent<MeshRenderer>() != null && obj.GetComponent<LODGroup>() == null)
                {
                    Undo.AddComponent<LODGroup>(obj);
                    LODGroup lodGroup = obj.GetComponent<LODGroup>();

                    // Dapatkan Renderer jalan utama
                    MeshRenderer mainRenderer = obj.GetComponent<MeshRenderer>();

                    // Setup 3 tingkatan LOD (LOD 0, LOD 1, LOD 2)
                    LOD[] lods = new LOD[3];

                    // 1. LOD 0 (Jarak dekat 0 - 30m / > 60% ukuran di layar)
                    // Merender mesh jalan utama lengkap beserta semua detailnya (bahu jalan, marka, guardrail)
                    Renderer[] lod0Renderers = GetRoadAndDetailRenderers(obj);
                    lods[0] = new LOD(0.6f, lod0Renderers);

                    // 2. LOD 1 (Jarak sedang 30 - 80m / 15% - 60% ukuran di layar)
                    // Hanya merender mesh jalan aspal utama, menyembunyikan detail seperti guardrail/marka kecil
                    Renderer[] lod1Renderers = new Renderer[] { mainRenderer };
                    lods[1] = new LOD(0.15f, lod1Renderers);

                    // 3. LOD 2 (Jarak jauh 80m+ / < 15% ukuran di layar)
                    // Digantikan oleh flat plane sederhana atau disembunyikan seluruhnya untuk menghemat vertex GPU
                    // Di sini kita biarkan renderer kosong (artinya dijarak jauh segmen ini ter-cull/tidak dirender)
                    lods[2] = new LOD(0.05f, new Renderer[0]);

                    // Terapkan LOD array ke LOD Group
                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();

                    EditorUtility.SetDirty(obj);
                    lodGroupCreated++;
                }
            }

            string report = $"Setup LOD Sirkuit Selesai:\n" +
                            $"- Berhasil memasang {lodGroupCreated} komponen LOD Group pada segmen jalan.";
            
            Debug.Log($"[RoadLODSetup] {report}");
            EditorUtility.DisplayDialog("LOD Setup Selesai", report, "OK");
        }

        // Mencari renderer jalan utama beserta anak-anak objeknya (bahu jalan, marka, guardrail)
        private static Renderer[] GetRoadAndDetailRenderers(GameObject parent)
        {
            return parent.GetComponentsInChildren<Renderer>();
        }
    }
}
