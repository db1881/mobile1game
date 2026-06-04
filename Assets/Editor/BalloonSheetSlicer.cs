#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class BalloonSheetSlicer
    {
        private const string SheetPath = "Assets/Sprites/balloons_sheet.png";
        private const string OutputFolder = "Assets/Sprites";
        private const int Cols = 4;
        private const int Rows = 3;

        private static readonly string[] Names = {
            "balloon_red",    "balloon_blue",   "balloon_green",   "balloon_yellow",
            "balloon_purple", "balloon_orange", "balloon_pink",    "balloon_bomb",
            "balloon_lineH",  "balloon_lineV",  "balloon_rainbow", "balloon_gold"
        };

        [MenuItem("BalloonPop/Slice Balloon Sheet")]
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
            Debug.Log($"[Slicer] Alpha range: {minAlpha:F2} - {maxAlpha:F2}");

            bool useAlpha = (maxAlpha - minAlpha) > 0.2f;
            Debug.Log($"[Slicer] Detection mode: {(useAlpha ? "ALPHA" : "COLOR")}");

            var content = new bool[W * H];
            int contentCount = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                bool isContent;
                if (useAlpha)
                {
                    isContent = c.a > 0.5f;
                }
                else
                {
                    float maxDiff = Mathf.Max(1f - c.r, Mathf.Max(1f - c.g, 1f - c.b));
                    isContent = maxDiff > 0.30f;
                }
                content[i] = isContent;
                if (isContent) contentCount++;
            }
            Debug.Log($"[Slicer] Content pixels: {contentCount} ({contentCount * 100f / pixels.Length:F1}%)");

            // Peak detection ile balonların gerçek merkezlerini bul (sheet boyutu / 4 sabit
            // bölmesi yanlışlanabiliyor — balonlar arası boşluk uniform değil).
            int[] rowProj = new int[H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (content[y * W + x]) rowProj[y]++;
            var rowCenters = FindPeaks(rowProj, Rows);
            System.Array.Sort(rowCenters);
            Debug.Log($"[Slicer] rowCenters: {string.Join(",", rowCenters)}");

            var centersGrid = new (int x, int y)[Names.Length];
            for (int r = 0; r < Rows; r++)
            {
                int rowY = rowCenters[r];
                int bandHalf = H / (Rows * 3);
                int yMin = Mathf.Max(0, rowY - bandHalf);
                int yMax = Mathf.Min(H - 1, rowY + bandHalf);
                int[] bandColProj = new int[W];
                for (int y = yMin; y <= yMax; y++)
                    for (int x = 0; x < W; x++)
                        if (content[y * W + x]) bandColProj[x]++;
                var colCenters = FindPeaks(bandColProj, Cols);
                System.Array.Sort(colCenters);

                int displayRow = Rows - 1 - r;
                for (int cc = 0; cc < Cols; cc++)
                {
                    int idx = displayRow * Cols + cc;
                    if (idx >= Names.Length) break;
                    centersGrid[idx] = (colCenters[cc], rowY);
                }
            }

            int[] savedSizes = new int[Names.Length];

            for (int i = 0; i < Names.Length; i++)
            {
                int cx = centersGrid[i].x;
                int cy = centersGrid[i].y;

                // Balonun gerçek bbox'unu balon merkezinden başlayarak flood-fill ile bul
                // (komşu balonun pikselleri ile karışmasın diye sınırlı band tarama).
                int searchHalf = Mathf.Min(W, H) / 8;
                int sxMin = Mathf.Max(0, cx - searchHalf);
                int sxMax = Mathf.Min(W - 1, cx + searchHalf);
                int syMin = Mathf.Max(0, cy - searchHalf);
                int syMax = Mathf.Min(H - 1, cy + searchHalf);

                int bboxMinX = sxMax + 1, bboxMaxX = sxMin - 1;
                int bboxMinY = syMax + 1, bboxMaxY = syMin - 1;
                int cnt = 0;
                for (int y = syMin; y <= syMax; y++)
                {
                    for (int x = sxMin; x <= sxMax; x++)
                    {
                        if (content[y * W + x])
                        {
                            if (x < bboxMinX) bboxMinX = x;
                            if (x > bboxMaxX) bboxMaxX = x;
                            if (y < bboxMinY) bboxMinY = y;
                            if (y > bboxMaxY) bboxMaxY = y;
                            cnt++;
                        }
                    }
                }

                int iconSz;
                int ccx, ccy;
                if (cnt < 100)
                {
                    ccx = cx; ccy = cy;
                    iconSz = searchHalf;
                }
                else
                {
                    ccx = (bboxMinX + bboxMaxX) / 2;
                    ccy = (bboxMinY + bboxMaxY) / 2;
                    int iconW = bboxMaxX - bboxMinX + 1;
                    int iconH = bboxMaxY - bboxMinY + 1;
                    iconSz = Mathf.Max(iconW, iconH) + 16; // padding
                }

                int srcX = Mathf.Clamp(ccx - iconSz / 2, 0, W - iconSz);
                int srcY = Mathf.Clamp(ccy - iconSz / 2, 0, H - iconSz);
                Debug.Log($"[Slicer] {Names[i]} peak=({cx},{cy}) bbox=({bboxMinX},{bboxMinY})-({bboxMaxX},{bboxMaxY}) crop @ ({srcX},{srcY}) sz={iconSz}");
                var cellPixels = sheet.GetPixels(srcX, srcY, iconSz, iconSz);
                var processed = RemoveWhiteBackground(cellPixels, iconSz, iconSz);
                SaveAsPng(processed, iconSz, iconSz, Names[i]);
                savedSizes[i] = iconSz;
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

            Debug.Log($"[Slicer] {Names.Length} sprite üretildi");
            EditorUtility.DisplayDialog("Slicing Done", $"{Names.Length} balon sprite üretildi.", "OK");
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

        private static Color[] RemoveWhiteBackground(Color[] src, int w, int h)
        {
            float minA = 1f, maxA = 0f;
            foreach (var p in src)
            {
                if (p.a < minA) minA = p.a;
                if (p.a > maxA) maxA = p.a;
            }
            bool hasAlphaInfo = (maxA - minA) > 0.2f;

            var dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                var c = src[i];
                if (hasAlphaInfo)
                {
                    dst[i] = c;
                    continue;
                }
                float distFromWhite = Mathf.Sqrt(
                    (1f - c.r) * (1f - c.r) +
                    (1f - c.g) * (1f - c.g) +
                    (1f - c.b) * (1f - c.b));
                float alpha;
                if (distFromWhite < 0.08f) alpha = 0f;
                else if (distFromWhite < 0.25f) alpha = (distFromWhite - 0.08f) / 0.17f;
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
