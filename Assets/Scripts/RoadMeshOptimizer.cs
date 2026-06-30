using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RacingGame
{
    /// <summary>
    /// Mengoptimalkan mesh jalan sirkuit (Road) secara massal.
    /// Mengaktifkan GPU Instancing pada material renderer, menandai objek sebagai Occludee Static
    /// untuk occlusion culling, dan menggabungkan mesh menggunakan Static Batching Utility.
    /// </summary>
    public class RoadMeshOptimizer : MonoBehaviour
    {
        [Header("Optimization Settings")]
        [Tooltip("Jalankan optimasi static batching otomatis saat game baru dimulai (runtime).")]
        [SerializeField] private bool optimizeOnStart = true;

        private void Start()
        {
            if (optimizeOnStart)
            {
                OptimizeRoadMeshRuntime();
            }
        }

        /// <summary>
        /// Mengoptimalkan sirkuit jalan saat game dijalankan (Runtime).
        /// </summary>
        public void OptimizeRoadMeshRuntime()
        {
            // Cari objek induk atau mesh yang mengandung nama "Road"
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int optimizedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (IsRoadObject(obj))
                {
                    // 1. Aktifkan GPU Instancing pada material renderer
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            if (mat != null && !mat.enableInstancing)
                            {
                                mat.enableInstancing = true;
                            }
                        }
                    }

                    optimizedCount++;
                }
            }

            // 2. Gabungkan mesh jalan berdekatan menggunakan Static Batching
            StaticBatchingUtility.Combine(gameObject);
            Debug.Log($"[RoadMeshOptimizer] Runtime: Berhasil mengoptimalkan {optimizedCount} sub-mesh jalan dan melakukan Static Batching Combine.");
        }

        private static bool IsRoadObject(GameObject obj)
        {
            return obj.CompareTag("Road") || obj.name.ToLower().Contains("road") || obj.name.ToLower().Contains("asphalt");
        }

#if UNITY_EDITOR
        [MenuItem("Tools/RacingGame/Optimize Road Mesh")]
        public static void OptimizeRoadMeshEditor()
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int flagCount = 0;
            int materialCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (IsRoadObject(obj))
                {
                    Undo.RecordObject(obj, "Optimize Road Mesh Flags");

                    // 1. Aktifkan static flag Occludee Static & Batching Static di Editor
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
                    flags |= StaticEditorFlags.OccludeeStatic;
                    flags |= StaticEditorFlags.ContributeGI; // Batching Static
                    GameObjectUtility.SetStaticEditorFlags(obj, flags);
                    flagCount++;

                    // 2. Aktifkan GPU Instancing pada material di Editor
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.sharedMaterials)
                        {
                            if (mat != null && !mat.enableInstancing)
                            {
                                Undo.RecordObject(mat, "Enable GPU Instancing");
                                mat.enableInstancing = true;
                                EditorUtility.SetDirty(mat);
                                materialCount++;
                            }
                        }
                    }

                    EditorUtility.SetDirty(obj);
                }
            }

            AssetDatabase.SaveAssets();
            string msg = $"Optimasi Mesh Editor Selesai:\n" +
                         $"- {flagCount} objek jalan di-set static flags (Occludee Static).\n" +
                         $"- {materialCount} material jalan diaktifkan GPU Instancing.";
            
            Debug.Log($"[RoadMeshOptimizer] {msg}");
            EditorUtility.DisplayDialog("Optimasi Mesh Selesai", msg, "OK");
        }
#endif
    }
}
