using UnityEngine;
using UnityEngine.EventSystems;
using BalloonPop.Core;
using BalloonPop.Grid;
using BalloonPop.Gameplay;

namespace BalloonPop.InputSystem
{
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float swipeThreshold = 0.5f;
        [SerializeField] private LayerMask balloonLayer;

        public bool InputEnabled { get; set; } = true;

        private Vector2 pressWorld;
        private bool isPressing;
        private int selX = -1, selY = -1;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!InputEnabled) return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                OnPressDown(GetWorldPoint());
            }
            else if (UnityEngine.Input.GetMouseButton(0) && isPressing) OnPressHold(GetWorldPoint());
            else if (UnityEngine.Input.GetMouseButtonUp(0) && isPressing) OnPressUp(GetWorldPoint());
        }

        private Vector2 GetWorldPoint()
        {
            return mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        private void OnPressDown(Vector2 world)
        {
            if (GridManager.Instance == null) return;
            if (!GridManager.Instance.WorldToGrid(world, out int x, out int y)) return;
            if (!GridManager.Instance.Cells[x, y].HasBalloon) return;

            if (BoosterManager.Instance != null && BoosterManager.Instance.IsAwaitingTarget)
            {
                BoosterManager.Instance.TryHammerAt(x, y);
                return;
            }

            pressWorld = world;
            isPressing = true;
            selX = x; selY = y;
        }

        private void OnPressHold(Vector2 world)
        {
            Vector2 delta = world - pressWorld;
            if (delta.magnitude < swipeThreshold) return;

            int dx = 0, dy = 0;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                dx = delta.x > 0 ? 1 : -1;
            else
                dy = delta.y > 0 ? 1 : -1;

            int tx = selX + dx, ty = selY + dy;
            if (!GridManager.Instance.IsInside(tx, ty)) { Reset(); return; }

            GameplayController.Instance?.TryMakeMove(selX, selY, tx, ty);
            Reset();
        }

        private void OnPressUp(Vector2 world) => Reset();

        private void Reset()
        {
            isPressing = false;
            selX = -1; selY = -1;
        }
    }
}
