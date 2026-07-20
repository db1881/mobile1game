using UnityEngine;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    /// <summary> Üst HUD'da kalp ikonu + sayı + sonraki kalbe kalan süre. </summary>
    public class HeartDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text timerText;

        private float refreshAccumulator;
        private RectTransform countRect;
        private Vector2 countAnchorMin;
        private Vector2 countAnchorMax;
        private bool layoutCached;

        private void Awake()
        {
            CacheLayout();
        }

        private void OnEnable()
        {
            CacheLayout();
            Refresh();
            refreshAccumulator = 0f;
        }

        private void Update()
        {
            refreshAccumulator += Time.unscaledDeltaTime;
            if (refreshAccumulator >= 1f)
            {
                refreshAccumulator = 0f;
                Refresh();
            }
        }

        public void Refresh()
        {
            int hearts = HeartSystem.Current();
            if (countText != null) countText.text = hearts.ToString();
            if (timerText != null)
            {
                if (hearts >= HeartSystem.MaxHearts)
                {
                    timerText.text = "TAM";
                    timerText.gameObject.SetActive(false);
                    SetCompactLayout(false);
                }
                else
                {
                    int sec = HeartSystem.SecondsToNextHeart();
                    timerText.text = HeartSystem.FormatSecondsAsClock(sec);
                    timerText.gameObject.SetActive(true);
                    SetCompactLayout(true);
                }
            }
        }

        private void CacheLayout()
        {
            if (layoutCached || countText == null) return;

            countRect = countText.rectTransform;
            countAnchorMin = countRect.anchorMin;
            countAnchorMax = countRect.anchorMax;
            layoutCached = true;
        }

        private void SetCompactLayout(bool showTimer)
        {
            if (!layoutCached || countRect == null) return;

            if (showTimer)
            {
                countRect.anchorMin = countAnchorMin;
                countRect.anchorMax = new Vector2(0.58f, countAnchorMax.y);
            }
            else
            {
                countRect.anchorMin = countAnchorMin;
                countRect.anchorMax = countAnchorMax;
            }
        }
    }
}
