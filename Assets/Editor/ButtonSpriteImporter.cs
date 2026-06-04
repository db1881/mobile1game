#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    /// <summary>
    /// button_primary, button_icon_socket, button_shine için import ayarlarını uygular.
    /// 9-slice border, PPU, alpha settings.
    /// </summary>
    public static class ButtonSpriteImporter
    {
        public static void BatchConfigure()
        {
            Configure("Assets/Sprites/button_primary.png", new Vector4(90, 70, 90, 70), 256);
            Configure("Assets/Sprites/button_icon_socket.png", Vector4.zero, 256);
            Configure("Assets/Sprites/button_shine.png", new Vector4(80, 0, 80, 0), 256);
        }

        [MenuItem("BalloonPop/Configure Button Sprites")]
        public static void ConfigureMenu() => BatchConfigure();

        private static void Configure(string path, Vector4 border, int ppu)
        {
            if (!File.Exists(path)) return;
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.spritePixelsPerUnit = ppu;
            imp.spriteBorder = border;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
    }
}
#endif
