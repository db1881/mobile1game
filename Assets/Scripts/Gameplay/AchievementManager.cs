using System;
using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Save;

namespace BalloonPop.Gameplay
{
    [System.Serializable]
    public class AchievementDef
    {
        public string Key;
        public string Title;
        public string Description;
        public int Target;
        public int CoinReward;
    }

    public class AchievementManager : Singleton<AchievementManager>
    {
        public static event Action<AchievementDef> OnUnlocked;

        public List<AchievementDef> Definitions { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) DontDestroyOnLoad(gameObject);

            Definitions = new List<AchievementDef>
            {
                // 🎯 İlk adımlar
                new AchievementDef { Key = "first_match",   Title = "İlk Adım",        Description = "İlk eşleşmeyi yap",                Target = 1,    CoinReward = 25 },
                new AchievementDef { Key = "matches_50",    Title = "Eşleştirici",     Description = "50 eşleşme yap",                   Target = 50,   CoinReward = 75 },
                new AchievementDef { Key = "matches_500",   Title = "Eşleşme Ustası",  Description = "500 eşleşme yap",                  Target = 500,  CoinReward = 300 },

                // 🎈 Balon patlatma
                new AchievementDef { Key = "pop_100",       Title = "Balon Avcısı",    Description = "100 balon patlat",                 Target = 100,  CoinReward = 100 },
                new AchievementDef { Key = "pop_1000",      Title = "Pop Master",      Description = "1000 balon patlat",                Target = 1000, CoinReward = 500 },
                new AchievementDef { Key = "pop_5000",      Title = "Balon Tufanı",    Description = "5000 balon patlat",                Target = 5000, CoinReward = 1500 },
                new AchievementDef { Key = "pop_10000",     Title = "Pop Efsanesi",    Description = "10000 balon patlat",               Target = 10000,CoinReward = 3000 },

                // 💣 Bombalar
                new AchievementDef { Key = "bombs_5",       Title = "Bombacı",         Description = "5 bomba patlat",                   Target = 5,    CoinReward = 75 },
                new AchievementDef { Key = "bombs_50",      Title = "Yıkım Uzmanı",    Description = "50 bomba patlat",                  Target = 50,   CoinReward = 300 },
                new AchievementDef { Key = "bombs_200",     Title = "Patlama Lordu",   Description = "200 bomba patlat",                 Target = 200,  CoinReward = 1000 },

                // 🌈 Combo / Zincir
                new AchievementDef { Key = "combo_3",       Title = "Zincir Ustası",   Description = "3 zincirli combo yap",             Target = 1,    CoinReward = 50 },
                new AchievementDef { Key = "combo_5",       Title = "Mega Combo",      Description = "5 zincirli combo yap",             Target = 1,    CoinReward = 150 },
                new AchievementDef { Key = "combo_8",       Title = "Patlama Furyası", Description = "8 zincirli combo yap",             Target = 1,    CoinReward = 500 },

                // 🏆 Seviye ilerleme
                new AchievementDef { Key = "level_5",       Title = "Yolun Başı",      Description = "Seviye 5'i tamamla",               Target = 5,    CoinReward = 100 },
                new AchievementDef { Key = "level_15",      Title = "Yarıyol",         Description = "Seviye 15'i tamamla",              Target = 15,   CoinReward = 250 },
                new AchievementDef { Key = "level_30",      Title = "Tecrübeli",       Description = "Seviye 30'u tamamla",              Target = 30,   CoinReward = 500 },
                new AchievementDef { Key = "level_50",      Title = "Yarışçı",         Description = "Seviye 50'yi tamamla",             Target = 50,   CoinReward = 1000 },
                new AchievementDef { Key = "level_75",      Title = "Veteran",         Description = "Seviye 75'i tamamla",              Target = 75,   CoinReward = 1500 },
                new AchievementDef { Key = "level_100",     Title = "Şampiyon",        Description = "Tüm 100 seviyeyi tamamla",         Target = 100,  CoinReward = 5000 },

                // ⭐ Yıldız toplama
                new AchievementDef { Key = "stars_30",      Title = "Yıldız Toplayıcı",Description = "30 yıldız topla",                  Target = 30,   CoinReward = 200 },
                new AchievementDef { Key = "stars_75",      Title = "Mükemmel",        Description = "75 yıldız topla",                  Target = 75,   CoinReward = 500 },
                new AchievementDef { Key = "stars_150",     Title = "Yıldız Avcısı",   Description = "150 yıldız topla",                 Target = 150,  CoinReward = 1500 },
                new AchievementDef { Key = "stars_250",     Title = "Galaktik",        Description = "250 yıldız topla",                 Target = 250,  CoinReward = 3000 },

                // 🎯 Skor odaklı
                new AchievementDef { Key = "score_5k",      Title = "Skor Avcısı",     Description = "Tek seviyede 5000 puan",           Target = 1,    CoinReward = 100 },
                new AchievementDef { Key = "score_15k",     Title = "Puan Canavarı",   Description = "Tek seviyede 15000 puan",          Target = 1,    CoinReward = 400 },
                new AchievementDef { Key = "score_50k",     Title = "Skor Tanrısı",    Description = "Tek seviyede 50000 puan",          Target = 1,    CoinReward = 2000 },

                // 💪 Azim
                new AchievementDef { Key = "lose_5",        Title = "Pes Etmeyen",     Description = "5 seviye kaybet",                  Target = 5,    CoinReward = 100 },
                new AchievementDef { Key = "lose_25",       Title = "Yenilmez Ruh",    Description = "25 seviye kaybet",                 Target = 25,   CoinReward = 500 },

                // 🎉 Özel
                new AchievementDef { Key = "play_3days",    Title = "Sadık Oyuncu",    Description = "3 farklı gün oyna",                Target = 3,    CoinReward = 200 },
                new AchievementDef { Key = "play_7days",    Title = "Hafta Bağımlısı", Description = "7 farklı gün oyna",                Target = 7,    CoinReward = 700 },
            };

            GameEvents.OnMatchMade += HandleMatch;
            GameEvents.OnLevelWon += HandleLevelWon;
            GameEvents.OnLevelLost += HandleLevelLost;
            GameEvents.OnComboChain += HandleComboChain;
            GameEvents.OnScoreChanged += HandleScore;

            TrackDailyPlay();
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            GameEvents.OnMatchMade -= HandleMatch;
            GameEvents.OnLevelWon -= HandleLevelWon;
            GameEvents.OnLevelLost -= HandleLevelLost;
            GameEvents.OnComboChain -= HandleComboChain;
            GameEvents.OnScoreChanged -= HandleScore;
        }

        private void HandleMatch(int count)
        {
            AddProgress("first_match", 1);
            AddProgress("matches_50", 1);
            AddProgress("matches_500", 1);
            AddProgress("pop_100", count);
            AddProgress("pop_1000", count);
            AddProgress("pop_5000", count);
            AddProgress("pop_10000", count);
        }

        public void NotifyBombTriggered()
        {
            AddProgress("bombs_5", 1);
            AddProgress("bombs_50", 1);
            AddProgress("bombs_200", 1);
        }

        private void HandleComboChain(int chain)
        {
            if (chain >= 3) AddProgress("combo_3", 1);
            if (chain >= 5) AddProgress("combo_5", 1);
            if (chain >= 8) AddProgress("combo_8", 1);
        }

        private void HandleScore(int score)
        {
            // Tek seviyedeki anlık skor — milestone'lara ulaşınca tetikle
            if (score >= 5000)  AddProgress("score_5k", 1);
            if (score >= 15000) AddProgress("score_15k", 1);
            if (score >= 50000) AddProgress("score_50k", 1);
        }

        private void HandleLevelLost()
        {
            AddProgress("lose_5", 1);
            AddProgress("lose_25", 1);
        }

        private void HandleLevelWon()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentLevel == null) return;
            int n = GameManager.Instance.CurrentLevel.LevelNumber;

            SetAtLeast("level_5", n);
            SetAtLeast("level_15", n);
            SetAtLeast("level_30", n);
            SetAtLeast("level_50", n);
            SetAtLeast("level_75", n);
            SetAtLeast("level_100", n);

            int totalStars = SaveSystem.GetTotalStars();
            SetAtLeast("stars_30",  totalStars);
            SetAtLeast("stars_75",  totalStars);
            SetAtLeast("stars_150", totalStars);
            SetAtLeast("stars_250", totalStars);
        }

        /// <summary>
        /// Bu kaydedilmiş "son oynama tarihi"ni kontrol eder; yeni gün ise gün sayacını artırır.
        /// </summary>
        private void TrackDailyPlay()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            string lastDay = SaveSystem.Data.LastDailyAchievementDay ?? "";
            if (lastDay == today) return;
            SaveSystem.Data.LastDailyAchievementDay = today;
            SaveSystem.Save();

            AddProgress("play_3days", 1);
            AddProgress("play_7days", 1);
        }

        public void AddProgress(string key, int amount)
        {
            var def = Definitions.Find(d => d.Key == key);
            if (def == null) return;
            if (IsUnlocked(key)) return;
            int prev = GetProgress(key);
            SetProgress(key, prev + amount);
            if (prev + amount >= def.Target) Unlock(def);
        }

        public void SetAtLeast(string key, int value)
        {
            var def = Definitions.Find(d => d.Key == key);
            if (def == null) return;
            if (IsUnlocked(key)) return;
            int prev = GetProgress(key);
            if (value > prev) SetProgress(key, value);
            if (value >= def.Target) Unlock(def);
        }

        private void Unlock(AchievementDef def)
        {
            SaveSystem.Data.AchievementUnlocks ??= new List<string>();
            if (SaveSystem.Data.AchievementUnlocks.Contains(def.Key)) return;
            SaveSystem.Data.AchievementUnlocks.Add(def.Key);
            SaveSystem.AddCoins(def.CoinReward);
            SaveSystem.Save();
            OnUnlocked?.Invoke(def);
        }

        public bool IsUnlocked(string key)
        {
            return SaveSystem.Data.AchievementUnlocks != null
                && SaveSystem.Data.AchievementUnlocks.Contains(key);
        }

        public int GetProgress(string key)
        {
            if (SaveSystem.Data.AchievementProgress == null) return 0;
            foreach (var kv in SaveSystem.Data.AchievementProgress)
                if (kv.Key == key) return kv.Value;
            return 0;
        }

        private void SetProgress(string key, int value)
        {
            if (SaveSystem.Data.AchievementProgress == null)
                SaveSystem.Data.AchievementProgress = new List<AchievementProgressEntry>();
            foreach (var kv in SaveSystem.Data.AchievementProgress)
            {
                if (kv.Key == key) { kv.Value = value; SaveSystem.Save(); return; }
            }
            SaveSystem.Data.AchievementProgress.Add(new AchievementProgressEntry { Key = key, Value = value });
            SaveSystem.Save();
        }
    }
}
