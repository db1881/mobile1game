#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    /// <summary>
    /// Alt 4 mini butonu (magaza/gunluk/istatistik/basarim) eşit kare boyuta normalize eder.
    /// En büyük boyut bulunup hepsi o kare içine center pad ile yerleşir.
    /// </summary>
    public static class MiniButtonNormalize
    {
        private static readonly string[][] Groups = {
            // TR group
            new[] { "btn_magaza", "btn_gunluk", "btn_istatistik", "btn_basarim" },
            // EN group
            new[] { "btn_shop", "btn_daily", "btn_stats", "btn_awards" }
        };

        [MenuItem("BalloonPop/Normalize Mini Buttons")]
        public static void RunMenu() => BatchRun();

        public static void BatchRun()
        {
            foreach (var group in Groups)
            {
                NormalizeGroup(group);
            }
            AssetDatabase.Refresh();
            foreach (var group in Groups)
            {
                foreach (var name in group)
                {
                    var path = $"Assets/Sprites/{name}.png";
                    if (File.Exists(path)) ConfigureSprite(path);
                }
            }
            Debug.Log("[MiniButtonNormalize] Tamamlandı.");
        }

        private const int TargetSize = 512;

        private static void NormalizeGroup(string[] names)
        {
            foreach (var name in names)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                if (imp != null && !imp.isReadable) { imp.isReadable = true; imp.SaveAndReimport(); }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                int srcW = tex.width;
                int srcH = tex.height;

                // 1) İçeriğin gerçek bbox'unu bul (alpha > threshold veya yeterli luminance)
                var srcPx = tex.GetPixels();
                int minX = srcW, maxX = -1, minY = srcH, maxY = -1;
                for (int y = 0; y < srcH; y++)
                {
                    for (int x = 0; x < srcW; x++)
                    {
                        var c = srcPx[y * srcW + x];
                        float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                        bool isContent = c.a > 0.05f && (lum > 0.15f);
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
                    Debug.LogWarning($"[MiniButtonNormalize] {name}: content yok, atlanıyor");
                    continue;
                }
                // Pad ekle
                const int Pad = 8;
                minX = Mathf.Max(0, minX - Pad);
                maxX = Mathf.Min(srcW - 1, maxX + Pad);
                minY = Mathf.Max(0, minY - Pad);
                maxY = Mathf.Min(srcH - 1, maxY + Pad);
                int bboxW = maxX - minX + 1;
                int bboxH = maxY - minY + 1;

                // 2) Bbox'u kare hale getir (en uzun side)
                int squareSide = Mathf.Max(bboxW, bboxH);
                // Bbox merkezi
                int bcx = (minX + maxX) / 2;
                int bcy = (minY + maxY) / 2;
                // Kare bbox koordinatları (centroid merkezli)
                int sqMinX = bcx - squareSide / 2;
                int sqMinY = bcy - squareSide / 2;

                // 3) Kare bbox'tan TargetSize'a bilinear scale
                var dst = new Color[TargetSize * TargetSize];
                for (int i = 0; i < dst.Length; i++) dst[i] = new Color(0, 0, 0, 0);

                for (int y = 0; y < TargetSize; y++)
                {
                    for (int x = 0; x < TargetSize; x++)
                    {
                        // src koordinatı
                        float u = (sqMinX + (x + 0.5f) * squareSide / TargetSize) / srcW;
                        float v = (sqMinY + (y + 0.5f) * squareSide / TargetSize) / srcH;
                        if (u < 0f || u > 1f || v < 0f || v > 1f) continue; // dışında ise transparent
                        dst[y * TargetSize + x] = tex.GetPixelBilinear(u, v);
                    }
                }

                var outTex = new Texture2D(TargetSize, TargetSize, TextureFormat.RGBA32, false);
                outTex.SetPixels(dst);
                outTex.Apply();
                File.WriteAllBytes(path, outTex.EncodeToPNG());
                Object.DestroyImmediate(outTex);
                Debug.Log($"[MiniButtonNormalize] {name}.png: src {srcW}x{srcH} bbox {bboxW}x{bboxH}@({bcx},{bcy}) → {TargetSize}x{TargetSize}");
            }
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
