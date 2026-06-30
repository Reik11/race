using UnityEngine;
using UnityEditor;
using System.IO;

namespace RacingGame
{
    /// <summary>
    /// Script Editor untuk mengimpor dan mengonfigurasi aset gambar menu utama secara otomatis.
    /// Menyalin gambar dari direktori chat Gemini ke folder proyek Assets/Textures/MainMenu
    /// dan menyetel tipe teksturnya menjadi Sprite (2D and UI).
    /// </summary>
    public class MainMenuAssetImporter : EditorWindow
    {
        [MenuItem("Tools/RacingGame/Import Main Menu Assets")]
        public static void ImportAssets()
        {
            // Direktori sumber file media dari chat Gemini
            string sourceDir = @"C:\Users\xirei\.gemini\antigravity\brain\0d73fb5d-e54d-4cbc-87cd-7ddc2f00b6cc";
            
            // Direktori tujuan di dalam folder Assets proyek Unity Anda
            string destDir = Path.Combine(Application.dataPath, "Textures/MainMenu");

            // Buat direktori tujuan jika belum ada
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Pemetaan nama file media acak ke nama file proyek yang rapi
            string[,] filesToCopy = new string[,] {
                { "media__1782756468298.jpg", "MainMenu_Reference.jpg" }, // Contoh referensi layout menu
                { "media__1782756468306.jpg", "Logo_DriftMaster.jpg" },    // Logo "Drift Master"
                { "media__1782756468322.jpg", "Menu_Background.jpg" }      // Gambar latar pemandangan senja
            };

            int copiedCount = 0;
            for (int i = 0; i < filesToCopy.GetLength(0); i++)
            {
                string sourceFile = Path.Combine(sourceDir, filesToCopy[i, 0]);
                string destFile = Path.Combine(destDir, filesToCopy[i, 1]);

                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, destFile, true);
                    Debug.Log($"[Importer] Berhasil menyalin: {filesToCopy[i, 0]} -> {filesToCopy[i, 1]}");
                    copiedCount++;
                }
                else
                {
                    Debug.LogWarning($"[Importer] File sumber tidak ditemukan: {sourceFile}");
                }
            }

            if (copiedCount == 0)
            {
                EditorUtility.DisplayDialog("Impor Gagal", "File gambar dari chat tidak ditemukan di folder temporary brain. Silakan periksa kembali.", "OK");
                return;
            }

            // Segarkan database aset Unity agar mendeteksi file baru
            AssetDatabase.Refresh();

            // Ubah tipe tekstur menjadi Sprite (2D and UI) secara otomatis
            SetAsSprite("Assets/Textures/MainMenu/Logo_DriftMaster.jpg");
            SetAsSprite("Assets/Textures/MainMenu/Menu_Background.jpg");

            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Impor Sukses", "Berhasil mengimpor aset Main Menu! Gambar telah disalin dan di-set sebagai Sprite (2D & UI).", "OK");
            Debug.Log("[Importer] Impor selesai! Tekstur telah diatur sebagai Sprite (2D and UI) dan siap digunakan di UI Image.");
        }

        private static void SetAsSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }
    }
}
