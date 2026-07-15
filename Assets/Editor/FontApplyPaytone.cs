using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using System.Linq;

// One-shot tool: build a TMP SDF font asset from PaytoneOne.ttf and retarget
// every TMP text in the main menu to it (dropping the faux-bold style).
// Run headless:  Unity -batchmode -quit -executeMethod FontApplyPaytone.Run
public static class FontApplyPaytone
{
    const string TtfPath   = "Assets/Fonts/PaytoneOne.ttf";
    const string SdfPath   = "Assets/Fonts/PaytoneOne SDF.asset";
    const string ScenePath = "Assets/Scenes/MainMenu.unity";
    const string L         = "[FontApplyPaytone]";

    [MenuItem("Tools/Fonts/Apply Paytone One to Main Menu")]
    public static void Run()
    {
        try
        {
            // 1) Make sure the TTF is imported as a Font.
            AssetDatabase.ImportAsset(TtfPath, ImportAssetOptions.ForceUpdate);
            var font = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
            if (font == null) { Debug.LogError($"{L} TTF not found at {TtfPath}"); return; }

            // 2) Build the SDF font asset (or reuse if it already exists).
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic, true);
                fontAsset.name = "PaytoneOne SDF";

                AssetDatabase.CreateAsset(fontAsset, SdfPath);

                // Persist atlas texture + material as sub-assets of the font asset.
                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                {
                    fontAsset.atlasTextures[0].name = "PaytoneOne Atlas";
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                }
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = "PaytoneOne SDF Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"{L} Created SDF font asset at {SdfPath}");
            }
            else Debug.Log($"{L} Reusing existing SDF font asset {SdfPath}");

            // 3) Retarget every TMP text in the main menu scene.
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int changed = 0, unbolded = 0;
            foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (!t.gameObject.scene.IsValid() || t.gameObject.scene != scene) continue;
                t.font = fontAsset;                          // TMP also sets the matching default material
                if ((t.fontStyle & FontStyles.Bold) != 0) { t.fontStyle &= ~FontStyles.Bold; unbolded++; }
                EditorUtility.SetDirty(t);
                changed++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"{L} DONE. Retargeted {changed} TMP texts ({unbolded} un-bolded) in {ScenePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{L} FAILED: {e}");
        }
    }
}
