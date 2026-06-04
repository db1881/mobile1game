using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Effects;
using BalloonPop.Grid;
using BalloonPop.InputSystem;
using BalloonPop.Save;

namespace BalloonPop.Gameplay
{
    public enum BoosterType
    {
        Hammer,
        Shuffle,
        MovePack
    }

    public class BoosterManager : Singleton<BoosterManager>
    {
        [SerializeField] private GameObject hammerCursorPrefab;

        public bool IsAwaitingTarget => awaitingType == BoosterType.Hammer && awaitingHammer;

        private BoosterType awaitingType;
        private bool awaitingHammer;

        public bool TryUseShuffle()
        {
            if (SaveSystem.Data.Shuffles <= 0) return false;
            SaveSystem.Data.Shuffles--;
            SaveSystem.Save();
            DoShuffle();
            return true;
        }

        public bool TryUseMovePack()
        {
            if (SaveSystem.Data.MovePacks <= 0) return false;
            if (GameManager.Instance == null) return false;
            SaveSystem.Data.MovePacks--;
            SaveSystem.Save();
            GameManager.Instance.AddMoves(5);
            return true;
        }

        public bool BeginHammer()
        {
            if (SaveSystem.Data.Hammers <= 0) return false;
            awaitingType = BoosterType.Hammer;
            awaitingHammer = true;
            return true;
        }

        public void CancelHammer()
        {
            awaitingHammer = false;
        }

        public void TryHammerAt(int x, int y)
        {
            if (!awaitingHammer) return;
            var grid = GridManager.Instance;
            if (grid == null) return;
            if (!grid.IsInside(x, y)) return;
            var cell = grid.Cells[x, y];
            if (!cell.HasBalloon) return;

            SaveSystem.Data.Hammers--;
            SaveSystem.Save();
            awaitingHammer = false;

            grid.HammerCell(x, y);
        }

        private void DoShuffle()
        {
            var grid = GridManager.Instance;
            if (grid == null) return;
            grid.ShuffleBoard();
        }
    }
}
