using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RacingGame
{
    /// <summary>
    /// Alat Editor untuk mengonversi material jalan sirkuit bawaan (Standard/Legacy)
    /// menjadi material yang kompatibel dengan Universal Render Pipeline (URP/Lit)
    /// serta mengaktifkan GPU Instancing secara otomatis untuk menekan draw calls.
    /// </summary>
    public class RoadMaterialConverter : EditorWindow
    {
        [MenuItem("Tools/RacingGame/Fix Road Materials URP")]
        public static void FixRoadMaterials()
        {
            // Ambil shader URP Lit bawaan Unity
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader == null)
            {
                EditorUtility.DisplayDialog("Error", "Shader 'Universal Render Pipeline/Lit' tidak ditemukan! Pastikan proyek Anda dikonfigurasi menggunakan URP.", "OK");
                return;
            }

            // Cari semua material di proyek yang namanya mengandung "road", "asphalt", atau "track"
            string[] guids = AssetDatabase.FindAssets("t:Material");
            List<Material> roadMaterials = new List<Material>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat != null)
                {
                    string matName = mat.name.ToLower();
                    // Deteksi nama material jalan khas Road Architect
                    if (matName.Contains("road") || matName.Contains("asphalt") || matName.Contains("shoulder") || matName.Contains("line") || matName.Contains("barrier"))
                    {
                        roadMaterials.Add(mat);
                    }
                }
            }

            int convertedCount = 0;
            int instancingCount = 0;

            foreach (Material mat in roadMaterials)
            {
                Undo.RecordObject(mat, "Convert Road Material to URP");

                // 1. Cek jika shader saat ini masih shader standard/legacy lama
                string shaderName = mat.shader.name;
                if (shaderName.Contains("Standard") || shaderName.Contains("Legacy Shaders") || shaderName.Contains("Mobile/"))
                {
                    // Simpan tekstur utama sebelum dikonversi
                    Texture mainTex = mat.mainTexture;
                    Color mainColor = mat.color;

                    // Konversi shader ke URP Lit
                    mat.shader = urpLitShader;

                    // Pulihkan tekstur dan warna utama di properti shader URP baru (_BaseMap & _BaseColor)
                    if (mat.HasProperty("_BaseMap") && mainTex != null)
                    {
                        mat.SetTexture("_BaseMap", mainTex);
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", mainColor);
                    }

                    convertedCount++;
                }

                // 2. Aktifkan GPU Instancing pada material
                if (!mat.enableInstancing)
                {
                    mat.enableInstancing = true;
                    instancingCount++;
                }

                // Tandai material agar tersimpan perubahannya
                EditorUtility.SetDirty(mat);
            }

            // Simpan semua perubahan aset ke disk
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string report = $"Hasil Konversi Material Sirkuit:\n" +
                            $"- Total material terdeteksi: {roadMaterials.Count}\n" +
                            $"- Berhasil dikonversi ke URP/Lit: {convertedCount}\n" +
                            $"- GPU Instancing diaktifkan: {instancingCount}";
            
            Debug.Log($"[RoadMaterialConverter] {report}");
            EditorUtility.DisplayDialog("Konversi Selesai", report, "OK");
        }
    }
}
