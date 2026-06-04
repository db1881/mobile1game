#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    /// <summary>
    /// GPT'den gelen görsellerin beyaz arka planını alpha'ya çevirir.
    /// Genel kural: piksel beyaza ne kadar yakınsa o kadar şeffaf.
    /// </summary>
    public static class PopSpriteAlphaFix
    {
        private static readonly string[] Targets = {
            "pop_chunk", "pop_glow", "pop_sparkle", "pop_ring", "pop_flash",
            "match_outline", "confetti_strip"
        };

        [MenuItem("BalloonPop/Fix Pop Sprite Alpha")]
        public static void Fix()
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
            Debug.Log($"[PopSpriteAlphaFix] {processed} sprite işlendi.");
            EditorUtility.DisplayDialog("Done", $"{processed} pop sprite alpha düzeltildi.", "OK");
        }

        public static void BatchFix() => Fix();

        private static void ProcessOne(string path)
        {
            // Texture'ı okunabilir hale getir
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

            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                // Beyaza uzaklık (0=tam beyaz, 1.7=siyah/koyu)
                float dist = Mathf.Sqrt((1f - c.r) * (1f - c.r) + (1f - c.g) * (1f - c.g) + (1f - c.b) * (1f - c.b));
                float a;
                if (dist < 0.04f) a = 0f;                              // tam beyaz → şeffaf
                else if (dist < 0.20f) a = (dist - 0.04f) / 0.16f;     // yumuşak geçiş
                else a = 1f;                                            // koyu → tam katı
                px[i] = new Color(c.r, c.g, c.b, a * c.a);
            }

            // Yeni PNG yaz
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
            // 9-slice border yalnızca match_outline için
            if (path.EndsWith("match_outline.png"))
            {
                imp.spritePixelsPerUnit = 256;
                imp.spriteBorder = new Vector4(80, 80, 80, 80);
            }
            else
            {
                imp.spritePixelsPerUnit = 256;
            }
            imp.SaveAndReimport();
        }
    }
}
#endif
