using System;
using System.Collections.Generic;
using BalloonPop.Core;
using BalloonPop.Gameplay;
using BalloonPop.Save;
using UnityEngine;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace BalloonPop.Services
{
    public sealed class LeaderboardEntry
    {
        public LeaderboardEntry(int rank, string playerName, long score, bool isCurrentPlayer)
        {
            Rank = rank;
            PlayerName = playerName;
            Score = score;
            IsCurrentPlayer = isCurrentPlayer;
        }

        public int Rank { get; }
        public string PlayerName { get; }
        public long Score { get; }
        public bool IsCurrentPlayer { get; }
    }

    public sealed class LeaderboardSnapshot
    {
        private LeaderboardSnapshot(bool success, string error, LeaderboardEntry[] entries,
            LeaderboardEntry playerEntry, bool isPreview)
        {
            Success = success;
            Error = error;
            Entries = entries ?? Array.Empty<LeaderboardEntry>();
            PlayerEntry = playerEntry;
            IsPreview = isPreview;
        }

        public bool Success { get; }
        public string Error { get; }
        public LeaderboardEntry[] Entries { get; }
        public LeaderboardEntry PlayerEntry { get; }
        public bool IsPreview { get; }

        public static LeaderboardSnapshot Loaded(LeaderboardEntry[] entries, LeaderboardEntry playerEntry,
            bool isPreview = false)
        {
            return new LeaderboardSnapshot(true, string.Empty, entries, playerEntry, isPreview);
        }

        public static LeaderboardSnapshot Failed(string error)
        {
            return new LeaderboardSnapshot(false, error, Array.Empty<LeaderboardEntry>(), null, false);
        }
    }

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
#if UNITY_ANDROID && !UNITY_EDITOR
        private Action authenticationContinuation;
        private Action authenticationFailure;
#endif

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

        /// <summary>
        /// Loads public, all-time high scores without leaving the game. Authentication is requested
        /// only when needed, then the same request continues automatically.
        /// </summary>
        public void LoadLeaderboard(int rowCount, Action<LeaderboardSnapshot> callback)
        {
            if (callback == null) return;
            if (!EnsureConfigured())
            {
                callback(LeaderboardSnapshot.Failed("Google Play yapılandırması eksik"));
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            int requestedRows = Mathf.Clamp(rowCount, 1, 25);
            if (!IsAuthenticated)
            {
                SignInAndThen(
                    () => LoadLeaderboardInternal(requestedRows, callback),
                    () => callback(LeaderboardSnapshot.Failed("Google Play girişi başarısız")));
                return;
            }

            LoadLeaderboardInternal(requestedRows, callback);
#elif UNITY_EDITOR
            long previewScore = Math.Max(12500L, GetBestLocalScore());
            var previewEntry = new LeaderboardEntry(1, "EDITOR PREVIEW", previewScore, true);
            callback(LeaderboardSnapshot.Loaded(new[] { previewEntry }, previewEntry, true));
#else
            callback(LeaderboardSnapshot.Failed("Google Play bu platformda desteklenmiyor"));
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

            Action continuation = authenticationContinuation;
            Action failure = authenticationFailure;
            authenticationContinuation = null;
            authenticationFailure = null;
            if (status == SignInStatus.Success)
                continuation?.Invoke();
            else
                failure?.Invoke();
        }

        private void SignInAndThen(Action continuation, Action failure = null)
        {
            if (IsAuthenticated)
            {
                continuation?.Invoke();
                return;
            }

            authenticationContinuation += continuation;
            authenticationFailure += failure;
            if (authenticationInProgress) return;
            PlayGamesPlatform.Activate();
            authenticationInProgress = true;
            LastStatus = "Google ile giriş açılıyor...";
            RaiseStateChanged();
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }

        private void ShowLeaderboardInternal()
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(config.HighScoreLeaderboardId);
        }

        private void LoadLeaderboardInternal(int rowCount, Action<LeaderboardSnapshot> callback)
        {
            LastStatus = "Liderlik tablosu yükleniyor...";
            RaiseStateChanged();

            PlayGamesPlatform.Instance.LoadScores(
                config.HighScoreLeaderboardId,
                LeaderboardStart.TopScores,
                rowCount,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                scoreData =>
                {
                    if (scoreData == null || !scoreData.Valid)
                    {
                        string status = scoreData == null ? "Empty response" : scoreData.Status.ToString();
                        CompleteLeaderboardFailure(callback, "Skorlar yüklenemedi (" + status + ")");
                        return;
                    }

                    var playerIds = new List<string>();
                    IScore[] scores = scoreData.Scores ?? Array.Empty<IScore>();
                    for (int i = 0; i < scores.Length; i++)
                        AddPlayerId(playerIds, scores[i]);
                    AddPlayerId(playerIds, scoreData.PlayerScore);

                    if (playerIds.Count == 0)
                    {
                        CompleteLeaderboardSuccess(callback, scoreData, scores,
                            Array.Empty<IUserProfile>());
                        return;
                    }

                    PlayGamesPlatform.Instance.LoadUsers(playerIds.ToArray(), profiles =>
                    {
                        CompleteLeaderboardSuccess(callback, scoreData, scores,
                            profiles ?? Array.Empty<IUserProfile>());
                    });
                });
        }

        private static void AddPlayerId(List<string> playerIds, IScore score)
        {
            if (score == null || string.IsNullOrWhiteSpace(score.userID)) return;
            if (!playerIds.Contains(score.userID)) playerIds.Add(score.userID);
        }

        private void CompleteLeaderboardSuccess(Action<LeaderboardSnapshot> callback,
            LeaderboardScoreData scoreData, IScore[] scores, IUserProfile[] profiles)
        {
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < profiles.Length; i++)
            {
                IUserProfile profile = profiles[i];
                if (profile == null || string.IsNullOrWhiteSpace(profile.id)) continue;
                names[profile.id] = profile.userName;
            }

            string localPlayerId = PlayGamesPlatform.Instance.localUser != null
                ? PlayGamesPlatform.Instance.localUser.id
                : string.Empty;
            var entries = new LeaderboardEntry[scores.Length];
            for (int i = 0; i < scores.Length; i++)
                entries[i] = CreateLeaderboardEntry(scores[i], names, localPlayerId);

            LeaderboardEntry playerEntry = scoreData.PlayerScore != null && scoreData.PlayerScore.rank > 0
                ? CreateLeaderboardEntry(scoreData.PlayerScore, names, localPlayerId)
                : null;

            LastStatus = "Liderlik tablosu hazır";
            RaiseStateChanged();
            callback(LeaderboardSnapshot.Loaded(entries, playerEntry));
        }

        private LeaderboardEntry CreateLeaderboardEntry(IScore score, Dictionary<string, string> names,
            string localPlayerId)
        {
            if (score == null) return null;
            bool isCurrentPlayer = !string.IsNullOrWhiteSpace(localPlayerId) &&
                                   string.Equals(score.userID, localPlayerId, StringComparison.Ordinal);
            string playerName;
            if (!names.TryGetValue(score.userID ?? string.Empty, out playerName) ||
                string.IsNullOrWhiteSpace(playerName))
            {
                playerName = isCurrentPlayer && !string.IsNullOrWhiteSpace(PlayerName)
                    ? PlayerName
                    : "Oyuncu";
            }

            return new LeaderboardEntry(score.rank, playerName, score.value, isCurrentPlayer);
        }

        private void CompleteLeaderboardFailure(Action<LeaderboardSnapshot> callback, string message)
        {
            LastStatus = message;
            RaiseStateChanged();
            callback(LeaderboardSnapshot.Failed(message));
        }
#endif

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
