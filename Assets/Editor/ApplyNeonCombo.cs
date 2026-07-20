using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// One-shot: build a glow-capable neon material from the Paytone SDF font and
// assign it (plus the Paytone font) to the ComboText's TMP label in Game.unity.
// Run headless:  Unity -batchmode -quit -executeMethod ApplyNeonCombo.Run
public static class ApplyNeonCombo
{
    const string SdfPath     = "Assets/Fonts/PaytoneOne SDF.asset";
    const string NeonMatPath = "Assets/Fonts/PaytoneOne SDF Neon.mat";
    const string ScenePath   = "Assets/Scenes/Game.unity";
    const string L           = "[ApplyNeonCombo]";

    [MenuItem("Tools/Fonts/Apply Neon Combo")]
    public static void Run()
    {
        try
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfPath);
            if (font == null) { Debug.LogError($"{L} Paytone SDF not found at {SdfPath}"); return; }

            // 1) Neon material: copy the font's SDF material, swap to the glow-capable
            //    Distance Field shader, enable + configure glow (padding bakes from this).
            var neon = AssetDatabase.LoadAssetAtPath<Material>(NeonMatPath);
            if (neon == null)
            {
                var df = Shader.Find("TextMeshPro/Distance Field");
                if (df == null) { Debug.LogError($"{L} 'TextMeshPro/Distance Field' shader not found"); return; }

                neon = new Material(font.material); // carries _MainTex + SDF params
                neon.shader = df;                   // mobile -> glow-capable
                neon.name = "PaytoneOne SDF Neon";
                AssetDatabase.CreateAsset(neon, NeonMatPath);
                Debug.Log($"{L} Created neon material at {NeonMatPath}");
            }
            else Debug.Log($"{L} Reusing neon material {NeonMatPath}");

            // Always (re)configure glow. The Distance Field shader gates glow behind the
            // GLOW_ON keyword — setting the float props alone renders nothing.
            neon.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            neon.SetColor(ShaderUtilities.ID_GlowColor, new Color(0.20f, 0.88f, 1f, 1f));
            neon.SetFloat(ShaderUtilities.ID_GlowPower, 1f);
            neon.SetFloat(ShaderUtilities.ID_GlowOuter, 0.9f);
            neon.SetFloat(ShaderUtilities.ID_GlowInner, 0.1f);
            neon.SetFloat(ShaderUtilities.ID_GlowOffset, 0f);
            neon.EnableKeyword("GLOW_ON");                 // <-- the fix
            EditorUtility.SetDirty(neon);
            AssetDatabase.SaveAssets();

            // 2) Wire the combo label(s) in Game.unity.
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int wired = 0;
            foreach (var ct in Resources.FindObjectsOfTypeAll<BalloonPop.Effects.ComboText>())
            {
                if (ct.gameObject.scene != scene) continue;
                var so = new SerializedObject(ct);
                var tmp = so.FindProperty("text")?.objectReferenceValue as TMP_Text;
                if (tmp == null) tmp = ct.GetComponentInChildren<TMP_Text>(true);
                if (tmp == null) continue;

                tmp.font = font;
                tmp.fontSharedMaterial = neon;
                tmp.color = Color.white;
                tmp.UpdateMeshPadding();
                EditorUtility.SetDirty(tmp);
                wired++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"{L} DONE. Wired {wired} combo label(s) with neon material in {ScenePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{L} FAILED: {e}");
        }
    }
}
