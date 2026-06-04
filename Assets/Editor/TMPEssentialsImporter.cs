#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class TMPEssentialsImporter
    {
        [MenuItem("BalloonPop/Import TMP Essentials")]
        public static void ImportEssentials()
        {
            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            {
                Debug.Log("[BalloonPop] TMP Essentials already imported");
                return;
            }

            string[] candidates = new[]
            {
                "Library/PackageCache",
                Path.Combine(EditorApplication.applicationContentsPath, "Resources", "PackageManager", "ProjectTemplates")
            };

            string essentialsPath = null;
            foreach (var root in candidates)
            {
                if (!Directory.Exists(root)) continue;
                var found = Directory.GetFiles(root, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
                if (found.Length > 0) { essentialsPath = found[0]; break; }
            }

            if (essentialsPath == null)
            {
                Debug.LogError("[BalloonPop] TMP Essentials .unitypackage bulunamadı!");
                return;
            }

            Debug.Log($"[BalloonPop] TMP Essentials import ediliyor: {essentialsPath}");
            AssetDatabase.ImportPackage(essentialsPath, false);
            AssetDatabase.Refresh();
            Debug.Log("[BalloonPop] TMP Essentials import tamamlandı");
        }
    }
}
#endif
