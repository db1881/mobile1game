using System.Collections;
using UnityEngine;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HandIcon : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.9f;
        [SerializeField] private float pauseDuration = 0.5f;
        [SerializeField] private float bobAmount = 0.05f;

        private SpriteRenderer sr;
        private bool active;
        private Vector3 fromPos;
        private Vector3 toPos;

        public static HandIcon Spawn(GameObject prefab, Vector3 a, Vector3 b)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, a, Quaternion.identity);
            var h = go.GetComponent<HandIcon>();
            if (h != null) h.AnimateSwap(a, b);
            return h;
        }

        private void Awake() => sr = GetComponent<SpriteRenderer>();

        public void AnimateSwap(Vector3 a, Vector3 b)
        {
            fromPos = a + Vector3.back * 0.5f;
            toPos = b + Vector3.back * 0.5f;
            active = true;
            StartCoroutine(Loop());
        }

        public void Hide()
        {
            active = false;
            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator Loop()
        {
            while (active)
            {
                yield return Animate(fromPos, toPos);
                yield return new WaitForSeconds(pauseDuration);
                yield return Animate(toPos, fromPos);
                yield return new WaitForSeconds(pauseDuration);
            }
        }

        private IEnumerator Animate(Vector3 a, Vector3 b)
        {
            float t = 0f;
            while (t < moveDuration && active)
            {
                t += Time.deltaTime;
                float u = Mathf.SmoothStep(0f, 1f, t / moveDuration);
                var p = Vector3.Lerp(a, b, u);
                p.y += Mathf.Sin(t * 8f) * bobAmount;
                transform.position = p;
                yield return null;
            }
        }

        private IEnumerator FadeOutAndDestroy()
        {
            float t = 0f;
            float dur = 0.3f;
            Color start = sr.color;
            while (t < dur)
            {
                t += Time.deltaTime;
                sr.color = new Color(start.r, start.g, start.b, 1f - t / dur);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
