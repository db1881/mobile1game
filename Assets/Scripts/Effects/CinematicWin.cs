using System.Collections;
using UnityEngine;
using BalloonPop.Core;

namespace BalloonPop.Effects
{
    public class CinematicWin : MonoBehaviour
    {
        [SerializeField] private float zoomTarget = 0.7f;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private float slowMoScale = 0.4f;

        public static CinematicWin Instance { get; private set; }
        private Coroutine current;
        private float origSize;

        private void Awake()
        {
            Instance = this;
            GameEvents.OnLevelWon += Play;
        }

        private void OnDestroy()
        {
            GameEvents.OnLevelWon -= Play;
            Time.timeScale = 1f;
        }

        private void Play()
        {
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            origSize = cam.orthographicSize;
            float startSize = cam.orthographicSize;
            float targetSize = startSize * zoomTarget;

            Time.timeScale = slowMoScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.SmoothStep(0f, 1f, t / duration);
                cam.orthographicSize = Mathf.Lerp(startSize, targetSize, u);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.4f);
            Time.timeScale = 1f;
        }
    }
}
