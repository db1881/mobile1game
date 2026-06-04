using UnityEngine;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GameAreaFrame : MonoBehaviour
    {
        [SerializeField] private float paddingCells = 0.55f;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Sliced;
        }

        private void LateUpdate()
        {
            var grid = GridManager.Instance;
            if (grid == null) return;
            float w = grid.Width + paddingCells * 2f;
            float h = grid.Height + paddingCells * 2f;
            float cx = (grid.Width - 1) * 0.5f;
            float cy = (grid.Height - 1) * 0.5f;
            transform.position = new Vector3(cx, cy, 5f);
            sr.size = new Vector2(w, h);
        }
    }
}
