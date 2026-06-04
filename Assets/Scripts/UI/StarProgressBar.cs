using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    /// <summary>
    /// HUD'da skor → 3 yıldız ilerleme çubuğu.
    /// Star1, Star2, Star3 eşiği için ışıyan yıldızlar ve aralarındaki çubuk dolar.
    /// </summary>
    public class StarProgressBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;        // sliced/filled bar
        [SerializeField] private RectTransform star1;
        [SerializeField] private RectTransform star2;
        [SerializeField] private RectTransform star3;
        [SerializeField] private Image star1Image;
        [SerializeField] private Image star2Image;
        [SerializeField] private Image star3Image;
        [SerializeField] private Color earnedColor = new Color(1f, 0.84f, 0.20f);
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private float popDuration = 0.45f;

        private int lastStarLevel = 0;

        private void OnEnable()
        {
            GameEvents.OnScoreChanged += HandleScore;
            GameEvents.OnLevelStarted += Reset;
            Reset();
        }

        private void OnDisable()
        {
            GameEvents.OnScoreChanged -= HandleScore;
            GameEvents.OnLevelStarted -= Reset;
        }

        private void Reset()
        {
            lastStarLevel = 0;
            if (fillImage != null) fillImage.fillAmount = 0f;
            SetStarVisual(star1Image, false);
            SetStarVisual(star2Image, false);
            SetStarVisual(star3Image, false);
        }

        private void HandleScore(int score)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentLevel == null) return;
            int s1 = gm.CurrentLevel.StarOneScore;
            int s2 = gm.CurrentLevel.StarTwoScore;
            int s3 = gm.CurrentLevel.StarThreeScore;
            if (s3 <= 0) return;

            float t = Mathf.Clamp01(score / (float)s3);
            if (fillImage != null) fillImage.fillAmount = t;

            int newStarLevel = score >= s3 ? 3 : score >= s2 ? 2 : score >= s1 ? 1 : 0;
            if (newStarLevel != lastStarLevel)
            {
                // Yeni kazanılan yıldızlar için pop animasyonu
                if (newStarLevel >= 1 && lastStarLevel < 1) PopStar(star1, star1Image);
                if (newStarLevel >= 2 && lastStarLevel < 2) PopStar(star2, star2Image);
                if (newStarLevel >= 3 && lastStarLevel < 3) PopStar(star3, star3Image);
                lastStarLevel = newStarLevel;
            }
        }

        private void PopStar(RectTransform star, Image img)
        {
            if (img != null) SetStarVisual(img, true);
            if (star != null) StartCoroutine(PopRoutine(star));
        }

        private IEnumerator PopRoutine(RectTransform t)
        {
            Vector3 baseScale = Vector3.one;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / popDuration;
                // 0→1.5→1.0 ease
                float s = p < 0.5f
                    ? Mathf.Lerp(1f, 1.6f, p / 0.5f)
                    : Mathf.Lerp(1.6f, 1f, (p - 0.5f) / 0.5f);
                t.localScale = baseScale * s;
                yield return null;
            }
            t.localScale = baseScale;
        }

        private void SetStarVisual(Image img, bool earned)
        {
            if (img == null) return;
            img.color = earned ? earnedColor : emptyColor;
        }
    }
}
