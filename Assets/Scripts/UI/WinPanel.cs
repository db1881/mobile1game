using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Core;
using BalloonPop.Gameplay;

namespace BalloonPop.UI
{
    public class WinPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private GameObject[] stars;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button replayButton;

        private void Awake()
        {
            gameObject.SetActive(false);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevel);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
            if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
        }

        private void OnReplay()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.RestartLevel();
        }

        public void Show()
        {
            Debug.Log("[WinPanel] Show() called");
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            var lvl = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : null;
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            int starCount = 0;
            if (lvl != null && ScoreManager.Instance != null)
                starCount = ScoreManager.Instance.GetStarRating(
                    lvl.StarOneScore, lvl.StarTwoScore, lvl.StarThreeScore);

            if (finalScoreText != null) finalScoreText.text = score.ToString("N0");
            if (stars != null)
                for (int i = 0; i < stars.Length; i++)
                    if (stars[i] != null) stars[i].SetActive(i < starCount);
        }

        private void OnNextLevel()
        {
            var current = GameManager.Instance.CurrentLevel;
            int nextNumber = current.LevelNumber + 1;
            var nextLevel = LevelLoader.Instance.Database != null
                ? LevelLoader.Instance.Database.GetByNumber(nextNumber)
                : null;
            if (nextLevel == null) LevelLoader.Instance.GoToMenu();
            else LevelLoader.Instance.LoadLevelByNumber(nextNumber);
        }

        private void OnMenu() => LevelLoader.Instance.GoToMenu();
    }
}
