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
            string label;
            Color color;
            if (chain >= 5)      { label = $"INANILMAZ! {chain}x"; color = new Color(1f, 0.3f, 0.7f); }
            else if (chain >= 4) { label = $"SÜPER! {chain}x"; color = new Color(1f, 0.5f, 0.2f); }
            else if (chain >= 3) { label = $"HARIKA! {chain}x"; color = new Color(1f, 0.83f, 0.24f); }
            else                 { label = $"COMBO! {chain}x"; color = new Color(0.31f, 0.80f, 0.92f); }

            if (text != null)
            {
                text.text = label;
                text.color = color;
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
