using UnityEngine;
using BalloonPop.Audio;
using BalloonPop.Data;

namespace BalloonPop.Core
{
    public class GameSceneBootstrap : MonoBehaviour
    {
        private static readonly Color[] WorldColors = {
            new Color(0.15f, 0.32f, 0.55f),
            new Color(0.30f, 0.55f, 0.85f),
            new Color(0.55f, 0.75f, 0.92f),
            new Color(0.10f, 0.05f, 0.28f),
            new Color(0.55f, 0.22f, 0.55f)
        };

        private static readonly string[] WorldNames = { "", "SAHİL", "KIŞ", "UZAY", "ŞEKER" };

        private void Start()
        {
            // Self-healing: pause panel butonlarına eksik olabilecek hook'ları ekle
            // (sahne cache'i eski olabilir, scriptler güncellenmiş olabilir)
            EnsurePauseHooks();

            AudioManager.Instance?.PlayMusic("game");
            if (LevelLoader.Instance == null) return;

            if (LevelLoader.Instance.PendingLevel != null)
            {
                ApplyWorldTheme(LevelLoader.Instance.PendingLevel.WorldIndex);
                LevelLoader.Instance.OnGameSceneLoaded();
                return;
            }

            var db = LevelLoader.Instance.Database;
            if (db == null) db = Resources.Load<LevelDatabase>("LevelDatabase");
            var fallback = db != null ? db.GetByNumber(1) : null;
            if (fallback != null && GameManager.Instance != null)
            {
                ApplyWorldTheme(fallback.WorldIndex);
                GameManager.Instance.LoadLevel(fallback);
            }
            else
            {
                Debug.LogError("[GameSceneBootstrap] No pending level and no fallback LevelData found.");
            }
        }

        private void ApplyWorldTheme(int worldIndex)
        {
            int idx = Mathf.Clamp(worldIndex, 1, WorldColors.Length - 1);
            var cam = Camera.main;
            if (cam != null) cam.backgroundColor = WorldColors[idx];
            if (BalloonPop.Effects.ThemeBackground.Instance != null)
                BalloonPop.Effects.ThemeBackground.Instance.SetWorld(worldIndex);
        }

        public static string GetWorldName(int worldIndex)
        {
            int idx = Mathf.Clamp(worldIndex, 0, WorldNames.Length - 1);
            return WorldNames[idx];
        }

        /// <summary>
        /// Sahne cache'inde eksik kalan pause panel hook'larını ekler.
        /// Idempotent: hook varsa atlar.
        /// </summary>
        private void EnsurePauseHooks()
        {
            // PausePanel başlangıçta inactive olduğu için Find() ile bulunmaz —
            // tüm Transform'ları tarayıp ismiyle eşleştir
            GameObject pp = null;
            var allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in allRoots)
            {
                var t = root.transform.Find("PausePanel");
                if (t != null) { pp = t.gameObject; break; }
                // Recursive: belki canvas içinde
                var trans = root.GetComponentsInChildren<Transform>(true);
                foreach (var tr in trans)
                {
                    if (tr.gameObject.name == "PausePanel") { pp = tr.gameObject; break; }
                }
                if (pp != null) break;
            }
            if (pp == null) return;
            var buttons = pp.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var b in buttons)
            {
                string n = b.gameObject.name;
                if (n == "ReplayButton" && b.gameObject.GetComponent<BalloonPop.UI.PauseReplayHook>() == null)
                {
                    b.gameObject.AddComponent<BalloonPop.UI.PauseReplayHook>();
                }
                else if (n == "MenuButton" && b.gameObject.GetComponent<BalloonPop.UI.PauseMenuHook>() == null)
                {
                    b.gameObject.AddComponent<BalloonPop.UI.PauseMenuHook>();
                }
                else if (n == "ResumeButton" && b.gameObject.GetComponent<BalloonPop.UI.PauseResumeHook>() == null)
                {
                    var hook = b.gameObject.AddComponent<BalloonPop.UI.PauseResumeHook>();
                    hook.SetPanel(pp);
                }
            }
        }
    }
}
