using System.Collections;
using TMPro;
using UnityEngine;
using BalloonPop.Core;

namespace BalloonPop.Effects
{
    public class ComboText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float showDuration = 1.0f;

        private Coroutine current;

        private void OnEnable()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0;
            GameEvents.OnComboChain += OnCombo;
        }

        private void OnDisable()
        {
            GameEvents.OnComboChain -= OnCombo;
        }

        private void OnCombo(int chain)
        {
            if (chain < 2) return;
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(Show(chain));
        }

        private IEnumerator Show(int chain)
        {
            // Vivid tier color used for BOTH the text face and its neon glow,
            // escalating cyan -> green -> orange -> magenta with the chain.
            string label;
            Color color;
            if (chain >= 5)      { label = $"INANILMAZ! {chain}x"; color = new Color(1f, 0.22f, 0.68f); }   // magenta
            else if (chain >= 4) { label = $"SÜPER! {chain}x";     color = new Color(1f, 0.60f, 0.12f); }   // orange
            else if (chain >= 3) { label = $"HARIKA! {chain}x";    color = new Color(0.35f, 1f, 0.42f); }   // green
            else                 { label = $"COMBO! {chain}x";     color = new Color(0.25f, 0.80f, 1f); }   // cyan

            if (text != null)
            {
                text.text = label;
                text.color = color;                    // solid, vivid COLORED face (was flat white)
                var mat = text.fontMaterial;           // per-instance material
                if (mat != null)
                {
                    // dark outline so the bright text stays crisp over the busy board
                    if (mat.HasProperty(ShaderUtilities.ID_OutlineColor))
                    {
                        mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.04f, 0.02f, 0.10f, 1f));
                        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.22f);
                        mat.EnableKeyword("OUTLINE_ON");
                    }
                    // matching neon glow, escalating with the chain
                    if (mat.HasProperty(ShaderUtilities.ID_GlowColor))
                    {
                        mat.SetColor(ShaderUtilities.ID_GlowColor, color);
                        mat.SetFloat(ShaderUtilities.ID_GlowPower, Mathf.Clamp01(1.0f + (chain - 2) * 0.05f));
                    }
                }
                text.UpdateMeshPadding();
            }

            float t = 0f;
            Vector3 baseScale = Vector3.one;
            while (t < showDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / showDuration);
                // Alpha: fast fade-in, HOLD fully opaque, quick fade-out
                // (was a brief Sin pulse that only hit full opacity for an instant).
                float a;
                if (u < 0.12f)      a = u / 0.12f;
                else if (u > 0.78f) a = 1f - (u - 0.78f) / 0.22f;
                else                a = 1f;
                if (canvasGroup != null) canvasGroup.alpha = a;
                // Punchy pop-in that settles to normal size.
                float pop = 1f + (0.35f + (chain - 2) * 0.08f) * Mathf.Exp(-u * 6f);
                transform.localScale = baseScale * pop;
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 0;
            transform.localScale = baseScale;
            current = null;
        }
    }
}
