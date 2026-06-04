using System.Collections;
using TMPro;
using UnityEngine;

namespace BalloonPop.Effects
{
    public class BoomEffect : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float duration = 0.7f;
        [SerializeField] private float startScale = 0.3f;
        [SerializeField] private float peakScale = 1.15f;   // 1.6 → 1.15 (taşma önleme)
        [SerializeField] private float floatUpSpeed = 1.2f;

        public static BoomEffect Spawn(GameObject prefab, Vector3 worldPos, string label = "BOOM!")
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, worldPos + Vector3.back * 0.5f, Quaternion.identity);
            var fx = go.GetComponent<BoomEffect>();
            if (fx != null) fx.Play(label);
            return fx;
        }

        public void Play(string label)
        {
            if (text != null) text.text = label;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float t = 0f;
            Vector3 startS = Vector3.one * startScale;
            Vector3 peakS = Vector3.one * peakScale;
            Color baseCol = text != null ? text.color : Color.white;

            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);

                if (u < 0.25f)
                {
                    float p = u / 0.25f;
                    transform.localScale = Vector3.LerpUnclamped(startS, peakS, EaseOutBack(p));
                }
                else
                {
                    transform.localScale = peakS;
                    float fade = 1f - (u - 0.25f) / 0.75f;
                    if (text != null) text.color = new Color(baseCol.r, baseCol.g, baseCol.b, fade);
                    transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
                }
                yield return null;
            }
            Destroy(gameObject);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float k = t - 1f;
            return 1f + c3 * k * k * k + c1 * k * k;
        }
    }
}
