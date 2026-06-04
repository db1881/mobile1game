using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Data;

namespace BalloonPop.UI
{
    public class GoalItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI remainingText;
        [SerializeField] private GameObject checkMark;
        [SerializeField] private Sprite[] colorIcons;

        private BalloonType type;
        private int initial;

        public void Setup(BalloonType color, int amount)
        {
            type = color;
            initial = amount;
            int idx = (int)color - 1;
            if (icon != null && colorIcons != null && idx >= 0 && idx < colorIcons.Length)
                icon.sprite = colorIcons[idx];
            UpdateRemaining(amount);
        }

        public void UpdateRemaining(int remaining)
        {
            if (remainingText != null) remainingText.text = remaining.ToString();
            if (checkMark != null) checkMark.SetActive(remaining <= 0);
            if (remaining <= 0 && remainingText != null) remainingText.gameObject.SetActive(false);
        }
    }
}
