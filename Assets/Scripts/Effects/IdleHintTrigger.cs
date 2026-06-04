using System.Collections;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    public class IdleHintTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject handPrefab;
        [SerializeField] private float idleSeconds = 10f;
        [Tooltip("Idle hint balonların üstüne çıkıp kafa karıştırdığı için varsayılan KAPALI.")]
        [SerializeField] private bool enableHints = false;

        private float lastActivity;
        private HandIcon currentHand;

        private void OnEnable()
        {
            if (!enableHints) { enabled = false; return; }
            GameEvents.OnMatchMade += ResetTimer;
            GameEvents.OnMovesChanged += ResetTimerOnMoves;
            lastActivity = Time.time;
        }

        private void OnDisable()
        {
            if (!enableHints) return;
            GameEvents.OnMatchMade -= ResetTimer;
            GameEvents.OnMovesChanged -= ResetTimerOnMoves;
        }

        private void ResetTimer(int _) { lastActivity = Time.time; ClearHand(); }
        private void ResetTimerOnMoves(int _) { lastActivity = Time.time; ClearHand(); }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            // HER input'ta el ikonu temizle ve timer'ı resetle
            // (Mouse veya touch, başarılı/başarısız swap fark etmez)
            bool inputDetected = UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.touchCount > 0;
            if (inputDetected)
            {
                lastActivity = Time.time;
                if (currentHand != null) ClearHand();
                return;
            }

            if (currentHand != null) return;
            if (Time.time - lastActivity < idleSeconds) return;
            ShowHint();
        }

        private void ShowHint()
        {
            var hint = FindHint();
            if (hint == null) return;
            var grid = GridManager.Instance;
            var a = grid.GridToWorld(hint.Value.ax, hint.Value.ay);
            var b = grid.GridToWorld(hint.Value.bx, hint.Value.by);
            currentHand = HandIcon.Spawn(handPrefab, a, b);
        }

        private void ClearHand()
        {
            if (currentHand != null) currentHand.Hide();
            currentHand = null;
        }

        private struct SwapHint { public int ax, ay, bx, by; }

        private SwapHint? FindHint()
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
            bool hasMatch = QuickMatch(x, y) || QuickMatch(nx, ny);
            grid.Cells[x, y].CurrentBalloon = a;
            grid.Cells[nx, ny].CurrentBalloon = b;

            if (hasMatch) { hint = new SwapHint { ax = x, ay = y, bx = nx, by = ny }; return true; }
            return false;
        }

        private bool QuickMatch(int x, int y)
        {
            var grid = GridManager.Instance;
            if (!grid.Cells[x, y].HasBalloon) return false;
            var type = grid.Cells[x, y].CurrentBalloon.Type;
            int count = 1;
            for (int i = x - 1; i >= 0 && grid.Cells[i, y].HasBalloon && grid.Cells[i, y].CurrentBalloon.Type == type; i--) count++;
            for (int i = x + 1; i < grid.Width && grid.Cells[i, y].HasBalloon && grid.Cells[i, y].CurrentBalloon.Type == type; i++) count++;
            if (count >= 3) return true;
            count = 1;
            for (int i = y - 1; i >= 0 && grid.Cells[x, i].HasBalloon && grid.Cells[x, i].CurrentBalloon.Type == type; i--) count++;
            for (int i = y + 1; i < grid.Height && grid.Cells[x, i].HasBalloon && grid.Cells[x, i].CurrentBalloon.Type == type; i++) count++;
            return count >= 3;
        }
    }
}
