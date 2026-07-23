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

                PlaySpecialFx(special);

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

            SpawnScorePopups(matches, toPop);

            // One camera shake for the whole pop (per-balloon FX now happens as each pops).
            if (toPop.Count >= 3 && CameraFitter.Instance != null)
                CameraFitter.Instance.Shake(popShakeAmount * Mathf.Min(toPop.Count, 6));

            // Split the matched balloons into connected clusters; send ONE laser snaking over
            // each cluster and pop every balloon the instant the laser tip reaches it, so the
            // pops stay in sync with the wandering laser instead of all bursting at once.
            var clusters = BuildLaserPaths(matches, toPop);
            var onPath = new HashSet<Balloon>();
            var coroutines = new List<Coroutine>();
            foreach (var cluster in clusters)
            {
                var pts = new List<Vector3>(cluster.Count);
                foreach (var b in cluster) pts.Add(GridToWorld(b.X, b.Y));
                float travel;
                var arrive = LaserArrivalTimes(pts, out travel);
                // 1.95 = bolt kalınlığı 1.3 + neon halesi için dokuya eklenen dikey pay (×1.5)
                FlameRibbonEffect.SpawnPath(pts, cellSize * 1.95f, null, travel);
                // Yürüyüş geri izleme yüzünden aynı balonu birden çok kez içerebilir:
                // her balonu SADECE ilk uğranıldığı anda patlat.
                for (int i = 0; i < cluster.Count; i++)
                    if (onPath.Add(cluster[i]))
                        coroutines.Add(StartCoroutine(PopBalloonAt(cluster[i], arrive[i])));
            }
            // Special / bomb chain-reaction balloons aren't part of the traced line — pop now.
            foreach (var b in toPop)
                if (!onPath.Contains(b)) coroutines.Add(StartCoroutine(PopBalloonAt(b, 0f)));

            foreach (var c in coroutines) yield return c;
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

        // Bir balonu, lazer ucu ona vardığında (delay) patlatır: hücreyi boşaltır, patlama
        // efektini o an çıkarır, pop animasyonunu oynatır ve balonu havuza döndürür.
        private System.Collections.IEnumerator PopBalloonAt(Balloon b, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (b == null) yield break;
            int bx = b.X, by = b.Y;
            if (IsInside(bx, by) && cells[bx, by].CurrentBalloon == b)
                cells[bx, by].CurrentBalloon = null;
            SpawnPopFxFor(b);
            yield return b.PopRoutine();
            ReturnToPool(b);
        }

        // Tek bir balonun patlama efekti (parçacık + flash) — patladığı anda çıkar.
        private void SpawnPopFxFor(Balloon b)
        {
            if (b == null) return;
            var color = GetVisualColor(b);
            var pos = GridToWorld(b.X, b.Y);
            if (particlePrefab != null) ParticleEffect.Spawn(particlePrefab, pos, color);
            if (flashPrefab != null) FlashEffect.Spawn(flashPrefab, pos, Color.white, 1.0f, 0.18f);
        }

        // Bir özel balonun tetiklenme efektleri (BOOM/flash/şok dalgası/sarsıntı/ses).
        // Hem match zincirinde hem takasla patlatmada kullanılır, böylece ZİNCİRLENEN
        // bombalar da kendi efektlerini oynatır.
        private void PlaySpecialFx(Balloon special)
        {
            if (special == null) return;
            var pos = GridToWorld(special.X, special.Y);
            if (special.Special == SpecialType.Bomb)
            {
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
                BoomEffect.Spawn(boomEffectPrefab, pos, "RAINBOW!");
            }
            else if (special.Special == SpecialType.LineH || special.Special == SpecialType.LineV)
            {
                BoomEffect.Spawn(boomEffectPrefab, pos, "ZAP!");
            }
        }

        // Patlayacak eşleşme balonlarını 4-komşulukla bağlı kümelere ayırır; her kümeyi
        // en-yakın-komşu "yılan" sırasına dizer ki tek bir lazer hepsinin üzerinden geçebilsin.
        private List<List<Balloon>> BuildLaserPaths(List<MatchGroup> matches, HashSet<Balloon> toPop)
        {
            var byCoord = new Dictionary<int, Balloon>();
            foreach (var group in matches)
            {
                if (group.Cells == null) continue;
                foreach (var cell in group.Cells)
                {
                    if (cell == null || !cell.HasBalloon) continue;
                    var b = cell.CurrentBalloon;
                    if (!toPop.Contains(b)) continue;      // özel'e terfi eden hücreler patlamaz
                    byCoord[cell.X * 100 + cell.Y] = b;
                }
            }

            var result = new List<List<Balloon>>();
            var visited = new HashSet<int>();
            foreach (var kv in byCoord)
            {
                if (visited.Contains(kv.Key)) continue;
                var comp = new List<Balloon>();
                var stack = new Stack<int>();
                stack.Push(kv.Key); visited.Add(kv.Key);
                while (stack.Count > 0)
                {
                    int k = stack.Pop();
                    comp.Add(byCoord[k]);
                    int x = k / 100, y = k % 100;
                    TryNeighbour(byCoord, visited, stack, (x + 1) * 100 + y);
                    TryNeighbour(byCoord, visited, stack, (x - 1) * 100 + y);
                    TryNeighbour(byCoord, visited, stack, x * 100 + (y + 1));
                    TryNeighbour(byCoord, visited, stack, x * 100 + (y - 1));
                }
                result.Add(BuildAdjacencyWalk(comp));
            }
            return result;
        }

        private static void TryNeighbour(Dictionary<int, Balloon> byCoord, HashSet<int> visited, Stack<int> stack, int key)
        {
            if (byCoord.ContainsKey(key) && !visited.Contains(key)) { visited.Add(key); stack.Push(key); }
        }

        // Kümeyi SADECE komşu adımlarla gezen bir yürüyüş üretir (DFS + geri izleme).
        // Ardışık iki nokta her zaman 4-komşudur, dolayısıyla lazer çizgisi ASLA patlamayan
        // bir balonun üstünden atlamaz. (Eski "en yakın komşu" sıralaması L/T gibi şekillerde
        // komşu olmayan hücreler arasında atlıyor, aradaki patlamayan balonu kesiyordu.)
        // Geri izlemede hücreler tekrar ziyaret edilebilir; balon İLK ziyaretinde patlar.
        private static List<Balloon> BuildAdjacencyWalk(List<Balloon> comp)
        {
            var byKey = new Dictionary<int, Balloon>();
            foreach (var b in comp) byKey[b.X * 100 + b.Y] = b;

            var start = comp[0];
            foreach (var b in comp)
                if (b.Y < start.Y || (b.Y == start.Y && b.X < start.X)) start = b;

            var walk = new List<Balloon>();
            DfsWalk(start, byKey, new HashSet<int>(), walk);

            // Sondaki geri izleme kuyruğu yeni hücre getirmez — lazer boşuna geri dönmesin diye kırp.
            var seen = new HashSet<Balloon>();
            int lastNew = 0;
            for (int i = 0; i < walk.Count; i++) if (seen.Add(walk[i])) lastNew = i;
            if (lastNew < walk.Count - 1) walk.RemoveRange(lastNew + 1, walk.Count - 1 - lastNew);
            return walk;
        }

        private static void DfsWalk(Balloon cur, Dictionary<int, Balloon> byKey, HashSet<int> visited, List<Balloon> walk)
        {
            visited.Add(cur.X * 100 + cur.Y);
            walk.Add(cur);
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            for (int d = 0; d < 4; d++)
            {
                int nk = (cur.X + dx[d]) * 100 + (cur.Y + dy[d]);
                Balloon nb;
                if (!byKey.TryGetValue(nk, out nb) || visited.Contains(nk)) continue;
                DfsWalk(nb, byKey, visited, walk);
                walk.Add(cur);   // geri izleme: komşuluk bozulmasın diye üstünden geri dön
            }
        }

        // Sabit hızlı geçiş: her yol noktasının varış anı + toplam süre. Lazer ucu da aynı
        // modelle ilerlediği için balon patlamaları lazerle birebir senkron kalır.
        private float[] LaserArrivalTimes(List<Vector3> pts, out float travel)
        {
            int n = pts.Count;
            var cum = new float[n];
            cum[0] = 0f;
            for (int i = 1; i < n; i++) cum[i] = cum[i - 1] + Vector3.Distance(pts[i - 1], pts[i]);
            float total = cum[n - 1];
            travel = Mathf.Clamp(total * 0.08f, 0.16f, 0.75f);
            var arrive = new float[n];
            for (int i = 0; i < n; i++) arrive[i] = total > 1e-4f ? (cum[i] / total) * travel : 0f;
            return arrive;
        }

        // Bir hücre kümesinin (bomba 3x3, çizgi, rainbow) üzerinden yıldırım geçirir.
        // Küme 4-komşulukla bağlı parçalara ayrılır ve HER PARÇA İÇİN TEK bir yıldırım
        // hepsinin üstünden dolaşır (eskiden satır satır ayrı düz çizgiler çıkıyordu).
        // Dönen sözlük: her balonun, yıldırım ucunun ona varış anı → patlamalar senkron olur.
        private Dictionary<Balloon, float> SpawnBoltsForCells(ICollection<Cell> cellSet)
        {
            var delays = new Dictionary<Balloon, float>();
            if (cellSet == null) return delays;

            var byCoord = new Dictionary<int, Balloon>();
            foreach (var c in cellSet)
            {
                if (c == null || !c.HasBalloon) continue;
                byCoord[c.X * 100 + c.Y] = c.CurrentBalloon;
            }
            if (byCoord.Count == 0) return delays;

            var visited = new HashSet<int>();
            foreach (var kv in byCoord)
            {
                if (visited.Contains(kv.Key)) continue;

                var comp = new List<Balloon>();
                var stack = new Stack<int>();
                stack.Push(kv.Key); visited.Add(kv.Key);
                while (stack.Count > 0)
                {
                    int k = stack.Pop();
                    comp.Add(byCoord[k]);
                    int x = k / 100, y = k % 100;
                    TryNeighbour(byCoord, visited, stack, (x + 1) * 100 + y);
                    TryNeighbour(byCoord, visited, stack, (x - 1) * 100 + y);
                    TryNeighbour(byCoord, visited, stack, x * 100 + (y + 1));
                    TryNeighbour(byCoord, visited, stack, x * 100 + (y - 1));
                }

                var walk = BuildAdjacencyWalk(comp);
                var pts = new List<Vector3>(walk.Count);
                foreach (var b in walk) pts.Add(GridToWorld(b.X, b.Y));
                float travel;
                var arrive = LaserArrivalTimes(pts, out travel);
                FlameRibbonEffect.SpawnPath(pts, cellSize * 1.95f, null, travel);
                for (int i = 0; i < walk.Count; i++)
                    if (!delays.ContainsKey(walk[i])) delays[walk[i]] = arrive[i];   // ilk uğrayış
            }
            return delays;
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

            // ZİNCİR: etki alanına giren BAŞKA bir özel balon da kendi etkisini tetikler.
            // (Eskiden sadece tetikleyicinin alanı toplanıyordu; alandaki 2. bomba sıradan
            //  balon gibi patlayıp etkisi kayboluyordu.)
            var set = new HashSet<Cell>();
            var queue = new Queue<Balloon>();
            var activated = new HashSet<Balloon>();
            queue.Enqueue(trigger);
            while (queue.Count > 0)
            {
                var sp = queue.Dequeue();
                if (sp == null || activated.Contains(sp)) continue;
                activated.Add(sp);

                if (IsInside(sp.X, sp.Y)) set.Add(cells[sp.X, sp.Y]);
                PlaySpecialFx(sp);

                foreach (var cell in SpecialBalloonResolver.CollectAffectedCells(this, sp))
                {
                    if (cell == null) continue;
                    set.Add(cell);
                    if (!cell.HasBalloon) continue;
                    var nb = cell.CurrentBalloon;
                    if (nb != sp && nb.Special != SpecialType.Normal && nb.Special != SpecialType.Gold
                        && !activated.Contains(nb))
                        queue.Enqueue(nb);
                }
            }

            // Patlama alanının üzerinden TEK PARÇA yıldırım geçir (eskiden satır satır ayrı
            // düz çizgiler çıkıyordu) ve balonları yıldırım onlara vardıkça patlat — match'lerdeki gibi.
            if (set.Count >= 3 && CameraFitter.Instance != null)
                CameraFitter.Instance.Shake(popShakeAmount * Mathf.Min(set.Count, 6));
            var delays = SpawnBoltsForCells(set);

            yield return PopCells(set, delays);
            yield return CollapseColumns();
            yield return RefillTop();
        }

        private IEnumerator PopCells(HashSet<Cell> set, Dictionary<Balloon, float> delays = null)
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
                // Hücreyi boşaltma + patlama efekti + havuza dönüş PopBalloonAt içinde;
                // gecikme = yıldırımın o balona varış anı (yoksa 0 → anında).
                float delay = 0f;
                if (delays != null) delays.TryGetValue(bb, out delay);
                coroutines.Add(StartCoroutine(PopBalloonAt(bb, delay)));
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
            // Çekiç tek balon patlatır (match oluşmaz) → boşluğu yine de doldur:
            // önce sütunu aşağı kaydır + üstten yeni balon ekle, sonra oluşan match'leri çöz.
            yield return CollapseColumns();
            yield return RefillTop();
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

            // Karıştırma sonucu oluşan 3+ eşleşmeler oyuncu müdahale etmeden patlasın.
            StartCoroutine(ResolveAfterShuffle());
        }

        private System.Collections.IEnumerator ResolveAfterShuffle()
        {
            yield return new WaitForSeconds(0.30f);   // balonlar yeni yerlerine otursun
            yield return ResolveMatchesLoop(null);
        }

        public bool AreAdjacent(int ax, int ay, int bx, int by)
        {
            int dx = Mathf.Abs(ax - bx);
            int dy = Mathf.Abs(ay - by);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
