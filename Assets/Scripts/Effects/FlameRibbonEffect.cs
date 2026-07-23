using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BalloonPop.Effects
{
    /// <summary>
    /// Lightning-bolt effects for popped balloons. Two flavours:
    ///
    ///  • <see cref="SpawnPath"/> — a bolt that WANDERS over a whole connected cluster of
    ///    popping balloons. Each step between two neighbouring balloon centres is drawn as its
    ///    OWN straight quad, so the bolt runs dead-straight from centre to centre and corners
    ///    can never bend or smear it (a single LineRenderer polyline bulges and warps its
    ///    texture at corners once the width approaches the segment length). Consecutive
    ///    segments show consecutive horizontal slices of the art, so the bolt reads as one
    ///    continuous streak along the path. The tip advances at constant speed so the caller
    ///    can pop each balloon exactly as it arrives. This is the main match effect.
    ///
    ///  • <see cref="Spawn"/> — a straight beam that draws itself end-to-end. Kept for bomb /
    ///    special detonations (whole rows).
    ///
    /// Art comes from Assets/Resources ("lightning_line"); if it's missing the effect falls back
    /// to a procedural texture so it always renders. There is deliberately no tip burst.
    /// Class/method names are left as "FlameRibbon" to avoid churn in the shared repo.
    /// </summary>
    public class FlameRibbonEffect : MonoBehaviour
    {
        static Sprite _beamSprite;      // procedural fallback
        static Sprite _lineArt;
        static bool _lineTried;

        static Sprite LineArt()
        {
            if (!_lineTried) { _lineArt = Resources.Load<Sprite>("lightning_line"); _lineTried = true; }
            return _lineArt != null ? _lineArt : BeamSprite();
        }

        // ============================================================================
        //  PATH BOLT — straight per-step segments through every balloon centre
        // ============================================================================

        /// <summary>Spawn a bolt that grows through <paramref name="path"/> (world points, one
        /// per balloon centre) at constant speed.</summary>
        /// <param name="width">Bolt thickness in world units.</param>
        /// <param name="travelDuration">Seconds for the tip to travel the whole path (time the
        /// balloon pops with the same value to keep them in sync).</param>
        public static FlameRibbonEffect SpawnPath(IList<Vector3> path, float width,
                                                  Color? tint, float travelDuration)
        {
            if (path == null || path.Count == 0) return null;
            var go = new GameObject("BoltPath");
            var fx = go.AddComponent<FlameRibbonEffect>();
            fx.StartCoroutine(fx.RunPath(go, path, width, tint ?? Color.white, travelDuration));
            return fx;
        }

        IEnumerator RunPath(GameObject go, IList<Vector3> pts, float width, Color tint, float dur)
        {
            int n = pts.Count;
            var art = LineArt();
            float over = width * 0.30f;              // overlap past each end, so corners have no notch

            if (n < 2)
            {
                // Single balloon: a short flash in place.
                var only = NewSegment(go, art, 0, 1, tint);
                only.size = new Vector2(width, width);
                only.transform.position = WithZ(pts[0], -0.4f);
                yield return FadeSegments(new[] { only }, 0.25f);
                Cleanup(go, new[] { only });
                yield break;
            }

            int segCount = n - 1;
            var segs = new SpriteRenderer[segCount];
            var dirs = new Vector3[segCount];
            var lens = new float[segCount];
            var cumStart = new float[segCount];
            float total = 0f;
            for (int i = 0; i < segCount; i++)
            {
                Vector3 a = pts[i], b = pts[i + 1];
                Vector3 d = b - a;
                float len = d.magnitude;
                lens[i] = len;
                dirs[i] = len > 1e-5f ? d / len : Vector3.right;
                cumStart[i] = total;
                total += len;

                var sr = NewSegment(go, art, i, segCount, tint);
                sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dirs[i].y, dirs[i].x) * Mathf.Rad2Deg);
                sr.enabled = false;
                segs[i] = sr;
            }

            float d2 = Mathf.Max(dur, 0.0001f);
            float elapsed = 0f;
            while (elapsed < d2)
            {
                Apply(segs, pts, dirs, lens, cumStart, (elapsed / d2) * total, width, over);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Apply(segs, pts, dirs, lens, cumStart, total, width, over);

            float hold = 0.10f, h = 0f;
            while (h < hold) { h += Time.deltaTime; yield return null; }

            yield return FadeSegments(segs, 0.22f);
            Cleanup(go, segs);
        }

        // Grow each straight segment as the tip passes over it. A segment starts slightly
        // BEFORE its first centre and, once complete, runs slightly PAST its last centre, so
        // neighbouring segments overlap at the corner instead of leaving a notch.
        static void Apply(SpriteRenderer[] segs, IList<Vector3> pts, Vector3[] dirs,
                          float[] lens, float[] cumStart, float dist, float width, float over)
        {
            for (int i = 0; i < segs.Length; i++)
            {
                float drawn = Mathf.Clamp(dist - cumStart[i], 0f, lens[i]);
                if (drawn <= 1e-4f) { segs[i].enabled = false; continue; }
                segs[i].enabled = true;
                bool complete = drawn >= lens[i] - 1e-4f;
                float L = drawn + over + (complete ? over : 0f);
                Vector3 anchor = pts[i] - dirs[i] * over;
                segs[i].size = new Vector2(L, width);
                segs[i].transform.position = WithZ(anchor + dirs[i] * (L * 0.5f), -0.4f);
            }
        }

        IEnumerator FadeSegments(SpriteRenderer[] segs, float dur)
        {
            float f = 0f;
            while (f < dur)
            {
                float a = 1f - f / dur;
                for (int i = 0; i < segs.Length; i++)
                {
                    if (segs[i] == null) continue;
                    var c = segs[i].color; c.a = a; segs[i].color = c;
                }
                f += Time.deltaTime;
                yield return null;
            }
        }

        static void Cleanup(GameObject go, SpriteRenderer[] segs)
        {
            // The per-segment slice sprites are created at runtime — release them (this does
            // NOT touch the shared texture the slices point at).
            for (int i = 0; i < segs.Length; i++)
                if (segs[i] != null && segs[i].sprite != null) Destroy(segs[i].sprite);
            Destroy(go);
        }

        static SpriteRenderer NewSegment(GameObject parent, Sprite art, int index, int count, Color tint)
        {
            var sgo = new GameObject("BoltSeg");
            sgo.transform.SetParent(parent.transform, false);
            var sr = sgo.AddComponent<SpriteRenderer>();
            sr.sprite = SliceOf(art, index, count);
            sr.drawMode = SpriteDrawMode.Sliced;     // lets us drive size directly
            sr.sortingOrder = 40;                    // above balloons (order 0)
            sr.color = tint;
            sr.size = new Vector2(0.001f, 0.001f);
            return sr;
        }

        // Carve the art into `count` horizontal slices so consecutive segments continue the
        // same streak instead of each repeating the whole bolt. Always returns a NEW sprite,
        // so Cleanup can release it without touching the imported asset.
        static Sprite SliceOf(Sprite src, int index, int count)
        {
            var tex = src != null ? src.texture : null;
            if (tex == null) return src;
            int w = tex.width, h = tex.height;
            if (count < 1) count = 1;
            int x0 = Mathf.Clamp(Mathf.RoundToInt(index * (w / (float)count)), 0, w - 1);
            int x1 = Mathf.Clamp(Mathf.RoundToInt((index + 1) * (w / (float)count)), x0 + 1, w);
            return Sprite.Create(tex, new Rect(x0, 0f, x1 - x0, h), new Vector2(0.5f, 0.5f),
                                 100f, 0, SpriteMeshType.FullRect);
        }

        static Vector3 WithZ(Vector3 v, float z) { v.z = z; return v; }

        // ============================================================================
        //  STRAIGHT BEAM — draws end-to-end (kept for bomb / special detonations)
        // ============================================================================

        /// <param name="frames">Ignored (kept for signature compatibility; art is loaded from Resources).</param>
        public static FlameRibbonEffect Spawn(Sprite[] frames, Vector3 startWorld, Vector3 endWorld,
                                              float thickness, float overhang, Color? tint = null, float duration = 0.8f)
        {
            var go = new GameObject("BoltBeam");
            var fx = go.AddComponent<FlameRibbonEffect>();
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LineArt();
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sortingOrder = 40;
            sr.color = tint ?? Color.white;

            fx.StartCoroutine(fx.Run(sr, startWorld, endWorld, thickness, overhang, duration));
            return fx;
        }

        IEnumerator Run(SpriteRenderer sr, Vector3 a, Vector3 b, float thickness, float overhang, float duration)
        {
            Vector3 dir = b - a;
            float span = dir.magnitude;
            Vector3 dirHat = span > 0.0001f ? dir / span : Vector3.right;
            float fullLen = span + overhang;
            Vector3 startAnchor = a - dirHat * (overhang * 0.5f);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dirHat.y, dirHat.x) * Mathf.Rad2Deg);

            const float drawEnd = 0.62f;
            const float holdEnd = 0.82f;

            yield return null;   // let the heavy pop frame pass so its deltaTime doesn't swallow the sweep

            float t = 0f;
            while (t < duration)
            {
                float k = t / duration;

                float x = Mathf.Clamp01(k / drawEnd);
                float p = k < drawEnd ? x * x * (3f - 2f * x) : 1f;
                float curLen = Mathf.Max(fullLen * p, 0.001f);

                float beamA;
                if (k < 0.08f) beamA = k / 0.08f;
                else if (k < holdEnd) beamA = 1f;
                else beamA = Mathf.Clamp01(1f - (k - holdEnd) / (1f - holdEnd));

                sr.size = new Vector2(curLen, thickness);
                Vector3 center = startAnchor + dirHat * (curLen * 0.5f);
                center.z = -0.4f;
                transform.position = center;
                var bc = sr.color; bc.a = beamA; sr.color = bc;

                t += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }

        // ============================================================================
        //  Procedural fallback (used only if the Resources art is missing)
        // ============================================================================

        static Sprite BeamSprite()
        {
            if (_beamSprite != null) return _beamSprite;
            const int W = 8, H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "BoltFallbackTex"
            };
            for (int y = 0; y < H; y++)
            {
                float tt = Mathf.Abs((y + 0.5f) / H * 2f - 1f);
                Color col;
                if (tt < 0.16f) col = new Color(1f, 0.98f, 0.90f);
                else if (tt < 0.42f)
                    col = Color.Lerp(new Color(0.85f, 0.94f, 1f), new Color(0.35f, 0.65f, 1f), Mathf.InverseLerp(0.16f, 0.42f, tt));
                else
                    col = Color.Lerp(new Color(0.35f, 0.65f, 1f), new Color(0.12f, 0.25f, 0.85f), Mathf.InverseLerp(0.42f, 1f, tt));
                float a = tt < 0.16f ? 1f : Mathf.Pow(1f - Mathf.InverseLerp(0.16f, 1f, tt), 1.9f);
                col.a = Mathf.Clamp01(a);
                for (int x = 0; x < W; x++) tex.SetPixel(x, y, col);
            }
            tex.Apply();
            _beamSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _beamSprite;
        }
    }
}
