using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RacingGame
{
    /// <summary>
    /// Mengoptimalkan collider jalan sirkuit untuk performa physics yang lebih efisien.
    /// Mengaktifkan opsi cooking optimal pada MeshCollider dan memastikan semua collider jalan di-set sebagai Static.
    /// </summary>
    public class RoadColliderOptimizer : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/RacingGame/Optimize Road Colliders")]
        public static void OptimizeRoadColliders()
        {
            // Temukan semua GameObject di scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int optimizedColliders = 0;
            int staticSetCount = 0;

            foreach (GameObject obj in allObjects)
            {
                // Cek apakah objek tersebut merupakan bagian dari jalan
                if (obj.CompareTag("Road") || obj.name.ToLower().Contains("road") || obj.name.ToLower().Contains("asphalt"))
                {
                    Undo.RecordObject(obj, "Set Road Object Static");

                    // 1. Pastikan objek di-set sebagai static (untuk kalkulasi physics static yang cepat)
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
                    if ((flags & StaticEditorFlags.ContributeGI) == 0)
                    {
                        flags |= StaticEditorFlags.ContributeGI; // Menyatakan objek statis bagi renderer/physics
                        GameObjectUtility.SetStaticEditorFlags(obj, flags);
                        staticSetCount++;
                    }

                    // 2. Optimalkan MeshCollider yang terpasang pada jalan
                    MeshCollider[] meshColliders = obj.GetComponents<MeshCollider>();
                    foreach (MeshCollider col in meshColliders)
                    {
                        if (col != null)
                        {
                            Undo.RecordObject(col, "Optimize MeshCollider Cooking Options");

                            // Set opsi cooking optimal untuk mempercepat loading scene dan kalkulasi physics ban mobil
                            col.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | 
                                                 MeshColliderCookingOptions.EnableMeshCleaning | 
                                                 MeshColliderCookingOptions.WeldColocatedVertices;
                            
                            EditorUtility.SetDirty(col);
                            optimizedColliders++;
                        }
                    }

                    EditorUtility.SetDirty(obj);
                }
            }

            string report = $"Optimasi Collider Sirkuit Selesai:\n" +
                            $"- {staticSetCount} objek jalan di-set ke Static.\n" +
                            $"- {optimizedColliders} MeshCollider dioptimalkan cooking options-nya.";
            
            Debug.Log($"[RoadColliderOptimizer] {report}");
            EditorUtility.DisplayDialog("Optimasi Collider Selesai", report, "OK");
        }
#endif
    }
}
