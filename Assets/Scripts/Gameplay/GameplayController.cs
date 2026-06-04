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

            // Klasik match-3: swap sadece düz çizgi match oluşturursa kabul edilir.
            // Bomba/rainbow/line swap özel mantığı KAPATILDI — bombaya değil,
            // bombanın yanındaki match'i çözmek gerekiyor.
            var preChain = new MatchFinder(grid);
            var matches = preChain.FindAllMatches();
            bool isValid = matches.Count > 0;

            if (!isValid)
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

            InputManager.Instance.InputEnabled = true;
            IsBusy = false;
        }
    }
}
