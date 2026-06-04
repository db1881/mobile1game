#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using BalloonPop.Data;

namespace BalloonPop.EditorTools
{
    public static class SampleLevelCreator
    {
        private const string LevelFolder = "Assets/Resources/Levels";
        private const string DatabasePath = "Assets/Resources/LevelDatabase.asset";

        [MenuItem("BalloonPop/Create Sample Levels (1-10)")]
        public static void CreateSamples()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            if (!AssetDatabase.IsValidFolder(LevelFolder))
            {
                Directory.CreateDirectory(LevelFolder);
            }
            AssetDatabase.Refresh();

            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            db.Levels.Clear();

            for (int i = 1; i <= 100; i++)
            {
                var data = ScriptableObject.CreateInstance<LevelData>();
                data.LevelNumber = i;
                data.LevelName = $"Level {i}";
                data.Width = Mathf.Clamp(6 + i / 10, 6, 9);
                data.Height = Mathf.Clamp(8 + i / 10, 8, 11);
                data.ColorVariety = Mathf.Clamp(4 + i / 15, 4, 7);
                // Hamle: yumuşak düşüş + yüksek taban (eski: max(14, 30-i/5) → L99=14)
                // Yeni: L1=32, L25=29, L50=27, L75=24, L99=22
                data.MaxMoves = Mathf.Max(22, 32 - i / 10);

                data.Goals.Clear();
                int goalCount = Mathf.Clamp(1 + i / 7, 1, 3);
                // Goal miktarı: eski 12+i*1.6 → L100=172 (imkansız). Yeni: L1=11, L50=40, L100=70.
                int baseAmount = 10 + Mathf.RoundToInt(i * 0.6f);
                for (int g = 0; g < goalCount; g++)
                {
                    data.Goals.Add(new Goal
                    {
                        Color = (BalloonType)(((i + g) % data.ColorVariety) + 1),
                        Amount = baseAmount + g * 4
                    });
                }

                data.StarOneScore  = 600  + i * 180;
                data.StarTwoScore  = 1600 + i * 420;
                data.StarThreeScore = 3200 + i * 800;

                data.WorldIndex = i <= 25 ? 1 : (i <= 50 ? 2 : (i <= 75 ? 3 : 4));

                if (i >= 15)
                {
                    data.IceCells.Clear();
                    int iceCount = Mathf.Min(10, (i - 14) / 3 + 1);
                    var iceRng = new System.Random(i * 1597);
                    for (int b = 0; b < iceCount; b++)
                    {
                        int bx = iceRng.Next(0, data.Width);
                        int by = iceRng.Next(1, data.Height - 1);
                        int layers = iceRng.Next(1, 3);
                        bool exists = false;
                        foreach (var ic in data.IceCells) if (ic.Position == new Vector2Int(bx, by)) { exists = true; break; }
                        if (!exists)
                            data.IceCells.Add(new IceCellData { Position = new Vector2Int(bx, by), Layers = layers });
                    }
                }

                if (i >= 8)
                {
                    data.BlockedCells.Clear();
                    int blockedCount = Mathf.Min(8, (i - 7) / 2 + 1);
                    var rng = new System.Random(i * 7919);
                    int safeRows = 2;
                    for (int b = 0; b < blockedCount; b++)
                    {
                        int bx = rng.Next(0, data.Width);
                        int by = rng.Next(safeRows, data.Height - safeRows);
                        if (!data.BlockedCells.Contains(new Vector2Int(bx, by)))
                            data.BlockedCells.Add(new Vector2Int(bx, by));
                    }
                }

                bool isBoss = (i % 10 == 0);
                if (isBoss)
                {
                    data.LevelName = $"BOSS {i}";
                    data.Width = 9;
                    data.Height = 11;
                    data.ColorVariety = Mathf.Clamp(5 + i / 10, 5, 7);
                    // Boss: cömert hamle (eski: max(12, 22-i/4) → L100=12). Yeni: L10=34, L50=30, L100=27
                    data.MaxMoves = Mathf.Max(27, 35 - i / 12);
                    data.Goals.Clear();
                    // Boss goal: eski 30+i → L100=130 per goal. Yeni: L10=30, L50=50, L100=75
                    int bossAmount = 25 + i / 2;
                    for (int g = 0; g < 3; g++)
                    {
                        data.Goals.Add(new Goal
                        {
                            Color = (BalloonType)(((i + g * 2) % data.ColorVariety) + 1),
                            Amount = bossAmount - g * 3
                        });
                    }
                    data.StarOneScore  = 3000 + i * 400;
                    data.StarTwoScore  = 6500 + i * 800;
                    data.StarThreeScore = 12000 + i * 1500;
                }

                string path = $"{LevelFolder}/Level_{i:D2}.asset";
                AssetDatabase.CreateAsset(data, path);
                db.Levels.Add(data);
            }

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created 10 sample levels in " + LevelFolder);
            Selection.activeObject = db;
        }
    }
}
#endif
