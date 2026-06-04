using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class MysteryBoxPanel : MonoBehaviour
    {
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private CanvasGroup boxGroup;

        private bool opened;

        private void OnEnable()
        {
            opened = false;
            if (boxGroup != null) boxGroup.alpha = 1;
            if (rewardText != null) rewardText.text = "Hazine kutusunu aç!";
            if (openButton != null) { openButton.onClick.RemoveAllListeners(); openButton.onClick.AddListener(Open); }
            if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(() => gameObject.SetActive(false)); }
        }

        private void Open()
        {
            if (opened) return;
            opened = true;
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                if (boxGroup != null) boxGroup.alpha = 1f - t / 0.4f;
                yield return null;
            }

            int roll = Random.Range(0, 100);
            int coins, hammers = 0, shuffles = 0, moves = 0;
            string label;
            if (roll < 50)      { coins = 30;  label = $"+{coins} Coin!"; }
            else if (roll < 75) { coins = 80;  label = $"+{coins} Coin!"; }
            else if (roll < 90) { coins = 20;  hammers = 1; label = $"+{coins} Coin + Çekiç!"; }
            else if (roll < 98) { coins = 60;  shuffles = 1; label = $"+{coins} Coin + Karıştır!"; }
            else                { coins = 200; hammers = 1; shuffles = 1; moves = 1; label = $"JACKPOT! +{coins} + Tüm Boosterlar!"; }

            SaveSystem.AddCoins(coins);
            SaveSystem.Data.Hammers += hammers;
            SaveSystem.Data.Shuffles += shuffles;
            SaveSystem.Data.MovePacks += moves;
            SaveSystem.Save();

            if (rewardText != null) rewardText.text = label;
        }
    }
}
