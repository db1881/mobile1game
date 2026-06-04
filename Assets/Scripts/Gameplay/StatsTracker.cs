using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Save;

namespace BalloonPop.Gameplay
{
    public class StatsTracker : Singleton<StatsTracker>
    {
        private int sessionCombo;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) DontDestroyOnLoad(gameObject);
            GameEvents.OnMatchMade += HandleMatch;
            GameEvents.OnComboChain += HandleCombo;
            GameEvents.OnLevelStarted += HandleLevelStart;
            GameEvents.OnLevelWon += HandleLevelWon;
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            GameEvents.OnMatchMade -= HandleMatch;
            GameEvents.OnComboChain -= HandleCombo;
            GameEvents.OnLevelStarted -= HandleLevelStart;
            GameEvents.OnLevelWon -= HandleLevelWon;
        }

        public void NotifyBomb()
        {
            SaveSystem.Data.TotalBombsTriggered++;
            SaveSystem.Save();
        }

        private void HandleMatch(int count)
        {
            SaveSystem.Data.TotalBalloonsPopped += count;
            SaveSystem.Save();
        }

        private void HandleCombo(int chain)
        {
            if (chain > SaveSystem.Data.LongestCombo)
            {
                SaveSystem.Data.LongestCombo = chain;
                SaveSystem.Save();
            }
        }

        private void HandleLevelStart()
        {
            SaveSystem.Data.TotalGamesPlayed++;
            SaveSystem.Save();
        }

        private void HandleLevelWon()
        {
            SaveSystem.Data.TotalLevelsWon++;
            SaveSystem.Save();
        }
    }
}
