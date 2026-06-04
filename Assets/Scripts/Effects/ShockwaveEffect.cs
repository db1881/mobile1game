using System.Collections;
using UnityEngine;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShockwaveEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float startScale = 0.3f;
        [SerializeField] private float endScale = 4.0f;

        private SpriteRenderer sr;

        public static ShockwaveEffect Spawn(GameObject prefab, Vector3 pos, Color color, float scale = 4f)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, pos + Vector3.back * 0.3f, Quaternion.identity);
            var fx = go.GetComponent<ShockwaveEffect>();
            if (fx != null)
            {
                fx.endScale = scale;
                fx.Play(color);
            }
            return fx;
        }

        private void Awake() => sr = GetComponent<SpriteRenderer>();

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
                transform.localScale = Vector3.LerpUnclamped(startS, endS, EaseOutCubic(u));
                sr.color = new Color(startColor.r, startColor.g, startColor.b, (1f - u) * 0.7f);
                yield return null;
            }
            Destroy(gameObject);
        }

        private static float EaseOutCubic(float t)
        {
            float k = 1f - t;
            return 1f - k * k * k;
        }
    }
}
