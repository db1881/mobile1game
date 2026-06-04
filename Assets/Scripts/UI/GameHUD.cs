using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Core;
using BalloonPop.Data;

namespace BalloonPop.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Goals")]
        [SerializeField] private Transform goalContainer;
        [SerializeField] private GoalItemUI goalItemPrefab;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private WinPanel winPanel;
        [SerializeField] private LosePanel losePanel;
        [SerializeField] private GameObject mysteryBoxPanel;

        private readonly Dictionary<BalloonType, GoalItemUI> goalItems = new Dictionary<BalloonType, GoalItemUI>();

        private void OnEnable()
        {
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnMovesChanged += UpdateMoves;
            GameEvents.OnGoalProgress += UpdateGoal;
            GameEvents.OnLevelStarted += BuildGoals;
            GameEvents.OnLevelWon += ShowWin;
            GameEvents.OnLevelLost += ShowLose;
        }

        private void OnDisable()
        {
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnMovesChanged -= UpdateMoves;
            GameEvents.OnGoalProgress -= UpdateGoal;
            GameEvents.OnLevelStarted -= BuildGoals;
            GameEvents.OnLevelWon -= ShowWin;
            GameEvents.OnLevelLost -= ShowLose;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        private void UpdateMoves(int moves)
        {
            if (movesText != null) movesText.text = moves.ToString();
        }

        private void BuildGoals()
        {
            foreach (Transform child in goalContainer) Destroy(child.gameObject);
            goalItems.Clear();

            var lvl = GameManager.Instance.CurrentLevel;
            if (lvl == null) return;

            if (levelText != null)
            {
                string worldName = GameSceneBootstrap.GetWorldName(lvl.WorldIndex);
                levelText.text = string.IsNullOrEmpty(worldName)
                    ? $"Level {lvl.LevelNumber}"
                    : $"{worldName} • {lvl.LevelNumber}";
            }

            foreach (var goal in lvl.Goals)
            {
                var item = Instantiate(goalItemPrefab, goalContainer);
                item.Setup(goal.Color, goal.Amount);
                goalItems[goal.Color] = item;
            }
        }

        private void UpdateGoal(BalloonType type, int remaining)
        {
            if (goalItems.TryGetValue(type, out var item))
                item.UpdateRemaining(remaining);
        }

        private void ShowWin()
        {
            Debug.Log($"[GameHUD] ShowWin called. winPanel={(winPanel != null ? "OK" : "NULL")}");
            if (winPanel != null)
            {
                winPanel.Show();
            }
            else
            {
                Debug.LogError("[GameHUD] winPanel reference is NULL! WinPanel will not show.");
            }

            if (mysteryBoxPanel != null && GameManager.Instance != null && GameManager.Instance.CurrentLevel != null)
            {
                int lvl = GameManager.Instance.CurrentLevel.LevelNumber;
                if (lvl % 5 == 0) StartCoroutine(ShowMysteryAfter(2.5f));
            }
        }

        private System.Collections.IEnumerator ShowMysteryAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (mysteryBoxPanel != null) mysteryBoxPanel.SetActive(true);
        }

        private void ShowLose()
        {
            Debug.Log($"[GameHUD] ShowLose called. losePanel={(losePanel != null ? "OK" : "NULL")}");
            if (losePanel != null) losePanel.Show();
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State == GameState.Won && winPanel != null && !winPanel.gameObject.activeSelf)
                winPanel.Show();
            else if (GameManager.Instance.State == GameState.Lost && losePanel != null && !losePanel.gameObject.activeSelf)
                losePanel.Show();
        }

        public void TogglePause()
        {
            if (pausePanel == null) return;
            bool active = !pausePanel.activeSelf;
            pausePanel.SetActive(active);
            if (active) GameManager.Instance.PauseGame();
            else GameManager.Instance.ResumeGame();
        }
    }
}
