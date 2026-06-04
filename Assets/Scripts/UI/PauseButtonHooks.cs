using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    /// <summary> Pause panel'inde "DEVAM ET" tıklanınca panel'i kapatır + GameManager resume. </summary>
    [RequireComponent(typeof(Button))]
    public class PauseResumeHook : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        public void SetPanel(GameObject panel) => pausePanel = panel;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                if (pausePanel != null) pausePanel.SetActive(false);
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });
        }
    }

    /// <summary> Pause panel'inde "TEKRAR DENE" tıklanınca mevcut sahneyi yeniden yükler. </summary>
    [RequireComponent(typeof(Button))]
    public class PauseReplayHook : MonoBehaviour
    {
        private void OnEnable()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(DoReplay);
        }

        private void DoReplay()
        {
            Debug.Log("[PauseReplayHook] Replay clicked");
            Time.timeScale = 1f;
            if (LevelLoader.Instance != null && GameManager.Instance != null && GameManager.Instance.CurrentLevel != null)
            {
                LevelLoader.Instance.LoadLevelByNumber(GameManager.Instance.CurrentLevel.LevelNumber);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }

    /// <summary> Pause panel'inde "ANA MENÜ" tıklanınca menüye dön. </summary>
    [RequireComponent(typeof(Button))]
    public class PauseMenuHook : MonoBehaviour
    {
        private void OnEnable()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(DoGoToMenu);
        }

        private void DoGoToMenu()
        {
            Time.timeScale = 1f;
            int count = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            string activeName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            for (int i = 0; i < count; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string n = System.IO.Path.GetFileNameWithoutExtension(path);
                if (n != activeName)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(n);
                    return;
                }
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
