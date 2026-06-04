using UnityEngine;
using BalloonPop.Gameplay;

namespace BalloonPop.Grid
{
    [System.Serializable]
    public class Cell
    {
        public int X;
        public int Y;
        public Vector3 WorldPosition;
        public Balloon CurrentBalloon;
        public bool IsBlocked;
        public int IceLayers;

        public Cell(int x, int y, Vector3 worldPos)
        {
            X = x;
            Y = y;
            WorldPosition = worldPos;
            CurrentBalloon = null;
            IsBlocked = false;
            IceLayers = 0;
        }

        public bool IsEmpty => CurrentBalloon == null && !IsBlocked;
        public bool HasBalloon => CurrentBalloon != null;
        public bool HasIce => IceLayers > 0;
    }
}
