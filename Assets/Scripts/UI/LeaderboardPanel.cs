using BalloonPop.Core;
using BalloonPop.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalloonPop.UI
{
    public sealed class LeaderboardPanel : MonoBehaviour
    {
        private const int ScoreRowCount = 50;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text rankHeaderText;
        [SerializeField] private TMP_Text playerHeaderText;
        [SerializeField] private TMP_Text scoreHeaderText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text playerSummaryText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TMP_Text refreshLabel;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private LeaderboardRowUI rowTemplate;

        private LeaderboardRowUI[] rows;
        private LeaderboardSnapshot lastSnapshot;
        private int requestVersion;
        private bool isLoading;

        private void Awake()
        {
            EnsureRows();
            if (refreshButton != null) refreshButton.onClick.AddListener(Reload);
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += RefreshLanguage;
            RefreshLanguage();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= RefreshLanguage;
            requestVersion++;
            isLoading = false;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Reload();
        }

        public void Reload()
        {
            if (isLoading) return;
            EnsureRows();

            isLoading = true;
            int version = ++requestVersion;
            SetLoadingState();
            GooglePlayGamesService.Instance.LoadLeaderboard(ScoreRowCount, snapshot =>
            {
                if (this == null || !gameObject.activeInHierarchy || version != requestVersion) return;
                isLoading = false;
                lastSnapshot = snapshot;
                if (refreshButton != null) refreshButton.interactable = true;
                Render(snapshot);
            });
        }

        private void SetLoadingState()
        {
            bool turkish = LocalizationManager.Current == LocalizationManager.Lang.TR;
            if (refreshButton != null) refreshButton.interactable = false;
            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = turkish ? "Yıldız sıralaması yükleniyor..." : "Loading star ranking...";
            }
            if (playerSummaryText != null) playerSummaryText.gameObject.SetActive(false);
            if (rows == null) return;
            for (int i = 0; i < rows.Length; i++) rows[i].Hide();
        }

        private void Render(LeaderboardSnapshot snapshot)
        {
            bool turkish = LocalizationManager.Current == LocalizationManager.Lang.TR;
            EnsureRows();
            for (int i = 0; i < rows.Length; i++) rows[i].Hide();

            if (snapshot == null || !snapshot.Success)
            {
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = turkish
                        ? "Sıralama yüklenemedi. Tekrar dene."
                        : "Could not load the leaderboard. Try again.";
                }
                if (playerSummaryText != null) playerSummaryText.gameObject.SetActive(false);
                return;
            }

            int shown = Mathf.Min(rows.Length, snapshot.Entries.Length);
            for (int i = 0; i < shown; i++) rows[i].Show(snapshot.Entries[i], turkish, i);

            if (statusText != null)
            {
                bool showStatus = snapshot.IsPreview || snapshot.IsCached || shown == 0;
                statusText.gameObject.SetActive(showStatus);
                if (snapshot.IsPreview)
                    statusText.text = turkish ? "Editör önizlemesi" : "Editor preview";
                else if (snapshot.IsCached)
                    statusText.text = turkish
                        ? "Çevrimdışı • son kaydedilen sıralama"
                        : "Offline • last saved ranking";
                else if (shown == 0)
                    statusText.text = turkish ? "Henüz yıldız kaydı yok. İlk sırayı sen al!" : "No stars yet. Take first place!";
            }

            if (playerSummaryText != null)
            {
                LeaderboardEntry player = snapshot.PlayerEntry;
                playerSummaryText.gameObject.SetActive(player != null);
                if (player != null)
                {
                    string rank = player.Rank > 0 ? "#" + player.Rank : "-";
                    playerSummaryText.text = turkish
                        ? "SENİN SIRAN  " + rank + "     YILDIZ  " + player.TotalStars.ToString("N0")
                        : "YOUR RANK  " + rank + "     STARS  " + player.TotalStars.ToString("N0");
                }
            }
        }

        private void RefreshLanguage()
        {
            bool turkish = LocalizationManager.Current == LocalizationManager.Lang.TR;
            if (titleText != null) titleText.text = turkish ? "YILDIZ LİGİ" : "STAR LEAGUE";
            if (subtitleText != null) subtitleText.text = turkish ? "TÜM OYUNCULAR • TOPLAM YILDIZ" : "ALL PLAYERS • TOTAL STARS";
            if (rankHeaderText != null) rankHeaderText.text = turkish ? "SIRA" : "RANK";
            if (playerHeaderText != null) playerHeaderText.text = turkish ? "OYUNCU" : "PLAYER";
            if (scoreHeaderText != null) scoreHeaderText.text = turkish ? "YILDIZ" : "STARS";
            if (refreshLabel != null) refreshLabel.text = turkish ? "YENİLE" : "REFRESH";

            if (isLoading)
                SetLoadingState();
            else if (lastSnapshot != null)
                Render(lastSnapshot);
        }

        private void EnsureRows()
        {
            if (rows != null && rows.Length == ScoreRowCount) return;
            if (rowTemplate == null || rowsRoot == null)
            {
                rows = new LeaderboardRowUI[0];
                return;
            }

            rowTemplate.gameObject.SetActive(false);
            rows = new LeaderboardRowUI[ScoreRowCount];
            for (int i = 0; i < rows.Length; i++)
            {
                LeaderboardRowUI row = Instantiate(rowTemplate, rowsRoot);
                row.name = "StarRow_" + (i + 1);
                RectTransform rt = (RectTransform)row.transform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(0f, 54f);
                row.Hide();
                rows[i] = row;
            }
        }
    }
}
