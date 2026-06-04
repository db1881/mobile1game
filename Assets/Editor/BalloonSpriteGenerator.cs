#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class BalloonSpriteGenerator
    {
        private const string SpriteFolder = "Assets/Sprites";
        private const int SpriteSize = 512;
        private const int Supersample = 2;

        private static readonly (string name, Color color)[] Balloons = new[]
        {
            ("balloon_red",    new Color(1.00f, 0.28f, 0.34f)),
            ("balloon_blue",   new Color(0.31f, 0.80f, 0.92f)),
            ("balloon_green",  new Color(0.18f, 0.80f, 0.44f)),
            ("balloon_yellow", new Color(1.00f, 0.85f, 0.24f)),
            ("balloon_purple", new Color(0.65f, 0.37f, 0.92f)),
            ("balloon_orange", new Color(1.00f, 0.55f, 0.26f)),
            ("balloon_pink",   new Color(1.00f, 0.42f, 0.62f)),
        };

        [MenuItem("BalloonPop/Generate Placeholder Sprites")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder))
            {
                Directory.CreateDirectory(SpriteFolder);
                AssetDatabase.Refresh();
            }

            foreach (var (name, color) in Balloons)
            {
                var path = $"{SpriteFolder}/{name}.png";
                if (File.Exists(path)) continue;
                var tex = CreateBalloonTexture(color);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }

            if (!File.Exists($"{SpriteFolder}/balloon_bomb.png"))
            {
                var bombTex = CreateBombTexture();
                File.WriteAllBytes($"{SpriteFolder}/balloon_bomb.png", bombTex.EncodeToPNG());
                Object.DestroyImmediate(bombTex);
            }

            if (!File.Exists($"{SpriteFolder}/balloon_lineH.png"))
            {
                var lineHTex = CreateLineTexture(true);
                File.WriteAllBytes($"{SpriteFolder}/balloon_lineH.png", lineHTex.EncodeToPNG());
                Object.DestroyImmediate(lineHTex);
            }

            if (!File.Exists($"{SpriteFolder}/balloon_lineV.png"))
            {
                var lineVTex = CreateLineTexture(false);
                File.WriteAllBytes($"{SpriteFolder}/balloon_lineV.png", lineVTex.EncodeToPNG());
                Object.DestroyImmediate(lineVTex);
            }

            if (!File.Exists($"{SpriteFolder}/balloon_rainbow.png"))
            {
                var rainbowTex = CreateRainbowTexture();
                File.WriteAllBytes($"{SpriteFolder}/balloon_rainbow.png", rainbowTex.EncodeToPNG());
                Object.DestroyImmediate(rainbowTex);
            }

            if (!File.Exists($"{SpriteFolder}/balloon_gold.png"))
            {
                var goldTex = CreateBalloonTexture(new Color(1.00f, 0.83f, 0.20f));
                File.WriteAllBytes($"{SpriteFolder}/balloon_gold.png", goldTex.EncodeToPNG());
                Object.DestroyImmediate(goldTex);
            }

            AssetDatabase.Refresh();
            foreach (var (name, _) in Balloons)
            {
                var path = $"{SpriteFolder}/{name}.png";
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    imp.spritePixelsPerUnit = tex != null ? tex.width : SpriteSize;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.alphaIsTransparency = true;
                    imp.mipmapEnabled = false;
                    imp.SaveAndReimport();
                }
            }
            foreach (var n in new[] { "balloon_bomb", "balloon_lineH", "balloon_lineV", "balloon_rainbow", "balloon_gold" })
            {
                var path = $"{SpriteFolder}/{n}.png";
                var sp = (TextureImporter)AssetImporter.GetAtPath(path);
                if (sp != null)
                {
                    sp.textureType = TextureImporterType.Sprite;
                    sp.spriteImportMode = SpriteImportMode.Single;
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    sp.spritePixelsPerUnit = tex != null ? tex.width : SpriteSize;
                    sp.filterMode = FilterMode.Bilinear;
                    sp.alphaIsTransparency = true;
                    sp.mipmapEnabled = false;
                    sp.SaveAndReimport();
                }
            }

            Debug.Log("Generated modern balloon sprites in " + SpriteFolder);
        }

        private static Texture2D CreateLineTexture(bool horizontal)
        {
            var tex = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[SpriteSize * SpriteSize];

            Color baseColor = new Color(0.95f, 0.78f, 0.20f);
            float bodyR = SpriteSize * 0.40f;
            Vector2 bodyCenter = new Vector2(SpriteSize * 0.5f, SpriteSize * 0.50f);

            Color darkEdge = Color.Lerp(baseColor, Color.black, 0.45f);
            Color midColor = Color.Lerp(baseColor, Color.black, 0.10f);
            Color lightColor = Color.Lerp(baseColor, Color.white, 0.30f);

            Vector2 mainHL = new Vector2(SpriteSize * 0.38f, SpriteSize * 0.64f);

            Color stripeColor = Color.white;

            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float dx = p.x - bodyCenter.x;
                    float dy = (p.y - bodyCenter.y) * 0.92f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    Color outCol = Color.clear;

                    if (dist <= bodyR + 1f)
                    {
                        float t = Mathf.Clamp01(dist / bodyR);
                        Color body = t < 0.5f
                            ? Color.Lerp(lightColor, midColor, t / 0.5f)
                            : Color.Lerp(midColor, darkEdge, (t - 0.5f) / 0.5f);

                        float coord = horizontal ? (p.y - bodyCenter.y + bodyR) : (p.x - bodyCenter.x + bodyR);
                        float stripeT = Mathf.Repeat(coord / (bodyR * 0.45f), 1f);
                        if (stripeT > 0.6f && stripeT < 0.95f)
                        {
                            body = Color.Lerp(body, stripeColor, 0.85f);
                        }

                        float hl = Mathf.Max(0f, 1f - Vector2.Distance(p, mainHL) / (bodyR * 0.4f));
                        hl = Mathf.Pow(hl, 2.5f);
                        body = Color.Lerp(body, Color.white, hl * 0.4f);

                        body.a = Mathf.Clamp01(bodyR - dist + 0.5f);
                        outCol = body;
                    }

                    pixels[y * SpriteSize + x] = outCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRainbowTexture()
        {
            var tex = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[SpriteSize * SpriteSize];

            float bodyR = SpriteSize * 0.40f;
            Vector2 bodyCenter = new Vector2(SpriteSize * 0.5f, SpriteSize * 0.50f);
            Vector2 mainHL = new Vector2(SpriteSize * 0.38f, SpriteSize * 0.66f);

            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float dx = p.x - bodyCenter.x;
                    float dy = (p.y - bodyCenter.y) * 0.92f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    Color outCol = Color.clear;

                    if (dist <= bodyR + 1f)
                    {
                        float angle = Mathf.Atan2(p.y - bodyCenter.y, p.x - bodyCenter.x);
                        float h = (angle / (2f * Mathf.PI) + 1f) % 1f;
                        Color body = Color.HSVToRGB(h, 0.85f, 0.95f);

                        float t = Mathf.Clamp01(dist / bodyR);
                        body = Color.Lerp(body, body * 0.6f, t);

                        float hl = Mathf.Max(0f, 1f - Vector2.Distance(p, mainHL) / (bodyR * 0.35f));
                        hl = Mathf.Pow(hl, 2.5f);
                        body = Color.Lerp(body, Color.white, hl * 0.55f);

                        body.a = Mathf.Clamp01(bodyR - dist + 0.5f);
                        outCol = body;
                    }

                    pixels[y * SpriteSize + x] = outCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBombTexture()
        {
            var tex = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[SpriteSize * SpriteSize];

            float bodyR = SpriteSize * 0.40f;
            Vector2 bodyCenter = new Vector2(SpriteSize * 0.5f, SpriteSize * 0.45f);

            Color dark = new Color(0.10f, 0.10f, 0.13f);
            Color mid = new Color(0.18f, 0.18f, 0.22f);
            Color light = new Color(0.45f, 0.45f, 0.50f);

            Color shadowColor = new Color(0, 0, 0, 0.22f);
            Vector2 shadowCenter = new Vector2(SpriteSize * 0.5f, SpriteSize * 0.07f);
            float shadowRX = SpriteSize * 0.26f;
            float shadowRY = SpriteSize * 0.05f;

            Vector2 mainHL = new Vector2(SpriteSize * 0.38f, SpriteSize * 0.60f);
            Vector2 smallHL = new Vector2(SpriteSize * 0.44f, SpriteSize * 0.68f);

            Vector2 fuseStart = new Vector2(SpriteSize * 0.5f, bodyCenter.y + bodyR * 0.95f);
            Vector2 fuseMid = new Vector2(SpriteSize * 0.58f, SpriteSize * 0.94f);
            Vector2 sparkCenter = new Vector2(SpriteSize * 0.62f, SpriteSize * 0.97f);

            Color fuseColor = new Color(0.55f, 0.40f, 0.20f);
            Color sparkOrange = new Color(1.00f, 0.65f, 0.10f);
            Color sparkYellow = new Color(1.00f, 0.95f, 0.30f);

            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    Color outCol = Color.clear;

                    float sdx = (p.x - shadowCenter.x) / shadowRX;
                    float sdy = (p.y - shadowCenter.y) / shadowRY;
                    float shadowDist = sdx * sdx + sdy * sdy;
                    if (shadowDist < 1f)
                        outCol = new Color(0, 0, 0, (1f - shadowDist) * shadowColor.a);

                    float bodyDist = Vector2.Distance(p, bodyCenter);
                    if (bodyDist <= bodyR + 1f)
                    {
                        float t = Mathf.Clamp01(bodyDist / bodyR);
                        Color body = t < 0.5f
                            ? Color.Lerp(light, mid, t / 0.5f)
                            : Color.Lerp(mid, dark, (t - 0.5f) / 0.5f);

                        float hl1 = Mathf.Max(0f, 1f - Vector2.Distance(p, mainHL) / (bodyR * 0.45f));
                        hl1 = Mathf.Pow(hl1, 2.5f);
                        body = Color.Lerp(body, new Color(0.85f, 0.85f, 0.90f), hl1 * 0.6f);

                        float hl2 = Mathf.Max(0f, 1f - Vector2.Distance(p, smallHL) / (bodyR * 0.15f));
                        hl2 = Mathf.Pow(hl2, 1.5f);
                        body = Color.Lerp(body, Color.white, hl2 * 0.8f);

                        float edgeAA = Mathf.Clamp01(bodyR - bodyDist + 0.5f);
                        body.a = edgeAA;
                        outCol = AlphaBlend(outCol, body);
                    }

                    float fuseD1 = DistanceToSegment(p, fuseStart, fuseMid);
                    if (fuseD1 < 3f)
                    {
                        float aa = Mathf.Clamp01(3f - fuseD1);
                        outCol = AlphaBlend(outCol, new Color(fuseColor.r, fuseColor.g, fuseColor.b, aa));
                    }

                    float sparkDist = Vector2.Distance(p, sparkCenter);
                    if (sparkDist < SpriteSize * 0.08f)
                    {
                        float st = sparkDist / (SpriteSize * 0.08f);
                        Color spark = Color.Lerp(sparkYellow, sparkOrange, st);
                        spark.a = Mathf.Clamp01(1f - st);
                        outCol = AlphaBlend(outCol, spark);
                    }

                    pixels[y * SpriteSize + x] = outCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBalloonTexture(Color baseColor)
        {
            int hi = SpriteSize * Supersample;
            var pixelsHi = new Color[hi * hi];

            float bodyR = hi * 0.40f;
            Vector2 bodyCenter = new Vector2(hi * 0.5f, hi * 0.56f);

            Color shadowColor = new Color(0, 0, 0, 0.22f);
            Vector2 shadowCenter = new Vector2(hi * 0.5f, hi * 0.09f);
            float shadowRX = hi * 0.23f;
            float shadowRY = hi * 0.04f;

            Color darkEdge = Color.Lerp(baseColor, Color.black, 0.55f);
            Color midColor = Color.Lerp(baseColor, Color.black, 0.10f);
            Color lightColor = Color.Lerp(baseColor, Color.white, 0.25f);
            Color rimGlow = Color.Lerp(baseColor, Color.white, 0.55f);

            Vector2 mainHighlight = new Vector2(hi * 0.38f, hi * 0.70f);
            Vector2 smallHighlight = new Vector2(hi * 0.44f, hi * 0.78f);
            Vector2 rimHighlight = new Vector2(hi * 0.74f, hi * 0.42f);

            Vector2 stringTop = new Vector2(hi * 0.5f, hi * 0.16f);
            Vector2 stringBottom = new Vector2(hi * 0.5f, bodyCenter.y - bodyR + 2);
            Color stringColor = new Color(baseColor.r * 0.35f, baseColor.g * 0.35f, baseColor.b * 0.35f, 1f);

            Vector2 knotCenter = new Vector2(hi * 0.5f, bodyCenter.y - bodyR - hi * 0.003f);
            float knotW = hi * 0.045f;
            float knotH = hi * 0.038f;
            Color knotColor = Color.Lerp(baseColor, Color.black, 0.60f);

            for (int y = 0; y < hi; y++)
            {
                for (int x = 0; x < hi; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    Color outCol = Color.clear;

                    float sdx = (p.x - shadowCenter.x) / shadowRX;
                    float sdy = (p.y - shadowCenter.y) / shadowRY;
                    float shadowDist = sdx * sdx + sdy * sdy;
                    if (shadowDist < 1f)
                    {
                        float fade = Mathf.Pow(1f - shadowDist, 2f);
                        outCol = new Color(0, 0, 0, fade * shadowColor.a);
                    }

                    float distToString = DistanceToSegment(p, stringTop, stringBottom);
                    if (distToString < 2.5f && p.y < stringTop.y && p.y > stringBottom.y - 1)
                    {
                        float aa = Mathf.Clamp01(2.5f - distToString);
                        outCol = AlphaBlend(outCol, new Color(stringColor.r, stringColor.g, stringColor.b, aa));
                    }

                    float kdx = (p.x - knotCenter.x) / knotW;
                    float kdy = (p.y - knotCenter.y) / knotH;
                    float knotDist = kdx * kdx + kdy * kdy;
                    if (knotDist < 1.2f)
                    {
                        float aa = Mathf.Clamp01((1.2f - knotDist) * 3f);
                        outCol = AlphaBlend(outCol, new Color(knotColor.r, knotColor.g, knotColor.b, aa));
                    }

                    float dx = p.x - bodyCenter.x;
                    float dy = (p.y - bodyCenter.y) * 0.92f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= bodyR + 2f)
                    {
                        float t = Mathf.Clamp01(dist / bodyR);
                        Color body;
                        if (t < 0.35f)
                            body = Color.Lerp(lightColor, midColor, t / 0.35f);
                        else if (t < 0.75f)
                            body = Color.Lerp(midColor, darkEdge, (t - 0.35f) / 0.40f);
                        else
                            body = darkEdge;

                        float rimDist = Vector2.Distance(p, rimHighlight) / (bodyR * 0.55f);
                        float rim = Mathf.Pow(Mathf.Max(0f, 1f - rimDist), 1.5f);
                        body = Color.Lerp(body, rimGlow, rim * 0.25f);

                        float hl1Dist = Vector2.Distance(p, mainHighlight) / (bodyR * 0.50f);
                        float hl1 = Mathf.Pow(Mathf.Max(0f, 1f - hl1Dist), 2.0f);
                        body = Color.Lerp(body, Color.white, hl1 * 0.50f);

                        float hl2Dist = Vector2.Distance(p, smallHighlight) / (bodyR * 0.13f);
                        float hl2 = Mathf.Pow(Mathf.Max(0f, 1f - hl2Dist), 1.4f);
                        body = Color.Lerp(body, Color.white, hl2 * 0.92f);

                        float edgeAlpha = Mathf.Clamp01((bodyR - dist + 1.5f) * 0.7f);
                        body.a = edgeAlpha;
                        outCol = AlphaBlend(outCol, body);
                    }

                    pixelsHi[y * hi + x] = outCol;
                }
            }

            var pixels = new Color[SpriteSize * SpriteSize];
            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int sy = 0; sy < Supersample; sy++)
                    for (int sx = 0; sx < Supersample; sx++)
                    {
                        var c = pixelsHi[(y * Supersample + sy) * hi + (x * Supersample + sx)];
                        r += c.r * c.a; g += c.g * c.a; b += c.b * c.a; a += c.a;
                    }
                    float ss = Supersample * Supersample;
                    if (a > 0.001f)
                        pixels[y * SpriteSize + x] = new Color(r / a, g / a, b / a, a / ss);
                    else
                        pixels[y * SpriteSize + x] = Color.clear;
                }
            }

            var tex = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Color AlphaBlend(Color bottom, Color top)
        {
            if (top.a <= 0f) return bottom;
            if (bottom.a <= 0f) return top;
            float a = top.a + bottom.a * (1f - top.a);
            float r = (top.r * top.a + bottom.r * bottom.a * (1f - top.a)) / a;
            float g = (top.g * top.a + bottom.g * bottom.a * (1f - top.a)) / a;
            float b = (top.b * top.a + bottom.b * bottom.a * (1f - top.a)) / a;
            return new Color(r, g, b, a);
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return Vector2.Distance(p, a);
            float t = Vector2.Dot(p - a, ab) / lenSq;
            t = Mathf.Clamp01(t);
            Vector2 proj = a + t * ab;
            return Vector2.Distance(p, proj);
        }
    }
}
#endif
