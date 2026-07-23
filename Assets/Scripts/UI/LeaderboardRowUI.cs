using BalloonPop.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalloonPop.UI
{
    public sealed class LeaderboardRowUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text playerText;
        [SerializeField] private TMP_Text scoreText;

        public void Show(LeaderboardEntry entry, bool turkish, int visualIndex)
        {
            if (entry == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);
            if (rankText != null)
            {
                rankText.text = entry.Rank > 0 ? "#" + entry.Rank : "-";
                rankText.color = RankColor(entry.Rank);
            }

            if (playerText != null)
            {
                string suffix = entry.IsCurrentPlayer ? (turkish ? "  • SEN" : "  • YOU") : string.Empty;
                playerText.text = string.IsNullOrWhiteSpace(entry.PlayerName)
                    ? (turkish ? "Oyuncu" : "Player") + suffix
                    : entry.PlayerName + suffix;
                playerText.color = entry.IsCurrentPlayer
                    ? new Color(0.05f, 0.48f, 0.60f, 1f)
                    : new Color(0.34f, 0.20f, 0.10f, 1f);
            }

            if (scoreText != null)
                scoreText.text = "★ " + entry.TotalStars.ToString("N0");

            if (background != null)
            {
                background.color = entry.IsCurrentPlayer
                    ? new Color(0.60f, 0.94f, 0.96f, 0.92f)
                    : visualIndex % 2 == 0
                        ? new Color(1f, 0.94f, 0.76f, 0.78f)
                        : new Color(1f, 0.86f, 0.62f, 0.58f);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static Color RankColor(int rank)
        {
            switch (rank)
            {
                case 1: return new Color(0.92f, 0.57f, 0.04f, 1f);
                case 2: return new Color(0.45f, 0.52f, 0.60f, 1f);
                case 3: return new Color(0.72f, 0.35f, 0.12f, 1f);
                default: return new Color(0.48f, 0.31f, 0.19f, 1f);
            }
        }
    }
}
