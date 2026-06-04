using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    /// <summary>
    /// Her grid hücresinin arkasına çok hafif (yarı şeffaf) yuvarlatılmış kare
    /// bir "yuva" sprite'ı yerleştirir. Candy Crush'taki gibi balonların altında
    /// belli belirsiz bir grid hissi verir.
    ///
    /// Blocked (taş) hücrelerde tile gizlenir — taşın arkasında kare görünmez.
    /// </summary>
    public class CellTileGrid : MonoBehaviour
    {
        [SerializeField] private Sprite tileSprite;
        [Tooltip("Renk overlay (alfa burada düşük tutulur).")]
        [SerializeField] private Color tileColor = new Color(1f, 1f, 1f, 0.95f);
        [Tooltip("Hücre boyutunun bu oranı kadar tile bas. 0.95 = neredeyse tam hücre, kare köşeler balonun etrafında görünür.")]
        [SerializeField, Range(0.5f, 1.0f)] private float fillRatio = 0.95f;
        [Tooltip("Balonların arkasında kalsın diye düşük sortingOrder.")]
        [SerializeField] private int sortingOrder = -4;
        [SerializeField] private string sortingLayer = "Default";

        private readonly List<SpriteRenderer> tiles = new List<SpriteRenderer>();
        private int builtW = -1, builtH = -1;
        private bool visibilityDirty = true;
        private float pollInterval = 0.5f;   // Grid boyut/durum değişikliğini yarım saniyede bir kontrol et
        private float nextPoll;

        private void Update()
        {
            // Her frame değil — düşük frekansta poll yeterli.
            if (Time.time < nextPoll) return;
            nextPoll = Time.time + pollInterval;

            var grid = GridManager.Instance;
            if (grid == null || tileSprite == null) return;
            int w = grid.Width, h = grid.Height;
            if (w <= 0 || h <= 0) return;

            if (w != builtW || h != builtH)
            {
                Rebuild(grid, w, h);
                visibilityDirty = true;
            }
            if (visibilityDirty)
            {
                UpdateBlockedVisibility(grid, w, h);
                visibilityDirty = false;
            }
        }

        /// <summary>Dışarıdan tetiklemek için (ileride event ile bağlanabilir).</summary>
        public void MarkDirty() => visibilityDirty = true;

        private void Rebuild(GridManager grid, int w, int h)
        {
            // Eski tile'ları temizle
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null) Destroy(tiles[i].gameObject);
            }
            tiles.Clear();

            // Yeni tile'ları oluştur
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var pos = grid.GridToWorld(x, y);
                    var go = new GameObject($"CellTile_{x}_{y}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = new Vector3(pos.x, pos.y, 4f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = tileSprite;
                    sr.color = tileColor;
                    sr.sortingLayerName = sortingLayer;
                    sr.sortingOrder = sortingOrder;
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(fillRatio, fillRatio);
                    tiles.Add(sr);
                }
            }
            builtW = w;
            builtH = h;
        }

        private void UpdateBlockedVisibility(GridManager grid, int w, int h)
        {
            var cells = grid.Cells;
            if (cells == null) return;

            int idx = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (idx >= tiles.Count) return;
                    var sr = tiles[idx];
                    if (sr != null)
                    {
                        // Blocked hücrelerde tile'ı gizle
                        bool shouldShow = !cells[x, y].IsBlocked;
                        if (sr.enabled != shouldShow)
                            sr.enabled = shouldShow;
                    }
                    idx++;
                }
            }
        }

        public void SetColor(Color c)
        {
            tileColor = c;
            for (int i = 0; i < tiles.Count; i++)
                if (tiles[i] != null) tiles[i].color = c;
        }
    }
}
