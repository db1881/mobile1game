using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private GameObject[] starIcons;

        private int levelNumber;

        public void Setup(int number, bool unlocked, int stars)
        {
            levelNumber = number;
            numberText.text = number.ToString();
            lockedIcon.SetActive(!unlocked);
            button.interactable = unlocked;

            for (int i = 0; i < starIcons.Length; i++)
                starIcons[i].SetActive(unlocked && i < stars);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => LevelLoader.Instance.LoadLevelByNumber(levelNumber));
        }
    }
}
