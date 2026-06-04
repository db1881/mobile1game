using System.Collections;
using UnityEngine;
using BalloonPop.Data;

namespace BalloonPop.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Balloon : MonoBehaviour
    {
        [Header("Type")]
        [SerializeField] private BalloonType type;
        [SerializeField] private SpecialType special = SpecialType.Normal;

        [Header("Visuals")]
        [SerializeField] private Sprite[] colorSprites;
        [SerializeField] private Sprite lineHSprite;
        [SerializeField] private Sprite lineVSprite;
        [SerializeField] private Sprite bombSprite;
        [SerializeField] private Sprite rainbowSprite;
        [SerializeField] private Sprite goldSprite;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float popDuration = 0.25f;

        public int X { get; set; }
        public int Y { get; set; }
        public BalloonType Type => type;
        public SpecialType Special => special;
        public bool IsMoving { get; private set; }
        public bool IsMarkedForPop { get; set; }

        private SpriteRenderer spriteRenderer;
        private Coroutine moveRoutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(BalloonType newType, int x, int y, Vector3 worldPos)
        {
            type = newType;
            special = SpecialType.Normal;
            X = x;
            Y = y;
            transform.position = worldPos;
            transform.localScale = Vector3.one;
            IsMarkedForPop = false;
            RefreshSprite();
        }

        public void SetSpecial(SpecialType specialType)
        {
            special = specialType;
            RefreshSprite();
        }

        public void SetGridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void MoveTo(Vector3 target, System.Action onArrive = null)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveRoutine(target, onArrive));
        }

        private IEnumerator MoveRoutine(Vector3 target, System.Action onArrive)
        {
            IsMoving = true;
            while ((transform.position - target).sqrMagnitude > 0.0005f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            IsMoving = false;
            onArrive?.Invoke();
        }

        public IEnumerator PopRoutine()
        {
            // İki fazlı şişip-patlama: önce 1.3x büyür ve parıldar, sonra hızla küçülüp kaybolur
            Vector3 startScale = transform.localScale;
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            // FAZ 1: şişme (0.08 sn, scale 1→1.3 + flash beyaza)
            float puffDur = 0.08f;
            float t = 0f;
            while (t < puffDur)
            {
                t += Time.deltaTime;
                float p = t / puffDur;
                transform.localScale = startScale * Mathf.Lerp(1f, 1.30f, p);
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.Lerp(startColor, Color.white, p * 0.8f);
                yield return null;
            }

            // FAZ 2: patlama (0.18 sn, scale 1.3→0, alpha kaybolur)
            float shrinkDur = 0.18f;
            t = 0f;
            Vector3 puffedScale = startScale * 1.30f;
            while (t < shrinkDur)
            {
                t += Time.deltaTime;
                float p = t / shrinkDur;
                // Ease-out cubic
                float ease = 1f - (1f - p) * (1f - p) * (1f - p);
                transform.localScale = Vector3.Lerp(puffedScale, Vector3.zero, ease);
                if (spriteRenderer != null)
                {
                    var c = spriteRenderer.color;
                    c.a = 1f - ease;
                    spriteRenderer.color = c;
                }
                yield return null;
            }

            transform.localScale = Vector3.zero;
            if (spriteRenderer != null) spriteRenderer.color = startColor;
            gameObject.SetActive(false);
        }

        private void RefreshSprite()
        {
            if (spriteRenderer == null) return;

            switch (special)
            {
                case SpecialType.LineH:    spriteRenderer.sprite = lineHSprite;    break;
                case SpecialType.LineV:    spriteRenderer.sprite = lineVSprite;    break;
                case SpecialType.Bomb:     spriteRenderer.sprite = bombSprite;     break;
                case SpecialType.Rainbow:  spriteRenderer.sprite = rainbowSprite;  break;
                case SpecialType.Gold:     spriteRenderer.sprite = goldSprite;     break;
                default:
                    int idx = (int)type - 1;
                    if (colorSprites != null && idx >= 0 && idx < colorSprites.Length)
                        spriteRenderer.sprite = colorSprites[idx];
                    break;
            }
        }
    }
}
