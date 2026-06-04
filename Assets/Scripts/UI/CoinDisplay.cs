using UnityEngine;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class CoinDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private float pollInterval = 0.5f;

        private float timer;
        private int lastShown = -1;

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= pollInterval)
            {
                timer = 0f;
                Refresh();
            }
        }

        private void Refresh()
        {
            var c = SaveSystem.Data.Coins;
            if (c == lastShown) return;
            lastShown = c;
            if (coinText != null) coinText.text = c.ToString("N0");
        }
    }
}
