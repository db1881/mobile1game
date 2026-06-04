using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    public class LosePanel : MonoBehaviour
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            gameObject.SetActive(false);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
        }

        public void Show()
        {
            Debug.Log("[LosePanel] Show() called");
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        private void OnRetry()
        {
            gameObject.SetActive(false);
            GameManager.Instance.RestartLevel();
        }

        private void OnMenu() => LevelLoader.Instance.GoToMenu();
    }
}
