using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BalloonPop.Gameplay;

namespace BalloonPop.UI
{
    public class AchievementToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform card;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private float showDuration = 3.5f;
        [SerializeField] private float slideAmount = 200f;

        private readonly Queue<AchievementDef> queue = new Queue<AchievementDef>();
        private bool showing;

        private void OnEnable()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0;
            AchievementManager.OnUnlocked += Enqueue;
        }

        private void OnDisable()
        {
            AchievementManager.OnUnlocked -= Enqueue;
        }

        private void Enqueue(AchievementDef def)
        {
            queue.Enqueue(def);
            if (!showing) StartCoroutine(ShowNext());
        }

        private IEnumerator ShowNext()
        {
            showing = true;
            while (queue.Count > 0)
            {
                var def = queue.Dequeue();
                if (titleText != null) titleText.text = def.Title;
                if (descText != null) descText.text = def.Description;
                if (rewardText != null) rewardText.text = $"+{def.CoinReward}";

                yield return Animate(0f, 1f, 0.3f);
                yield return new WaitForSeconds(showDuration);
                yield return Animate(1f, 0f, 0.3f);
            }
            showing = false;
        }

        private IEnumerator Animate(float from, float to, float dur)
        {
            float t = 0;
            float startY = (from < to) ? -slideAmount : 0;
            float endY = (from < to) ? 0 : -slideAmount;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(from, to, u);
                if (card != null) card.anchoredPosition = new Vector2(card.anchoredPosition.x, Mathf.Lerp(startY, endY, u));
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = to;
        }
    }
}
