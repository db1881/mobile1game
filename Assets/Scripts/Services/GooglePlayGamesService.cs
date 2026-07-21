using System;
using BalloonPop.Core;
using BalloonPop.Gameplay;
using BalloonPop.Save;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace BalloonPop.Services
{
    /// <summary>
    /// Google Play Games authentication and high-score leaderboard integration.
    /// The service is bootstrapped before the first scene and survives scene changes.
    /// </summary>
    public sealed class GooglePlayGamesService : MonoBehaviour
    {
        private const string ConfigResourceName = "GooglePlayGamesConfig";

        private static GooglePlayGamesService instance;
        private GooglePlayGamesConfig config;
        private bool authenticationInProgress;
        private long pendingHighScore;

        public static GooglePlayGamesService Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject(nameof(GooglePlayGamesService));
                    instance = go.AddComponent<GooglePlayGamesService>();
                }

                return instance;
            }
        }

        public event Action StateChanged;

        public bool IsAuthenticationInProgress => authenticationInProgress;
        public bool IsConfigured => config != null && !string.IsNullOrWhiteSpace(config.HighScoreLeaderboardId);

        public bool IsAuthenticated
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return PlayGamesPlatform.Instance.IsAuthenticated();
#else
                return false;
#endif
            }
        }

        public string PlayerName
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (IsAuthenticated && PlayGamesPlatform.Instance.localUser != null)
                    return PlayGamesPlatform.Instance.localUser.userName;
#endif
                return string.Empty;
            }
        }

        public string LastStatus { get; private set; } = "Google Play hazır";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            config = Resources.Load<GooglePlayGamesConfig>(ConfigResourceName);
            GameEvents.OnLevelWon += HandleLevelWon;
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            GameEvents.OnLevelWon -= HandleLevelWon;
        }

        /// <summary>Attempts the non-intrusive automatic sign-in performed at game start.</summary>
        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsAuthenticated || authenticationInProgress) return;

            PlayGamesPlatform.Activate();
            authenticationInProgress = true;
            LastStatus = "Google Play'e bağlanıyor...";
            RaiseStateChanged();
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#elif UNITY_EDITOR
            LastStatus = IsConfigured
                ? "Google Play yalnızca Android cihazda çalışır"
                : "Play Console liderlik kimliği bekleniyor";
            RaiseStateChanged();
#else
            LastStatus = "Google Play bu platformda desteklenmiyor";
            RaiseStateChanged();
#endif
        }

        /// <summary>Shows Google's user-facing sign-in flow after a button press.</summary>
        public void SignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsAuthenticated)
            {
                LastStatus = "Google Play'e giriş yapıldı";
                RaiseStateChanged();
                return;
            }

            if (authenticationInProgress) return;
            PlayGamesPlatform.Activate();
            authenticationInProgress = true;
            LastStatus = "Google ile giriş açılıyor...";
            RaiseStateChanged();
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
#else
            Initialize();
#endif
        }

        public void ShowLeaderboard()
        {
            if (!EnsureConfigured()) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsAuthenticated)
            {
                SignInAndThen(ShowLeaderboardInternal);
                return;
            }

            ShowLeaderboardInternal();
#else
            Initialize();
#endif
        }

        /// <summary>Queues and submits a score; Google keeps the player's highest value.</summary>
        public void SubmitHighScore(long score)
        {
            if (score <= 0) return;
            pendingHighScore = Math.Max(pendingHighScore, score);
            if (!IsConfigured || !IsAuthenticated) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            long scoreToSubmit = pendingHighScore;
            PlayGamesPlatform.Instance.ReportScore(scoreToSubmit, config.HighScoreLeaderboardId, success =>
            {
                if (success)
                {
                    pendingHighScore = 0;
                    LastStatus = "En yüksek skor gönderildi";
                }
                else
                {
                    LastStatus = "Skor gönderilemedi; tekrar denenecek";
                }

                RaiseStateChanged();
            });
#endif
        }

        private void HandleLevelWon()
        {
            if (ScoreManager.Instance != null)
                SubmitHighScore(ScoreManager.Instance.CurrentScore);
        }

        private long GetBestLocalScore()
        {
            long best = pendingHighScore;
            LevelRecord[] levels = SaveSystem.Data.Levels;
            if (levels == null) return best;

            foreach (LevelRecord level in levels)
            {
                if (level != null) best = Math.Max(best, level.BestScore);
            }

            return best;
        }

        private bool EnsureConfigured()
        {
            if (IsConfigured) return true;
            LastStatus = "Play Console liderlik kimliği bekleniyor";
            RaiseStateChanged();
            Debug.LogWarning("[GooglePlayGames] GooglePlayGamesConfig içindeki leaderboard kimliği henüz ayarlanmadı.");
            return false;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void ProcessAuthentication(SignInStatus status)
        {
            authenticationInProgress = false;
            if (status == SignInStatus.Success)
            {
                LastStatus = "Google Play'e giriş yapıldı";
                RaiseStateChanged();
                SubmitHighScore(GetBestLocalScore());
            }
            else
            {
                LastStatus = "Google Play girişi başarısız: " + status;
                RaiseStateChanged();
            }
        }

        private void SignInAndThen(Action continuation)
        {
            if (authenticationInProgress) return;
            PlayGamesPlatform.Activate();
            authenticationInProgress = true;
            LastStatus = "Google ile giriş açılıyor...";
            RaiseStateChanged();
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
            {
                ProcessAuthentication(status);
                if (status == SignInStatus.Success) continuation?.Invoke();
            });
        }

        private void ShowLeaderboardInternal()
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(config.HighScoreLeaderboardId);
        }
#endif

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
