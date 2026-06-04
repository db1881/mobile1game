#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class IconSheetSlicer
    {
        private const string SheetPath = "Assets/Sprites/icons_sheet.png";
        private const string OutputFolder = "Assets/Sprites";
        private const int Cols = 3;
        private const int Rows = 1;

        private static readonly string[] Names = {
            "icon_hammer", "icon_shuffle", "icon_plus"
        };

        [MenuItem("BalloonPop/Slice Icon Sheet")]
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
            Debug.Log($"[IconSlicer] Detection mode: {(useAlpha ? "ALPHA" : "COLOR")}");

            var content = new bool[W * H];
            int contentCount = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                bool isContent;
                if (useAlpha) isContent = c.a > 0.5f;
                else
                {
                    float maxDiff = Mathf.Max(1f - c.r, Mathf.Max(1f - c.g, 1f - c.b));
                    isContent = maxDiff > 0.30f;
                }
                content[i] = isContent;
                if (isContent) contentCount++;
            }
            Debug.Log($"[IconSlicer] Content pixels: {contentCount} ({contentCount * 100f / pixels.Length:F1}%)");

            int[] colProj = new int[W];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (content[y * W + x]) colProj[x]++;
            var colCenters = FindPeaks(colProj, Cols);
            Debug.Log($"[IconSlicer] Col centers: {string.Join(",", colCenters)}");

            int rowCenter = H / 2;
            var centers = new (int x, int y)[Names.Length];
            for (int c = 0; c < Cols; c++)
            {
                centers[c] = (colCenters[c], rowCenter);
            }

            int minColGap = int.MaxValue;
            for (int c = 1; c < Cols; c++)
            {
                int gap = Mathf.Abs(centers[c].x - centers[c-1].x);
                if (gap > 0 && gap < minColGap) minColGap = gap;
            }
            int sz = Mathf.Min(minColGap - 10, H - 20);
            sz = Mathf.Max(sz, 128);
            Debug.Log($"[IconSlicer] Crop size: {sz}");

            for (int i = 0; i < Names.Length; i++)
            {
                int cx = centers[i].x;
                int cy = centers[i].y;
                int srcX = Mathf.Clamp(cx - sz / 2, 0, W - sz);
                int srcY = Mathf.Clamp(cy - sz / 2, 0, H - sz);
                var cellPixels = sheet.GetPixels(srcX, srcY, sz, sz);
                var processed = useAlpha ? cellPixels : RemoveWhiteBackground(cellPixels);
                SaveAsPng(processed, sz, sz, Names[i]);
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
                    imp.spritePixelsPerUnit = sz;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.alphaIsTransparency = true;
                    imp.mipmapEnabled = false;
                    imp.SaveAndReimport();
                }
            }

            Debug.Log($"[IconSlicer] {Names.Length} icon üretildi ({sz}x{sz})");
            EditorUtility.DisplayDialog("Icon Slicing Done", $"{Names.Length} icon üretildi.", "OK");
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
            {
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }
            if (changed) imp.SaveAndReimport();
        }

        private static Color[] RemoveWhiteBackground(Color[] src)
        {
            var dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                var c = src[i];
                float distFromWhite = Mathf.Sqrt(
                    (1f - c.r) * (1f - c.r) +
                    (1f - c.g) * (1f - c.g) +
                    (1f - c.b) * (1f - c.b));
                float alpha;
                if (distFromWhite < 0.10f) alpha = 0f;
                else if (distFromWhite < 0.30f) alpha = (distFromWhite - 0.10f) / 0.20f;
                else alpha = 1f;
                dst[i] = new Color(c.r, c.g, c.b, alpha * c.a);
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
