using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Audio;
using BalloonPop.Effects;
using BalloonPop.Gameplay;

namespace BalloonPop.Grid
{
    public class GridManager : Singleton<GridManager>
    {
        [Header("Grid Configuration")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 9;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 gridOrigin = Vector2.zero;
        [SerializeField] private Transform balloonContainer;

        [Header("Spawning")]
        [SerializeField] private Balloon balloonPrefab;
        [SerializeField] private int colorVariety = 6;
        [SerializeField] private float fallDelay = 0.05f;
        [SerializeField, Range(0f, 0.30f)] private float bombSpawnChance = 0.10f;   // 6% → 10%
        [SerializeField, Range(0f, 0.05f)] private float goldSpawnChance = 0.015f;

        [Header("Effects")]
        [SerializeField] private GameObject boomEffectPrefab;
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private GameObject flashPrefab;
        [SerializeField] private GameObject shockwavePrefab;
        [SerializeField] private Sprite[] flameRibbonFrames;   // boyalı alev şeridi (4 kare flipbook) — her çizgi patlamasında
        [SerializeField] private float bombShakeAmount = 0.55f;
        [SerializeField] private float popShakeAmount = 0.05f;

        private Cell[,] cells;
        private readonly Queue<Balloon> pool = new Queue<Balloon>();
        private MatchFinder matchFinder;

        public int Width => width;
        public int Height => height;
        public Cell[,] Cells => cells;

        protected override void Awake()
        {
            base.Awake();
            matchFinder = new MatchFinder(this);
        }

        public void BuildGrid(int w, int h, int colors)
        {
            width = w;
            height = h;
            colorVariety = Mathf.Clamp(colors, 3, 7);
            cells = new Cell[width, height];

            // Eski balon GameObject'lerini temizle (replay'de eski balonlar kalmasın)
            if (balloonContainer != null)
            {
                for (int i = balloonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = balloonContainer.GetChild(i);
                    if (child != null) Destroy(child.gameObject);
                }
            }
            pool.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new Cell(x, y, GridToWorld(x, y));
                }
            }
        }

        public void FillInitial()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].IsBlocked) continue;
                    var type = PickNonMatchingType(x, y);
                    SpawnBalloon(x, y, type);
                }
            }
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                gridOrigin.x + x * cellSize,
                gridOrigin.y + y * cellSize,
                0f);
        }

        public bool WorldToGrid(Vector3 world, out int x, out int y)
        {
            x = Mathf.RoundToInt((world.x - gridOrigin.x) / cellSize);
            y = Mathf.RoundToInt((world.y - gridOrigin.y) / cellSize);
            return IsInside(x, y);
        }

        public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        public Balloon SpawnBalloon(int x, int y, BalloonType type)
        {
            Balloon b = pool.Count > 0 ? pool.Dequeue() : Instantiate(balloonPrefab, balloonContainer);
            b.gameObject.SetActive(true);
            b.Initialize(type, x, y, GridToWorld(x, y));
            cells[x, y].CurrentBalloon = b;
            return b;
        }

        public void ReturnToPool(Balloon b)
        {
            if (b == null) return;
            b.gameObject.SetActive(false);
            pool.Enqueue(b);
        }

        private bool HasSpecialNeighbor(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (!IsInside(nx, ny)) continue;
                    if (!cells[nx, ny].HasBalloon) continue;
                    var sp = cells[nx, ny].CurrentBalloon.Special;
                    if (sp == SpecialType.Bomb || sp == SpecialType.LineH || sp == SpecialType.LineV || sp == SpecialType.Rainbow)
                        return true;
                }
            }
            return false;
        }

        private BalloonType PickNonMatchingType(int x, int y)
        {
            var candidates = new List<BalloonType>();
            for (int i = 1; i <= colorVariety; i++) candidates.Add((BalloonType)i);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }
            foreach (var c in candidates)
                if (!WouldCreateConnectedMatch(x, y, c)) return c;
            return candidates[0];
        }

        private bool WouldCreateConnectedMatch(int x, int y, BalloonType color)
        {
            int count = 1;
            var visited = new HashSet<long> { Key(x, y) };
            var stack = new Stack<(int, int)>();
            for (int d = 0; d < 4; d++)
            {
                int nx = x + DX[d], ny = y + DY[d];
                if (!IsInside(nx, ny)) continue;
                if (!cells[nx, ny].HasBalloon) continue;
                if (cells[nx, ny].CurrentBalloon.Special != SpecialType.Normal) continue;
                if (cells[nx, ny].CurrentBalloon.Type == color) stack.Push((nx, ny));
            }
            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Pop();
                long k = Key(cx, cy);
                if (visited.Contains(k)) continue;
                visited.Add(k);
                count++;
                if (count >= 3) return true;
                for (int d = 0; d < 4; d++)
                {
                    int nx = cx + DX[d], ny = cy + DY[d];
                    if (!IsInside(nx, ny)) continue;
                    if (visited.Contains(Key(nx, ny))) continue;
                    if (!cells[nx, ny].HasBalloon) continue;
                    if (cells[nx, ny].CurrentBalloon.Special != SpecialType.Normal) continue;
                    if (cells[nx, ny].CurrentBalloon.Type == color) stack.Push((nx, ny));
                }
            }
            return false;
        }

        private static readonly int[] DX = { 1, -1, 0, 0 };
        private static readonly int[] DY = { 0, 0, 1, -1 };
        private static long Key(int x, int y) => ((long)x << 16) | (uint)y;

        public IEnumerator SwapBalloons(int ax, int ay, int bx, int by)
        {
            var a = cells[ax, ay].CurrentBalloon;
            var b = cells[bx, by].CurrentBalloon;
            if (a == null || b == null) yield break;

            cells[ax, ay].CurrentBalloon = b;
            cells[bx, by].CurrentBalloon = a;
            b.SetGridPosition(ax, ay);
            a.SetGridPosition(bx, by);

            a.MoveTo(GridToWorld(bx, by));
            b.MoveTo(GridToWorld(ax, ay));

            while (a.IsMoving || b.IsMoving) yield return null;
        }

        // Bir oyuncu hamlesi başına en fazla bu kadar match zinciri.
        // Klasik Candy Crush davranışı: düşen balonlar 3+ eşleşme yaparsa
        // onlar da patlar, yenileri düşer, yine 3+ varsa onlar da patlar...
        // 20 = sonsuza yakın (safety limit, infinite loop koruması)
        private const int MaxCascadeChain = 20;

        public IEnumerator ResolveMatchesLoop(System.Action<int> onChainComplete = null)
        {
            int chain = 0;
            while (true)
            {
                var matches = matchFinder.FindAllMatches();
                if (matches.Count == 0) break;
                if (chain >= MaxCascadeChain) break; // otomatik cascade'i kes
                chain++;
                yield return PopMatches(matches);
                yield return CollapseColumns();
                yield return RefillTop();
                // Cascade'ler arası kısa nefes → neyin patladığı net görünür ama akıcı kalır
                if (chain >= 1) yield return new WaitForSeconds(0.30f);
            }
            onChainComplete?.Invoke(chain);
        }

        private IEnumerator PopMatches(List<MatchGroup> matches)
        {
            var toPop = new HashSet<Balloon>();
            var perColor = new Dictionary<BalloonType, int>();
            var promotedCells = new HashSet<Cell>();

            foreach (var group in matches)
            {
                var specialType = SpecialBalloonResolver.DetermineSpecial(group);
                if (specialType == SpecialType.Normal) continue;
                var centerCell = group.Cells[group.Cells.Count / 2];
                if (centerCell.HasBalloon)
                {
                    centerCell.CurrentBalloon.SetSpecial(specialType);
                    promotedCells.Add(centerCell);
                }
            }

            foreach (var group in matches)
            {
                foreach (var cell in group.Cells)
                {
                    if (promotedCells.Contains(cell)) continue;
                    AddToPop(cell.CurrentBalloon, toPop, perColor);
                }
            }

            var specialsToActivate = new Queue<Balloon>();
            var activatedSpecials = new HashSet<Balloon>();

            // YENİ promote edilen bombalar bu turda TETİKLENMEMELİ (yerlerinde kalıp
            // sonraki turda oyuncu kullansın). Bunları "zaten işlendi" işaretle.
            foreach (var pc in promotedCells)
                if (pc.HasBalloon && pc.CurrentBalloon.Special != SpecialType.Normal)
                    activatedSpecials.Add(pc.CurrentBalloon);

            foreach (var b in new List<Balloon>(toPop))
            {
                if (b.Special != SpecialType.Normal)
                    specialsToActivate.Enqueue(b);
                // Match olan balonun yan komşusundaki ESKİ BOMBA tetiklenir (4-yönlü).
                // Bu turda promote edilen yeni bombalar exclude edilir (yukarıda işaretlendi).
                QueueAdjacentBombsCardinal(b.X, b.Y, specialsToActivate, activatedSpecials, toPop);
            }

            while (specialsToActivate.Count > 0)
            {
                var special = specialsToActivate.Dequeue();
                if (activatedSpecials.Contains(special)) continue;
                activatedSpecials.Add(special);

                AddToPop(special, toPop, perColor);

                if (special.Special == SpecialType.Bomb)
                {
                    var pos = GridToWorld(special.X, special.Y);
                    FlashEffect.Spawn(flashPrefab, pos, new Color(1f, 0.95f, 0.7f), 3.2f, 0.18f);
                    ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(1f, 0.85f, 0.4f), 5f);
                    BoomEffect.Spawn(boomEffectPrefab, pos, "BOOM!");
                    if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(bombShakeAmount, 8f);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                    if (AchievementManager.Instance != null) AchievementManager.Instance.NotifyBombTriggered();
                    if (StatsTracker.Instance != null) StatsTracker.Instance.NotifyBomb();
                }
                else if (special.Special == SpecialType.Rainbow)
                {
                    BoomEffect.Spawn(boomEffectPrefab, GridToWorld(special.X, special.Y), "RAINBOW!");
                }
                else if (special.Special == SpecialType.LineH || special.Special == SpecialType.LineV)
                {
                    BoomEffect.Spawn(boomEffectPrefab, GridToWorld(special.X, special.Y), "ZAP!");
                }

                var affected = SpecialBalloonResolver.CollectAffectedCells(this, special);
                foreach (var cell in affected)
                {
                    if (!cell.HasBalloon || cell.CurrentBalloon == special) continue;
                    var nb = cell.CurrentBalloon;
                    if (nb.Special != SpecialType.Normal && !activatedSpecials.Contains(nb))
                        specialsToActivate.Enqueue(nb);
                    else
                        AddToPop(nb, toPop, perColor);
                }
            }

            GameEvents.RaiseMatchMade(toPop.Count);
            if (GameManager.Instance != null)
            {
                foreach (var kv in perColor)
                    GameManager.Instance.UpdateGoal(kv.Key, kv.Value);
            }

            SpawnParticlesForPopped(toPop);
            SpawnScorePopups(matches, toPop);
            SpawnFlameRibbons(matches);

            var coroutines = new List<Coroutine>();
            foreach (var b in toPop)
            {
                int bx = b.X, by = b.Y;
                if (IsInside(bx, by) && cells[bx, by].CurrentBalloon == b)
                    cells[bx, by].CurrentBalloon = null;
                coroutines.Add(StartCoroutine(b.PopRoutine()));
            }
            foreach (var c in coroutines) yield return c;

            foreach (var b in toPop) ReturnToPool(b);
        }

        private void SpawnParticlesForPopped(HashSet<Balloon> popped)
        {
            if (particlePrefab == null) return;
            foreach (var b in popped)
            {
                var color = GetVisualColor(b);
                var pos = GridToWorld(b.X, b.Y);
                ParticleEffect.Spawn(particlePrefab, pos, color);
                if (flashPrefab != null)
                    FlashEffect.Spawn(flashPrefab, pos, Color.white, 1.0f, 0.18f);
            }
            if (popped.Count >= 3 && CameraFitter.Instance != null)
                CameraFitter.Instance.Shake(popShakeAmount * Mathf.Min(popped.Count, 6));
        }

        private void SpawnScorePopups(List<MatchGroup> matches, HashSet<Balloon> popped)
        {
            if (scorePopupPrefab == null) return;
            int amount = popped.Count * 60;
            if (matches.Count == 0) return;
            var first = matches[0].Cells.Count > 0 ? matches[0].Cells[0] : null;
            if (first == null) return;
            ScorePopup.Spawn(scorePopupPrefab, GridToWorld(first.X, first.Y) + Vector3.up * 0.5f, $"+{amount}");
        }

        // Her eşleşme MatchFinder tarafından zaten düz bir yatay/dikey dizi olarak bulunur;
        // grubun ilk ve son hücresi çizginin iki ucudur. Her çizgi için üzerinden alevli bir
        // şerit geçiririz (start -> end). Renk boyalı sprite'tan gelir (tint = beyaz).
        private void SpawnFlameRibbons(List<MatchGroup> matches)
        {
            if (flameRibbonFrames == null || flameRibbonFrames.Length == 0 || matches == null) return;
            foreach (var group in matches)
            {
                if (group.Cells == null || group.Cells.Count < 3) continue;
                var startCell = group.Cells[0];
                var endCell = group.Cells[group.Cells.Count - 1];
                Vector3 a = GridToWorld(startCell.X, startCell.Y);
                Vector3 b = GridToWorld(endCell.X, endCell.Y);
                // uzunluk = patlayan balon sayısı: span=(N-1)*cellSize, +cellSize taşma => tam N hücre kapanır
                FlameRibbonEffect.Spawn(flameRibbonFrames, a, b, cellSize * 2.4f, cellSize);
            }
        }

        private static readonly Color[] BalloonColorPalette = {
            Color.clear,
            new Color(1.00f, 0.28f, 0.34f),
            new Color(0.31f, 0.80f, 0.92f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(1.00f, 0.85f, 0.24f),
            new Color(0.65f, 0.37f, 0.92f),
            new Color(1.00f, 0.55f, 0.26f),
            new Color(1.00f, 0.42f, 0.62f),
        };

        private static Color GetVisualColor(Balloon b)
        {
            if (b == null) return Color.white;
            int idx = (int)b.Type;
            if (idx < 0 || idx >= BalloonColorPalette.Length) return Color.white;
            return BalloonColorPalette[idx];
        }

        private void AddToPop(Balloon b, HashSet<Balloon> toPop, Dictionary<BalloonType, int> perColor)
        {
            if (b == null || b.IsMarkedForPop) return;
            b.IsMarkedForPop = true;
            toPop.Add(b);
            if (b.Special == SpecialType.Normal)
            {
                perColor.TryGetValue(b.Type, out int existing);
                perColor[b.Type] = existing + 1;
            }
            if (b.Special == SpecialType.Gold)
            {
                BalloonPop.Save.SaveSystem.AddCoins(15);
                if (boomEffectPrefab != null)
                    BoomEffect.Spawn(boomEffectPrefab, GridToWorld(b.X, b.Y), "+15!");
            }

            var cell = cells[b.X, b.Y];
            if (cell.IceLayers > 0)
            {
                cell.IceLayers--;
                if (cell.IceLayers == 0 && GameManager.Instance != null)
                    GameManager.Instance.ReportIceCleared();
                if (BalloonPop.Effects.ObstacleVisualizer.Instance != null)
                    BalloonPop.Effects.ObstacleVisualizer.Instance.RefreshIceAt(b.X, b.Y);
            }
        }

        private void QueueAdjacentBombs(int x, int y, Queue<Balloon> queue, HashSet<Balloon> processed, HashSet<Balloon> alreadyPopping)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (!IsInside(nx, ny)) continue;
                    var c = cells[nx, ny];
                    if (!c.HasBalloon) continue;
                    var nb = c.CurrentBalloon;
                    if (nb.Special == SpecialType.Bomb && !processed.Contains(nb))
                        queue.Enqueue(nb);
                }
            }
        }

        private void QueueAdjacentBombsCardinal(int x, int y, Queue<Balloon> queue, HashSet<Balloon> processed, HashSet<Balloon> alreadyPopping)
        {
            int[] dxs = { 1, -1, 0, 0 };
            int[] dys = { 0, 0, 1, -1 };
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dxs[d], ny = y + dys[d];
                if (!IsInside(nx, ny)) continue;
                var c = cells[nx, ny];
                if (!c.HasBalloon) continue;
                var nb = c.CurrentBalloon;
                if (nb.Special == SpecialType.Bomb && !processed.Contains(nb))
                    queue.Enqueue(nb);
            }
        }

        private IEnumerator CollapseColumns()
        {
            bool anyMoved = false;
            for (int x = 0; x < width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].IsBlocked) { writeY = y + 1; continue; }
                    if (cells[x, y].HasBalloon)
                    {
                        if (writeY != y)
                        {
                            var b = cells[x, y].CurrentBalloon;
                            cells[x, y].CurrentBalloon = null;
                            cells[x, writeY].CurrentBalloon = b;
                            b.SetGridPosition(x, writeY);
                            b.MoveTo(GridToWorld(x, writeY));
                            anyMoved = true;
                        }
                        writeY++;
                    }
                }
            }
            if (anyMoved) yield return WaitForAllStill();
        }

        private IEnumerator RefillTop()
        {
            var spawned = new List<Balloon>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].IsBlocked || cells[x, y].HasBalloon) continue;
                    var type = PickNonMatchingType(x, y);
                    var spawnPos = GridToWorld(x, height + (y - height) + 2);
                    var b = SpawnBalloon(x, y, type);

                    // Yan/alt komşusunda zaten bomba/special varsa, bu cell'e special spawn etme
                    // (yan yana bombalar otomatik zincirleme patlama yapıyor — bunu önle).
                    bool hasSpecialNeighbor = HasSpecialNeighbor(x, y);
                    if (!hasSpecialNeighbor)
                    {
                        float r = Random.value;
                        if (r < goldSpawnChance * 0.2f) b.SetSpecial(SpecialType.Gold);
                        // Bomba multipliyer 0.25 → 0.7 (görünür sıklıkta çıksın). Etkin: ~7%
                        else if (r < (goldSpawnChance * 0.2f) + (bombSpawnChance * 0.7f)) b.SetSpecial(SpecialType.Bomb);
                    }

                    b.transform.position = spawnPos;
                    b.MoveTo(GridToWorld(x, y));
                    spawned.Add(b);
                }
            }
            if (spawned.Count > 0) yield return WaitForAllStill();
        }

        private IEnumerator WaitForAllStill()
        {
            bool stillMoving = true;
            while (stillMoving)
            {
                stillMoving = false;
                for (int x = 0; x < width && !stillMoving; x++)
                {
                    for (int y = 0; y < height && !stillMoving; y++)
                    {
                        if (cells[x, y].HasBalloon && cells[x, y].CurrentBalloon.IsMoving)
                            stillMoving = true;
                    }
                }
                yield return null;
            }
        }

        public IEnumerator TriggerSpecialCombo(int ax, int ay, int bx, int by)
        {
            var a = cells[ax, ay].CurrentBalloon;
            var b = cells[bx, by].CurrentBalloon;
            if (a == null || b == null) yield break;

            var pos = GridToWorld(bx, by);
            var cellsToPop = new HashSet<Cell>();
            bool aBomb = a.Special == SpecialType.Bomb;
            bool bBomb = b.Special == SpecialType.Bomb;
            bool aLine = a.Special == SpecialType.LineH || a.Special == SpecialType.LineV;
            bool bLine = b.Special == SpecialType.LineH || b.Special == SpecialType.LineV;
            bool aRainbow = a.Special == SpecialType.Rainbow;
            bool bRainbow = b.Special == SpecialType.Rainbow;

            // Her kombo tipi için ayrı görsel efekt
            if (aRainbow && bRainbow)
            {
                // Tüm tahta patlat + cinematic
                BoomEffect.Spawn(boomEffectPrefab, pos, "OMG!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(1f, 1f, 1f), 12f, 0.4f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(0.9f, 0.5f, 1f), 14f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(1.4f, 10f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                StartCoroutine(TimeScaleBlip(0.25f, 0.5f));
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        if (cells[x, y].HasBalloon) cellsToPop.Add(cells[x, y]);
            }
            else if (aRainbow || bRainbow)
            {
                // O rengin tümü + her birine küçük flash
                var otherType = aRainbow ? b.Type : a.Type;
                BoomEffect.Spawn(boomEffectPrefab, pos, "RAINBOW!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(1f, 0.6f, 1f), 6f, 0.3f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(0.9f, 0.5f, 1f), 8f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(0.5f, 7f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        if (cells[x, y].HasBalloon && cells[x, y].CurrentBalloon.Type == otherType)
                        {
                            cellsToPop.Add(cells[x, y]);
                            FlashEffect.Spawn(flashPrefab, GridToWorld(x, y), new Color(1f, 0.8f, 1f), 1.5f, 0.15f);
                        }
            }
            else if (aBomb && bBomb)
            {
                // 5x5 mega patlama
                BoomEffect.Spawn(boomEffectPrefab, pos, "BIG BOOM!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(1f, 0.85f, 0.4f), 7f, 0.35f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(1f, 0.5f, 0.1f), 10f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(1.0f, 8f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                for (int x = bx - 2; x <= bx + 2; x++)
                    for (int y = by - 2; y <= by + 2; y++)
                        if (IsInside(x, y) && cells[x, y].HasBalloon) cellsToPop.Add(cells[x, y]);
            }
            else if ((aBomb && bLine) || (aLine && bBomb))
            {
                // 3 sıra haç şekilli
                BoomEffect.Spawn(boomEffectPrefab, pos, "CROSS!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(0.6f, 0.9f, 1f), 6f, 0.3f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(0.3f, 0.8f, 1f), 9f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(0.8f, 7f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                for (int x = 0; x < width; x++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (IsInside(x, by + dy) && cells[x, by + dy].HasBalloon) cellsToPop.Add(cells[x, by + dy]);
                for (int y = 0; y < height; y++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (IsInside(bx + dx, y) && cells[bx + dx, y].HasBalloon) cellsToPop.Add(cells[bx + dx, y]);
            }
            else if (aLine && bLine)
            {
                // Çift çizgi: tam satır + tam sütun
                BoomEffect.Spawn(boomEffectPrefab, pos, "STRIKE!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(0.7f, 1f, 0.7f), 5f, 0.28f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(0.3f, 1f, 0.5f), 8f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(0.6f, 6f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                for (int x = 0; x < width; x++)
                    if (IsInside(x, by) && cells[x, by].HasBalloon) cellsToPop.Add(cells[x, by]);
                for (int y = 0; y < height; y++)
                    if (IsInside(bx, y) && cells[bx, y].HasBalloon) cellsToPop.Add(cells[bx, y]);
            }
            else
            {
                // Generic kombo (3×3)
                BoomEffect.Spawn(boomEffectPrefab, pos, "MEGA!");
                FlashEffect.Spawn(flashPrefab, pos, new Color(1f, 1f, 0.8f), 4.5f, 0.25f);
                ShockwaveEffect.Spawn(shockwavePrefab, pos, new Color(1f, 0.85f, 0.4f), 7f);
                if (CameraFitter.Instance != null) CameraFitter.Instance.Shake(0.7f, 6f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("bomb");
                VibrateShort();
                for (int x = bx - 1; x <= bx + 1; x++)
                    for (int y = by - 1; y <= by + 1; y++)
                        if (IsInside(x, y) && cells[x, y].HasBalloon) cellsToPop.Add(cells[x, y]);
            }

            cellsToPop.Add(cells[ax, ay]);
            cellsToPop.Add(cells[bx, by]);

            yield return PopCells(cellsToPop);
            yield return CollapseColumns();
            yield return RefillTop();
            yield return ResolveMatchesLoop(null);
        }

        private IEnumerator TimeScaleBlip(float scale, float duration)
        {
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        public IEnumerator ActivateSingleSpecial(int ax, int ay, int bx, int by)
        {
            var a = cells[ax, ay].CurrentBalloon;
            var b = cells[bx, by].CurrentBalloon;
            Balloon trigger = null;
            if (a != null && a.Special != SpecialType.Normal && a.Special != SpecialType.Gold) trigger = a;
            else if (b != null && b.Special != SpecialType.Normal && b.Special != SpecialType.Gold) trigger = b;
            if (trigger == null) yield break;

            var affected = SpecialBalloonResolver.CollectAffectedCells(this, trigger);
            var set = new HashSet<Cell>(affected) { cells[trigger.X, trigger.Y] };
            yield return PopCells(set);
            yield return CollapseColumns();
            yield return RefillTop();
        }

        private IEnumerator PopCells(HashSet<Cell> set)
        {
            var perColor = new Dictionary<BalloonType, int>();
            int goldBonus = 0;
            var coroutines = new List<Coroutine>();
            foreach (var c in set)
            {
                if (!c.HasBalloon) continue;
                var bb = c.CurrentBalloon;
                if (bb.Special == SpecialType.Gold) goldBonus += 15;
                if (bb.Special == SpecialType.Normal)
                {
                    perColor.TryGetValue(bb.Type, out int existing);
                    perColor[bb.Type] = existing + 1;
                }
                cells[c.X, c.Y].CurrentBalloon = null;
                coroutines.Add(StartCoroutine(bb.PopRoutine()));
            }
            if (GameManager.Instance != null)
                foreach (var kv in perColor) GameManager.Instance.UpdateGoal(kv.Key, kv.Value);
            GameEvents.RaiseMatchMade(set.Count);
            if (goldBonus > 0)
            {
                BalloonPop.Save.SaveSystem.AddCoins(goldBonus);
                if (boomEffectPrefab != null && set.Count > 0)
                {
                    var first = System.Linq.Enumerable.First(set);
                    BoomEffect.Spawn(boomEffectPrefab, GridToWorld(first.X, first.Y), $"+{goldBonus}!");
                }
            }
            foreach (var c in coroutines) yield return c;
            foreach (var c in set)
                if (c.CurrentBalloon != null) ReturnToPool(c.CurrentBalloon);
        }

        private static void VibrateShort()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch { }
#endif
        }

        public void HammerCell(int x, int y)
        {
            if (!IsInside(x, y)) return;
            var cell = cells[x, y];
            if (!cell.HasBalloon) return;
            var b = cell.CurrentBalloon;
            cell.CurrentBalloon = null;
            var pos = GridToWorld(x, y);
            if (particlePrefab != null) ParticleEffect.Spawn(particlePrefab, pos, GetVisualColor(b));
            if (flashPrefab != null) FlashEffect.Spawn(flashPrefab, pos, Color.white, 1.2f, 0.2f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("pop");
            StartCoroutine(HammerThenResolve(b));
        }

        private System.Collections.IEnumerator HammerThenResolve(Balloon b)
        {
            yield return b.PopRoutine();
            ReturnToPool(b);
            yield return ResolveMatchesLoop(null);
        }

        public void ShuffleBoard()
        {
            var allBalloons = new List<Balloon>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (cells[x, y].HasBalloon && cells[x, y].CurrentBalloon.Special == SpecialType.Normal)
                        allBalloons.Add(cells[x, y].CurrentBalloon);

            for (int i = allBalloons.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allBalloons[i], allBalloons[j]) = (allBalloons[j], allBalloons[i]);
            }

            int idx = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!cells[x, y].HasBalloon) continue;
                    if (cells[x, y].CurrentBalloon.Special != SpecialType.Normal) continue;
                    if (idx >= allBalloons.Count) continue;
                    var b = allBalloons[idx++];
                    cells[x, y].CurrentBalloon = b;
                    b.SetGridPosition(x, y);
                    b.MoveTo(GridToWorld(x, y));
                }
            }
        }

        public bool AreAdjacent(int ax, int ay, int bx, int by)
        {
            int dx = Mathf.Abs(ax - bx);
            int dy = Mathf.Abs(ay - by);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
