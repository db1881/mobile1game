using System;
using System.Collections.Generic;
using BalloonPop.Data;

namespace BalloonPop.Core
{
    public static class GameEvents
    {
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnMovesChanged;
        public static event Action<BalloonType, int> OnGoalProgress;
        public static event Action OnLevelStarted;
        public static event Action OnLevelWon;
        public static event Action OnLevelLost;
        public static event Action<int> OnMatchMade;
        public static event Action<int> OnComboChain;

        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void RaiseMovesChanged(int moves) => OnMovesChanged?.Invoke(moves);
        public static void RaiseGoalProgress(BalloonType type, int remaining) => OnGoalProgress?.Invoke(type, remaining);
        public static void RaiseLevelStarted() => OnLevelStarted?.Invoke();
        public static void RaiseLevelWon() => OnLevelWon?.Invoke();
        public static void RaiseLevelLost() => OnLevelLost?.Invoke();
        public static void RaiseMatchMade(int count) => OnMatchMade?.Invoke(count);
        public static void RaiseComboChain(int chain) => OnComboChain?.Invoke(chain);
    }
}
