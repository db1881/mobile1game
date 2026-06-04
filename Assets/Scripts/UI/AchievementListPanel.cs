using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Gameplay;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class AchievementListPanel : MonoBehaviour
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab;

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            if (itemContainer == null || itemPrefab == null) return;
            if (AchievementManager.Instance == null) return;

            foreach (Transform child in itemContainer) Destroy(child.gameObject);

            foreach (var def in AchievementManager.Instance.Definitions)
            {
                var item = Instantiate(itemPrefab, itemContainer);
                bool unlocked = AchievementManager.Instance.IsUnlocked(def.Key);
                int progress = AchievementManager.Instance.GetProgress(def.Key);

                var texts = item.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0) texts[0].text = def.Title;
                if (texts.Length > 1) texts[1].text = def.Description;
                if (texts.Length > 2)
                    texts[2].text = unlocked ? "TAMAM" : $"{Mathf.Min(progress, def.Target)}/{def.Target}";

                var images = item.GetComponentsInChildren<Image>(true);
                if (images.Length > 0)
                    images[0].color = unlocked
                        ? new Color(1f, 0.83f, 0.24f, 1f)
                        : new Color(0.3f, 0.3f, 0.4f, 0.5f);
            }
        }
    }
}
