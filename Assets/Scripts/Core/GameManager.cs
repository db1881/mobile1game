using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Data;
using BalloonPop.Grid;
using BalloonPop.Gameplay;
using BalloonPop.Save;

namespace BalloonPop.Core
{
    public enum GameState
    {
        Menu,
        Loading,
        Playing,
        Paused,
        Won,
        Lost
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private LevelData currentLevel;

        public GameState State { get; private set; } = GameState.Menu;
        public int MovesLeft { get; private set; }
        public Dictionary<BalloonType, int> RemainingGoals { get; private set; }
        public int ScoreTarget { get; private set; }  // 0 = hedef yok
        public int RemainingIce { get; private set; } // 0 = hedef yok
        public LevelData CurrentLevel => currentLevel;

        protected override void Awake()
        {
            base.Awake();
        }

        public void LoadLevel(LevelData level)
        {
            currentLevel = level;
            State = GameState.Loading;

            MovesLeft = level.MaxMoves;
            RemainingGoals = new Dictionary<BalloonType, int>();
            ScoreTarget = 0;
            RemainingIce = 0;
            foreach (var goal in level.Goals)
            {
                switch (goal.Type)
                {
                    case GoalType.Color:
                        RemainingGoals[goal.Color] = goal.Amount;
                        break;
                    case GoalType.Score:
                        ScoreTarget = goal.Amount;
                        break;
                    // GoalType.IceClear: Buz tamamen kaldırıldı — IceClear hedefleri yok say.
                    // Level data'da kalsa bile bu blok atlanır.
                }
            }

            GridManager.Instance.BuildGrid(level.Width, level.Height, level.ColorVariety);
            foreach (var blocked in level.BlockedCells)
            {
                if (GridManager.Instance.IsInside(blocked.x, blocked.y))
                    GridManager.Instance.Cells[blocked.x, blocked.y].IsBlocked = true;
            }
            // Buz uygulaması KAPALI: balon üzerine semi-transparent buz tile'ı eklenmiyor.
            // (Eski: foreach (var ice in level.IceCells) ... IceLayers = ice.Layers;)
            GridManager.Instance.FillInitial();

            GameEvents.RaiseMovesChanged(MovesLeft);
            foreach (var g in RemainingGoals)
                GameEvents.RaiseGoalProgress(g.Key, g.Value);

            State = GameState.Playing;
            GameEvents.RaiseLevelStarted();

            // Başlangıçta yan yana gelen 3+ balonu otomatik patlat (PickNonMatchingType
            // tüm match'leri önleyemez; oyuncu beklemeden cascade ile çözülsün).
            StartCoroutine(ResolveInitialMatches());
        }

        private System.Collections.IEnumerator ResolveInitialMatches()
        {
            // Bir frame bekle — grid spawn tamamlansın, görsel olarak balonlar yerlerinde
            yield return null;
            yield return null;
            // İlk match'leri patlat (skor saymadan? — saysın, normal cascade gibi)
            yield return GridManager.Instance.ResolveMatchesLoop();
        }

        public void ConsumeMove()
        {
            if (State != GameState.Playing) return;
            MovesLeft = Mathf.Max(0, MovesLeft - 1);
            GameEvents.RaiseMovesChanged(MovesLeft);
        }

        public void AddMoves(int amount)
        {
            MovesLeft += amount;
            GameEvents.RaiseMovesChanged(MovesLeft);
        }

        public void UpdateGoal(BalloonType type, int amountPopped)
        {
            if (RemainingGoals == null) return;
            if (!RemainingGoals.ContainsKey(type)) return;

            RemainingGoals[type] = Mathf.Max(0, RemainingGoals[type] - amountPopped);
            GameEvents.RaiseGoalProgress(type, RemainingGoals[type]);

            if (State == GameState.Playing && AreAllGoalsMet())
                TriggerWin();
        }

        private bool AreAllGoalsMet()
        {
            bool hasAny = false;
            // Renk hedefleri
            if (RemainingGoals != null && RemainingGoals.Count > 0)
            {
                hasAny = true;
                foreach (var g in RemainingGoals)
                    if (g.Value > 0) return false;
            }
            // Skor hedefi
            if (ScoreTarget > 0)
            {
                hasAny = true;
                int cur = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
                if (cur < ScoreTarget) return false;
            }
            // Buz hedefi
            if (RemainingIce > 0)
            {
                hasAny = true;
                return false;
            }
            return hasAny;
        }

        /// <summary> Buz katmanı temizlendiğinde GridManager çağırır. </summary>
        public void ReportIceCleared()
        {
            if (RemainingIce <= 0) return;
            RemainingIce--;
            if (State == GameState.Playing && AreAllGoalsMet()) TriggerWin();
        }

        private void TriggerWin()
        {
            if (State == GameState.Won) return;
            State = GameState.Won;

            // Kalan hamle başına bonus puan — verimli oyuncu daha yüksek yıldız alır.
            int movesLeftBonus = 0;
            if (ScoreManager.Instance != null)
                movesLeftBonus = ScoreManager.Instance.ApplyMovesLeftBonus(MovesLeft);

            int levelNum = currentLevel != null ? currentLevel.LevelNumber : -1;
            int curScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            Debug.Log($"[GameManager] Level {levelNum} WON! Score: {curScore} (movesLeft bonus: +{movesLeftBonus})");

            try { GameEvents.RaiseLevelWon(); }
            catch (System.Exception e) { Debug.LogError($"[GameManager] RaiseLevelWon error: {e}"); }

            if (currentLevel != null && ScoreManager.Instance != null)
            {
                try
                {
                    SaveSystem.MarkLevelComplete(
                        currentLevel.LevelNumber,
                        ScoreManager.Instance.CurrentScore,
                        ScoreManager.Instance.GetStarRating(
                            currentLevel.StarOneScore,
                            currentLevel.StarTwoScore,
                            currentLevel.StarThreeScore));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GameManager] MarkLevelComplete error: {e}");
                }
            }
        }

        private void Update()
        {
            if (State == GameState.Playing && AreAllGoalsMet())
                TriggerWin();
        }

        public void CheckLevelCompletion()
        {
            if (State != GameState.Playing) return;

            if (AreAllGoalsMet())
            {
                TriggerWin();
                return;
            }

            if (MovesLeft <= 0)
            {
                State = GameState.Lost;
                Debug.Log($"[GameManager] Level {currentLevel.LevelNumber} LOST (out of moves)");
                BalloonPop.Save.HeartSystem.Spend();
                GameEvents.RaiseLevelLost();
            }
        }

        public void PauseGame()  { if (State == GameState.Playing) State = GameState.Paused; }
        public void ResumeGame() { if (State == GameState.Paused) State = GameState.Playing; }

        public void RestartLevel()
        {
            if (currentLevel != null) LoadLevel(currentLevel);
        }
    }
}
