using UnityEngine;
using BalloonPop.Core;

namespace BalloonPop.Gameplay
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        [SerializeField] private int baseMatchScore = 80;
        [SerializeField] private int comboBonus = 100;
        [SerializeField] private int specialBonus = 300;
        [SerializeField] private int movesLeftBonus = 150;   // win sırasında her kalan hamle için

        public int CurrentScore { get; private set; }
        public int LastMovesLeftBonus { get; private set; }    // WinPanel/UI'da ayrıca göstermek için

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnMatchMade += HandleMatch;
            GameEvents.OnComboChain += HandleCombo;
            GameEvents.OnLevelStarted += Reset;
        }

        private void OnDestroy()
        {
            GameEvents.OnMatchMade -= HandleMatch;
            GameEvents.OnComboChain -= HandleCombo;
            GameEvents.OnLevelStarted -= Reset;
        }

        private void Reset()
        {
            CurrentScore = 0;
            GameEvents.RaiseScoreChanged(CurrentScore);
        }

        private void HandleMatch(int count)
        {
            int extra = Mathf.Max(0, count - 3) * 50;
            CurrentScore += baseMatchScore * count + extra;
            GameEvents.RaiseScoreChanged(CurrentScore);
        }

        /// <summary>
        /// Level kazanıldığında çağrılır. Kalan hamleleri puan + bonus alana yansıtır.
        /// Bu sayede erken/verimli oyuncu yüksek yıldız alır.
        /// </summary>
        public int ApplyMovesLeftBonus(int movesLeft)
        {
            int bonus = Mathf.Max(0, movesLeft) * movesLeftBonus;
            LastMovesLeftBonus = bonus;
            CurrentScore += bonus;
            GameEvents.RaiseScoreChanged(CurrentScore);
            return bonus;
        }

        private void HandleCombo(int chainCount)
        {
            CurrentScore += comboBonus * (chainCount - 1);
            GameEvents.RaiseScoreChanged(CurrentScore);
        }

        public void AddSpecialBonus()
        {
            CurrentScore += specialBonus;
            GameEvents.RaiseScoreChanged(CurrentScore);
        }

        public int GetStarRating(int oneStar, int twoStar, int threeStar)
        {
            if (CurrentScore >= threeStar) return 3;
            if (CurrentScore >= twoStar) return 2;
            if (CurrentScore >= oneStar) return 1;
            return 0;
        }
    }
}
