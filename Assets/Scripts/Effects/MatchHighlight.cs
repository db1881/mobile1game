using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Grid;

namespace BalloonPop.Effects
{
    /// <summary>
    /// Patlamadan önce match grubunun etrafına dikdörtgen çerçeve çizer.
    /// Hem LineRenderer (sprite yoksa) hem SpriteRenderer 9-slice (sprite varsa) yöntemini destekler.
    /// </summary>
    public class MatchHighlight : MonoBehaviour
    {
        // Sahnedeki herhangi bir referans tarafından beslenebilir (AutoSetup atar).
        public static Sprite OutlineSprite;

        public static IEnumerator FlashAround(IList<Cell> cells, float cellSize, Vector3 origin, float duration = 0.22f)
        {
            if (cells == null || cells.Count == 0) yield break;

            // Bounding box hesapla
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var c in cells)
            {
                if (c.X < minX) minX = c.X;
                if (c.X > maxX) maxX = c.X;
                if (c.Y < minY) minY = c.Y;
                if (c.Y > maxY) maxY = c.Y;
            }

            float pad = cellSize * 0.45f;
            Vector3 bl = new Vector3(origin.x + minX * cellSize - pad, origin.y + minY * cellSize - pad, -1f);
            Vector3 tr = new Vector3(origin.x + maxX * cellSize + pad, origin.y + maxY * cellSize + pad, -1f);
            Vector3 center = (bl + tr) * 0.5f;
            Vector2 size = new Vector2(tr.x - bl.x, tr.y - bl.y);

            var go = new GameObject("MatchHighlight");
            go.transform.position = center;

            if (OutlineSprite != null)
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = OutlineSprite;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = size;
                sr.color = new Color(1f, 0.95f, 0.25f, 0.9f);
                sr.sortingOrder = 30;
                yield return AnimateSprite(sr, go, duration);
            }
            else
            {
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = true;
                lr.positionCount = 4;
                lr.startWidth = lr.endWidth = 0.08f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                var col = new Color(1f, 0.95f, 0.25f, 0.95f);
                lr.startColor = lr.endColor = col;
                lr.sortingOrder = 30;
                float hw = size.x * 0.5f;
                float hh = size.y * 0.5f;
                lr.SetPosition(0, new Vector3(-hw, -hh, 0));
                lr.SetPosition(1, new Vector3(hw, -hh, 0));
                lr.SetPosition(2, new Vector3(hw, hh, 0));
                lr.SetPosition(3, new Vector3(-hw, hh, 0));
                yield return AnimateLine(lr, go, duration);
            }

            if (go != null) Object.Destroy(go);
        }

        private static IEnumerator AnimateSprite(SpriteRenderer sr, GameObject go, float duration)
        {
            float t = 0f;
            Vector3 baseScale = Vector3.one;
            while (t < duration && go != null)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                // ilk yarı büyür, sonra biraz küçülüp kaybolur
                float s = p < 0.4f ? Mathf.Lerp(0.85f, 1.08f, p / 0.4f)
                                   : Mathf.Lerp(1.08f, 0.95f, (p - 0.4f) / 0.6f);
                go.transform.localScale = baseScale * s;
                var c = sr.color;
                c.a = p < 0.7f ? 0.9f : Mathf.Lerp(0.9f, 0f, (p - 0.7f) / 0.3f);
                sr.color = c;
                yield return null;
            }
        }

        private static IEnumerator AnimateLine(LineRenderer lr, GameObject go, float duration)
        {
            float t = 0f;
            Vector3 baseScale = Vector3.one;
            while (t < duration && go != null)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float s = p < 0.4f ? Mathf.Lerp(0.85f, 1.05f, p / 0.4f)
                                   : Mathf.Lerp(1.05f, 0.95f, (p - 0.4f) / 0.6f);
                go.transform.localScale = baseScale * s;
                float a = p < 0.7f ? 0.95f : Mathf.Lerp(0.95f, 0f, (p - 0.7f) / 0.3f);
                var col = new Color(1f, 0.95f, 0.25f, a);
                lr.startColor = lr.endColor = col;
                yield return null;
            }
        }
    }
}
