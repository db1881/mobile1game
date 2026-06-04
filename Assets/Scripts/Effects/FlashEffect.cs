using System.Collections;
using UnityEngine;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlashEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float startScale = 0.2f;
        [SerializeField] private float endScale = 2.4f;

        private SpriteRenderer sr;

        public static FlashEffect Spawn(GameObject prefab, Vector3 pos, Color color, float scale = 2.4f, float dur = 0.3f)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, pos + Vector3.back * 0.4f, Quaternion.identity);
            var fx = go.GetComponent<FlashEffect>();
            if (fx != null)
            {
                fx.endScale = scale;
                fx.duration = dur;
                fx.Play(color);
            }
            return fx;
        }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void Play(Color color)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.color = color;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float t = 0f;
            Color startColor = sr.color;
            Vector3 startS = Vector3.one * startScale;
            Vector3 endS = Vector3.one * endScale;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                transform.localScale = Vector3.LerpUnclamped(startS, endS, EaseOutQuart(u));
                sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - u);
                yield return null;
            }
            Destroy(gameObject);
        }

        private static float EaseOutQuart(float t)
        {
            float k = 1f - t;
            return 1f - k * k * k * k;
        }
    }
}
