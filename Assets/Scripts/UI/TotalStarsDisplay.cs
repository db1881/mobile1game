using UnityEngine;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class TotalStarsDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text starText;
        [SerializeField] private float pollInterval = 0.5f;

        private float timer;
        private int lastShown = -1;

        private void OnEnable() => Refresh();

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
            int total = SaveSystem.GetTotalStars();
            if (total == lastShown) return;
            lastShown = total;
            if (starText != null) starText.text = total.ToString();
        }
    }
}
