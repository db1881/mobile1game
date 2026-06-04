using UnityEngine;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text balloonsText;
        [SerializeField] private TMP_Text bombsText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text gamesText;
        [SerializeField] private TMP_Text winsText;
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private TMP_Text endlessText;

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            var d = SaveSystem.Data;
            if (balloonsText != null) balloonsText.text = d.TotalBalloonsPopped.ToString("N0");
            if (bombsText != null) bombsText.text = d.TotalBombsTriggered.ToString("N0");
            if (comboText != null) comboText.text = d.LongestCombo.ToString();
            if (gamesText != null) gamesText.text = d.TotalGamesPlayed.ToString("N0");
            if (winsText != null) winsText.text = d.TotalLevelsWon.ToString("N0");
            if (starsText != null) starsText.text = SaveSystem.GetTotalStars().ToString();
            if (endlessText != null) endlessText.text = d.EndlessHighScore.ToString("N0");
        }
    }
}
