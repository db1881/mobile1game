using System.Collections;
using UnityEngine;

namespace BalloonPop.Effects
{
    public class ConfettiRain : MonoBehaviour
    {
        [SerializeField] private Sprite particleSprite;
        [SerializeField] private Sprite starSprite;
        [SerializeField] private int particleCount = 180;
        [SerializeField] private float duration = 4f;
        [SerializeField] private float spread = 6f;
        [SerializeField] private float verticalSpeed = -3f;
        [SerializeField] private float horizontalSpeed = 1.5f;
        [SerializeField] private int sortingOrder = 80;

        private static readonly Color[] Palette = {
            new Color(1.00f, 0.28f, 0.34f),
            new Color(0.31f, 0.80f, 0.92f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(1.00f, 0.85f, 0.24f),
            new Color(0.65f, 0.37f, 0.92f),
            new Color(1.00f, 0.55f, 0.26f),
            new Color(1.00f, 0.42f, 0.62f),
            new Color(1.00f, 0.95f, 0.45f), // altın
            new Color(0.95f, 0.95f, 1.00f), // beyaz parıltı
        };

        private void OnEnable() => StartCoroutine(Burst());

        private IEnumerator Burst()
        {
            float spawnDuration = duration * 0.6f;
            float elapsed = 0f;
            int spawned = 0;
            while (elapsed < spawnDuration && spawned < particleCount)
            {
                int toSpawn = Mathf.CeilToInt(particleCount * Time.deltaTime / spawnDuration);
                for (int i = 0; i < toSpawn && spawned < particleCount; i++)
                {
                    SpawnOne();
                    spawned++;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void SpawnOne()
        {
            var go = new GameObject("Confetti");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            // %20 yıldız, %80 normal — boyut da çeşitli
            bool isStar = starSprite != null && Random.value < 0.20f;
            float size = isStar ? Random.Range(36f, 56f) : Random.Range(18f, 32f);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.sprite = isStar ? starSprite : particleSprite;
            img.color = isStar ? new Color(1f, 0.95f, 0.35f) : Palette[Random.Range(0, Palette.Length)];
            img.raycastTarget = false;
            img.preserveAspect = true;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(Random.Range(-Screen.width * 0.5f, Screen.width * 0.5f), 100);

            float life = 3.0f + Random.value * 1.5f;
            float horizDir = Random.Range(-1f, 1f);
            StartCoroutine(MoveConfetti(rt, img, life, horizDir));
        }

        private IEnumerator MoveConfetti(RectTransform rt, UnityEngine.UI.Image img, float life, float horizDir)
        {
            float t = 0f;
            float startAlpha = img.color.a;
            float rotSpeed = Random.Range(-360f, 360f);
            float swayAmp = Random.Range(40f, 100f);
            float swaySpeed = Random.Range(1f, 3f);
            Vector2 start = rt.anchoredPosition;
            while (t < life && rt != null)
            {
                t += Time.deltaTime;
                float u = t / life;
                rt.anchoredPosition = new Vector2(
                    start.x + Mathf.Sin(t * swaySpeed) * swayAmp + horizDir * t * 60f,
                    start.y - Mathf.Pow(u, 1.3f) * Screen.height * 1.2f);
                rt.localRotation = Quaternion.Euler(0, 0, rotSpeed * t);
                if (u > 0.7f)
                {
                    var c = img.color;
                    c.a = startAlpha * (1f - (u - 0.7f) / 0.3f);
                    img.color = c;
                }
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }
    }
}
