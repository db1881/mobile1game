#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    /// <summary>
    /// btn_*.png sprite'larının etrafındaki koyu/düşük-luminance gradient bg'yi atar.
    /// İçerik bbox'unu luminance threshold ile bulur, sprite'ı bbox + padding'e crop eder.
    /// </summary>
    public static class ButtonAutoCrop
    {
        // Sadece YATAY ANA butonları crop ediyoruz.
        // Alt 4 mini buton (magaza/gunluk/istatistik/basarim) crop yapmıyoruz çünkü
        // her sprite kendi içeriğine göre farklı yerden crop'lanıp hizasız görünüyor.
        // Onlar için MiniButtonNormalize bilinear scale ile aynı kanvasa fit ediyor.
        private static readonly string[] Targets = {
            "btn_oyna", "btn_levelsec", "btn_ayarlar", "btn_cikis",
            "btn_play", "btn_levelselect", "btn_settings", "btn_quit"
        };

        // Bu eşiğin ÜZERİNDE luminance + saturation olan pixel = content
        private const float LumThreshold = 0.35f;
        private const float SatThreshold = 0.25f;
        private const int PaddingPx = 12;

        [MenuItem("BalloonPop/Auto Crop Buttons")]
        public static void RunMenu() => BatchRun();

        public static void BatchRun()
        {
            int processed = 0;
            foreach (var name in Targets)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                if (CropOne(path)) processed++;
            }
            AssetDatabase.Refresh();
            foreach (var name in Targets)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                ConfigureSprite(path);
            }
            Debug.Log($"[ButtonAutoCrop] {processed} buton crop edildi.");
        }

        private static bool CropOne(string path)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp != null)
            {
                bool changed = false;
                if (!imp.isReadable) { imp.isReadable = true; changed = true; }
                if (imp.textureCompression != TextureImporterCompression.Uncompressed)
                { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
                if (changed) imp.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) return false;
            int w = tex.width, h = tex.height;
            var px = tex.GetPixels();

            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = px[y * w + x];
                    float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                    float maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                    float minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                    float sat = maxC > 0.01f ? (maxC - minC) / maxC : 0f;
                    bool isContent = (lum > LumThreshold && sat > SatThreshold)
                                  || lum > 0.85f; // beyaz/parlak alanlar da content
                    if (isContent)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
            {
                Debug.LogWarning($"[ButtonAutoCrop] {path}: content bulunamadı, atlanıyor.");
                return false;
            }

            int cropMinX = Mathf.Max(0, minX - PaddingPx);
            int cropMaxX = Mathf.Min(w - 1, maxX + PaddingPx);
            int cropMinY = Mathf.Max(0, minY - PaddingPx);
            int cropMaxY = Mathf.Min(h - 1, maxY + PaddingPx);
            int cropW = cropMaxX - cropMinX + 1;
            int cropH = cropMaxY - cropMinY + 1;

            var outPx = new Color[cropW * cropH];
            for (int y = 0; y < cropH; y++)
            {
                for (int x = 0; x < cropW; x++)
                {
                    outPx[y * cropW + x] = px[(cropMinY + y) * w + (cropMinX + x)];
                }
            }

            var outTex = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            outTex.SetPixels(outPx);
            outTex.Apply();
            File.WriteAllBytes(path, outTex.EncodeToPNG());
            Object.DestroyImmediate(outTex);
            Debug.Log($"[ButtonAutoCrop] {Path.GetFileName(path)}: {w}x{h} → {cropW}x{cropH}");
            return true;
        }

        private static void ConfigureSprite(string path)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.spritePixelsPerUnit = 256;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
    }
}
#endif
