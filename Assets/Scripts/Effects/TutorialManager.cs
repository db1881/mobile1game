using System.Collections;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private GameObject handPrefab;
        [SerializeField] private int onlyForLevelNumber = 1;
        [SerializeField] private float startDelay = 2f;

        private HandIcon currentHand;
        private bool tutorialActive;

        private void OnEnable()
        {
            GameEvents.OnLevelStarted += OnLevelStarted;
            GameEvents.OnMatchMade += OnMatchMade;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelStarted -= OnLevelStarted;
            GameEvents.OnMatchMade -= OnMatchMade;
        }

        private void OnLevelStarted()
        {
            if (GameManager.Instance == null) return;
            var lvl = GameManager.Instance.CurrentLevel;
            if (lvl == null || lvl.LevelNumber != onlyForLevelNumber) return;
            tutorialActive = true;
            StartCoroutine(ShowHint());
        }

        private void OnMatchMade(int _)
        {
            if (!tutorialActive) return;
            tutorialActive = false;
            if (currentHand != null) currentHand.Hide();
            currentHand = null;
        }

        private IEnumerator ShowHint()
        {
            yield return new WaitForSeconds(startDelay);
            if (!tutorialActive) yield break;
            var hint = FindSuggestedSwap();
            if (hint == null) yield break;
            var grid = GridManager.Instance;
            var a = grid.GridToWorld(hint.Value.ax, hint.Value.ay);
            var b = grid.GridToWorld(hint.Value.bx, hint.Value.by);
            currentHand = HandIcon.Spawn(handPrefab, a, b);
        }

        private struct SwapHint { public int ax, ay, bx, by; }

        private SwapHint? FindSuggestedSwap()
        {
            var grid = GridManager.Instance;
            if (grid == null || grid.Cells == null) return null;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (TestSwap(x, y, 1, 0, out var r)) return r;
                    if (TestSwap(x, y, 0, 1, out r)) return r;
                }
            }
            return null;
        }

        private bool TestSwap(int x, int y, int dx, int dy, out SwapHint hint)
        {
            hint = default;
            var grid = GridManager.Instance;
            int nx = x + dx, ny = y + dy;
            if (!grid.IsInside(nx, ny)) return false;
            if (!grid.Cells[x, y].HasBalloon || !grid.Cells[nx, ny].HasBalloon) return false;

            var a = grid.Cells[x, y].CurrentBalloon;
            var b = grid.Cells[nx, ny].CurrentBalloon;
            if (a.Type == b.Type) return false;

            grid.Cells[x, y].CurrentBalloon = b;
            grid.Cells[nx, ny].CurrentBalloon = a;
            bool hasMatch = QuickMatchCheck(x, y) || QuickMatchCheck(nx, ny);
            grid.Cells[x, y].CurrentBalloon = a;
            grid.Cells[nx, ny].CurrentBalloon = b;

            if (hasMatch) { hint = new SwapHint { ax = x, ay = y, bx = nx, by = ny }; return true; }
            return false;
        }

        private bool QuickMatchCheck(int x, int y)
        {
            var grid = GridManager.Instance;
            if (!grid.Cells[x, y].HasBalloon) return false;
            var type = grid.Cells[x, y].CurrentBalloon.Type;

            int count = 1;
            for (int i = x - 1; i >= 0; i--) { if (grid.Cells[i, y].HasBalloon && grid.Cells[i, y].CurrentBalloon.Type == type) count++; else break; }
            for (int i = x + 1; i < grid.Width; i++) { if (grid.Cells[i, y].HasBalloon && grid.Cells[i, y].CurrentBalloon.Type == type) count++; else break; }
            if (count >= 3) return true;

            count = 1;
            for (int i = y - 1; i >= 0; i--) { if (grid.Cells[x, i].HasBalloon && grid.Cells[x, i].CurrentBalloon.Type == type) count++; else break; }
            for (int i = y + 1; i < grid.Height; i++) { if (grid.Cells[x, i].HasBalloon && grid.Cells[x, i].CurrentBalloon.Type == type) count++; else break; }
            return count >= 3;
        }
    }
}
