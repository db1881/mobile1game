using UnityEngine;
using BalloonPop.Grid;

namespace BalloonPop.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private float paddingCells = 0.5f;
        [SerializeField, Range(0f, 0.4f)] private float topUiFraction = 0.18f;
        [SerializeField, Range(0f, 0.4f)] private float bottomUiFraction = 0.20f;

        private Camera cam;
        private float shakeAmount;
        private float shakeDecay = 6f;

        public static CameraFitter Instance { get; private set; }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            Instance = this;
        }

        public void Shake(float amount, float decay = 6f)
        {
            shakeAmount = Mathf.Max(shakeAmount, amount);
            shakeDecay = decay;
        }

        private void LateUpdate()
        {
            if (GridManager.Instance == null) return;

            int w = GridManager.Instance.Width;
            int h = GridManager.Instance.Height;
            if (w <= 0 || h <= 0) return;

            float aspect = (float)Screen.width / Screen.height;
            float playAreaFrac = Mathf.Max(0.4f, 1f - topUiFraction - bottomUiFraction);

            float halfHeightForGrid = ((h * 0.5f) + paddingCells) / playAreaFrac;
            float halfWidthForGrid  = ((w * 0.5f) + paddingCells) / aspect;
            float requiredHalfHeight = Mathf.Max(halfHeightForGrid, halfWidthForGrid);

            cam.orthographic = true;
            cam.orthographicSize = requiredHalfHeight;

            float gridCenterX = (w - 1) * 0.5f;
            float gridCenterY = (h - 1) * 0.5f;
            float verticalShift = requiredHalfHeight * (topUiFraction - bottomUiFraction);

            var pos = cam.transform.position;
            pos.x = gridCenterX;
            pos.y = gridCenterY + verticalShift;
            if (pos.z >= 0) pos.z = -10f;

            if (shakeAmount > 0.001f)
            {
                Vector3 offset = Random.insideUnitSphere * shakeAmount;
                offset.z = 0;
                pos += offset;
                shakeAmount = Mathf.Max(0f, shakeAmount - shakeDecay * Time.deltaTime);
            }

            cam.transform.position = pos;
        }
    }
}
