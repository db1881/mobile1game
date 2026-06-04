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

        private void OnEnable()
        {
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
                }
                else
                {
                    int sec = HeartSystem.SecondsToNextHeart();
                    timerText.text = HeartSystem.FormatSecondsAsClock(sec);
                }
            }
        }
    }
}
