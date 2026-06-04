using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Core;
using BalloonPop.Audio;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject noHeartsPanel;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(PlayLastLevel);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(() => levelSelectPanel?.SetActive(true));
            if (settingsButton != null) settingsButton.onClick.AddListener(() => settingsPanel?.SetActive(true));
            if (quitButton != null) quitButton.onClick.AddListener(Application.Quit);
        }

        private void Start()
        {
            AudioManager.Instance?.PlayMusic("menu");
        }

        private void PlayLastLevel()
        {
            if (!HeartSystem.CanPlay())
            {
                if (noHeartsPanel != null) noHeartsPanel.SetActive(true);
                return;
            }
            int last = SaveSystem.GetHighestUnlockedLevel();
            LevelLoader.Instance.LoadLevelByNumber(last);
        }
    }
}
