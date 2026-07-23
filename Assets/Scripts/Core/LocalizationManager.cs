using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Save;

namespace BalloonPop.Core
{
    /// <summary>
    /// Basit anahtar-değer çevirisi. Aktif dil SaveData.Language ile saklanır.
    /// </summary>
    public static class LocalizationManager
    {
        public enum Lang { TR = 0, EN = 1 }

        public static System.Action OnLanguageChanged;

        private static readonly Dictionary<string, string[]> Dict = new Dictionary<string, string[]>
        {
            // Menü
            { "menu.play",         new[] { "OYNA",          "PLAY" } },
            { "menu.level_select", new[] { "LEVEL SEÇ",     "LEVEL SELECT" } },
            { "menu.settings",     new[] { "AYARLAR",       "SETTINGS" } },
            { "menu.quit",         new[] { "ÇIKIŞ",         "QUIT" } },
            { "menu.shop",         new[] { "MAĞAZA",        "SHOP" } },
            { "menu.daily",        new[] { "GÜNLÜK",        "DAILY" } },
            { "menu.stats",        new[] { "İSTATİSTİK",    "STATS" } },
            { "menu.achievements", new[] { "BAŞARIM",       "AWARDS" } },
            { "menu.google_signin",new[] { "GOOGLE İLE GİRİŞ", "SIGN IN WITH GOOGLE" } },
            { "menu.leaderboard",  new[] { "LİDERLİK",      "LEADERBOARD" } },

            // HUD
            { "hud.score",         new[] { "SKOR",          "SCORE" } },
            { "hud.moves",         new[] { "HAMLE",         "MOVES" } },

            // Pause
            { "pause.title",       new[] { "DURDURULDU",    "PAUSED" } },
            { "pause.resume",      new[] { "DEVAM ET",      "RESUME" } },
            { "pause.replay",      new[] { "TEKRAR DENE",   "REPLAY" } },
            { "pause.menu",        new[] { "ANA MENÜ",      "MAIN MENU" } },

            // Win
            { "win.title",         new[] { "TEBRİKLER!",    "CONGRATS!" } },
            { "win.sub",           new[] { "Seviye Tamamlandı", "Level Complete" } },
            { "win.next",          new[] { "SONRAKİ",       "NEXT" } },
            { "win.replay",        new[] { "TEKRAR",        "REPLAY" } },
            { "win.menu",          new[] { "MENÜ",          "MENU" } },

            // Lose
            { "lose.title",        new[] { "BAŞARISIZ!",    "FAILED!" } },
            { "lose.sub",          new[] { "Hamleler bitti", "Out of moves" } },
            { "lose.retry",        new[] { "TEKRAR",        "RETRY" } },

            // Hearts
            { "hearts.full",       new[] { "TAM",           "FULL" } },
            { "hearts.title",      new[] { "HAYATIN BİTTİ!", "OUT OF LIVES!" } },
            { "hearts.next",       new[] { "Sonraki hayat:", "Next life:" } },
            { "hearts.buy",        new[] { "SATIN AL",      "BUY" } },

            // Settings
            { "settings.music",    new[] { "Müzik",         "Music" } },
            { "settings.sfx",      new[] { "Efektler",      "Effects" } },
            { "settings.vibration",new[] { "Titreşim",      "Vibration" } },
            { "settings.language", new[] { "Dil",           "Language" } },
            { "settings.reset",    new[] { "İLERLEMEYİ SIFIRLA", "RESET PROGRESS" } },
            { "settings.unlock",   new[] { "TÜM SEVİYELERİ AÇ", "UNLOCK ALL LEVELS" } },

            // Common
            { "common.close",      new[] { "KAPAT",         "CLOSE" } },
            { "common.ok",         new[] { "TAMAM",         "OK" } },
            { "common.cancel",     new[] { "İPTAL",         "CANCEL" } },
        };

        public static Lang Current
        {
            get => (Lang)Mathf.Clamp(SaveSystem.Data.Language, 0, 1);
            set { SaveSystem.Data.Language = (int)value; SaveSystem.Save(); OnLanguageChanged?.Invoke(); }
        }

        public static string Get(string key)
        {
            if (Dict.TryGetValue(key, out var values))
                return values[(int)Current];
            return key;
        }

        public static void Toggle()
        {
            Current = Current == Lang.TR ? Lang.EN : Lang.TR;
        }
    }
}
