using System.Collections.Generic;
using UnityEngine;

namespace BalloonPop.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "BalloonPop/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> Levels = new List<LevelData>();

        public LevelData GetByNumber(int levelNumber)
        {
            return Levels.Find(l => l != null && l.LevelNumber == levelNumber);
        }

        public LevelData GetByIndex(int index)
        {
            if (index < 0 || index >= Levels.Count) return null;
            return Levels[index];
        }

        public int Count => Levels.Count;
    }
}
