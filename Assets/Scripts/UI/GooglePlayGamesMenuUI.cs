using BalloonPop.Core;
using BalloonPop.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalloonPop.UI
{
    public sealed class GooglePlayGamesMenuUI : MonoBehaviour
    {
        [SerializeField] private Button signInButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private TMP_Text signInLabel;
        [SerializeField] private TMP_Text leaderboardLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private LeaderboardPanel leaderboardPanel;

        private GooglePlayGamesService service;

        private void Awake()
        {
            if (signInButton != null) signInButton.onClick.AddListener(SignIn);
            if (leaderboardButton != null) leaderboardButton.onClick.AddListener(ShowLeaderboard);
        }

        private void OnEnable()
        {
            service = GooglePlayGamesService.Instance;
            service.StateChanged += Refresh;
            LocalizationManager.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (service != null) service.StateChanged -= Refresh;
            LocalizationManager.OnLanguageChanged -= Refresh;
        }

        private void SignIn()
        {
            service?.SignIn();
        }

        private void ShowLeaderboard()
        {
            leaderboardPanel?.Open();
        }

        private void Refresh()
        {
            if (service == null) return;
            bool turkish = LocalizationManager.Current == LocalizationManager.Lang.TR;

            if (signInLabel != null)
            {
                if (service.IsAuthenticated)
                    signInLabel.text = string.IsNullOrWhiteSpace(service.PlayerName)
                        ? (turkish ? "GİRİŞ YAPILDI" : "SIGNED IN")
                        : service.PlayerName;
                else if (service.IsAuthenticationInProgress)
                    signInLabel.text = turkish ? "BAĞLANIYOR..." : "CONNECTING...";
                else
                    signInLabel.text = turkish ? "GOOGLE İLE GİRİŞ" : "SIGN IN WITH GOOGLE";
            }

            if (leaderboardLabel != null)
                leaderboardLabel.text = turkish ? "YILDIZ LİGİ" : "STAR LEAGUE";

            if (statusLabel != null)
            {
                statusLabel.text = service.IsAuthenticated
                    ? (turkish ? "Google Play'e bağlı" : "Connected to Google Play")
                    : (turkish ? "Google Play'e bağlanıyor..." : "Connecting to Google Play...");
                bool showStatus = service.IsAuthenticationInProgress || service.IsAuthenticated;
#if !UNITY_EDITOR
                showStatus |= !service.IsConfigured;
#endif
                if (!service.IsConfigured)
                    statusLabel.text = turkish ? "Google Play yapılandırması eksik" : "Google Play setup required";
                statusLabel.gameObject.SetActive(showStatus);
            }

            if (signInButton != null)
                signInButton.interactable = !service.IsAuthenticationInProgress && !service.IsAuthenticated;
            if (leaderboardButton != null)
                leaderboardButton.interactable = service.IsConfigured;
        }
    }
}
