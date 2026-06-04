using System.Collections.Generic;
using UnityEngine;

namespace BalloonPop.Data
{
    public enum GoalType { Color = 0, Score = 1, IceClear = 2 }

    [System.Serializable]
    public class Goal
    {
        public GoalType Type = GoalType.Color;
        public BalloonType Color;  // Type=Color için
        public int Amount;         // Color: balon sayısı, Score: hedef skor, IceClear: hücre sayısı
    }

    [CreateAssetMenu(fileName = "Level_", menuName = "BalloonPop/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber = 1;
        public string LevelName = "Level 1";

        [Header("Grid")]
        [Range(5, 10)] public int Width = 8;
        [Range(6, 12)] public int Height = 9;
        [Range(3, 7)]  public int ColorVariety = 6;

        [Header("Gameplay")]
        [Range(5, 60)] public int MaxMoves = 25;
        public List<Goal> Goals = new List<Goal>();

        [Header("Scoring")]
        public int StarOneScore = 1000;
        public int StarTwoScore = 2500;
        public int StarThreeScore = 5000;

        [Header("Optional")]
        public List<Vector2Int> BlockedCells = new List<Vector2Int>();
        public List<IceCellData> IceCells = new List<IceCellData>();

        [Header("World")]
        public int WorldIndex = 1;
    }

    [System.Serializable]
    public class IceCellData
    {
        public Vector2Int Position;
        public int Layers = 1;
    }
}
