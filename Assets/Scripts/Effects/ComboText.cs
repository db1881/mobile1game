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
            // Neon: bright white face, tier-colored glow that escalates cyan -> lime -> orange -> magenta.
            string label;
            Color glow;
            if (chain >= 5)      { label = $"INANILMAZ! {chain}x"; glow = new Color(1f, 0.18f, 0.61f); }   // magenta
            else if (chain >= 4) { label = $"SÜPER! {chain}x";     glow = new Color(1f, 0.54f, 0.24f); }   // orange
            else if (chain >= 3) { label = $"HARIKA! {chain}x";    glow = new Color(0.30f, 1f, 0.64f); }   // lime
            else                 { label = $"COMBO! {chain}x";     glow = new Color(0.20f, 0.88f, 1f); }   // cyan

            if (text != null)
            {
                text.text = label;
                text.color = Color.white;
                var mat = text.fontMaterial; // per-instance material
                if (mat != null && mat.HasProperty(ShaderUtilities.ID_GlowColor))
                {
                    mat.SetColor(ShaderUtilities.ID_GlowColor, glow);
                    // stronger glow for higher chains
                    mat.SetFloat(ShaderUtilities.ID_GlowPower, Mathf.Clamp01(0.8f + (chain - 2) * 0.08f));
                }
            }

            float t = 0f;
            Vector3 baseScale = Vector3.one;
            while (t < showDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / showDuration);
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Sin(u * Mathf.PI);
                float bigger = 0.4f + (chain - 2) * 0.1f;
                transform.localScale = baseScale * (1f + bigger * Mathf.Sin(u * Mathf.PI));
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 0;
            transform.localScale = baseScale;
            current = null;
        }
    }
}
