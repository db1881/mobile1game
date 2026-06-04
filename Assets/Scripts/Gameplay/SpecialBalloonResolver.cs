using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Data;
using BalloonPop.Grid;

namespace BalloonPop.Gameplay
{
    public static class SpecialBalloonResolver
    {
        public static SpecialType DetermineSpecial(MatchGroup group)
        {
            int count = group.Cells.Count;
            if (count >= 5) return SpecialType.Rainbow;
            // 4-match çizgi balonu kapatıldı (user istemiyor) → bomba'ya yükselt
            if (count == 4) return SpecialType.Bomb;
            if (group.Shape == MatchShape.Tshape || group.Shape == MatchShape.Lshape)
                return SpecialType.Bomb;
            return SpecialType.Normal;
        }

        public static HashSet<Cell> CollectAffectedCells(GridManager grid, Balloon trigger)
        {
            var affected = new HashSet<Cell>();
            if (trigger == null) return affected;

            int x = trigger.X, y = trigger.Y;
            switch (trigger.Special)
            {
                case SpecialType.LineH:
                    for (int i = 0; i < grid.Width; i++)
                        if (grid.Cells[i, y].HasBalloon) affected.Add(grid.Cells[i, y]);
                    break;

                case SpecialType.LineV:
                    for (int i = 0; i < grid.Height; i++)
                        if (grid.Cells[x, i].HasBalloon) affected.Add(grid.Cells[x, i]);
                    break;

                case SpecialType.Bomb:
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (grid.IsInside(nx, ny) && grid.Cells[nx, ny].HasBalloon)
                                affected.Add(grid.Cells[nx, ny]);
                        }
                    }
                    break;

                case SpecialType.Rainbow:
                    var color = trigger.Type;
                    for (int i = 0; i < grid.Width; i++)
                        for (int j = 0; j < grid.Height; j++)
                            if (grid.Cells[i, j].HasBalloon && grid.Cells[i, j].CurrentBalloon.Type == color)
                                affected.Add(grid.Cells[i, j]);
                    break;
            }
            return affected;
        }
    }
}
