using System;
using System.Collections.Generic;
using BalloonPop.Core;
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
        public LeaderboardEntry(int rank, string playerName, long totalStars, bool isCurrentPlayer)
        {
            Rank = rank;
            PlayerName = playerName;
            TotalStars = totalStars;
            IsCurrentPlayer = isCurrentPlayer;
        }

        public int Rank { get; }
        public string PlayerName { get; }
        public long TotalStars { get; }
        public bool IsCurrentPlayer { get; }
    }

    public sealed class LeaderboardSnapshot
    {
        private LeaderboardSnapshot(bool success, string error, LeaderboardEntry[] entries,
            LeaderboardEntry playerEntry, bool isPreview, bool isCached)
        {
            Success = success;
            Error = error;
            Entries = entries ?? Array.Empty<LeaderboardEntry>();
            PlayerEntry = playerEntry;
            IsPreview = isPreview;
            IsCached = isCached;
        }

        public bool Success { get; }
        public string Error { get; }
        public LeaderboardEntry[] Entries { get; }
        public LeaderboardEntry PlayerEntry { get; }
        public bool IsPreview { get; }
        public bool IsCached { get; }

        public static LeaderboardSnapshot Loaded(LeaderboardEntry[] entries, LeaderboardEntry playerEntry,
            bool isPreview = false, bool isCached = false)
        {
            return new LeaderboardSnapshot(true, string.Empty, entries, playerEntry, isPreview, isCached);
        }

        public static LeaderboardSnapshot Failed(string error)
        {
            return new LeaderboardSnapshot(false, error, Array.Empty<LeaderboardEntry>(), null, false, false);
        }
    }

    /// <summary>
    /// Google Play Games authentication and high-score leaderboard integration.
    /// The service is bootstrapped before the first scene and survives scene changes.
    /// </summary>
    public sealed class GooglePlayGamesService : MonoBehaviour
    {
        private const string ConfigResourceName = "GooglePlayGamesConfig";
        private const string LeaderboardCacheKey = "google_play_total_stars_top_50_v1";
        private const int GooglePageSize = 25;

        private static GooglePlayGamesService instance;
        private GooglePlayGamesConfig config;
        private bool authenticationInProgress;
        private long pendingTotalStars;
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
        public bool IsConfigured => config != null && !string.IsNullOrWhiteSpace(config.TotalStarsLeaderboardId);

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
            int requestedRows = Mathf.Clamp(rowCount, 1, 50);
            if (!IsAuthenticated)
            {
                SignInAndThen(
                    () => LoadLeaderboardInternal(requestedRows, callback),
                    () => callback(LoadCachedOrLocalSnapshot()));
                return;
            }

            LoadLeaderboardInternal(requestedRows, callback);
#elif UNITY_EDITOR
            callback(CreateEditorPreview());
#else
            callback(LoadCachedOrLocalSnapshot());
#endif
        }

        /// <summary>Queues and submits total stars; Google keeps the player's highest value.</summary>
        public void SubmitTotalStars(long totalStars)
        {
            if (totalStars < 0) return;
            pendingTotalStars = Math.Max(pendingTotalStars, totalStars);
            if (!IsConfigured || !IsAuthenticated) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            long starsToSubmit = pendingTotalStars;
            PlayGamesPlatform.Instance.ReportScore(starsToSubmit, config.TotalStarsLeaderboardId, success =>
            {
                if (success)
                {
                    pendingTotalStars = 0;
                    LastStatus = "Toplam yıldız gönderildi";
                }
                else
                {
                    LastStatus = "Yıldızlar gönderilemedi; tekrar denenecek";
                }

                RaiseStateChanged();
            });
#endif
        }

        private void HandleLevelWon()
        {
            SubmitTotalStars(SaveSystem.GetTotalStars());
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
                SubmitTotalStars(SaveSystem.GetTotalStars());
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
            PlayGamesPlatform.Instance.ShowLeaderboardUI(config.TotalStarsLeaderboardId);
        }

        private void LoadLeaderboardInternal(int rowCount, Action<LeaderboardSnapshot> callback)
        {
            LastStatus = "Liderlik tablosu yükleniyor...";
            RaiseStateChanged();

            PlayGamesPlatform.Instance.LoadScores(
                config.TotalStarsLeaderboardId,
                LeaderboardStart.TopScores,
                Mathf.Min(GooglePageSize, rowCount),
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                scoreData =>
                {
                    if (scoreData == null || !scoreData.Valid)
                    {
                        string status = scoreData == null ? "Empty response" : scoreData.Status.ToString();
                        CompleteLeaderboardFailure(callback, "Yıldız sıralaması yüklenemedi (" + status + ")");
                        return;
                    }

                    var scores = new List<IScore>(scoreData.Scores ?? Array.Empty<IScore>());
                    int remaining = rowCount - scores.Count;
                    if (remaining > 0 && scoreData.NextPageToken != null)
                    {
                        PlayGamesPlatform.Instance.LoadMoreScores(scoreData.NextPageToken,
                            Mathf.Min(GooglePageSize, remaining), nextPage =>
                            {
                                if (nextPage != null && nextPage.Valid && nextPage.Scores != null)
                                    scores.AddRange(nextPage.Scores);
                                LoadProfilesAndComplete(callback, scoreData, scores.ToArray());
                            });
                        return;
                    }

                    LoadProfilesAndComplete(callback, scoreData, scores.ToArray());
                });
        }

        private void LoadProfilesAndComplete(Action<LeaderboardSnapshot> callback,
            LeaderboardScoreData scoreData, IScore[] scores)
        {
            var playerIds = new List<string>();
            for (int i = 0; i < scores.Length; i++) AddPlayerId(playerIds, scores[i]);
            AddPlayerId(playerIds, scoreData.PlayerScore);

            if (playerIds.Count == 0)
            {
                CompleteLeaderboardSuccess(callback, scoreData, scores, Array.Empty<IUserProfile>());
                return;
            }

            PlayGamesPlatform.Instance.LoadUsers(playerIds.ToArray(), profiles =>
                CompleteLeaderboardSuccess(callback, scoreData, scores,
                    profiles ?? Array.Empty<IUserProfile>()));
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
            var snapshot = LeaderboardSnapshot.Loaded(entries, playerEntry);
            SaveLeaderboardCache(snapshot);
            callback(snapshot);
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
            callback(LoadCachedOrLocalSnapshot());
        }
#endif

        [Serializable]
        private sealed class CachedEntry
        {
            public int rank;
            public string playerName;
            public long totalStars;
            public bool isCurrentPlayer;
        }

        [Serializable]
        private sealed class CachedLeaderboard
        {
            public CachedEntry[] entries;
            public CachedEntry playerEntry;
        }

        private void SaveLeaderboardCache(LeaderboardSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.Success || snapshot.IsPreview) return;
            var cache = new CachedLeaderboard
            {
                entries = ToCachedEntries(snapshot.Entries),
                playerEntry = ToCachedEntry(snapshot.PlayerEntry)
            };
            PlayerPrefs.SetString(LeaderboardCacheKey, JsonUtility.ToJson(cache));
            PlayerPrefs.Save();
        }

        private LeaderboardSnapshot LoadCachedOrLocalSnapshot()
        {
            string json = PlayerPrefs.GetString(LeaderboardCacheKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    CachedLeaderboard cache = JsonUtility.FromJson<CachedLeaderboard>(json);
                    if (cache != null && cache.entries != null)
                    {
                        LeaderboardEntry[] entries = FromCachedEntries(cache.entries);
                        LeaderboardEntry player = FromCachedEntry(cache.playerEntry);
                        long localStars = SaveSystem.GetTotalStars();
                        if (player != null && localStars > player.TotalStars)
                            player = new LeaderboardEntry(player.Rank, player.PlayerName, localStars, true);
                        for (int i = 0; i < entries.Length; i++)
                        {
                            if (entries[i] != null && entries[i].IsCurrentPlayer &&
                                localStars > entries[i].TotalStars)
                                entries[i] = new LeaderboardEntry(entries[i].Rank, entries[i].PlayerName,
                                    localStars, true);
                        }
                        return LeaderboardSnapshot.Loaded(entries, player, false, true);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[GooglePlayGames] Leaderboard cache could not be read: " + exception.Message);
                }
            }

            var local = new LeaderboardEntry(0,
                string.IsNullOrWhiteSpace(PlayerName) ? "Sen" : PlayerName,
                SaveSystem.GetTotalStars(), true);
            return LeaderboardSnapshot.Loaded(new[] { local }, local, false, true);
        }

        private static CachedEntry[] ToCachedEntries(LeaderboardEntry[] entries)
        {
            if (entries == null) return Array.Empty<CachedEntry>();
            var result = new CachedEntry[entries.Length];
            for (int i = 0; i < entries.Length; i++) result[i] = ToCachedEntry(entries[i]);
            return result;
        }

        private static CachedEntry ToCachedEntry(LeaderboardEntry entry)
        {
            return entry == null ? null : new CachedEntry
            {
                rank = entry.Rank,
                playerName = entry.PlayerName,
                totalStars = entry.TotalStars,
                isCurrentPlayer = entry.IsCurrentPlayer
            };
        }

        private static LeaderboardEntry[] FromCachedEntries(CachedEntry[] entries)
        {
            var result = new LeaderboardEntry[entries.Length];
            for (int i = 0; i < entries.Length; i++) result[i] = FromCachedEntry(entries[i]);
            return result;
        }

        private static LeaderboardEntry FromCachedEntry(CachedEntry entry)
        {
            return entry == null ? null : new LeaderboardEntry(entry.rank, entry.playerName,
                entry.totalStars, entry.isCurrentPlayer);
        }

        private LeaderboardSnapshot CreateEditorPreview()
        {
            long localStars = SaveSystem.GetTotalStars();
            string[] names = { "CandyNova", "BalonUstası", "StarMina", "PopKing", "RenkliBulut",
                "ŞekerKız", "SkyPanda", "MaviRüya", "BubbleHero", "MutluTilki", "Pofuduk", "EDITOR PREVIEW" };
            var entries = new LeaderboardEntry[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                bool current = i == names.Length - 1;
                long stars = current ? localStars : Math.Max(1, 150 - i * 11);
                entries[i] = new LeaderboardEntry(i + 1, names[i], stars, current);
            }
            return LeaderboardSnapshot.Loaded(entries, entries[entries.Length - 1], true);
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
