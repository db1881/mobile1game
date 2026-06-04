using System.Collections;
using TMPro;
using UnityEngine;

namespace BalloonPop.Effects
{
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float duration = 0.9f;
        [SerializeField] private float floatSpeed = 1.5f;

        public static ScorePopup Spawn(GameObject prefab, Vector3 worldPos, string label)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, worldPos + Vector3.back * 0.3f, Quaternion.identity);
            var sp = go.GetComponent<ScorePopup>();
            if (sp != null) sp.Play(label);
            return sp;
        }

        public void Play(string label)
        {
            if (text != null) text.text = label;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float t = 0f;
            Color baseCol = text != null ? text.color : Color.white;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 targetScale = Vector3.one;

            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);

                if (u < 0.2f)
                {
                    float p = u / 0.2f;
                    transform.localScale = Vector3.Lerp(startScale, targetScale, EaseOutBack(p));
                }
                else
                {
                    transform.localScale = targetScale;
                }
                transform.position += Vector3.up * floatSpeed * Time.deltaTime;

                if (u > 0.4f && text != null)
                {
                    float fade = 1f - (u - 0.4f) / 0.6f;
                    text.color = new Color(baseCol.r, baseCol.g, baseCol.b, fade);
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
