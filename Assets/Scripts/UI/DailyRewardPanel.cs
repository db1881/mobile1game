using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class DailyRewardPanel : MonoBehaviour
    {
        [SerializeField] private Button claimButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private Transform daysContainer;

        private static readonly int[] DayRewards = { 20, 30, 40, 60, 90, 120, 200 };

        private void OnEnable()
        {
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(Claim);
            }
            Refresh();
        }

        private void Refresh()
        {
            long now = DateTime.UtcNow.Ticks;
            long last = SaveSystem.Data.LastDailyClaim;
            int streak = SaveSystem.Data.DailyStreak;

            bool canClaim = false;
            if (last == 0) canClaim = true;
            else
            {
                var lastDate = new DateTime(last, DateTimeKind.Utc).Date;
                var today = DateTime.UtcNow.Date;
                if (today > lastDate) canClaim = true;
            }

            if (statusText != null)
                statusText.text = canClaim ? "Ödülün hazır!" : "Yarın gel.";
            if (streakText != null)
                streakText.text = $"Streak: {streak} gün";
            if (claimButton != null)
                claimButton.interactable = canClaim;
        }

        private void Claim()
        {
            var last = SaveSystem.Data.LastDailyClaim;
            var lastDate = last == 0 ? DateTime.MinValue : new DateTime(last, DateTimeKind.Utc).Date;
            var today = DateTime.UtcNow.Date;
            if (today <= lastDate) return;

            int newStreak = (today - lastDate).Days == 1 ? SaveSystem.Data.DailyStreak + 1 : 1;
            if (newStreak > 7) newStreak = 1;
            int reward = DayRewards[Mathf.Clamp(newStreak - 1, 0, DayRewards.Length - 1)];

            SaveSystem.Data.DailyStreak = newStreak;
            SaveSystem.Data.LastDailyClaim = today.Ticks;
            SaveSystem.AddCoins(reward);
            Refresh();
        }
    }
}
