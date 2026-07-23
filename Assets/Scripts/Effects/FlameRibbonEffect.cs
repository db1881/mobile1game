using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BalloonPop.Effects
{
    /// <summary>
    /// Fiery "laser" effects for popped balloons. Two flavours:
    ///
    ///  • <see cref="SpawnPath"/> — a laser that WANDERS over a whole connected cluster of
    ///    popping balloons. It is a LineRenderer whose vertices are the balloon centres, so
    ///    the beam always passes exactly through each balloon's centre (never cuts corners).
    ///    The line grows one node at a time and its tip moves at constant speed, so the caller
    ///    can pop each balloon exactly as the tip arrives. This is the main match effect.
    ///
    ///  • <see cref="Spawn"/> — a straight beam that draws itself end-to-end. Kept for bomb /
    ///    special detonations (whole rows).
    ///
    /// Art comes from Assets/Resources ("Lazer_Line" beam, "Lazer_blast" tip burst); if those
    /// assets are missing the effect falls back to a procedural texture so it always renders.
    /// Class/method names are left as "FlameRibbon" to avoid churn in the shared repo.
    /// </summary>
    public class FlameRibbonEffect : MonoBehaviour
    {
        // Procedural fallbacks, built once.
        static Sprite _beamSprite;
        static Sprite _headSprite;
        static Material _lineMat;

        // Real painted art (loaded once from Resources).
        static Sprite _lineArt, _blastArt;
        static bool _lineTried, _blastTried;

        static Sprite LineArt()
        {
            if (!_lineTried) { _lineArt = Resources.Load<Sprite>("Lazer_Line"); _lineTried = true; }
            return _lineArt != null ? _lineArt : BeamSprite();
        }

        static Sprite BlastArt()
        {
            if (!_blastTried) { _blastArt = Resources.Load<Sprite>("Lazer_blast"); _blastTried = true; }
            return _blastArt != null ? _blastArt : HeadSprite();
        }

        // ============================================================================
        //  PATH LASER — wanders over a connected cluster, through every balloon centre
        // ============================================================================

        /// <summary>Spawn a laser that grows through <paramref name="path"/> (world points, one
        /// per balloon centre) at constant speed. The line's vertices ARE those centres, so it
        /// passes exactly through each one.</summary>
        /// <param name="width">Beam width in world units.</param>
        /// <param name="travelDuration">Seconds for the tip to travel the whole path (time the
        /// balloon pops with the same value to keep them in sync).</param>
        public static FlameRibbonEffect SpawnPath(IList<Vector3> path, float width,
                                                  Color? tint, float travelDuration)
        {
            if (path == null || path.Count == 0) return null;

            var go = new GameObject("LaserPath");
            var fx = go.AddComponent<FlameRibbonEffect>();

            var lr = go.AddComponent<LineRenderer>();
            lr.material = LineMat();                  // painted fire beam texture
            lr.textureMode = LineTextureMode.Stretch;
            lr.alignment = LineAlignment.TransformZ;  // lock ribbon flat in the XY plane (no camera billboard = no wobble)
            lr.numCornerVertices = 6;
            lr.numCapVertices = 6;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.useWorldSpace = true;
            lr.sortingOrder = 40;                     // above balloons (order 0)
            var col = tint ?? Color.white;
            lr.startColor = col; lr.endColor = col;
            lr.positionCount = 1;
            lr.SetPosition(0, WithZ(path[0], -0.4f));

            // Bright painted burst that rides the moving tip.
            var headGO = new GameObject("LaserPathHead");
            headGO.transform.SetParent(go.transform, false);
            var headSR = headGO.AddComponent<SpriteRenderer>();
            headSR.sprite = BlastArt();
            headSR.sortingOrder = 41;

            fx.StartCoroutine(fx.RunPath(go, lr, headSR, path, width, travelDuration));
            return fx;
        }

        IEnumerator RunPath(GameObject go, LineRenderer lr, SpriteRenderer head,
                            IList<Vector3> pts, float width, float dur)
        {
            int n = pts.Count;
            var cum = new float[n];
            cum[0] = 0f;
            for (int i = 1; i < n; i++) cum[i] = cum[i - 1] + Vector3.Distance(pts[i - 1], pts[i]);
            float total = cum[n - 1];

            Color headCol = Color.white;                       // show the burst art's own colours
            float headBaseWorld = head.sprite.bounds.size.x;
            float d2 = Mathf.Max(dur, 0.0001f);

            // Draw: grow the line through the centres while the tip travels at constant speed.
            float elapsed = 0f;
            while (elapsed < d2)
            {
                float d = total > 1e-4f ? (elapsed / d2) * total : 0f;
                SetPolyline(lr, pts, cum, d);
                Vector3 tip = total > 1e-4f ? SampleAlong(pts, cum, d) : pts[0];
                head.transform.position = WithZ(tip, -0.45f);
                SetHead(head, headCol, 1f, width, headBaseWorld, elapsed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetPolylineFull(lr, pts);
            head.transform.position = WithZ(pts[n - 1], -0.45f);

            // Short hold, then fade the whole beam + head out together.
            float hold = 0.10f, h = 0f;
            while (h < hold) { SetHead(head, headCol, 1f, width, headBaseWorld, elapsed + h); h += Time.deltaTime; yield return null; }

            float fade = 0.22f, f = 0f;
            while (f < fade)
            {
                float a = 1f - f / fade;
                SetLineAlpha(lr, a);
                SetHead(head, headCol, a, width, headBaseWorld, elapsed + hold + f);
                f += Time.deltaTime;
                yield return null;
            }
            Destroy(go);
        }

        // Reveal the polyline up to travelled distance d: full nodes already passed, plus the
        // moving tip on the current segment. Because the nodes ARE the balloon centres, the
        // rendered line always runs exactly through them.
        static void SetPolyline(LineRenderer lr, IList<Vector3> pts, float[] cum, float d)
        {
            int n = pts.Count;
            if (d >= cum[n - 1]) { SetPolylineFull(lr, pts); return; }
            int i = 1;
            while (i < n && cum[i] < d) i++;
            lr.positionCount = i + 1;
            for (int k = 0; k < i; k++) lr.SetPosition(k, WithZ(pts[k], -0.4f));
            float segLen = cum[i] - cum[i - 1];
            float u = segLen > 1e-5f ? (d - cum[i - 1]) / segLen : 0f;
            lr.SetPosition(i, WithZ(Vector3.Lerp(pts[i - 1], pts[i], u), -0.4f));
        }

        static void SetPolylineFull(LineRenderer lr, IList<Vector3> pts)
        {
            int n = pts.Count;
            lr.positionCount = n;
            for (int k = 0; k < n; k++) lr.SetPosition(k, WithZ(pts[k], -0.4f));
        }

        static void SetLineAlpha(LineRenderer lr, float a)
        {
            var c1 = lr.startColor; c1.a = a; lr.startColor = c1;
            var c2 = lr.endColor; c2.a = a; lr.endColor = c2;
        }

        static Vector3 SampleAlong(IList<Vector3> pts, float[] cum, float d)
        {
            int n = pts.Count;
            if (d <= 0f) return pts[0];
            if (d >= cum[n - 1]) return pts[n - 1];
            int i = 1;
            while (i < n && cum[i] < d) i++;
            float segLen = cum[i] - cum[i - 1];
            float u = segLen > 1e-5f ? (d - cum[i - 1]) / segLen : 0f;
            return Vector3.Lerp(pts[i - 1], pts[i], u);
        }

        static Vector3 WithZ(Vector3 v, float z) { v.z = z; return v; }

        void SetHead(SpriteRenderer head, Color baseCol, float alpha, float width, float baseWorld, float t)
        {
            var c = baseCol; c.a = Mathf.Clamp01(alpha); head.color = c;
            float w = width * (1.3f + 0.18f * Mathf.Sin(t * 48f));
            float s = baseWorld > 1e-4f ? w / baseWorld : w;
            head.transform.localScale = new Vector3(s, s, 1f);
        }

        // ============================================================================
        //  STRAIGHT BEAM — draws end-to-end (kept for bomb / special detonations)
        // ============================================================================

        /// <param name="frames">Ignored (kept for signature compatibility; art is loaded from Resources).</param>
        public static FlameRibbonEffect Spawn(Sprite[] frames, Vector3 startWorld, Vector3 endWorld,
                                              float thickness, float overhang, Color? tint = null, float duration = 0.8f)
        {
            var go = new GameObject("LaserBeam");
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
            float angleDeg = Mathf.Atan2(dirHat.y, dirHat.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

            var headGO = new GameObject("LaserHead");
            var headSR = headGO.AddComponent<SpriteRenderer>();
            headSR.sprite = BlastArt();
            headSR.sortingOrder = 41;
            float headBaseWorld = headSR.sprite.bounds.size.x;
            Color headBase = Color.white;

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

                float flick = 1f + 0.08f * Mathf.Sin(t * 45f);
                sr.size = new Vector2(curLen, thickness * flick);
                Vector3 center = startAnchor + dirHat * (curLen * 0.5f);
                center.z = -0.4f;
                transform.position = center;
                var bc = sr.color; bc.a = beamA; sr.color = bc;

                float headA;
                if (k < drawEnd) headA = (k < 0.08f) ? k / 0.08f : 1f;
                else headA = Mathf.Clamp01(1f - (k - drawEnd) / 0.14f);
                Vector3 tip = startAnchor + dirHat * curLen;
                tip.z = -0.45f;
                headGO.transform.position = tip;
                float headWorld = thickness * (1.6f + 0.20f * Mathf.Sin(t * 50f));
                float headScale = headBaseWorld > 1e-4f ? headWorld / headBaseWorld : headWorld;
                headGO.transform.localScale = new Vector3(headScale, headScale, 1f);
                var hc = headBase; hc.a = headA; headSR.color = hc;

                t += Time.deltaTime;
                yield return null;
            }
            Destroy(headGO);
            Destroy(gameObject);
        }

        // ============================================================================
        //  Materials
        // ============================================================================

        // Alpha-blended material carrying the painted fire-beam texture (mobile-safe).
        static Material LineMat()
        {
            if (_lineMat != null) return _lineMat;
            Shader sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Legacy Shaders/Diffuse");
            _lineMat = new Material(sh) { name = "LaserLineMat(runtime)" };
            var ls = LineArt();
            if (ls != null && ls.texture != null) _lineMat.mainTexture = ls.texture;
            return _lineMat;
        }

        // ============================================================================
        //  Procedural fallbacks (used only if the Resources art is missing)
        // ============================================================================

        static Sprite BeamSprite()
        {
            if (_beamSprite != null) return _beamSprite;
            const int W = 8, H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "LaserBeamTex"
            };
            for (int y = 0; y < H; y++)
            {
                float tt = Mathf.Abs((y + 0.5f) / H * 2f - 1f);
                Color col;
                if (tt < 0.16f) col = new Color(1f, 0.98f, 0.90f);
                else if (tt < 0.42f)
                    col = Color.Lerp(new Color(1f, 0.92f, 0.55f), new Color(1f, 0.55f, 0.14f), Mathf.InverseLerp(0.16f, 0.42f, tt));
                else
                    col = Color.Lerp(new Color(1f, 0.45f, 0.12f), new Color(0.95f, 0.18f, 0.05f), Mathf.InverseLerp(0.42f, 1f, tt));
                float a = tt < 0.16f ? 1f : Mathf.Pow(1f - Mathf.InverseLerp(0.16f, 1f, tt), 1.9f);
                col.a = Mathf.Clamp01(a);
                for (int x = 0; x < W; x++) tex.SetPixel(x, y, col);
            }
            tex.Apply();
            _beamSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _beamSprite;
        }

        static Sprite HeadSprite()
        {
            if (_headSprite != null) return _headSprite;
            const int S = 64;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "LaserHeadTex"
            };
            Vector2 c = new Vector2((S - 1) / 2f, (S - 1) / 2f);
            float r = S / 2f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), c) / r);
                    float a = Mathf.Pow(1f - d, 2.2f);
                    Color col = Color.Lerp(new Color(1f, 0.97f, 0.85f), new Color(1f, 0.50f, 0.15f), Mathf.Clamp01(d * 1.3f));
                    col.a = Mathf.Clamp01(a);
                    tex.SetPixel(x, y, col);
                }
            tex.Apply();
            _headSprite = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _headSprite;
        }
    }
}
