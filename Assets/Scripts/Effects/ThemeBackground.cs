using UnityEngine;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ThemeBackground : MonoBehaviour
    {
        [SerializeField] private Sprite[] worldBgs;
        [SerializeField] private int sortingOrder = -20;
        [SerializeField] private string sortingLayerName = "Default";

        private SpriteRenderer sr;
        private Camera cam;

        public static ThemeBackground Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            sr.sortingLayerName = sortingLayerName;
            cam = Camera.main;
        }

        public void SetWorld(int worldIndex)
        {
            if (worldBgs == null || worldBgs.Length == 0) return;
            int idx = Mathf.Clamp(worldIndex - 1, 0, worldBgs.Length - 1);
            sr.sprite = worldBgs[idx];
        }

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || sr.sprite == null) return;
            var camPos = cam.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, 10);
            float viewH = cam.orthographicSize * 2f;
            float viewW = viewH * cam.aspect;
            var b = sr.sprite.bounds.size;
            float scaleX = viewW / b.x;
            float scaleY = viewH / b.y;
            float s = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
