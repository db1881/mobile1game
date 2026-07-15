using System.Collections;
using UnityEngine;

namespace BalloonPop.Effects
{
    /// <summary>
    /// A painted flame "ribbon" that stretches along a popped line of balloons
    /// (start cell -> end cell): flares in, flickers through a multi-frame flipbook,
    /// then fades out. Length spans exactly the popped cells (span + a small overhang
    /// so the two end balloons are fully covered). Pure-code, no prefab — pass the
    /// flame frames in. Mirrors the project's static Spawn(...) effect convention.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlameRibbonEffect : MonoBehaviour
    {
        Sprite[] _frames;

        // The painted flame fills only ~75% of each frame's width (transparent/faded
        // side margins). Scale the sprite X up by 1/this so the VISIBLE flame spans the
        // full intended length — an N-balloon line then shows N cells of actual fire.
        const float FlameContentFracX = 0.70f;

        /// <summary>Spawn a flame ribbon between two world points.</summary>
        /// <param name="frames">Flame flipbook frames (cycled for a living-fire look).</param>
        /// <param name="startWorld">World pos of the first balloon in the line.</param>
        /// <param name="endWorld">World pos of the last balloon in the line.</param>
        /// <param name="thickness">Cross-line width in world units.</param>
        /// <param name="overhang">Extra length past the endpoints (use cellSize so an
        /// N-balloon line = span (N-1)*cellSize + cellSize = N cells covered exactly).</param>
        public static FlameRibbonEffect Spawn(Sprite[] frames, Vector3 startWorld, Vector3 endWorld,
                                              float thickness, float overhang, Color? tint = null, float duration = 0.5f)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null) return null;

            var go = new GameObject("FlameRibbon");
            var fx = go.AddComponent<FlameRibbonEffect>();   // RequireComponent auto-adds the SpriteRenderer
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = frames[0];
            sr.drawMode = SpriteDrawMode.Sliced;   // lets us drive size directly
            sr.sortingOrder = 40;                  // above balloons (they render at order 0)
            sr.color = tint ?? Color.white;

            fx._frames = frames;
            fx.Play(sr, startWorld, endWorld, thickness, overhang, duration);
            return fx;
        }

        void Play(SpriteRenderer sr, Vector3 a, Vector3 b, float thickness, float overhang, float duration)
        {
            Vector3 mid = (a + b) * 0.5f;
            mid.z = -0.4f;                          // toward the camera, over the popping balloons
            transform.position = mid;

            Vector3 dir = b - a;
            float span = dir.magnitude;             // (N-1)*cellSize for an N-balloon line
            float len = span + overhang;            // + cellSize => covers all N balloons
            if (len < 0.01f) len = overhang;

            float angle = (span > 0.0001f) ? Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            StartCoroutine(Animate(sr, len, thickness, duration));
        }

        IEnumerator Animate(SpriteRenderer sr, float len, float thickness, float duration)
        {
            float t = 0f;
            Color baseColor = sr.color;
            const float fps = 16f;                  // flipbook speed
            while (t < duration)
            {
                float k = t / duration;

                // Alpha: quick flare-in (first 15%), then ease out.
                float alpha = (k < 0.15f) ? (k / 0.15f) : (1f - (k - 0.15f) / 0.85f);
                var c = baseColor; c.a = Mathf.Clamp01(alpha); sr.color = c;

                // Length: pop from 60% -> 100% in the first 20% for a "streak" feel.
                float lenK = (k < 0.20f) ? Mathf.Lerp(0.6f, 1f, k / 0.20f) : 1f;
                // Thickness: subtle flicker so it reads as living fire.
                float flick = 1f + 0.08f * Mathf.Sin(t * 40f);
                sr.size = new Vector2((len * lenK) / FlameContentFracX, thickness * flick);

                // Flipbook: cycle through the frames for animated flames.
                if (_frames != null && _frames.Length > 0)
                {
                    int idx = ((int)(t * fps)) % _frames.Length;
                    if (_frames[idx] != null) sr.sprite = _frames[idx];
                }

                t += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
