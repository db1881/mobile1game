using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BalloonPop.Core;

namespace BalloonPop.Effects
{
    public class SceneTransitionFader : Singleton<SceneTransitionFader>
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private float fadeDuration = 0.45f;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) DontDestroyOnLoad(gameObject);
            if (overlay != null) overlay.alpha = 0f;
        }

        public void FadeOutAndLoad(string sceneName, System.Action onSceneLoaded = null)
        {
            StartCoroutine(Run(sceneName, onSceneLoaded));
        }

        private IEnumerator Run(string sceneName, System.Action onSceneLoaded)
        {
            yield return FadeTo(1f);
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;
            onSceneLoaded?.Invoke();
            yield return null;
            yield return FadeTo(0f);
        }

        public IEnumerator FadeTo(float target)
        {
            if (overlay == null) yield break;
            float start = overlay.alpha;
            float t = 0f;
            overlay.blocksRaycasts = true;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            overlay.alpha = target;
            overlay.blocksRaycasts = target > 0.01f;
        }
    }
}
