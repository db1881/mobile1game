using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Gameplay;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class BoosterPanel : MonoBehaviour
    {
        [SerializeField] private Button hammerButton;
        [SerializeField] private Button shuffleButton;
        [SerializeField] private Button movePackButton;
        [SerializeField] private TMP_Text hammerCount;
        [SerializeField] private TMP_Text shuffleCount;
        [SerializeField] private TMP_Text movePackCount;

        private void Awake()
        {
            if (hammerButton != null) hammerButton.onClick.AddListener(OnHammer);
            if (shuffleButton != null) shuffleButton.onClick.AddListener(OnShuffle);
            if (movePackButton != null) movePackButton.onClick.AddListener(OnMovePack);
        }

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            var d = SaveSystem.Data;
            if (hammerCount != null) hammerCount.text = d.Hammers.ToString();
            if (shuffleCount != null) shuffleCount.text = d.Shuffles.ToString();
            if (movePackCount != null) movePackCount.text = d.MovePacks.ToString();
        }

        private void OnHammer()
        {
            if (BoosterManager.Instance != null && BoosterManager.Instance.BeginHammer())
                Refresh();
        }

        private void OnShuffle()
        {
            if (BoosterManager.Instance != null && BoosterManager.Instance.TryUseShuffle())
                Refresh();
        }

        private void OnMovePack()
        {
            if (BoosterManager.Instance != null && BoosterManager.Instance.TryUseMovePack())
                Refresh();
        }
    }
}
