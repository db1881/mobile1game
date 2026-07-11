using UnityEngine;
using TMPro;

namespace BalloonPop.UI
{
    /// <summary>
    /// Bends a TMP text along a simple parabolic arc so it follows a curved banner ribbon.
    /// Positive arcHeight raises the middle (arch up); negative lowers it (smile down).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPArcText : MonoBehaviour
    {
        [Tooltip("Vertical offset (pixels) of the middle vs the ends. + = middle up, - = middle down.")]
        public float arcHeight = 34f;

        private TMP_Text tmp;

        private void OnEnable()
        {
            if (tmp == null) tmp = GetComponent<TMP_Text>();
            tmp.RegisterDirtyVerticesCallback(Warp);
            Warp();
        }

        private void OnDisable()
        {
            if (tmp != null) tmp.UnregisterDirtyVerticesCallback(Warp);
        }

        private bool warping;

        public void Warp()
        {
            if (warping) return;
            if (tmp == null) tmp = GetComponent<TMP_Text>();
            if (tmp == null) return;
            warping = true;

            tmp.ForceMeshUpdate();
            var info = tmp.textInfo;
            int count = info.characterCount;
            if (count == 0) { warping = false; return; }

            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                if (!info.characterInfo[i].isVisible) continue;
                minX = Mathf.Min(minX, info.characterInfo[i].bottomLeft.x);
                maxX = Mathf.Max(maxX, info.characterInfo[i].topRight.x);
            }
            float width = Mathf.Max(0.0001f, maxX - minX);

            for (int i = 0; i < count; i++)
            {
                if (!info.characterInfo[i].isVisible) continue;
                int mat = info.characterInfo[i].materialReferenceIndex;
                int vIdx = info.characterInfo[i].vertexIndex;
                Vector3[] verts = info.meshInfo[mat].vertices;

                float cx = (info.characterInfo[i].bottomLeft.x + info.characterInfo[i].topRight.x) * 0.5f;
                float t = (cx - minX) / width;             // 0..1 across the text

                // Parabolic arc: y peak at center; tangent slope for per-letter rotation
                float y = arcHeight * (1f - 4f * (t - 0.5f) * (t - 0.5f));
                float slope = arcHeight * (-8f * (t - 0.5f)) / width;     // dy/dx
                float angleDeg = Mathf.Atan(slope) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg);

                // pivot = character centre
                Vector3 pivot = (verts[vIdx + 0] + verts[vIdx + 2]) * 0.5f;
                for (int k = 0; k < 4; k++)
                {
                    Vector3 v = verts[vIdx + k] - pivot;
                    v = rot * v;
                    v += pivot;
                    v.y += y;
                    verts[vIdx + k] = v;
                }
            }

            for (int m = 0; m < info.meshInfo.Length; m++)
            {
                info.meshInfo[m].mesh.vertices = info.meshInfo[m].vertices;
                tmp.UpdateGeometry(info.meshInfo[m].mesh, m);
            }
            warping = false;
        }
    }
}
