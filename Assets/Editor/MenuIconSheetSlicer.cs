#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class MenuIconSheetSlicer
    {
        private const string SheetPath = "Assets/Sprites/menu_icons_sheet.png";
        private const string OutputFolder = "Assets/Sprites";
        private const int Cols = 8;
        private const int Rows = 1;

        private static readonly string[] Names = {
            "menu_play", "menu_levelmap", "menu_settings", "menu_quit",
            "menu_shop", "menu_daily", "menu_stats", "menu_trophy"
        };

        [MenuItem("BalloonPop/Slice Menu Icon Sheet")]
        public static void Slice()
        {
            if (!File.Exists(SheetPath))
            {
                EditorUtility.DisplayDialog("Sheet Not Found", $"'{SheetPath}' bulunamadı.", "OK");
                return;
            }

            EnsureReadable(SheetPath);
            var sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            if (sheet == null) { Debug.LogError("Sheet yüklenemedi"); return; }

            int W = sheet.width;
            int H = sheet.height;
            var pixels = sheet.GetPixels();

            float minAlpha = 1f, maxAlpha = 0f;
            foreach (var p in pixels)
            {
                if (p.a < minAlpha) minAlpha = p.a;
                if (p.a > maxAlpha) maxAlpha = p.a;
            }
            bool useAlpha = (maxAlpha - minAlpha) > 0.2f;
            Debug.Log($"[MenuIconSlicer] Mode: {(useAlpha ? "ALPHA" : "COLOR")}");

            // Cell merkezinde dar band'de content centroid (kütle merkezi) bul + safe crop kullan.
            // Bbox kullanmak komşu icon'un taşan piksellerini de yakaladığı için yanlış.
            int cellW = W / Cols;
            int safeCrop = (int)(cellW * 0.86f);
            safeCrop = Mathf.Min(safeCrop, H - 4);
            safeCrop = Mathf.Max(safeCrop, 64);
            int[] savedSizes = new int[Names.Length];
            Debug.Log($"[MenuIconSlicer] W={W} H={H} cellW={cellW} safeCrop={safeCrop}");

            for (int i = 0; i < Names.Length; i++)
            {
                int cellStartX = i * cellW;
                int cellEndX = Mathf.Min((i + 1) * cellW, W);
                int cellMid = (cellStartX + cellEndX) / 2;

                // Cell'in orta %60 band'inde centroid bul (kenar pikselleri komşudan değil)
                int bandStart = cellStartX + (cellEndX - cellStartX) / 5;
                int bandEnd   = cellEndX - (cellEndX - cellStartX) / 5;
                int bandTop    = H * 1 / 6;
                int bandBottom = H * 5 / 6;

                double sumX = 0, sumY = 0;
                int cnt = 0;
                for (int y = bandTop; y < bandBottom; y++)
                {
                    for (int x = bandStart; x < bandEnd; x++)
                    {
                        var c = pixels[y * W + x];
                        bool isContent;
                        if (useAlpha) isContent = c.a > 0.5f;
                        else
                        {
                            float maxDiff = Mathf.Max(1f - c.r, Mathf.Max(1f - c.g, 1f - c.b));
                            isContent = maxDiff > 0.30f;
                        }
                        if (isContent) { sumX += x; sumY += y; cnt++; }
                    }
                }

                int cx, cy;
                if (cnt < 80)
                {
                    cx = cellMid; cy = H / 2;
                }
                else
                {
                    cx = (int)(sumX / cnt);
                    cy = (int)(sumY / cnt);
                }

                int srcX = Mathf.Clamp(cx - safeCrop / 2, cellStartX, cellEndX - safeCrop);
                int srcY = Mathf.Clamp(cy - safeCrop / 2, 0, H - safeCrop);
                Debug.Log($"[MenuIconSlicer] {Names[i]} centroid=({cx},{cy}) crop @ ({srcX},{srcY}) size {safeCrop}");
                var cellPixels = sheet.GetPixels(srcX, srcY, safeCrop, safeCrop);
                var processed = useAlpha ? cellPixels : RemoveWhiteBackground(cellPixels);
                SaveAsPng(processed, safeCrop, safeCrop, Names[i]);
                savedSizes[i] = safeCrop;
            }

            AssetDatabase.Refresh();

            for (int i = 0; i < Names.Length; i++)
            {
                var path = $"{OutputFolder}/{Names[i]}.png";
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.spritePixelsPerUnit = savedSizes[i];
                    imp.filterMode = FilterMode.Bilinear;
                    imp.alphaIsTransparency = true;
                    imp.mipmapEnabled = false;
                    imp.SaveAndReimport();
                }
            }

            Debug.Log($"[MenuIconSlicer] {Names.Length} icon üretildi");
            EditorUtility.DisplayDialog("Done", $"{Names.Length} menu icon üretildi.", "OK");
        }

        private static int[] FindPeaks(int[] projection, int desiredCount)
        {
            int n = projection.Length;
            int minDistance = n / (desiredCount * 2);
            var candidates = new List<(int idx, int val)>();
            for (int i = 0; i < n; i++) candidates.Add((i, projection[i]));
            candidates.Sort((a, b) => b.val.CompareTo(a.val));

            var selected = new List<int>();
            foreach (var c in candidates)
            {
                if (c.val <= 0) break;
                bool tooClose = false;
                foreach (var s in selected) if (Mathf.Abs(s - c.idx) < minDistance) { tooClose = true; break; }
                if (!tooClose) selected.Add(c.idx);
                if (selected.Count >= desiredCount) break;
            }
            while (selected.Count < desiredCount) selected.Add(n / 2);
            selected.Sort();
            return selected.ToArray();
        }

        private static void EnsureReadable(string path)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null) return;
            bool changed = false;
            if (!imp.isReadable) { imp.isReadable = true; changed = true; }
            if (imp.textureCompression != TextureImporterCompression.Uncompressed)
            { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (changed) imp.SaveAndReimport();
        }

        private static Color[] RemoveWhiteBackground(Color[] src)
        {
            var dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                var c = src[i];
                float d = Mathf.Sqrt((1f-c.r)*(1f-c.r) + (1f-c.g)*(1f-c.g) + (1f-c.b)*(1f-c.b));
                float a = d < 0.10f ? 0f : (d < 0.30f ? (d - 0.10f) / 0.20f : 1f);
                dst[i] = new Color(c.r, c.g, c.b, a * c.a);
            }
            return dst;
        }

        private static void SaveAsPng(Color[] pixels, int w, int h, string name)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes($"{OutputFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
#endif
