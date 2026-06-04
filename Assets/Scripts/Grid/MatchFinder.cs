using System.Collections.Generic;
using BalloonPop.Data;

namespace BalloonPop.Grid
{
    public class MatchGroup
    {
        public List<Cell> Cells = new List<Cell>();
        public BalloonType Color;
        public MatchShape Shape;
    }

    public enum MatchShape
    {
        Three,
        FourHorizontal,
        FourVertical,
        FiveLine,
        Tshape,
        Lshape
    }

    public class MatchFinder
    {
        private readonly GridManager grid;
        private static readonly int[] DX = { 1, -1, 0, 0 };
        private static readonly int[] DY = { 0, 0, 1, -1 };

        public MatchFinder(GridManager gridManager)
        {
            grid = gridManager;
        }

        public List<MatchGroup> FindAllMatches()
        {
            var found = new List<MatchGroup>();
            if (grid == null || grid.Cells == null) return found;

            // Klasik düz çizgi matching: yatay 3+ veya dikey 3+
            // L, T, kare ve diğer şekiller match SAYILMAZ.

            // --- Yatay tarama ---
            for (int y = 0; y < grid.Height; y++)
            {
                int x = 0;
                while (x < grid.Width)
                {
                    var startColor = ColorOf(x, y);
                    if (startColor == null) { x++; continue; }
                    int run = 1;
                    while (x + run < grid.Width && Equals(ColorOf(x + run, y), startColor)) run++;
                    if (run >= 3)
                    {
                        var group = new MatchGroup { Color = startColor.Value };
                        for (int i = 0; i < run; i++) group.Cells.Add(grid.Cells[x + i, y]);
                        group.Shape = run == 3 ? MatchShape.Three
                                    : run == 4 ? MatchShape.FourHorizontal
                                               : MatchShape.FiveLine;
                        found.Add(group);
                    }
                    x += run;
                }
            }

            // --- Dikey tarama ---
            for (int x = 0; x < grid.Width; x++)
            {
                int y = 0;
                while (y < grid.Height)
                {
                    var startColor = ColorOf(x, y);
                    if (startColor == null) { y++; continue; }
                    int run = 1;
                    while (y + run < grid.Height && Equals(ColorOf(x, y + run), startColor)) run++;
                    if (run >= 3)
                    {
                        var group = new MatchGroup { Color = startColor.Value };
                        for (int i = 0; i < run; i++) group.Cells.Add(grid.Cells[x, y + i]);
                        group.Shape = run == 3 ? MatchShape.Three
                                    : run == 4 ? MatchShape.FourVertical
                                               : MatchShape.FiveLine;
                        found.Add(group);
                    }
                    y += run;
                }
            }

            return found;
        }

        /// <summary> Pop edilebilir (normal renkli) balonun rengi, yoksa null. </summary>
        private BalloonType? ColorOf(int x, int y)
        {
            if (x < 0 || y < 0 || x >= grid.Width || y >= grid.Height) return null;
            var c = grid.Cells[x, y];
            if (!c.HasBalloon) return null;
            if (c.CurrentBalloon.Special != SpecialType.Normal) return null;
            return c.CurrentBalloon.Type;
        }

        private MatchShape DetectShape(MatchGroup g)
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var c in g.Cells)
            {
                if (c.X < minX) minX = c.X; if (c.X > maxX) maxX = c.X;
                if (c.Y < minY) minY = c.Y; if (c.Y > maxY) maxY = c.Y;
            }
            int spanX = maxX - minX + 1;
            int spanY = maxY - minY + 1;
            int count = g.Cells.Count;

            if (spanX > 1 && spanY > 1)
                return count >= 6 ? MatchShape.Tshape : MatchShape.Lshape;
            if (count >= 5) return MatchShape.FiveLine;
            if (count == 4) return spanX > spanY ? MatchShape.FourHorizontal : MatchShape.FourVertical;
            return MatchShape.Three;
        }
    }
}
