using UnityEngine;
using UnityEngine.SceneManagement;
using BalloonPop.Data;
using BalloonPop.Effects;

namespace BalloonPop.Core
{
    public class LevelLoader : Singleton<LevelLoader>
    {
        [SerializeField] private LevelDatabase database;
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string menuSceneName = "MainMenu";

        public LevelDatabase Database => database;
        public LevelData PendingLevel { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) DontDestroyOnLoad(gameObject);
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            if (database != null) return;
            database = Resources.Load<LevelDatabase>("LevelDatabase");
            if (database == null)
                Debug.LogError("[LevelLoader] LevelDatabase not found. Expected at Resources/LevelDatabase.asset");
        }

        public void LoadLevelByNumber(int levelNumber)
        {
            var data = database != null ? database.GetByNumber(levelNumber) : null;
            if (data == null)
            {
                Debug.LogError($"Level {levelNumber} not found in database");
                return;
            }
            PendingLevel = data;
            LoadSceneWithFade(gameSceneName);
        }

        public void GoToMenu() => LoadSceneWithFade(menuSceneName);

        private void LoadSceneWithFade(string sceneName)
        {
            if (SceneTransitionFader.Instance != null)
                SceneTransitionFader.Instance.FadeOutAndLoad(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }

        public void OnGameSceneLoaded()
        {
            if (PendingLevel != null && GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevel(PendingLevel);
            }
        }
    }
}
