using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BalloonPop.Save
{
    [System.Serializable]
    public class LevelRecord
    {
        public int LevelNumber;
        public int BestScore;
        public int Stars;
    }

    [System.Serializable]
    public class AchievementProgressEntry
    {
        public string Key;
        public int Value;
    }

    [System.Serializable]
    public class GameData
    {
        public int HighestUnlockedLevel = 1;
        public LevelRecord[] Levels = new LevelRecord[0];
        public int TotalScore;
        public int Coins = 200;
        public int Hammers = 1;
        public int Shuffles = 1;
        public int MovePacks = 1;
        public long LastDailyClaim;
        public int DailyStreak;
        public int EndlessHighScore;
        public List<string> AchievementUnlocks = new List<string>();
        public List<AchievementProgressEntry> AchievementProgress = new List<AchievementProgressEntry>();
        public string LastDailyAchievementDay = "";

        public int TotalBalloonsPopped;
        public int TotalBombsTriggered;
        public int LongestCombo;
        public int TotalGamesPlayed;
        public int TotalLevelsWon;

        public float MusicVolume = 0.7f;
        public float SfxVolume = 0.8f;

        public int Hearts = 5;
        public long NextHeartTicks; // UTC ticks: bir sonraki hayat dolduğunda

        public int Language = 0; // 0=TR, 1=EN
    }

    public static class SaveSystem
    {
        private const string FileName = "balloonpop_save.json";
        private static GameData cache;

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static GameData Data
        {
            get
            {
                if (cache == null) Load();
                return cache;
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    cache = JsonUtility.FromJson<GameData>(json);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save load failed: {e.Message}");
            }

            if (cache == null) cache = new GameData();
        }

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(cache, true);
                File.WriteAllText(FilePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save write failed: {e.Message}");
            }
        }

        public static int GetHighestUnlockedLevel() => Data.HighestUnlockedLevel;

        public static int GetLevelStars(int levelNumber)
        {
            foreach (var rec in Data.Levels)
                if (rec.LevelNumber == levelNumber) return rec.Stars;
            return 0;
        }

        public static int GetLevelBestScore(int levelNumber)
        {
            foreach (var rec in Data.Levels)
                if (rec.LevelNumber == levelNumber) return rec.BestScore;
            return 0;
        }

        public static int GetTotalStars()
        {
            int total = 0;
            foreach (var rec in Data.Levels) total += rec.Stars;
            return total;
        }

        public static int Coins
        {
            get => Data.Coins;
            set { Data.Coins = Mathf.Max(0, value); Save(); }
        }

        public static float MusicVolume
        {
            get => Data.MusicVolume;
            set { Data.MusicVolume = Mathf.Clamp01(value); Save(); }
        }

        public static float SfxVolume
        {
            get => Data.SfxVolume;
            set { Data.SfxVolume = Mathf.Clamp01(value); Save(); }
        }

        public static bool TrySpendCoins(int amount)
        {
            if (Data.Coins < amount) return false;
            Data.Coins -= amount;
            Save();
            return true;
        }

        public static void AddCoins(int amount)
        {
            Data.Coins += amount;
            Save();
        }

        public static void MarkLevelComplete(int levelNumber, int score, int stars)
        {
            LevelRecord existing = null;
            foreach (var r in Data.Levels)
                if (r.LevelNumber == levelNumber) { existing = r; break; }

            if (existing == null)
            {
                var newList = new LevelRecord[Data.Levels.Length + 1];
                System.Array.Copy(Data.Levels, newList, Data.Levels.Length);
                existing = new LevelRecord { LevelNumber = levelNumber };
                newList[newList.Length - 1] = existing;
                Data.Levels = newList;
            }

            int starsEarned = Mathf.Max(0, stars - existing.Stars);
            if (score > existing.BestScore) existing.BestScore = score;
            if (stars > existing.Stars)
            {
                Data.TotalScore += (stars - existing.Stars) * 100;
                existing.Stars = stars;
            }

            // İyileştirilmiş ekonomi: ilk geçiş bonusu büyütüldü, yıldız başına ödül 2x.
            // (eski: 10 + starsEarned*12 → yeni: 25 + starsEarned*25)
            int coinReward = (existing.BestScore == 0 ? 25 : 0) + (starsEarned * 25);
            Data.Coins += coinReward;

            if (levelNumber + 1 > Data.HighestUnlockedLevel)
                Data.HighestUnlockedLevel = levelNumber + 1;

            Save();
        }

        public static void ResetAll()
        {
            cache = new GameData();
            Save();
        }
    }
}
