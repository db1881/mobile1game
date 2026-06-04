using System.Collections;
using UnityEngine;

namespace BalloonPop.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelAnimator : MonoBehaviour
    {
        [SerializeField] private float showDuration = 0.25f;
        [SerializeField] private float startScale = 0.85f;
        [SerializeField] private RectTransform cardTransform;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (cardTransform == null)
            {
                var t = transform.Find("Card");
                if (t != null) cardTransform = (RectTransform)t;
            }
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            if (cardTransform != null) cardTransform.localScale = Vector3.one * startScale;

            float t = 0f;
            while (t < showDuration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / showDuration);
                float eased = EaseOutBack(u);
                canvasGroup.alpha = u;
                if (cardTransform != null) cardTransform.localScale = Vector3.LerpUnclamped(Vector3.one * startScale, Vector3.one, eased);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            if (cardTransform != null) cardTransform.localScale = Vector3.one;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.40158f;
            const float c3 = c1 + 1f;
            float k = t - 1f;
            return 1f + c3 * k * k * k + c1 * k * k;
        }
    }
}
