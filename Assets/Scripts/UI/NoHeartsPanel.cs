using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    /// <summary> 0 hayat kalınca açılan modal: bekle veya coin'le satın al. </summary>
    public class NoHeartsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        private float refreshAccumulator;

        private void OnEnable()
        {
            if (titleText != null) titleText.text = "Hayatın bitti!";
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuy);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            }
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

        private void Refresh()
        {
            int hearts = HeartSystem.Current();
            if (hearts > 0)
            {
                gameObject.SetActive(false);
                return;
            }
            if (timerText != null)
            {
                int sec = HeartSystem.SecondsToNextHeart();
                timerText.text = $"Sonraki hayat: {HeartSystem.FormatSecondsAsClock(sec)}";
            }
            if (coinText != null)
                coinText.text = $"{HeartSystem.RefillCoinCost} 🪙 ile 1 hayat al";
        }

        private void OnBuy()
        {
            if (HeartSystem.BuyOne())
            {
                gameObject.SetActive(false);
            }
        }
    }
}
