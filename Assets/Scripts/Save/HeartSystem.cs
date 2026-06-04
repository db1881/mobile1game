using System;

namespace BalloonPop.Save
{
    /// <summary>
    /// 5 hayatlı klasik free-to-play hayat sistemi.
    /// Hayat dolma süresi: 30 dakika.
    /// </summary>
    public static class HeartSystem
    {
        public const int MaxHearts = 5;
        public const int RefillMinutes = 30;
        public const int RefillCoinCost = 50;

        /// <summary> Geçen zamana göre hayat saysısını günceller ve döner. </summary>
        public static int Current()
        {
            var d = SaveSystem.Data;

            // Üst sınıra ulaştıysa zaten dolu
            if (d.Hearts >= MaxHearts)
            {
                d.NextHeartTicks = 0;
                return d.Hearts;
            }

            // Hiç zaman damgası yoksa şimdiden başlat
            if (d.NextHeartTicks == 0)
            {
                d.NextHeartTicks = DateTime.UtcNow.AddMinutes(RefillMinutes).Ticks;
                SaveSystem.Save();
                return d.Hearts;
            }

            // Zaman geçtiyse hayatları doldur
            var now = DateTime.UtcNow;
            var next = new DateTime(d.NextHeartTicks, DateTimeKind.Utc);
            while (d.Hearts < MaxHearts && now >= next)
            {
                d.Hearts++;
                next = next.AddMinutes(RefillMinutes);
            }

            d.NextHeartTicks = d.Hearts >= MaxHearts ? 0 : next.Ticks;
            SaveSystem.Save();
            return d.Hearts;
        }

        public static bool CanPlay() => Current() > 0;

        /// <summary> 1 hayat harca (level başlangıcı veya kaybetme). </summary>
        public static bool Spend()
        {
            int current = Current();
            if (current <= 0) return false;
            var d = SaveSystem.Data;
            d.Hearts = current - 1;
            // İlk kez tama altına düştüyse refill timer'ı başlat
            if (d.NextHeartTicks == 0)
                d.NextHeartTicks = DateTime.UtcNow.AddMinutes(RefillMinutes).Ticks;
            SaveSystem.Save();
            return true;
        }

        /// <summary> Coin'le 1 hayat satın al. </summary>
        public static bool BuyOne()
        {
            int current = Current();
            if (current >= MaxHearts) return false;
            if (!SaveSystem.TrySpendCoins(RefillCoinCost)) return false;
            var d = SaveSystem.Data;
            d.Hearts = current + 1;
            if (d.Hearts >= MaxHearts) d.NextHeartTicks = 0;
            SaveSystem.Save();
            return true;
        }

        /// <summary> Tüm hayatları doldur (premium feature, şu an kullanılmıyor). </summary>
        public static void Refill()
        {
            var d = SaveSystem.Data;
            d.Hearts = MaxHearts;
            d.NextHeartTicks = 0;
            SaveSystem.Save();
        }

        /// <summary> Sonraki hayat için kalan saniye (0=hazır). </summary>
        public static int SecondsToNextHeart()
        {
            var d = SaveSystem.Data;
            Current(); // yan etkiyle güncelle
            if (d.Hearts >= MaxHearts || d.NextHeartTicks == 0) return 0;
            var span = new DateTime(d.NextHeartTicks, DateTimeKind.Utc) - DateTime.UtcNow;
            return (int)Math.Max(0, span.TotalSeconds);
        }

        public static string FormatSecondsAsClock(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:00}:{s:00}";
        }
    }
}
