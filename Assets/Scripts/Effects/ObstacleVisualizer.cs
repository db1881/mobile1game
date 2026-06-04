using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    public class ObstacleVisualizer : MonoBehaviour
    {
        [SerializeField] private Sprite stoneSprite;
        [SerializeField] private Sprite iceSprite;
        [SerializeField] private int sortingOrder = 1;
        [SerializeField] private int iceSortingOrder = 30;
        [SerializeField] private string sortingLayerName = "Default";

        public static ObstacleVisualizer Instance { get; private set; }

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly Dictionary<Vector2Int, SpriteRenderer> iceMap = new Dictionary<Vector2Int, SpriteRenderer>();

        private void Awake() => Instance = this;
        private void OnEnable() => GameEvents.OnLevelStarted += Refresh;
        private void OnDisable() => GameEvents.OnLevelStarted -= Refresh;

        public void RefreshIceAt(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (!iceMap.TryGetValue(key, out var sr) || sr == null) return;
            var cell = GridManager.Instance.Cells[x, y];
            if (cell.IceLayers <= 0)
            {
                Destroy(sr.gameObject);
                iceMap.Remove(key);
                return;
            }
            float alpha = 0.45f + 0.20f * Mathf.Clamp01(cell.IceLayers / 3f);
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        private void Refresh()
        {
            foreach (var go in spawned) if (go != null) Destroy(go);
            spawned.Clear();
            iceMap.Clear();

            var grid = GridManager.Instance;
            if (grid == null || grid.Cells == null) return;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Cells[x, y].IsBlocked)
                    {
                        var go = new GameObject($"Stone_{x}_{y}");
                        go.transform.SetParent(transform, false);
                        var cellPos = grid.GridToWorld(x, y);
                        var sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = stoneSprite;
                        sr.sortingOrder = sortingOrder;
                        sr.sortingLayerName = sortingLayerName;
                        sr.color = Color.white;

                        if (sr.sprite != null && sr.sprite.bounds.size.x > 0.001f)
                        {
                            // 1) Scale: sprite'ı hücre boyutuna sığdır
                            float w = sr.sprite.bounds.size.x;
                            float h = sr.sprite.bounds.size.y;
                            float maxDim = Mathf.Max(w, h);
                            float desired = 0.95f;
                            float s = desired / maxDim;
                            go.transform.localScale = new Vector3(s, s, 1f);

                            // 2) Pivot kompansasyonu: sprite.bounds.center sıfır değilse pivot
                            // merkezde değildir; gameobject'i pivot offsetini scale ile çarparak kaydır
                            Vector3 pivotCenterLocal = sr.sprite.bounds.center;
                            Vector3 pivotOffset = new Vector3(
                                -pivotCenterLocal.x * s,
                                -pivotCenterLocal.y * s,
                                0f);
                            go.transform.position = cellPos + pivotOffset;
                        }
                        else
                        {
                            go.transform.position = cellPos;
                            go.transform.localScale = Vector3.one * 0.95f;
                        }
                        spawned.Add(go);
                    }
                    if (grid.Cells[x, y].IceLayers > 0)
                    {
                        var go = new GameObject($"Ice_{x}_{y}");
                        go.transform.SetParent(transform, false);
                        go.transform.position = grid.GridToWorld(x, y);
                        var sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = iceSprite;
                        sr.sortingOrder = iceSortingOrder;
                        sr.sortingLayerName = sortingLayerName;
                        float alpha = 0.45f + 0.20f * Mathf.Clamp01(grid.Cells[x, y].IceLayers / 3f);
                        sr.color = new Color(0.7f, 0.85f, 1f, alpha);
                        spawned.Add(go);
                        iceMap[new Vector2Int(x, y)] = sr;
                    }
                }
            }
        }
    }
}
