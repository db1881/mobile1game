using System.Collections.Generic;
using UnityEngine;

namespace BalloonPop.Effects
{
    public class AnimatedBackground : MonoBehaviour
    {
        [Header("Floating Balloons")]
        [SerializeField] private Sprite[] balloonSprites;
        [SerializeField] private int balloonCount = 9;     // 14 → 9 (mobil performans için)
        [SerializeField] private float minSpeed = 0.25f;
        [SerializeField] private float maxSpeed = 0.7f;
        [SerializeField] private float minScale = 0.18f;
        [SerializeField] private float maxScale = 0.40f;
        [SerializeField] private float swayAmount = 0.4f;
        [SerializeField] private float swaySpeed = 0.5f;

        [Header("Area")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float horizontalPadding = 1.5f;
        [SerializeField] private float spawnBelow = 2f;
        [SerializeField] private float despawnAbove = 2f;

        [Header("Render")]
        [SerializeField] private int sortingOrder = -10;
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField, Range(0f, 1f)] private float alpha = 0.45f;

        private class FloatingItem
        {
            public Transform tr;
            public SpriteRenderer sr;
            public float speed;
            public float swaySeed;
            public float swayPhase;
            public float baseX;
        }

        private readonly List<FloatingItem> items = new List<FloatingItem>();

        private void Start()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (balloonSprites == null || balloonSprites.Length == 0) return;
            for (int i = 0; i < balloonCount; i++) SpawnItem(true);
        }

        private void Update()
        {
            if (targetCamera == null) return;
            float topY = targetCamera.transform.position.y + targetCamera.orthographicSize + despawnAbove;
            float bottomY = targetCamera.transform.position.y - targetCamera.orthographicSize - spawnBelow;
            float leftX = targetCamera.transform.position.x - targetCamera.orthographicSize * targetCamera.aspect - horizontalPadding;
            float rightX = targetCamera.transform.position.x + targetCamera.orthographicSize * targetCamera.aspect + horizontalPadding;

            foreach (var it in items)
            {
                var pos = it.tr.position;
                pos.y += it.speed * Time.deltaTime;
                pos.x = it.baseX + Mathf.Sin(Time.time * swaySpeed + it.swaySeed) * swayAmount;
                it.tr.position = pos;

                if (pos.y > topY)
                {
                    it.baseX = Random.Range(leftX, rightX);
                    it.tr.position = new Vector3(it.baseX, bottomY, pos.z);
                    AssignRandom(it);
                }
            }
        }

        private void SpawnItem(bool atRandomY)
        {
            if (targetCamera == null) return;
            float topY = targetCamera.transform.position.y + targetCamera.orthographicSize;
            float bottomY = targetCamera.transform.position.y - targetCamera.orthographicSize - spawnBelow;
            float leftX = targetCamera.transform.position.x - targetCamera.orthographicSize * targetCamera.aspect - horizontalPadding;
            float rightX = targetCamera.transform.position.x + targetCamera.orthographicSize * targetCamera.aspect + horizontalPadding;

            var go = new GameObject("BgBalloon");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            sr.sortingLayerName = sortingLayerName;

            var it = new FloatingItem
            {
                tr = go.transform,
                sr = sr,
                baseX = Random.Range(leftX, rightX),
                swaySeed = Random.Range(0f, Mathf.PI * 2f)
            };
            float y = atRandomY ? Random.Range(bottomY, topY) : bottomY;
            go.transform.position = new Vector3(it.baseX, y, 5f);
            AssignRandom(it);
            items.Add(it);
        }

        private void AssignRandom(FloatingItem it)
        {
            it.speed = Random.Range(minSpeed, maxSpeed);
            float scale = Random.Range(minScale, maxScale);
            it.tr.localScale = Vector3.one * scale;
            if (balloonSprites != null && balloonSprites.Length > 0)
                it.sr.sprite = balloonSprites[Random.Range(0, balloonSprites.Length)];
            it.sr.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
