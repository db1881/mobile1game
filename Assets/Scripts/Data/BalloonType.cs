namespace BalloonPop.Data
{
    public enum BalloonType
    {
        None    = 0,
        Red     = 1,
        Blue    = 2,
        Green   = 3,
        Yellow  = 4,
        Purple  = 5,
        Orange  = 6,
        Pink    = 7
    }

    public enum SpecialType
    {
        Normal      = 0,
        LineH       = 1,
        LineV       = 2,
        Bomb        = 3,
        Rainbow     = 4,
        Gold        = 5
    }

    public static class BalloonTypeExtensions
    {
        public static bool IsColorMatch(this BalloonType a, BalloonType b)
        {
            if (a == BalloonType.None || b == BalloonType.None) return false;
            return a == b;
        }
    }
}
