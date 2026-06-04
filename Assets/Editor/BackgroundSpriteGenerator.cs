#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class BackgroundSpriteGenerator
    {
        private const string SpriteFolder = "Assets/Sprites";
        private const int W = 1024;
        private const int H = 2048;

        [MenuItem("BalloonPop/Generate Theme Backgrounds")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder))
            {
                Directory.CreateDirectory(SpriteFolder);
                AssetDatabase.Refresh();
            }

            if (!File.Exists($"{SpriteFolder}/bg_beach.png"))  Save("bg_beach",  GenerateBeach());
            if (!File.Exists($"{SpriteFolder}/bg_winter.png")) Save("bg_winter", GenerateWinter());
            if (!File.Exists($"{SpriteFolder}/bg_space.png"))  Save("bg_space",  GenerateSpace());
            if (!File.Exists($"{SpriteFolder}/bg_candy.png"))  Save("bg_candy",  GenerateCandy());

            AssetDatabase.Refresh();
            foreach (var n in new[] { "bg_beach", "bg_winter", "bg_space", "bg_candy" })
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath($"{SpriteFolder}/{n}.png");
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spritePixelsPerUnit = 256;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.maxTextureSize = 2048;
                    imp.SaveAndReimport();
                }
            }
            Debug.Log("Theme backgrounds generated");
        }

        private static void Save(string name, Color[] pixels)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static Color[] GenerateBeach()
        {
            var p = new Color[W * H];
            var rng = new System.Random(101);
            Color skyTop = new Color(0.55f, 0.82f, 0.98f);
            Color skyBottom = new Color(0.85f, 0.96f, 1.00f);
            Color seaTop = new Color(0.30f, 0.60f, 0.85f);
            Color seaBottom = new Color(0.15f, 0.45f, 0.70f);
            Color sandTop = new Color(0.96f, 0.85f, 0.55f);
            Color sandBottom = new Color(0.80f, 0.65f, 0.35f);

            float seaLevel = 0.40f;
            float sandLevel = 0.20f;

            Vector2 sunCenter = new Vector2(W * 0.78f, H * 0.82f);
            float sunRadius = W * 0.10f;

            for (int y = 0; y < H; y++)
            {
                float ny = y / (float)H;
                Color row;
                if (ny < sandLevel)
                {
                    float t = ny / sandLevel;
                    row = Color.Lerp(sandBottom, sandTop, t);
                }
                else if (ny < seaLevel)
                {
                    float t = (ny - sandLevel) / (seaLevel - sandLevel);
                    row = Color.Lerp(seaBottom, seaTop, t);
                }
                else
                {
                    float t = (ny - seaLevel) / (1f - seaLevel);
                    row = Color.Lerp(skyBottom, skyTop, t);
                }
                for (int x = 0; x < W; x++)
                {
                    Color c = row;
                    float dx = x - sunCenter.x;
                    float dy = y - sunCenter.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < sunRadius * 1.6f)
                    {
                        float u = dist / (sunRadius * 1.6f);
                        Color sun = Color.Lerp(new Color(1f, 0.9f, 0.4f), new Color(1f, 0.7f, 0.3f), u);
                        c = Color.Lerp(sun, c, Mathf.Pow(u, 1.5f));
                    }
                    if (ny > sandLevel && ny < seaLevel)
                    {
                        float wave = Mathf.Sin(x * 0.02f + ny * 50f) * 0.04f;
                        c.r += wave; c.g += wave; c.b += wave;
                    }
                    p[y * W + x] = c;
                }
            }
            return p;
        }

        private static Color[] GenerateWinter()
        {
            var p = new Color[W * H];
            var rng = new System.Random(202);
            Color skyTop = new Color(0.20f, 0.30f, 0.50f);
            Color skyBottom = new Color(0.55f, 0.70f, 0.88f);
            Color snowTop = new Color(0.85f, 0.92f, 1.00f);
            Color snowBottom = new Color(0.65f, 0.78f, 0.92f);
            Color mountain = new Color(0.30f, 0.40f, 0.55f);
            Color mountainSnow = new Color(0.90f, 0.95f, 1.00f);

            float snowLevel = 0.30f;
            float mountainBase = 0.30f;

            int starCount = 80;
            var starX = new int[starCount];
            var starY = new int[starCount];
            for (int i = 0; i < starCount; i++)
            {
                starX[i] = rng.Next(W);
                starY[i] = rng.Next((int)(H * 0.6f), H);
            }

            int snowFlakes = 200;
            var snowX = new int[snowFlakes];
            var snowY = new int[snowFlakes];
            for (int i = 0; i < snowFlakes; i++)
            {
                snowX[i] = rng.Next(W);
                snowY[i] = rng.Next(H);
            }

            int peakCount = 4;
            var peakX = new int[peakCount];
            var peakH = new int[peakCount];
            for (int i = 0; i < peakCount; i++)
            {
                peakX[i] = (i + 1) * W / (peakCount + 1) + rng.Next(-100, 100);
                peakH[i] = (int)(H * 0.18f) + rng.Next(-40, 80);
            }

            for (int y = 0; y < H; y++)
            {
                float ny = y / (float)H;
                Color row;
                if (ny < snowLevel)
                {
                    float t = ny / snowLevel;
                    row = Color.Lerp(snowBottom, snowTop, t);
                }
                else
                {
                    float t = (ny - snowLevel) / (1f - snowLevel);
                    row = Color.Lerp(skyBottom, skyTop, t);
                }
                for (int x = 0; x < W; x++)
                {
                    Color c = row;

                    if (ny >= snowLevel && ny < snowLevel + mountainBase)
                    {
                        int relY = y - (int)(snowLevel * H);
                        for (int i = 0; i < peakCount; i++)
                        {
                            int distFromPeak = Mathf.Abs(x - peakX[i]);
                            int peakTop = peakH[i];
                            int mountainEdge = peakTop - (distFromPeak * 2);
                            if (relY < mountainEdge)
                            {
                                float topRatio = relY / (float)peakTop;
                                c = Color.Lerp(mountain, mountainSnow, Mathf.Clamp01(topRatio - 0.4f) * 2f);
                            }
                        }
                    }
                    p[y * W + x] = c;
                }
            }

            foreach (var i in new[] { 0 })
            {
                for (int k = 0; k < starCount; k++)
                {
                    int sx = starX[k], sy = starY[k];
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int x = sx + dx, y = sy + dy;
                        if (x < 0 || y < 0 || x >= W || y >= H) continue;
                        float alpha = (dx == 0 && dy == 0) ? 1f : 0.3f;
                        p[y * W + x] = Color.Lerp(p[y * W + x], Color.white, alpha);
                    }
                }
            }

            for (int k = 0; k < snowFlakes; k++)
            {
                int sx = snowX[k], sy = snowY[k];
                int radius = (int)Mathf.Lerp(2, 5, (float)rng.NextDouble());
                for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = sx + dx, y = sy + dy;
                    if (x < 0 || y < 0 || x >= W || y >= H) continue;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > radius) continue;
                    float alpha = (1f - d / radius) * 0.7f;
                    p[y * W + x] = Color.Lerp(p[y * W + x], Color.white, alpha);
                }
            }
            return p;
        }

        private static Color[] GenerateSpace()
        {
            var p = new Color[W * H];
            var rng = new System.Random(303);
            Color top = new Color(0.04f, 0.02f, 0.18f);
            Color mid = new Color(0.12f, 0.05f, 0.30f);
            Color bottom = new Color(0.20f, 0.10f, 0.40f);

            for (int y = 0; y < H; y++)
            {
                float ny = y / (float)H;
                Color row;
                if (ny < 0.5f) row = Color.Lerp(bottom, mid, ny * 2f);
                else row = Color.Lerp(mid, top, (ny - 0.5f) * 2f);
                for (int x = 0; x < W; x++)
                {
                    Color c = row;
                    float noise = Mathf.PerlinNoise(x * 0.005f, y * 0.005f);
                    c.r += (noise - 0.5f) * 0.05f;
                    c.g += (noise - 0.5f) * 0.06f;
                    c.b += (noise - 0.5f) * 0.10f;
                    p[y * W + x] = c;
                }
            }

            int starCount = 600;
            for (int k = 0; k < starCount; k++)
            {
                int sx = rng.Next(W);
                int sy = rng.Next(H);
                float brightness = (float)rng.NextDouble();
                int radius = brightness > 0.85f ? 3 : (brightness > 0.6f ? 2 : 1);
                Color sc = brightness > 0.7f ? Color.white : new Color(0.85f, 0.90f, 1f);
                for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = sx + dx, y = sy + dy;
                    if (x < 0 || y < 0 || x >= W || y >= H) continue;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > radius) continue;
                    float alpha = (1f - d / radius) * brightness;
                    p[y * W + x] = Color.Lerp(p[y * W + x], sc, alpha);
                }
            }

            int planetCx = (int)(W * 0.20f);
            int planetCy = (int)(H * 0.18f);
            int planetR = (int)(W * 0.13f);
            for (int dy = -planetR - 5; dy <= planetR + 5; dy++)
            for (int dx = -planetR - 5; dx <= planetR + 5; dx++)
            {
                int x = planetCx + dx, y = planetCy + dy;
                if (x < 0 || y < 0 || x >= W || y >= H) continue;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > planetR + 4) continue;
                Color pcol = Color.Lerp(new Color(0.95f, 0.55f, 0.25f), new Color(0.55f, 0.20f, 0.10f), d / planetR);
                if (d > planetR)
                {
                    float aa = 1f - (d - planetR) / 4f;
                    pcol = Color.Lerp(p[y * W + x], pcol, Mathf.Clamp01(aa));
                }
                float hl = Mathf.Max(0, 1f - Vector2.Distance(new Vector2(dx, dy), new Vector2(-planetR * 0.3f, planetR * 0.3f)) / (planetR * 0.6f));
                pcol = Color.Lerp(pcol, new Color(1f, 0.9f, 0.7f), hl * 0.4f);
                p[y * W + x] = pcol;
            }
            return p;
        }

        private static Color[] GenerateCandy()
        {
            var p = new Color[W * H];
            var rng = new System.Random(404);
            Color[] stripes = {
                new Color(1.00f, 0.65f, 0.85f),
                new Color(1.00f, 0.85f, 0.55f),
                new Color(0.75f, 0.85f, 1.00f),
                new Color(0.85f, 0.65f, 1.00f),
            };
            float stripeWidth = W / 6f;

            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float diag = (x + y) / stripeWidth;
                    int idx = (int)Mathf.Repeat(diag, stripes.Length);
                    float frac = diag - Mathf.Floor(diag);
                    int next = (idx + 1) % stripes.Length;
                    Color baseCol = Color.Lerp(stripes[idx], stripes[next], Mathf.SmoothStep(0.85f, 1f, frac));
                    p[y * W + x] = baseCol;
                }
            }

            int candyCount = 25;
            for (int k = 0; k < candyCount; k++)
            {
                int cx = rng.Next(W);
                int cy = rng.Next(H);
                int radius = rng.Next(30, 80);
                Color candy = stripes[rng.Next(stripes.Length)] * 1.1f;
                for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = cx + dx, y = cy + dy;
                    if (x < 0 || y < 0 || x >= W || y >= H) continue;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > radius) continue;
                    float t = d / radius;
                    Color cc = Color.Lerp(candy, candy * 0.7f, t);
                    float hl = Mathf.Max(0, 1f - Vector2.Distance(new Vector2(dx, dy), new Vector2(-radius * 0.3f, radius * 0.3f)) / (radius * 0.5f));
                    cc = Color.Lerp(cc, Color.white, hl * 0.35f);
                    float aa = Mathf.Clamp01(1f - (d - radius + 2) / 2);
                    p[y * W + x] = Color.Lerp(p[y * W + x], cc, aa);
                }
            }
            return p;
        }
    }
}
#endif
