#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    /// <summary>
    /// User-supplied btn_*.png sprite'larının köşelerden flood-fill ile
    /// arka plan gradient'ını alpha 0'a çevirir.
    /// </summary>
    public static class ButtonBgRemover
    {
        private static readonly string[] Targets = {
            "btn_play", "btn_levelselect", "btn_settings", "btn_quit",
            "btn_shop", "btn_daily", "btn_stats", "btn_awards",
            // İleride TR varyantları gelirse aynı isimle:
            "btn_oyna", "btn_levelsec", "btn_ayarlar", "btn_cikis",
            "btn_magaza", "btn_gunluk", "btn_istatistik", "btn_basarim"
        };

        // İki renk arası RGB Manhattan uzaklığı
        private const float ColorTolerance = 0.30f;

        [MenuItem("BalloonPop/Remove Button BG")]
        public static void RunMenu() => BatchRun();

        /// <summary> Sadece import ayarlarını uygular, görsele dokunmaz. </summary>
        public static void ConfigureSpritesOnly()
        {
            foreach (var name in Targets)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                ConfigureSprite(path);
            }
        }

        public static void BatchRun()
        {
            int processed = 0;
            foreach (var name in Targets)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                ProcessOne(path);
                processed++;
            }
            AssetDatabase.Refresh();
            foreach (var name in Targets)
            {
                var path = $"Assets/Sprites/{name}.png";
                if (!File.Exists(path)) continue;
                ConfigureSprite(path);
            }
            Debug.Log($"[ButtonBgRemover] {processed} sprite işlendi.");
        }

        private static void ProcessOne(string path)
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
            if (tex == null) return;
            int w = tex.width, h = tex.height;
            var px = tex.GetPixels();

            // 4 köşeden flood-fill ile bg pixellerini bul (queue BFS)
            var bg = new bool[w * h];
            var queue = new Queue<int>();
            int[] corners = { 0, w - 1, (h - 1) * w, h * w - 1 };
            foreach (var c in corners)
            {
                if (!bg[c]) { bg[c] = true; queue.Enqueue(c); }
            }

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                int x = idx % w;
                int y = idx / w;
                var refCol = px[idx];

                void Try(int nx, int ny)
                {
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) return;
                    int nidx = ny * w + nx;
                    if (bg[nidx]) return;
                    var c = px[nidx];
                    float dist = Mathf.Abs(c.r - refCol.r) + Mathf.Abs(c.g - refCol.g) + Mathf.Abs(c.b - refCol.b);
                    if (dist < ColorTolerance)
                    {
                        bg[nidx] = true;
                        queue.Enqueue(nidx);
                    }
                }
                Try(x + 1, y);
                Try(x - 1, y);
                Try(x, y + 1);
                Try(x, y - 1);
            }

            // BG pikselleri alpha = 0
            for (int i = 0; i < px.Length; i++)
            {
                if (bg[i]) px[i] = new Color(px[i].r, px[i].g, px[i].b, 0f);
            }

            var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels(px);
            outTex.Apply();
            File.WriteAllBytes(path, outTex.EncodeToPNG());
            Object.DestroyImmediate(outTex);
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
