#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class UISpriteGenerator
    {
        private const string SpriteFolder = "Assets/Sprites";

        [MenuItem("BalloonPop/Generate UI Sprites")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder))
            {
                Directory.CreateDirectory(SpriteFolder);
                AssetDatabase.Refresh();
            }

            CreateRoundedRect("ui_rounded_16", 96, 16);
            CreateRoundedRect("ui_rounded_24", 128, 24);
            CreateRoundedRect("ui_rounded_40", 160, 40);
            CreateRoundedRect("ui_rounded_56", 192, 56);
            CreateRoundedRect("ui_pill", 220, 96);   // büyük yarıçap → şeker/hap kenar
            // Ana menü şeker butonları — TÜM parlaklık/gradient/3D doku içine "pişirilmiş"
            CreateCandyButton("ui_btn_green",  256, new Color(0.30f, 0.78f, 0.34f));
            CreateCandyButton("ui_btn_blue",   256, new Color(0.20f, 0.60f, 0.95f));
            CreateCandyButton("ui_btn_purple", 256, new Color(0.64f, 0.41f, 0.95f));
            CreateCandyButton("ui_btn_red",    256, new Color(0.97f, 0.40f, 0.42f));
            CreateCircle("ui_circle", 128);
            CreateSoftShadow("ui_shadow", 192, 56, 24, 16);
            CreateGradientVertical("ui_gradient", 128, 256);
            CreateStar("ui_star", 128);
            CreateRing("ui_ring", 256, 96, 16);
            CreateRadialGlow("ui_glow", 256);
            CreateTapIcon("ui_tap", 256);
            if (!File.Exists($"{SpriteFolder}/ui_stone.png")) CreateStone("ui_stone", 256);
            if (!File.Exists($"{SpriteFolder}/icon_hammer.png"))  CreateHammerIcon("icon_hammer", 128);
            if (!File.Exists($"{SpriteFolder}/icon_shuffle.png")) CreateShuffleIcon("icon_shuffle", 128);
            if (!File.Exists($"{SpriteFolder}/icon_plus.png"))    CreatePlusIcon("icon_plus", 128);
            // Ana menü şerit buton ikonları (procedural, beyaz siluet → tint'lenebilir)
            CreatePlayIcon("icon_play", 128);
            CreateGridIcon("icon_grid", 128);
            CreateGearIcon("icon_gear", 128);
            CreatePowerIcon("icon_power", 128);
            CreateIceTile("ui_ice", 256);
            CreateGameFrame("ui_game_frame", 256, 32);
            CreateCellTile("ui_cell_tile", 128, 18);

            AssetDatabase.Refresh();

            ConfigureSprite("ui_rounded_16", 96,  new Vector4(16, 16, 16, 16));
            ConfigureSprite("ui_rounded_24", 128, new Vector4(24, 24, 24, 24));
            ConfigureSprite("ui_rounded_40", 160, new Vector4(40, 40, 40, 40));
            ConfigureSprite("ui_rounded_56", 192, new Vector4(56, 56, 56, 56));
            ConfigureSprite("ui_pill", 220, new Vector4(96, 96, 96, 96));
            ConfigureSprite("ui_btn_green",  128, new Vector4(96, 96, 96, 96));
            ConfigureSprite("ui_btn_blue",   128, new Vector4(96, 96, 96, 96));
            ConfigureSprite("ui_btn_purple", 128, new Vector4(96, 96, 96, 96));
            ConfigureSprite("ui_btn_red",    128, new Vector4(96, 96, 96, 96));
            ConfigureSprite("ui_circle", 128, Vector4.zero);
            ConfigureSprite("ui_shadow", 192, new Vector4(64, 64, 64, 64));
            ConfigureSprite("ui_gradient", 128, Vector4.zero);
            ConfigureSprite("ui_star", 128, Vector4.zero);
            ConfigureSprite("ui_ring", 256, Vector4.zero);
            ConfigureSprite("ui_glow", 256, Vector4.zero);
            ConfigureSprite("ui_tap", 256, Vector4.zero);
            ConfigureSprite("ui_stone", 1024, Vector4.zero);
            ConfigureSprite("icon_hammer", 128, Vector4.zero);
            ConfigureSprite("icon_shuffle", 128, Vector4.zero);
            ConfigureSprite("icon_plus", 128, Vector4.zero);
            ConfigureSprite("icon_play", 128, Vector4.zero);
            ConfigureSprite("icon_grid", 128, Vector4.zero);
            ConfigureSprite("icon_gear", 128, Vector4.zero);
            ConfigureSprite("icon_power", 128, Vector4.zero);
            ConfigureSprite("ui_ice", 256, Vector4.zero);
            ConfigureSprite("ui_game_frame", 100, new Vector4(48, 48, 48, 48));
            ConfigureSprite("ui_cell_tile", 128, new Vector4(28, 28, 28, 28));

            Debug.Log("Generated modern UI sprites in " + SpriteFolder);
        }

        private static void CreateRoundedRect(string name, int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = new Color(1, 1, 1, RoundedRectAlpha(x, y, size, size, radius));
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        /// <summary>
        /// Şeker/jöle buton dokusu: dikey gradient + üst cam parlaklığı (gloss) +
        /// üst kenar ışığı + alt 3D dudak + kenar bevel — hepsi piksel piksel tek
        /// dokuya pişirilmiş. 9-slice ile her boyuta yayılır, köşeler bozulmaz.
        /// </summary>
        private static void CreateCandyButton(string name, int size, Color baseColor)
        {
            int w = size, h = size, radius = 92;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            Color top    = Color.Lerp(baseColor, Color.white, 0.20f);
            Color bot    = Color.Lerp(baseColor, Color.black, 0.30f);
            Color glossC = Color.Lerp(baseColor, Color.white, 0.80f);
            Color rimT   = Color.Lerp(baseColor, Color.white, 0.55f);
            Color botLip = Color.Lerp(baseColor, Color.black, 0.46f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float aa = RoundedRectAlpha(x, y, w, h, radius);
                if (aa <= 0f) { px[y * w + x] = Color.clear; continue; }
                float ny = y / (float)(h - 1);                       // 0 alt → 1 üst
                float dEdge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));

                // 1) Dikey ana gradient
                Color col = Color.Lerp(bot, top, Mathf.SmoothStep(0f, 1f, ny));

                // 2) Üst cam parlaklığı (yumuşak alt kenarlı, en üstte hafif söner)
                float gloss = Mathf.SmoothStep(0.50f, 0.64f, ny) * (1f - Mathf.SmoothStep(0.95f, 1.0f, ny));
                col = Color.Lerp(col, glossC, gloss * 0.5f);

                // 3) Alt 3D dudak / temas gölgesi
                float lip = 1f - Mathf.SmoothStep(0f, 0.13f, ny);
                col = Color.Lerp(col, botLip, lip * 0.6f);

                // 4) Üst kenar ışığı (en üst ~5px)
                float rim = 1f - Mathf.SmoothStep(0f, 5f, (h - 1 - y));
                col = Color.Lerp(col, rimT, rim * 0.55f);

                // 5) Kenar bevel (içe doğru hafif koyulaşma)
                float bevel = 1f - Mathf.Clamp01(dEdge / 8f);
                col = Color.Lerp(col, Color.black, bevel * 0.12f);

                px[y * w + x] = new Color(col.r, col.g, col.b, aa);
            }
            tex.SetPixels(px); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateCircle(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float r = size / 2f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    pixels[y * size + x] = new Color(1, 1, 1, a);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateSoftShadow(string name, int size, int innerRadius, int innerW, int blur)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            int margin = (size - innerW * 2 - innerRadius * 2) / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Max(0, Mathf.Max(margin - x, x - (size - margin - 1)));
                    int dy = Mathf.Max(0, Mathf.Max(margin - y, y - (size - margin - 1)));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d / blur) * 0.35f;
                    pixels[y * size + x] = new Color(0, 0, 0, a);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateGradientVertical(string name, int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                float v = Mathf.Lerp(1.0f, 0.78f, t);
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = new Color(v, v, v, 1f);
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateRing(string name, int size, int outerInsetFromEdge, int thickness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float outerR = (size / 2f) - outerInsetFromEdge / 4f;
            float innerR = outerR - thickness;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float aOuter = Mathf.Clamp01(outerR - d + 0.5f);
                    float aInner = Mathf.Clamp01(d - innerR + 0.5f);
                    float a = Mathf.Min(aOuter, aInner);
                    pixels[y * size + x] = new Color(1, 1, 1, a);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateRadialGlow(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float maxR = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float t = Mathf.Clamp01(d / maxR);
                    float a = Mathf.Pow(1f - t, 2.4f);
                    pixels[y * size + x] = new Color(1, 1, 1, a);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateTapIcon(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float innerR = size * 0.22f;
            float midR   = size * 0.34f;
            float outerR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    Color c = Color.clear;
                    if (d < innerR)
                    {
                        c = new Color(1, 1, 1, 1);
                    }
                    else if (d < midR)
                    {
                        float t = (d - innerR) / (midR - innerR);
                        c = new Color(1, 1, 1, 1f - t * 0.3f);
                    }
                    else if (d < outerR)
                    {
                        float t = (d - midR) / (outerR - midR);
                        float ring = Mathf.Sin(t * Mathf.PI);
                        c = new Color(1, 1, 1, ring * 0.45f);
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateStone(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float baseR = size * 0.42f;
            var rng = new System.Random(42);
            int seedCount = 8;
            float[] seedAngles = new float[seedCount];
            float[] seedOffsets = new float[seedCount];
            for (int i = 0; i < seedCount; i++)
            {
                seedAngles[i] = (float)rng.NextDouble() * Mathf.PI * 2;
                seedOffsets[i] = ((float)rng.NextDouble() - 0.5f) * size * 0.05f;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    float lump = 0f;
                    for (int i = 0; i < seedCount; i++)
                    {
                        float diff = Mathf.Abs(Mathf.DeltaAngle(angle * Mathf.Rad2Deg, seedAngles[i] * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                        lump += Mathf.Max(0f, 1f - diff / 0.6f) * seedOffsets[i];
                    }
                    float r = baseR + lump;

                    if (dist <= r + 1f)
                    {
                        float t = Mathf.Clamp01(dist / r);
                        Color baseCol = Color.Lerp(new Color(0.68f, 0.66f, 0.62f), new Color(0.32f, 0.30f, 0.30f), t);
                        if (dy > 0)
                            baseCol = Color.Lerp(baseCol, new Color(0.9f, 0.88f, 0.85f), Mathf.Pow((dy / size), 1.5f) * 0.4f);
                        float edgeAA = Mathf.Clamp01(r - dist + 0.5f);
                        baseCol.a = edgeAA;
                        pixels[y * size + x] = baseCol;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateGameFrame(string name, int size, int borderThickness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            int radius = 48;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cx = x < radius ? radius : (x > size - radius - 1 ? size - radius - 1 : x);
                    int cy = y < radius ? radius : (y > size - radius - 1 ? size - radius - 1 : y);
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    float outerAlpha = Mathf.Clamp01(radius - d + 0.5f);
                    float innerAlpha = Mathf.Clamp01(radius - borderThickness - d + 0.5f);

                    if (outerAlpha <= 0)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (innerAlpha <= 0)
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, outerAlpha * 0.85f);
                    }
                    else
                    {
                        float fillAlpha = Mathf.Lerp(0.85f, 0.18f, innerAlpha);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, fillAlpha);
                    }
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        /// <summary>
        /// Cam panel tarzı yuvarlatılmış kare hücre tile'ı — parlak/net kenar
        /// + çok faint iç dolgu. Balon dairesi tile'ın iç kısmını kapatır,
        /// sadece kare köşeleri ve dış kenarı görünür (referans tasarımı gibi).
        /// </summary>
        private static void CreateCellTile(string name, int size, int cornerRadius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            int r = cornerRadius;
            // Parlak dış halka için yarı-genişlik
            const float borderBand = 4.0f;   // 4 px parlak kenar bandı
            const float fadeBand = 6.0f;     // 6 px yumuşak geçiş

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cx = x < r ? r : (x > size - r - 1 ? size - r - 1 : x);
                    int cy = y < r ? r : (y > size - r - 1 ? size - r - 1 : y);
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    float outerAlpha = Mathf.Clamp01(r - d + 0.5f);
                    if (outerAlpha <= 0f) { pixels[y * size + x] = Color.clear; continue; }

                    float edgeDist = (r - d); // kenardan içeri olan mesafe
                    float alpha;

                    if (edgeDist <= borderBand)
                    {
                        // Parlak crisp kenar (cam panel border)
                        alpha = 0.75f * outerAlpha;
                    }
                    else if (edgeDist <= borderBand + fadeBand)
                    {
                        // Border'dan içe yumuşak geçiş — tamamen şeffafa
                        float t = (edgeDist - borderBand) / fadeBand;
                        alpha = Mathf.Lerp(0.75f, 0f, t);
                    }
                    else
                    {
                        // İç tamamen şeffaf — balonların üstünde sızıntı olmasın
                        alpha = 0f;
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateIceTile(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float r = size * 0.45f;
            int radius = 20;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - (int)cx);
                    int dy = Mathf.Abs(y - (int)cy);
                    int halfSize = (int)(r);
                    int relX = dx - (halfSize - radius);
                    int relY = dy - (halfSize - radius);
                    bool inRoundedRect = false;
                    if (dx <= halfSize && dy <= halfSize)
                    {
                        if (relX <= 0 || relY <= 0) inRoundedRect = true;
                        else
                        {
                            float d = Mathf.Sqrt(relX * relX + relY * relY);
                            if (d <= radius) inRoundedRect = true;
                        }
                    }
                    if (!inRoundedRect) { pixels[y * size + x] = Color.clear; continue; }

                    float noise = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                    float n2 = Mathf.PerlinNoise(x * 0.12f + 100f, y * 0.12f + 100f);
                    Color baseCol = Color.Lerp(new Color(0.75f, 0.92f, 1f), new Color(0.5f, 0.7f, 0.95f), noise);
                    baseCol.r += (n2 - 0.5f) * 0.15f;
                    baseCol.g += (n2 - 0.5f) * 0.10f;
                    baseCol.b += (n2 - 0.5f) * 0.05f;
                    baseCol.a = 0.85f;

                    bool border = (relX > -3 && relY > -3) || Mathf.Abs(dx - halfSize) < 3 || Mathf.Abs(dy - halfSize) < 3;
                    if (border) baseCol = Color.Lerp(baseCol, Color.white, 0.4f);

                    pixels[y * size + x] = baseCol;
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateHammerIcon(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool head = (x > size * 0.30f && x < size * 0.85f && y > size * 0.50f && y < size * 0.85f);
                bool handle = (x > size * 0.55f && x < size * 0.68f && y > size * 0.15f && y < size * 0.55f);
                bool insideRound = false;
                if (head)
                {
                    float dx = Mathf.Max(0, x - (size * 0.80f));
                    float dy = Mathf.Max(0, (size * 0.55f) - y, y - (size * 0.80f));
                    if (dx * dx + dy * dy < 60f) insideRound = true;
                    else if (x <= size * 0.80f && y >= size * 0.55f && y <= size * 0.80f) insideRound = true;
                }
                if (insideRound || head || handle)
                    pixels[y * size + x] = Color.white;
                else
                    pixels[y * size + x] = Color.clear;
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void CreateShuffleIcon(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            for (int t = 0; t < 200; t++)
            {
                float u = t / 199f;
                float x1 = Mathf.Lerp(size * 0.15f, size * 0.85f, u);
                float y1 = Mathf.Lerp(size * 0.25f, size * 0.75f, u) + Mathf.Sin(u * Mathf.PI) * size * 0.05f;
                float x2 = Mathf.Lerp(size * 0.15f, size * 0.85f, u);
                float y2 = Mathf.Lerp(size * 0.75f, size * 0.25f, u) - Mathf.Sin(u * Mathf.PI) * size * 0.05f;
                FillDot(pixels, size, (int)x1, (int)y1, 6, Color.white);
                FillDot(pixels, size, (int)x2, (int)y2, 6, Color.white);
            }
            DrawArrow(pixels, size, (int)(size * 0.85f), (int)(size * 0.25f), 1, -1);
            DrawArrow(pixels, size, (int)(size * 0.85f), (int)(size * 0.75f), 1, 1);

            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void FillDot(Color[] pixels, int size, int cx, int cy, int radius, Color c)
        {
            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx, y = cy + dy;
                if (x < 0 || y < 0 || x >= size || y >= size) continue;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= radius)
                {
                    float a = Mathf.Clamp01(radius - dist + 0.5f);
                    var existing = pixels[y * size + x];
                    pixels[y * size + x] = new Color(c.r, c.g, c.b, Mathf.Max(existing.a, a));
                }
            }
        }

        private static void DrawArrow(Color[] pixels, int size, int cx, int cy, int dx, int dy)
        {
            int armLen = (int)(size * 0.12f);
            for (int i = 0; i < armLen; i++)
            {
                FillDot(pixels, size, cx - i * dx, cy - i, 4, Color.white);
                FillDot(pixels, size, cx - i, cy + i * dy * 0, 4, Color.white);
                FillDot(pixels, size, cx - i * dx, cy + i * dy, 4, Color.white);
            }
        }

        private static void CreatePlusIcon(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool horiz = (x > size * 0.18f && x < size * 0.82f && y > size * 0.42f && y < size * 0.58f);
                bool vert = (y > size * 0.18f && y < size * 0.82f && x > size * 0.42f && x < size * 0.58f);
                pixels[y * size + x] = (horiz || vert) ? Color.white : Color.clear;
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // ---- Ana menü şerit buton ikonları ----------------------------------
        // Tümü beyaz siluet; UI'da Image.color ile tint'lenir. 4x supersample ile
        // yumuşak (anti-aliased) kenar. Normalize koordinat: nx,ny ∈ [0,1], merkez (0.5,0.5).

        private static void CreateIconAA(string name, int size, System.Func<float, float, bool> inside)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int SS = 4;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int hits = 0;
                    for (int sy = 0; sy < SS; sy++)
                    for (int sx = 0; sx < SS; sx++)
                    {
                        float nx = (x + (sx + 0.5f) / SS) / size;
                        float ny = (y + (sy + 0.5f) / SS) / size;
                        if (inside(nx, ny)) hits++;
                    }
                    float a = hits / (float)(SS * SS);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static bool PointInTri(float px, float py,
            float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = (px - bx) * (ay - by) - (ax - bx) * (py - by);
            float d2 = (px - cx) * (by - cy) - (bx - cx) * (py - cy);
            float d3 = (px - ax) * (cy - ay) - (cx - ax) * (py - ay);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        // OYNA → sağa bakan üçgen (play)
        private static void CreatePlayIcon(string name, int size)
        {
            CreateIconAA(name, size, (x, y) =>
                PointInTri(x, y, 0.33f, 0.24f, 0.33f, 0.76f, 0.78f, 0.50f));
        }

        // LEVEL SEÇ → 2x2 kare ızgara (level grid)
        private static void CreateGridIcon(string name, int size)
        {
            CreateIconAA(name, size, (x, y) =>
            {
                bool inX1 = x >= 0.20f && x <= 0.45f;
                bool inX2 = x >= 0.55f && x <= 0.80f;
                bool inY1 = y >= 0.20f && y <= 0.45f;
                bool inY2 = y >= 0.55f && y <= 0.80f;
                return (inX1 || inX2) && (inY1 || inY2);
            });
        }

        // AYARLAR → 8 dişli çark (gear), merkezde delik
        private static void CreateGearIcon(string name, int size)
        {
            CreateIconAA(name, size, (x, y) =>
            {
                float dx = x - 0.5f, dy = y - 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < 0.135f) return false;   // merkez delik
                if (d > 0.46f) return false;
                float ang = Mathf.Atan2(dy, dx);
                const int teeth = 8;
                float seg = (ang / (2f * Mathf.PI) + 1f) * teeth;
                float frac = seg - Mathf.Floor(seg);
                bool onTooth = (frac > 0.27f && frac < 0.73f);
                float outer = onTooth ? 0.45f : 0.33f;
                return d <= outer;
            });
        }

        // ÇIKIŞ → güç (power) sembolü: üstte boşluklu halka + dikey çubuk
        private static void CreatePowerIcon(string name, int size)
        {
            CreateIconAA(name, size, (x, y) =>
            {
                float dx = x - 0.5f, dy = y - 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);     // +pi/2 = yukarı
                bool ring = (d > 0.25f && d < 0.355f);
                float a = ang - Mathf.PI / 2f;
                while (a > Mathf.PI) a -= 2f * Mathf.PI;
                while (a < -Mathf.PI) a += 2f * Mathf.PI;
                bool inGap = Mathf.Abs(a) < 0.46f;   // üstteki açıklık
                bool ringPart = ring && !inGap;
                bool stem = Mathf.Abs(dx) < 0.052f && dy > 0.0f && dy < 0.40f;
                return ringPart || stem;
            });
        }

        private static void CreateStar(string name, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Vector2 c = new Vector2(size / 2f, size / 2f);
            float outerR = size / 2f - 4f;
            float innerR = outerR * 0.5f;
            int points = 5;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    float angle = Mathf.Atan2(p.y, p.x);
                    float dist = p.magnitude;

                    float a = (angle + Mathf.PI / 2f) * points / (2 * Mathf.PI);
                    a = a - Mathf.Floor(a);
                    float segT = a < 0.5f ? a * 2f : (1f - a) * 2f;
                    float starR = Mathf.Lerp(innerR, outerR, segT);

                    if (dist < starR)
                    {
                        float edgeAA = Mathf.Clamp01(starR - dist + 0.5f);
                        pixels[y * size + x] = new Color(1, 1, 1, edgeAA);
                    }
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            File.WriteAllBytes($"{SpriteFolder}/{name}.png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void ConfigureSprite(string name, int ppu, Vector4 border)
        {
            var path = $"{SpriteFolder}/{name}.png";
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Bilinear;
            imp.alphaIsTransparency = true;
            // Pivot'u explicit center yap (kullanıcı sprite'ları için kritik)
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            imp.SetTextureSettings(settings);
            if (border != Vector4.zero) imp.spriteBorder = border;
            imp.SaveAndReimport();
        }

        private static float RoundedRectAlpha(int px, int py, int w, int h, int radius)
        {
            int cx = px < radius ? radius : (px > w - radius - 1 ? w - radius - 1 : px);
            int cy = py < radius ? radius : (py > h - radius - 1 ? h - radius - 1 : py);
            float dx = px - cx;
            float dy = py - cy;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(radius - d + 0.5f);
        }
    }
}
#endif
