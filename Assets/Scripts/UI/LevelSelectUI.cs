using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class LevelSelectUI : MonoBehaviour
    {
        [SerializeField] private LevelDatabase database;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private LevelButton buttonPrefab;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (database == null) database = Resources.Load<LevelDatabase>("LevelDatabase");
            if (database == null)
            {
                Debug.LogError("[LevelSelectUI] LevelDatabase not found.");
                return;
            }

            foreach (Transform t in buttonContainer) Destroy(t.gameObject);

            int unlocked = SaveSystem.GetHighestUnlockedLevel();
            foreach (var level in database.Levels)
            {
                if (level == null) continue;
                var btn = Instantiate(buttonPrefab, buttonContainer);
                bool isUnlocked = level.LevelNumber <= unlocked;
                int stars = SaveSystem.GetLevelStars(level.LevelNumber);
                btn.Setup(level.LevelNumber, isUnlocked, stars);
            }
        }
    }
}
