using System.Collections;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Grid;
using BalloonPop.InputSystem;

namespace BalloonPop.Gameplay
{
    public class GameplayController : Singleton<GameplayController>
    {
        [SerializeField] private GridManager grid;

        public bool IsBusy { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (grid == null) grid = GridManager.Instance;
        }

        public void TryMakeMove(int ax, int ay, int bx, int by)
        {
            if (IsBusy) return;
            if (!grid.AreAdjacent(ax, ay, bx, by)) return;
            StartCoroutine(MoveRoutine(ax, ay, bx, by));
        }

        private IEnumerator MoveRoutine(int ax, int ay, int bx, int by)
        {
            IsBusy = true;
            InputManager.Instance.InputEnabled = false;

            yield return grid.SwapBalloons(ax, ay, bx, by);

            // Bir özel balonu (bomba/rainbow/çizgi) bir komşunun üstüne atınca ANINDA patlar —
            // match aramaya gerek yok. (Bu "swap ile patlat" mekaniği önceden kapalıydı, geri açıldı.)
            var ba = grid.Cells[ax, ay].CurrentBalloon;
            var bb = grid.Cells[bx, by].CurrentBalloon;
            bool special = (ba != null && ba.Special != SpecialType.Normal && ba.Special != SpecialType.Gold)
                        || (bb != null && bb.Special != SpecialType.Normal && bb.Special != SpecialType.Gold);

            if (special)
            {
                GameManager.Instance?.ConsumeMove();
                yield return grid.ActivateSingleSpecial(ax, ay, bx, by);
                int chain = 0;
                yield return grid.ResolveMatchesLoop(c => chain = c);
                if (chain > 1) GameEvents.RaiseComboChain(chain);
                GameManager.Instance?.CheckLevelCompletion();
            }
            else
            {
                // Klasik match-3: swap sadece düz çizgi match oluşturursa kabul edilir.
                var preChain = new MatchFinder(grid);
                var matches = preChain.FindAllMatches();
                if (matches.Count == 0)
                {
                    // Geri al
                    yield return grid.SwapBalloons(ax, ay, bx, by);
                }
                else
                {
                    GameManager.Instance?.ConsumeMove();
                    int totalChain = 0;
                    yield return grid.ResolveMatchesLoop(c => totalChain = c);
                    if (totalChain > 1) GameEvents.RaiseComboChain(totalChain);
                    GameManager.Instance?.CheckLevelCompletion();
                }
            }

            InputManager.Instance.InputEnabled = true;
            IsBusy = false;
        }
    }
}
