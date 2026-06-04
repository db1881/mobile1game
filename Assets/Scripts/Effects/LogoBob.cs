using UnityEngine;

namespace BalloonPop.Effects
{
    public class LogoBob : MonoBehaviour
    {
        [SerializeField] private float bobAmount = 8f;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float rotationAmount = 1.5f;

        private Vector3 basePos;
        private RectTransform rt;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            basePos = rt != null ? (Vector3)rt.anchoredPosition : transform.localPosition;
        }

        private void Update()
        {
            float t = Time.time * speed;
            if (rt != null)
                rt.anchoredPosition = new Vector2(basePos.x, basePos.y + Mathf.Sin(t) * bobAmount);
            else
                transform.localPosition = basePos + Vector3.up * Mathf.Sin(t) * bobAmount * 0.01f;
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.7f) * rotationAmount);
        }
    }
}
