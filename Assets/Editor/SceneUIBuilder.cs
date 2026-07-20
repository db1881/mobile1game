#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Effects;
using BalloonPop.UI;

namespace BalloonPop.EditorTools
{
    public static class SceneUIBuilder
    {
        private static readonly Color C_Primary    = new Color(1.00f, 0.42f, 0.42f);
        private static readonly Color C_PrimaryDk  = new Color(0.91f, 0.30f, 0.34f);
        private static readonly Color C_Secondary  = new Color(0.31f, 0.80f, 0.77f);
        private static readonly Color C_Accent     = new Color(1.00f, 0.83f, 0.24f);
        private static readonly Color C_PanelBg    = new Color(0.18f, 0.22f, 0.38f, 0.94f);
        private static readonly Color C_CardSubtle = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color C_CardBg     = new Color(1f, 1f, 1f, 0.10f);
        private static readonly Color C_TextLight  = Color.white;
        private static readonly Color C_TextDim    = new Color(1f, 1f, 1f, 0.7f);
        private static readonly Color C_StarOn     = new Color(1.00f, 0.85f, 0.24f);
        private static readonly Color C_StarOff    = new Color(1f, 1f, 1f, 0.15f);
        private static readonly Color CandyTitleColor  = new Color(0.92f, 0.60f, 0.10f, 1f);
        private static readonly Color CandyTextColor   = new Color(0.34f, 0.20f, 0.10f, 1f);
        private static readonly Color CandyDimColor    = new Color(0.48f, 0.31f, 0.19f, 1f);
        private static readonly Color CandyAccentColor = new Color(0.88f, 0.40f, 0.12f, 1f);

        private static Sprite roundedXs;
        private static Sprite roundedSm;
        private static Sprite roundedMd;
        private static Sprite roundedLg;
        private static Sprite pill;
        private static Sprite glow;
        private static Sprite circle;
        private static Sprite shadow;
        private static Sprite star;
        private static Sprite buttonPrimary;
        private static Sprite buttonIconSocket;
        private static Sprite buttonShine;

        private static void LoadSprites()
        {
            roundedXs = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_16.png");
            roundedSm = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_24.png");
            roundedMd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_40.png");
            roundedLg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_56.png");
            pill      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_pill.png");
            glow      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_glow.png");
            circle    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_circle.png");
            shadow    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_shadow.png");
            star      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_star.png");
            buttonPrimary    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_primary.png");
            buttonIconSocket = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_icon_socket.png");
            buttonShine      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_shine.png");
        }

        public static GameHUD BuildGameHUD()
        {
            LoadSprites();
            EnsureEventSystem();
            var canvasGO = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasGO.AddComponent<GameHUD>();

            // Level pill: altın gradient + kalın koyu kenar + 3D glossy
            var levelPillWrap = new GameObject("LevelPillWrap", typeof(RectTransform));
            levelPillWrap.transform.SetParent(canvasGO.transform, false);
            var lpwRT = (RectTransform)levelPillWrap.transform;
            lpwRT.anchorMin = new Vector2(0.20f, 0.928f);
            lpwRT.anchorMax = new Vector2(0.80f, 0.985f);
            lpwRT.offsetMin = lpwRT.offsetMax = Vector2.zero;

            // Drop shadow
            var lpShadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            lpShadow.transform.SetParent(levelPillWrap.transform, false);
            var lpsRT = (RectTransform)lpShadow.transform;
            lpsRT.anchorMin = Vector2.zero; lpsRT.anchorMax = Vector2.one;
            lpsRT.offsetMin = new Vector2(-4, -14);
            lpsRT.offsetMax = new Vector2(4, -6);
            var lpsImg = lpShadow.GetComponent<Image>();
            lpsImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            lpsImg.type = Image.Type.Sliced;
            lpsImg.color = new Color(0, 0, 0, 0.45f);
            lpsImg.raycastTarget = false;

            // Border (koyu kenarlık)
            var lpBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
            lpBorder.transform.SetParent(levelPillWrap.transform, false);
            var lpbRT = (RectTransform)lpBorder.transform;
            lpbRT.anchorMin = Vector2.zero; lpbRT.anchorMax = Vector2.one;
            lpbRT.offsetMin = lpbRT.offsetMax = Vector2.zero;
            var lpbImg = lpBorder.GetComponent<Image>();
            lpbImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            lpbImg.type = Image.Type.Sliced;
            lpbImg.color = new Color(0.45f, 0.18f, 0.04f, 1f);
            lpbImg.raycastTarget = false;

            // Body (altın fill)
            var levelPill = new GameObject("LevelPill", typeof(RectTransform), typeof(Image));
            levelPill.transform.SetParent(levelPillWrap.transform, false);
            var lpRT = (RectTransform)levelPill.transform;
            lpRT.anchorMin = Vector2.zero; lpRT.anchorMax = Vector2.one;
            lpRT.offsetMin = new Vector2(6, 6); lpRT.offsetMax = new Vector2(-6, -6);
            var lpImg = levelPill.GetComponent<Image>();
            lpImg.sprite = roundedSm; lpImg.type = Image.Type.Sliced;
            lpImg.color = new Color(1.00f, 0.78f, 0.20f, 1f);
            lpImg.raycastTarget = false;

            // Üst parıltı
            var lpShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            lpShine.transform.SetParent(levelPill.transform, false);
            var lpShineRT = (RectTransform)lpShine.transform;
            lpShineRT.anchorMin = new Vector2(0, 0.5f);
            lpShineRT.anchorMax = new Vector2(1, 1);
            lpShineRT.offsetMin = new Vector2(8, 0);
            lpShineRT.offsetMax = new Vector2(-8, -6);
            var lpShineImg = lpShine.GetComponent<Image>();
            lpShineImg.sprite = roundedSm;
            lpShineImg.type = Image.Type.Sliced;
            lpShineImg.color = new Color(1f, 1f, 1f, 0.35f);
            lpShineImg.raycastTarget = false;

            // Alt gölge (içeride)
            var lpInner = new GameObject("InnerShade", typeof(RectTransform), typeof(Image));
            lpInner.transform.SetParent(levelPill.transform, false);
            var lpInnerRT = (RectTransform)lpInner.transform;
            lpInnerRT.anchorMin = new Vector2(0, 0);
            lpInnerRT.anchorMax = new Vector2(1, 0.4f);
            lpInnerRT.offsetMin = new Vector2(8, 6);
            lpInnerRT.offsetMax = new Vector2(-8, 0);
            var lpInnerImg = lpInner.GetComponent<Image>();
            lpInnerImg.sprite = roundedSm;
            lpInnerImg.type = Image.Type.Sliced;
            lpInnerImg.color = new Color(0.65f, 0.30f, 0.00f, 0.22f);
            lpInnerImg.raycastTarget = false;

            var levelText = CreateText(levelPill.transform, "LevelText", "SAHİL • 1",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 56, Color.white);
            levelText.fontStyle = FontStyles.Bold;
            levelText.outlineWidth = 0.30f;
            levelText.outlineColor = new Color(0.45f, 0.18f, 0.04f, 1f);
            var lpTxtShadow = levelText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            lpTxtShadow.effectColor = new Color(0.30f, 0.12f, 0.02f, 0.7f);
            lpTxtShadow.effectDistance = new Vector2(3, -4);

            var scoreCard = CreateStatCard(canvasGO.transform, "ScoreCard",
                new Vector2(0.04f, 0.88f), new Vector2(0.30f, 0.94f),
                "SKOR", new Color(0.20f, 0.40f, 0.75f), out var scoreText);

            var movesCard = CreateStatCard(canvasGO.transform, "MovesCard",
                new Vector2(0.70f, 0.88f), new Vector2(0.96f, 0.94f),
                "HAMLE", new Color(0.85f, 0.30f, 0.40f), out var movesText);

            BuildStarProgressBar(canvasGO.transform);

            var goalContainer = new GameObject("GoalContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            goalContainer.transform.SetParent(canvasGO.transform, false);
            var gcRT = goalContainer.GetComponent<RectTransform>();
            // Yukarı çekildi: oyun alanına/balonlara girmesin (eski: 0.74–0.81)
            // SKOR/HAMLE pill'leri 0.88-0.94'te, grid 0.72'de bitiyor → goal'ler aralarına
            gcRT.anchorMin = new Vector2(0.04f, 0.805f);
            gcRT.anchorMax = new Vector2(0.96f, 0.870f);
            gcRT.offsetMin = gcRT.offsetMax = Vector2.zero;
            var hlg = goalContainer.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 18;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            BuildComboTextOverlay(canvasGO.transform);

            BuildBoosterPanel(canvasGO.transform);

            var winPanel = BuildWinPanel(canvasGO.transform);
            var losePanel = BuildLosePanel(canvasGO.transform);
            var pausePanel = BuildPausePanel(canvasGO.transform);
            var mysteryPanel = BuildMysteryBoxPanel(canvasGO.transform);

            BuildPauseButton(canvasGO.transform, pausePanel);

            var goalItemPrefab = CreateGoalItemPrefab();

            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("scoreText").objectReferenceValue = scoreText;
            hudSO.FindProperty("movesText").objectReferenceValue = movesText;
            hudSO.FindProperty("levelText").objectReferenceValue = levelText;
            hudSO.FindProperty("goalContainer").objectReferenceValue = goalContainer.transform;
            hudSO.FindProperty("goalItemPrefab").objectReferenceValue = goalItemPrefab;
            hudSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            hudSO.FindProperty("winPanel").objectReferenceValue = winPanel.GetComponent<WinPanel>();
            hudSO.FindProperty("losePanel").objectReferenceValue = losePanel.GetComponent<LosePanel>();
            hudSO.FindProperty("mysteryBoxPanel").objectReferenceValue = mysteryPanel;
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            return hud;
        }

        public static void BuildMainMenu(LevelDatabase db)
        {
            LoadSprites();
            EnsureEventSystem();
            var canvasGO = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var menu = canvasGO.AddComponent<MainMenuUI>();

            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_menu.png");
            if (bgSprite != null)
            {
                var bgGO = new GameObject("MenuBg", typeof(RectTransform), typeof(Image));
                bgGO.transform.SetParent(canvasGO.transform, false);
                bgGO.transform.SetAsFirstSibling();
                var bgRT = (RectTransform)bgGO.transform;
                bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
                bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
                var bgImg = bgGO.GetComponent<Image>();
                bgImg.sprite = bgSprite;
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;
                bgImg.raycastTarget = false;
            }

            var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/logo_balloonpop.png");
            if (logoSprite != null)
            {
                var logoGO = new GameObject("Logo", typeof(RectTransform), typeof(Image));
                logoGO.transform.SetParent(canvasGO.transform, false);
                var lrt = (RectTransform)logoGO.transform;
                SetPixelRect(lrt, new Vector2(-140, 660), new Vector2(740, 400));
                var limg = logoGO.GetComponent<Image>();
                limg.sprite = logoSprite;
                limg.preserveAspect = true;
                limg.raycastTarget = false;
                logoGO.AddComponent<BalloonPop.Effects.LogoBob>();
            }
            else
            {
                var title = CreateText(canvasGO.transform, "Title", "BALLOON\nPOP",
                    new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.92f),
                    Vector2.zero, Vector2.zero,
                    TextAlignmentOptions.Center, 180, C_TextLight);
                title.fontStyle = FontStyles.Bold;
                title.gameObject.AddComponent<BalloonPop.Effects.LogoBob>();
            }

            var mascotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/moscot.png");
            if (mascotSprite == null) mascotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mascot.png");
            if (mascotSprite != null)
            {
                var mascotGO = new GameObject("Mascot", typeof(RectTransform), typeof(Image));
                mascotGO.transform.SetParent(canvasGO.transform, false);
                var mrt = (RectTransform)mascotGO.transform;
                SetPixelRect(mrt, new Vector2(360, 560), new Vector2(280, 340));
                var mimg = mascotGO.GetComponent<Image>();
                mimg.sprite = mascotSprite;
                mimg.preserveAspect = true;
                mimg.raycastTarget = false;
                mascotGO.AddComponent<BalloonPop.Effects.LogoBob>();
            }

            // Ana menü şerit butonları — procedural glossy "şeker" haplar.
            // Her aksiyon kendi rengi + ikonu ile akılda kalıcı: OYNA=yeşil/play,
            // LEVEL SEÇ=mavi/ızgara, AYARLAR=mor/çark, ÇIKIŞ=mercan/güç.
            // Sıkı dikey ritim (çok aralık yok) ile birbirine yakın dursunlar.
            // Tüm butonlar AYNI boyda — kullanıcının yüklediği plate PNG'leri olduğu gibi.
            // Boyutu biraz küçülttük (kullanıcı isteği), aralıklar da sıkılaştı.
            Vector2 stripSize = new Vector2(860, 230);
            int rowGap = 28;
            int row0 = 200;
            var playBtn = CreateStripButton(canvasGO.transform, "PlayButton",
                "OYNA", "menu.play", "icon_play", "btn_plate_green",
                new Color(0.30f, 0.78f, 0.34f), new Vector2(0, row0), stripSize, 72);
            var levelBtn = CreateStripButton(canvasGO.transform, "LevelSelectButton",
                "LEVEL SEÇ", "menu.level_select", "icon_grid", "btn_plate_blue",
                new Color(0.20f, 0.60f, 0.95f), new Vector2(0, row0 - (int)stripSize.y - rowGap), stripSize, 60);
            var settingsBtn = CreateStripButton(canvasGO.transform, "SettingsButton",
                "AYARLAR", "menu.settings", "icon_gear", "btn_plate_purple",
                new Color(0.64f, 0.41f, 0.95f), new Vector2(0, row0 - 2 * ((int)stripSize.y + rowGap)), stripSize, 66);
            var quitBtn = CreateStripButton(canvasGO.transform, "QuitButton",
                "ÇIKIŞ", "menu.quit", "icon_power", "btn_plate_red",
                new Color(0.97f, 0.40f, 0.42f), new Vector2(0, row0 - 3 * ((int)stripSize.y + rowGap)), stripSize, 72);

            var levelSelectPanel = BuildLevelSelectPanel(canvasGO.transform, db);
            var settingsPanel = BuildSettingsPanel(canvasGO.transform);
            var shopPanel = BuildCoinShopPanel(canvasGO.transform);
            var dailyPanel = BuildDailyRewardPanel(canvasGO.transform);
            var statsPanel = BuildStatsPanel(canvasGO.transform);
            var achievementsPanel = BuildAchievementListPanel(canvasGO.transform);

            // Shop panel is available now, so the coin capsule's embedded + hit area can target it.
            BuildHeaderHUD(canvasGO.transform, shopPanel);

            var shopIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/menu_shop.png");
            var dailyIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/menu_daily.png");
            var statsIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/menu_stats.png");
            var trophyIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/menu_trophy.png");

            // Alt bar 4 buton: tek sprite'lı (yazı + ikon sprite'ın içinde)
            int smallW = 260, smallH = 260, gap = 8, bottomY = -830;
            int totalW = 4 * smallW + 3 * gap;
            int startX = -totalW / 2 + smallW / 2;
            // Per-button rect uzatma değerleri: kullanıcının manuel olarak ayarlayıp
            // editor'da Ctrl+S ile kaydettiği değerler (rect tepesini bu kadar px yukarı uzat).
            // Her asset'in iç banner pill'i farklı Y pozisyonunda → her butona özel.
            var shopMini = CreateLocalizedSpriteButtonOpenTarget(canvasGO.transform, "ShopButton",
                "btn_magaza", "btn_shop", new Vector2(startX + 0 * (smallW + gap), bottomY),
                new Vector2(smallW, smallH), shopPanel);
            AddMiniButtonLabel(shopMini, "menu.shop", "MAĞAZA", 27.95f);

            var dailyMini = CreateLocalizedSpriteButtonOpenTarget(canvasGO.transform, "DailyButton",
                "btn_gunluk", "btn_daily", new Vector2(startX + 1 * (smallW + gap), bottomY),
                new Vector2(smallW, smallH), dailyPanel);
            AddMiniButtonLabel(dailyMini, "menu.daily", "GÜNLÜK", 11.18f);

            var statsMini = CreateLocalizedSpriteButtonOpenTarget(canvasGO.transform, "StatsButton",
                "btn_istatistik", "btn_stats", new Vector2(startX + 2 * (smallW + gap), bottomY),
                new Vector2(smallW, smallH), statsPanel);
            AddMiniButtonLabel(statsMini, "menu.stats", "İSTATİSTİK", 0f);

            var achMini = CreateLocalizedSpriteButtonOpenTarget(canvasGO.transform, "AchButton",
                "btn_basarim", "btn_awards", new Vector2(startX + 3 * (smallW + gap), bottomY),
                new Vector2(smallW, smallH), achievementsPanel);
            AddMiniButtonLabel(achMini, "menu.achievements", "BAŞARIM", 9.32f);

            var noHeartsPanel = BuildNoHeartsPanel(canvasGO.transform);

            // Tüm pop-up panel'leri canvas'ın en sonuna taşı —
            // açıldıklarında HUD, alt butonlar dahil her şeyin üstünde olsunlar.
            // Aksi halde alt mini butonlar veya başka UI elemanları paneldeki X/buton clicklerini bloklar.
            levelSelectPanel.transform.SetAsLastSibling();
            settingsPanel.transform.SetAsLastSibling();
            shopPanel.transform.SetAsLastSibling();
            dailyPanel.transform.SetAsLastSibling();
            statsPanel.transform.SetAsLastSibling();
            achievementsPanel.transform.SetAsLastSibling();
            noHeartsPanel.transform.SetAsLastSibling();

            var so = new SerializedObject(menu);
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("levelSelectButton").objectReferenceValue = levelBtn;
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("levelSelectPanel").objectReferenceValue = levelSelectPanel;
            so.FindProperty("noHeartsPanel").objectReferenceValue = noHeartsPanel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Rebuilds only the four secondary Main Menu popups without touching the rest of the scene.
        /// Existing navigation buttons are redirected to the newly created panel instances.
        /// </summary>
        public static void RebuildSecondaryMenuPanels()
        {
            LoadSprites();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            MainMenuUI menu = null;
            foreach (var candidate in Resources.FindObjectsOfTypeAll<MainMenuUI>())
            {
                if (candidate.gameObject.scene == scene)
                {
                    menu = candidate;
                    break;
                }
            }

            if (menu == null)
            {
                Debug.LogError("MainMenuUI was not found in the active scene.");
                return;
            }

            var canvas = menu.GetComponentInParent<Canvas>();
            var parent = canvas.transform;
            var panelNames = new System.Collections.Generic.HashSet<string>
            {
                "CoinShopPanel", "DailyRewardPanel", "StatsPanel_Menu", "AchievementListPanel"
            };
            var redirects = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<ButtonOpenTarget, string>>();
            foreach (var opener in canvas.GetComponentsInChildren<ButtonOpenTarget>(true))
            {
                if (opener.Target != null && panelNames.Contains(opener.Target.name))
                    redirects.Add(new System.Collections.Generic.KeyValuePair<ButtonOpenTarget, string>(opener, opener.Target.name));
            }

            foreach (var panelName in panelNames)
            {
                var oldPanel = parent.Find(panelName);
                if (oldPanel != null)
                    Object.DestroyImmediate(oldPanel.gameObject);
            }

            var shop = BuildCoinShopPanel(parent);
            var daily = BuildDailyRewardPanel(parent);
            var stats = BuildStatsPanel(parent);
            var achievements = BuildAchievementListPanel(parent);
            var replacements = new System.Collections.Generic.Dictionary<string, GameObject>
            {
                { "CoinShopPanel", shop },
                { "DailyRewardPanel", daily },
                { "StatsPanel_Menu", stats },
                { "AchievementListPanel", achievements }
            };

            foreach (var redirect in redirects)
            {
                if (redirect.Key != null && replacements.TryGetValue(redirect.Value, out var replacement))
                {
                    redirect.Key.Target = replacement;
                    EditorUtility.SetDirty(redirect.Key);
                }
            }

            shop.transform.SetAsLastSibling();
            daily.transform.SetAsLastSibling();
            stats.transform.SetAsLastSibling();
            achievements.transform.SetAsLastSibling();
            var noHearts = parent.Find("NoHeartsPanel");
            if (noHearts != null) noHearts.SetAsLastSibling();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log("Secondary Main Menu panels rebuilt with Candy UI v2.");
        }

        private static void BuildStarProgressBar(Transform parent)
        {
            var root = new GameObject("StarProgressBar", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            // Goal pill'ler 0.805-0.870 → progress bar onların hemen altına
            rt.anchorMin = new Vector2(0.06f, 0.755f);
            rt.anchorMax = new Vector2(0.94f, 0.795f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Track (dimmed bg)
            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(root.transform, false);
            var tRT = (RectTransform)track.transform;
            tRT.anchorMin = new Vector2(0, 0.30f); tRT.anchorMax = new Vector2(1, 0.70f);
            tRT.offsetMin = new Vector2(20, 0); tRT.offsetMax = new Vector2(-20, 0);
            var tImg = track.GetComponent<Image>();
            tImg.sprite = roundedXs; tImg.type = Image.Type.Sliced;
            tImg.color = new Color(0, 0, 0, 0.5f);

            // Fill (yellow gradient)
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            var fRT = (RectTransform)fill.transform;
            fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
            fRT.offsetMin = new Vector2(2, 2); fRT.offsetMax = new Vector2(-2, -2);
            var fImg = fill.GetComponent<Image>();
            fImg.sprite = roundedXs; fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fImg.fillAmount = 0f;
            fImg.color = new Color(1f, 0.84f, 0.20f);

            // 3 yıldız: x%33, x%66, x%99 konumlarda
            float[] positions = { 0.33f, 0.66f, 0.99f };
            var stars = new RectTransform[3];
            var starImgs = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var sGO = new GameObject($"Star{i+1}", typeof(RectTransform), typeof(Image));
                sGO.transform.SetParent(root.transform, false);
                var sRT = (RectTransform)sGO.transform;
                sRT.anchorMin = new Vector2(positions[i], 0.5f);
                sRT.anchorMax = new Vector2(positions[i], 0.5f);
                sRT.pivot = new Vector2(0.5f, 0.5f);
                sRT.sizeDelta = new Vector2(64, 64);
                sRT.anchoredPosition = Vector2.zero;
                var sImg = sGO.GetComponent<Image>();
                sImg.sprite = star;
                sImg.color = new Color(1f, 1f, 1f, 0.25f);
                stars[i] = sRT;
                starImgs[i] = sImg;
            }

            var spb = root.AddComponent<StarProgressBar>();
            var so = new SerializedObject(spb);
            so.FindProperty("fillImage").objectReferenceValue = fImg;
            so.FindProperty("star1").objectReferenceValue = stars[0];
            so.FindProperty("star2").objectReferenceValue = stars[1];
            so.FindProperty("star3").objectReferenceValue = stars[2];
            so.FindProperty("star1Image").objectReferenceValue = starImgs[0];
            so.FindProperty("star2Image").objectReferenceValue = starImgs[1];
            so.FindProperty("star3Image").objectReferenceValue = starImgs[2];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildNoHeartsPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "NoHeartsPanel", tapOutsideToClose: true);
            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.70f), C_PanelBg);

            var heartIcon = new GameObject("HeartIcon", typeof(RectTransform), typeof(Image));
            heartIcon.transform.SetParent(card.transform, false);
            var hrt = (RectTransform)heartIcon.transform;
            hrt.anchorMin = new Vector2(0.35f, 0.65f); hrt.anchorMax = new Vector2(0.65f, 0.95f);
            hrt.offsetMin = hrt.offsetMax = Vector2.zero;
            heartIcon.GetComponent<Image>().sprite = circle;
            heartIcon.GetComponent<Image>().color = new Color(1f, 0.30f, 0.40f);

            var title = CreateText(card.transform, "Title", "HAYATIN BİTTİ!",
                new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.66f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 56, C_TextLight);
            title.fontStyle = FontStyles.Bold;

            var timer = CreateText(card.transform, "Timer", "Sonraki hayat: 30:00",
                new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.52f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 36, new Color(1f, 0.95f, 0.85f));

            var coinInfo = CreateText(card.transform, "CoinInfo", "50 🪙 ile 1 hayat al",
                new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.38f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 30, new Color(1f, 0.85f, 0.30f));

            var buyBtn = CreatePrimaryButton(card.transform, "BuyButton", "SATIN AL",
                new Vector2(0.12f, 0.10f), new Vector2(0.50f, 0.24f));
            var closeBtn = CreateSecondaryButton(card.transform, "CloseButton", "KAPAT",
                new Vector2(0.52f, 0.10f), new Vector2(0.88f, 0.24f));

            var nh = overlay.AddComponent<NoHeartsPanel>();
            var so = new SerializedObject(nh);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("timerText").objectReferenceValue = timer;
            so.FindProperty("coinText").objectReferenceValue = coinInfo;
            so.FindProperty("buyButton").objectReferenceValue = buyBtn;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        /// <summary>
        /// Dile göre TR veya EN sprite ile buton oluşturur + LocalizedSpriteSwap component
        /// ile runtime'da dil değişince sprite swap eder.
        /// </summary>
        private static Button CreateLocalizedSpriteButton(Transform parent, string name,
            string trBaseName, string enBaseName, Vector2 anchoredPos, Vector2 size)
        {
            var trSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{trBaseName}.png");
            var enSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{enBaseName}.png");
            // Fallback: en yoksa TR, TR yoksa EN
            string activePath = trSprite != null
                ? $"Assets/Sprites/{trBaseName}.png"
                : $"Assets/Sprites/{enBaseName}.png";

            var btn = CreateSpriteButton(parent, name, activePath, anchoredPos, size);

            // Runtime'da dil değişimi için
            var swap = btn.gameObject.AddComponent<LocalizedSpriteSwap>();
            swap.TrSprite = trSprite;
            swap.EnSprite = enSprite;
            return btn;
        }

        private static Button CreateLocalizedSpriteButtonOpenTarget(Transform parent, string name,
            string trBaseName, string enBaseName, Vector2 anchoredPos, Vector2 size, GameObject openTarget)
        {
            var btn = CreateLocalizedSpriteButton(parent, name, trBaseName, enBaseName, anchoredPos, size);
            var hook = btn.gameObject.AddComponent<ButtonOpenTarget>();
            hook.Target = openTarget;
            return btn;
        }

        /// <summary>
        /// Mini butonun (260x260) alt banner alanına lokalize yazı bindirir.
        /// extendUpPx: rect'in tepesini bu kadar px yukarı uzat (banner'a göre kullanıcı
        /// tarafından ölçülmüş per-asset offset). Bottom sabit, top yukarı taşır.
        /// </summary>
        private static void AddMiniButtonLabel(Button btn, string locKey, string trText, float extendUpPx = 0f)
        {
            var labelGO = new GameObject("BannerLabel",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            labelGO.transform.SetParent(btn.transform, false);
            var rt = (RectTransform)labelGO.transform;
            // Banner pill'in iç dolu alanı dar (≈%64 genişlik).
            rt.anchorMin = new Vector2(0.18f, 0.10f);
            rt.anchorMax = new Vector2(0.82f, 0.24f);
            // Bottom sabit kalsın, top'u extendUpPx kadar yukarı uzat (banner pill'in
            // gerçek pozisyonuna göre per-button kalibrasyon, manuel ölçüm).
            rt.sizeDelta = new Vector2(0f, extendUpPx);
            rt.anchoredPosition = new Vector2(0f, extendUpPx * 0.5f);

            var tmp = labelGO.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = trText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            // Autosize: kısa kelimeler (MAĞAZA) büyür, uzun kelimeler (İSTATİSTİK)
            // küçülür → 4'ü de banner'ı aynı oranda doldurur.
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = 30f;
            tmp.characterSpacing = 0f;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TMPro.TextOverflowModes.Truncate;
            tmp.outlineWidth = 0.30f;
            tmp.outlineColor = new Color(0.05f, 0.20f, 0.10f, 1f);
            tmp.raycastTarget = false;

            var shadow = labelGO.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.55f);
            shadow.effectDistance = new Vector2(1.5f, -2f);

            var loc = labelGO.AddComponent<LocalizedText>();
            var lso = new SerializedObject(loc);
            lso.FindProperty("key").stringValue = locKey;
            lso.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Ana menü şerit butonu — procedural glossy "şeker" hap: gölge + koyu kenar +
        /// renkli gövde + üst parıltı + sol beyaz ikon rozeti + ortalanmış lokalize etiket.
        /// Renk + ikon kombinasyonu butonu akılda kalıcı yapar (Toon Blast / Royal Match tarzı).
        /// </summary>
        private static Button CreateStripButton(Transform parent, string name, string trLabel,
            string locKey, string iconName, string btnSpriteName, Color color, Vector2 anchoredPos, Vector2 size, int fontSize)
        {
            var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{iconName}.png");
            var btnSprite  = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{btnSpriteName}.png");
            var shape = pill != null ? pill : roundedLg;   // gölge için
            Color deep  = Color.Lerp(color, Color.black, 0.50f);  // etiket dış çizgisi

            var wrap = new GameObject(name + "Wrap", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrap.transform;
            SetPixelRect(wrt, anchoredPos, size);

            // Gövde = kullanıcının yüklediği parlak plate PNG'si OLDUĞU GİBİ.
            // preserveAspect'ı koru → sprite oranı bozulmaz, plate'in kendisinde
            // gölge zaten pişmiş, ekstra shadow eklemiyoruz.
            var face = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            face.transform.SetParent(wrap.transform, false);
            var faceRT = (RectTransform)face.transform;
            faceRT.anchorMin = Vector2.zero; faceRT.anchorMax = Vector2.one;
            faceRT.offsetMin = faceRT.offsetMax = Vector2.zero;
            var faceImg = face.GetComponent<Image>();
            faceImg.sprite = btnSprite != null ? btnSprite : shape;
            faceImg.type = Image.Type.Simple;
            // Plate kanvası 2:3 (içinde geniş pill var). preserveAspect=true ile
            // rect içine sığar; rect'in oranı pill'e yakın olursa pill rect'i doldurur.
            faceImg.preserveAspect = true;
            faceImg.color = Color.white;

            // Plate'in iç yatay bandı (yazı bu bandın ortasına oturur, 3D dudağa girmez)
            const float pillTop    = 0.78f;
            const float pillBottom = 0.22f;

            // İkon yok — geniş premium plate'te yazı tek başına temiz duruyor.
            // (iconName parametresi alındı ama sahneye yerleştirmiyoruz; ileride
            // istenirse plate içine entegre eklenebilir.)
            var _unused = iconSprite;

            // Etiket — pill'in iç bandında, TAM ORTALI
            var labelTxt = CreateText(face.transform, "Label", trLabel,
                new Vector2(0.05f, pillBottom), new Vector2(0.95f, pillTop),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, fontSize, Color.white);
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.characterSpacing = 3f;
            labelTxt.outlineWidth = 0.28f;
            labelTxt.outlineColor = deep;
            labelTxt.raycastTarget = false;
            var labelShadow = labelTxt.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            labelShadow.effectColor = new Color(0, 0, 0, 0.55f);
            labelShadow.effectDistance = new Vector2(2, -3);

            var loc = labelTxt.gameObject.AddComponent<LocalizedText>();
            var lso = new SerializedObject(loc);
            lso.FindProperty("key").stringValue = locKey;
            lso.ApplyModifiedPropertiesWithoutUndo();

            // Buton press feedback (yüzeye uygulanır)
            var btn = face.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            return btn;
        }

        /// <summary>
        /// Tek sprite'lı buton: yazı ve icon sprite'ın içinde, sadece görsel + Button component.
        /// </summary>
        private static Button CreateSpriteButton(Transform parent, string name, string spritePath,
            Vector2 anchoredPos, Vector2 size)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            SetPixelRect(rt, anchoredPos, size);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            // preserveAspect false → RectTransform boyutunu tam doldursun (ekran genişliğine otursun)
            img.preserveAspect = false;
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            // Tıklamada hafif scale animasyonu
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.05f, 1.05f, 1.05f, 1f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
            btn.colors = colors;

            return btn;
        }

        /// <summary> Sprite-only buton + ButtonOpenTarget hook. </summary>
        private static Button CreateSpriteButtonOpenTarget(Transform parent, string name,
            string spritePath, Vector2 anchoredPos, Vector2 size, GameObject openTarget)
        {
            var btn = CreateSpriteButton(parent, name, spritePath, anchoredPos, size);
            var hook = btn.gameObject.AddComponent<ButtonOpenTarget>();
            hook.Target = openTarget;
            return btn;
        }

        /// <summary> Sağ-üst köşede modern X kapatma butonu — kırmızı glossy daire. </summary>
        private static void BuildCloseXButton(Transform parent, Vector2 anchorMin, Vector2 anchorMax, GameObject closeTarget)
        {
            var wrap = new GameObject("CloseX", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrap.transform;
            wrt.anchorMin = anchorMin; wrt.anchorMax = anchorMax;
            wrt.offsetMin = wrt.offsetMax = Vector2.zero;

            // Shadow
            var sh = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            sh.transform.SetParent(wrap.transform, false);
            var shRT = (RectTransform)sh.transform;
            shRT.anchorMin = Vector2.zero; shRT.anchorMax = Vector2.one;
            shRT.offsetMin = new Vector2(-2, -10); shRT.offsetMax = new Vector2(2, -4);
            var shImg = sh.GetComponent<Image>();
            shImg.sprite = circle;
            shImg.color = new Color(0, 0, 0, 0.45f);
            shImg.raycastTarget = false;

            // Dark border
            var bd = new GameObject("Border", typeof(RectTransform), typeof(Image));
            bd.transform.SetParent(wrap.transform, false);
            var bdRT = (RectTransform)bd.transform;
            bdRT.anchorMin = Vector2.zero; bdRT.anchorMax = Vector2.one;
            bdRT.offsetMin = bdRT.offsetMax = Vector2.zero;
            var bdImg = bd.GetComponent<Image>();
            bdImg.sprite = circle;
            bdImg.color = new Color(0.45f, 0.05f, 0.10f, 1f);
            bdImg.raycastTarget = false;

            // Body (red)
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(Button));
            body.transform.SetParent(wrap.transform, false);
            var bRT = (RectTransform)body.transform;
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = new Vector2(5, 5); bRT.offsetMax = new Vector2(-5, -5);
            var bImg = body.GetComponent<Image>();
            bImg.sprite = circle;
            bImg.color = new Color(0.92f, 0.30f, 0.32f, 1f);

            // Top shine
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(body.transform, false);
            var shineRT = (RectTransform)shine.transform;
            shineRT.anchorMin = new Vector2(0.15f, 0.50f);
            shineRT.anchorMax = new Vector2(0.85f, 0.90f);
            shineRT.offsetMin = shineRT.offsetMax = Vector2.zero;
            var shineImg = shine.GetComponent<Image>();
            shineImg.sprite = circle;
            shineImg.color = new Color(1f, 1f, 1f, 0.40f);
            shineImg.raycastTarget = false;

            // X label
            var xText = CreateText(body.transform, "X", "X",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TMPro.TextAlignmentOptions.Center, 58, Color.white);
            xText.fontStyle = TMPro.FontStyles.Bold;
            xText.enableAutoSizing = true;
            xText.fontSizeMin = 34f;
            xText.fontSizeMax = 62f;
            xText.outlineWidth = 0.30f;
            xText.outlineColor = new Color(0.40f, 0.05f, 0.08f, 1f);
            xText.raycastTarget = false;

            var btn = body.GetComponent<Button>();
            var closer = body.gameObject.AddComponent<ButtonCloseTarget>();
            closer.Target = closeTarget;
        }

        private static void SetPixelRect(RectTransform rt, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        private static void BuildSmallButton(Transform parent, string name, string label, Sprite iconSprite, Color color,
            Vector2 anchoredPos, Vector2 size, GameObject openTarget)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            SetPixelRect(rt, anchoredPos, size);
            var img = go.GetComponent<Image>();
            img.sprite = roundedSm; img.type = Image.Type.Sliced;
            img.color = color;

            AddGradientHighlight(go.transform);
            AddInnerShade(go.transform);

            if (iconSprite != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(go.transform, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.5f, 1f);
                iconRT.anchorMax = new Vector2(0.5f, 1f);
                iconRT.pivot = new Vector2(0.5f, 1f);
                iconRT.sizeDelta = new Vector2(78f, 78f);
                iconRT.anchoredPosition = new Vector2(0f, -14f);
                var iconImg = iconGO.GetComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            var labelTxt = CreateText(go.transform, "Label", label,
                new Vector2(0.04f, 0f), new Vector2(0.96f, 0.32f), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 28, Color.white);
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.outlineWidth = 0.2f;
            labelTxt.outlineColor = new Color(0, 0, 0, 0.55f);

            var opener = go.AddComponent<ButtonOpenTarget>();
            opener.Target = openTarget;
        }

        private static void AttachLeftIcon(Button btn, string iconPath)
        {
            if (btn == null) return;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite == null) return;

            // (Icon socket beyaz arka plan çıkardığı için kapatıldı — sade icon yeterli)
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(btn.transform, false);
            var iconRT = (RectTransform)iconGO.transform;
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.sizeDelta = new Vector2(140f, 140f);
            iconRT.anchoredPosition = new Vector2(36f, 0f);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var label = btn.transform.Find("Label");
            if (label != null)
            {
                var lrt = (RectTransform)label;
                lrt.anchorMin = new Vector2(0.22f, 0f);
                lrt.anchorMax = new Vector2(1f, 1f);
                lrt.offsetMin = new Vector2(0f, 0f);
                lrt.offsetMax = new Vector2(-30f, 0f);
                var lt = label.GetComponent<TMPro.TMP_Text>();
                if (lt != null) lt.alignment = TMPro.TextAlignmentOptions.Center;
            }
        }

        private static void AddGradientHighlight(Transform parent)
        {
            var go = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(6, 0); rt.offsetMax = new Vector2(-6, -6);
            var img = go.GetComponent<Image>();
            img.sprite = roundedXs;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.22f);
            img.raycastTarget = false;
        }

        private static void AddInnerShade(Transform parent)
        {
            var go = new GameObject("InnerShade", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0.45f);
            rt.offsetMin = new Vector2(6, 6); rt.offsetMax = new Vector2(-6, 0);
            var img = go.GetComponent<Image>();
            img.sprite = roundedXs;
            img.type = Image.Type.Sliced;
            img.color = new Color(0f, 0f, 0f, 0.18f);
            img.raycastTarget = false;
        }

        private static GameObject CreateCandyPanelCard(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, bool preserveAspect = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/panel_pause_v2.png");
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;
            image.raycastTarget = false;
            return go;
        }

        private static void StyleCandyTitle(TMP_Text title)
        {
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 30f;
            title.fontSizeMax = 56f;
            title.outlineWidth = 0f;
            title.raycastTarget = false;
        }

        private static GameObject BuildCoinShopPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "CoinShopPanel", tapOutsideToClose: true);
            var card = CreateCandyPanelCard(overlay.transform, "Card",
                new Vector2(0.07f, 0.19f), new Vector2(0.93f, 0.81f));

            var title = CreateText(card.transform, "Title", "MAĞAZA",
                new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.97f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 52, CandyTitleColor);
            StyleCandyTitle(title);

            // Coin balance pill — glossy gradient
            var coinCard = new GameObject("CoinBalance", typeof(RectTransform), typeof(Image));
            coinCard.transform.SetParent(card.transform, false);
            var ccrt = (RectTransform)coinCard.transform;
            ccrt.anchorMin = new Vector2(0.24f, 0.70f); ccrt.anchorMax = new Vector2(0.76f, 0.79f);
            ccrt.offsetMin = ccrt.offsetMax = Vector2.zero;
            var coinBgImg = coinCard.GetComponent<Image>();
            coinBgImg.sprite = roundedMd; coinBgImg.type = Image.Type.Sliced;
            coinBgImg.color = new Color(1f, 0.88f, 0.50f, 0.92f);

            // Coin pill shine
            var coinShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            coinShine.transform.SetParent(coinCard.transform, false);
            var csRT = (RectTransform)coinShine.transform;
            csRT.anchorMin = new Vector2(0.05f, 0.55f); csRT.anchorMax = new Vector2(0.95f, 0.90f);
            csRT.offsetMin = csRT.offsetMax = Vector2.zero;
            var csImg = coinShine.GetComponent<Image>();
            csImg.sprite = roundedMd; csImg.type = Image.Type.Sliced;
            csImg.color = new Color(1f, 1f, 1f, 0.30f);
            csImg.raycastTarget = false;

            // Coin icon (sarı yuvarlak)
            var coinIcon = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
            coinIcon.transform.SetParent(coinCard.transform, false);
            var ciRT = (RectTransform)coinIcon.transform;
            ciRT.anchorMin = new Vector2(0, 0.5f); ciRT.anchorMax = new Vector2(0, 0.5f);
            ciRT.sizeDelta = new Vector2(60, 60);
            ciRT.anchoredPosition = new Vector2(40, 0);
            var ciImg = coinIcon.GetComponent<Image>();
            ciImg.sprite = circle;
            ciImg.color = new Color(1f, 0.85f, 0.20f, 1f);
            ciImg.raycastTarget = false;
            var coinIconText = CreateText(coinIcon.transform, "C", "C",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 36, new Color(0.50f, 0.30f, 0f));
            coinIconText.fontStyle = FontStyles.Bold;
            coinIconText.raycastTarget = false;

            var coinText = CreateText(coinCard.transform, "Coins", "0",
                new Vector2(0.30f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-20, 0),
                TextAlignmentOptions.Center, 42, CandyTextColor);
            coinText.fontStyle = FontStyles.Bold;
            coinText.outlineWidth = 0f;
            coinText.raycastTarget = false;
            var ctShadow = coinText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            ctShadow.effectColor = new Color(1f, 1f, 1f, 0.35f);
            ctShadow.effectDistance = new Vector2(1, 1);

            // İyileştirilmiş fiyatlar — booster'lar daha erişilebilir (%40 ucuz)
            var hammer = BuildShopRow(card.transform, "Hammer",  "Çekiç",     "booster_hammer",  150, 0.58f, new Color(0.95f, 0.55f, 0.25f));
            var shuffle = BuildShopRow(card.transform, "Shuffle", "Karıştır",  "booster_shuffle", 250, 0.40f, new Color(0.30f, 0.78f, 0.86f));
            var move = BuildShopRow(card.transform, "Move",       "+5 Hamle",  "booster_plus",    300, 0.22f, new Color(0.62f, 0.42f, 0.95f));

            // Modern glossy X kapatma butonu
            BuildCloseXButton(card.transform, new Vector2(0.84f, 0.86f), new Vector2(0.98f, 0.98f), overlay);

            var shop = overlay.AddComponent<CoinShopPanel>();
            var so = new SerializedObject(shop);
            so.FindProperty("coinText").objectReferenceValue = coinText;

            var hammerProp = so.FindProperty("hammerItem");
            hammerProp.FindPropertyRelative("Key").stringValue = "hammer";
            hammerProp.FindPropertyRelative("Title").stringValue = "Çekiç";
            hammerProp.FindPropertyRelative("Price").intValue = 150;
            hammerProp.FindPropertyRelative("BuyButton").objectReferenceValue = hammer.btn;
            hammerProp.FindPropertyRelative("PriceText").objectReferenceValue = hammer.priceText;

            var shuffleProp = so.FindProperty("shuffleItem");
            shuffleProp.FindPropertyRelative("Key").stringValue = "shuffle";
            shuffleProp.FindPropertyRelative("Title").stringValue = "Karıştır";
            shuffleProp.FindPropertyRelative("Price").intValue = 250;
            shuffleProp.FindPropertyRelative("BuyButton").objectReferenceValue = shuffle.btn;
            shuffleProp.FindPropertyRelative("PriceText").objectReferenceValue = shuffle.priceText;

            var moveProp = so.FindProperty("moveItem");
            moveProp.FindPropertyRelative("Key").stringValue = "move";
            moveProp.FindPropertyRelative("Title").stringValue = "+5 Hamle";
            moveProp.FindPropertyRelative("Price").intValue = 300;
            moveProp.FindPropertyRelative("BuyButton").objectReferenceValue = move.btn;
            moveProp.FindPropertyRelative("PriceText").objectReferenceValue = move.priceText;

            so.ApplyModifiedPropertiesWithoutUndo();
            overlay.SetActive(false);
            return overlay;
        }

        private struct ShopRow { public Button btn; public TMP_Text priceText; }

        private static ShopRow BuildShopRow(Transform parent, string name, string label, string iconName, int price, float yCenter, Color accentColor)
        {
            // Warm candy card that sits naturally on the cream panel.
            var row = new GameObject($"Row_{name}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            var rrt = (RectTransform)row.transform;
            rrt.anchorMin = new Vector2(0.05f, yCenter - 0.075f);
            rrt.anchorMax = new Vector2(0.95f, yCenter + 0.075f);
            rrt.offsetMin = rrt.offsetMax = Vector2.zero;
            var rowBg = row.GetComponent<Image>();
            rowBg.sprite = roundedMd; rowBg.type = Image.Type.Sliced;
            rowBg.color = new Color(1f, 0.93f, 0.74f, 0.78f);

            // Row shine üstte
            var rowShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            rowShine.transform.SetParent(row.transform, false);
            var rsRT = (RectTransform)rowShine.transform;
            rsRT.anchorMin = new Vector2(0.02f, 0.55f); rsRT.anchorMax = new Vector2(0.98f, 0.92f);
            rsRT.offsetMin = rsRT.offsetMax = Vector2.zero;
            var rsImg = rowShine.GetComponent<Image>();
            rsImg.sprite = roundedMd; rsImg.type = Image.Type.Sliced;
            rsImg.color = new Color(1f, 1f, 1f, 0.24f);
            rsImg.raycastTarget = false;

            // Item icon + colored glow disk
            var iconGlow = new GameObject("IconGlow", typeof(RectTransform), typeof(Image));
            iconGlow.transform.SetParent(row.transform, false);
            var igRT = (RectTransform)iconGlow.transform;
            igRT.anchorMin = new Vector2(0, 0.5f); igRT.anchorMax = new Vector2(0, 0.5f);
            igRT.sizeDelta = new Vector2(110, 110);
            igRT.anchoredPosition = new Vector2(70, 0);
            var igImg = iconGlow.GetComponent<Image>();
            igImg.sprite = circle;
            igImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.30f);
            igImg.raycastTarget = false;
            iconGlow.SetActive(false);

            var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{iconName}.png");
            if (iconSprite != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(row.transform, false);
                var iRT = (RectTransform)iconGO.transform;
                iRT.anchorMin = new Vector2(0, 0.5f); iRT.anchorMax = new Vector2(0, 0.5f);
                iRT.sizeDelta = new Vector2(120, 120);
                iRT.anchoredPosition = new Vector2(70, 0);
                var iImg = iconGO.GetComponent<Image>();
                iImg.sprite = iconSprite;
                iImg.color = Color.white;
                iImg.raycastTarget = false;
                var iconShadow = iconGO.AddComponent<UnityEngine.UI.Shadow>();
                iconShadow.effectColor = new Color(0, 0, 0, 0.55f);
                iconShadow.effectDistance = new Vector2(1, -2);
            }

            // Label — büyük outlined
            var labelTxt = CreateText(row.transform, "Label", label,
                new Vector2(0, 0.50f), new Vector2(0.55f, 1f),
                new Vector2(140, 0), new Vector2(0, 0),
                 TextAlignmentOptions.Left, 38, CandyTextColor);
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.outlineWidth = 0f;
            labelTxt.raycastTarget = false;
            var labelShadow = labelTxt.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            labelShadow.effectColor = new Color(1f, 1f, 1f, 0.40f);
            labelShadow.effectDistance = new Vector2(1, 1);

            // Price text — ikon yanında, alt yarıda
            var priceText = CreateText(row.transform, "Price", price.ToString(),
                new Vector2(0, 0f), new Vector2(0.55f, 0.50f),
                new Vector2(140, 0), new Vector2(0, 0),
                TextAlignmentOptions.Left, 34, CandyAccentColor);
            priceText.fontStyle = FontStyles.Bold;
            priceText.outlineWidth = 0f;
            priceText.raycastTarget = false;
            priceText.text = price + " ₺";

            // BUY button — same green candy asset used by the new pause/settings UI.
            var btnWrap = new GameObject($"BuyWrap", typeof(RectTransform));
            btnWrap.transform.SetParent(row.transform, false);
            var bwRT = (RectTransform)btnWrap.transform;
            bwRT.anchorMin = new Vector2(0.62f, 0.10f); bwRT.anchorMax = new Vector2(0.98f, 0.90f);
            bwRT.offsetMin = bwRT.offsetMax = Vector2.zero;

            // Shadow
            var btnSh = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            btnSh.transform.SetParent(btnWrap.transform, false);
            var bsRT = (RectTransform)btnSh.transform;
            bsRT.anchorMin = Vector2.zero; bsRT.anchorMax = Vector2.one;
            bsRT.offsetMin = new Vector2(0, -8); bsRT.offsetMax = new Vector2(0, -2);
            var bsImg = btnSh.GetComponent<Image>();
            bsImg.sprite = roundedMd; bsImg.type = Image.Type.Sliced;
            bsImg.color = new Color(0.45f, 0.22f, 0.07f, 0.22f);
            bsImg.raycastTarget = false;

            // Border (dark green)
            var btnBd = new GameObject("Border", typeof(RectTransform), typeof(Image));
            btnBd.transform.SetParent(btnWrap.transform, false);
            var bbRT = (RectTransform)btnBd.transform;
            bbRT.anchorMin = Vector2.zero; bbRT.anchorMax = Vector2.one;
            bbRT.offsetMin = bbRT.offsetMax = Vector2.zero;
            var bbImg = btnBd.GetComponent<Image>();
            bbImg.sprite = roundedMd; bbImg.type = Image.Type.Sliced;
            bbImg.color = Color.clear;
            bbImg.raycastTarget = false;

            // Body (bright green) — button is here
            var btnGO = new GameObject("Buy", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(btnWrap.transform, false);
            var brt = (RectTransform)btnGO.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(5, 5); brt.offsetMax = new Vector2(-5, -5);
            var btnImg = btnGO.GetComponent<Image>();
            btnImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_green_v2.png");
            btnImg.type = Image.Type.Simple;
            btnImg.preserveAspect = true;
            btnImg.color = Color.white;

            // Body shine (top)
            var btnShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            btnShine.transform.SetParent(btnGO.transform, false);
            var bsh = (RectTransform)btnShine.transform;
            bsh.anchorMin = new Vector2(0.10f, 0.50f); bsh.anchorMax = new Vector2(0.90f, 0.92f);
            bsh.offsetMin = bsh.offsetMax = Vector2.zero;
            var btnShineImg = btnShine.GetComponent<Image>();
            btnShineImg.sprite = roundedMd; btnShineImg.type = Image.Type.Sliced;
            btnShineImg.color = Color.clear;
            btnShineImg.raycastTarget = false;

            var buyTxt = CreateText(btnGO.transform, "Label", "AL",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 34, Color.white);
            buyTxt.fontStyle = FontStyles.Bold;
            buyTxt.outlineWidth = 0f;
            buyTxt.raycastTarget = false;
            var buyShadow = buyTxt.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            buyShadow.effectColor = new Color(0.18f, 0.30f, 0.05f, 0.55f);
            buyShadow.effectDistance = new Vector2(1, -2);

            return new ShopRow { btn = btnGO.GetComponent<Button>(), priceText = priceText };
        }

        private static GameObject BuildDailyRewardPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "DailyRewardPanel", tapOutsideToClose: true);
            var card = CreateCandyPanelCard(overlay.transform, "Card",
                new Vector2(0.14f, 0.24f), new Vector2(0.86f, 0.76f));

            var title = CreateText(card.transform, "Title", "GÜNLÜK ÖDÜL",
                new Vector2(0.16f, 0.84f), new Vector2(0.84f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 50, CandyTitleColor);
            StyleCandyTitle(title);

            var gift = new GameObject("GiftDeco", typeof(RectTransform), typeof(Image));
            gift.transform.SetParent(card.transform, false);
            var giftRT = (RectTransform)gift.transform;
            giftRT.anchorMin = giftRT.anchorMax = new Vector2(0.5f, 0.41f);
            giftRT.sizeDelta = new Vector2(160f, 160f);
            giftRT.anchoredPosition = Vector2.zero;
            var giftImage = gift.GetComponent<Image>();
            giftImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/menu_daily.png");
            giftImage.preserveAspect = true;
            giftImage.raycastTarget = false;

            var statusText = CreateText(card.transform, "Status", "Ödülün hazır!",
                new Vector2(0.10f, 0.60f), new Vector2(0.90f, 0.70f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 38, CandyAccentColor);
            statusText.fontStyle = FontStyles.Bold;
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = 24f;
            statusText.fontSizeMax = 40f;

            var streakText = CreateText(card.transform, "Streak", "Streak: 0 gün",
                new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.58f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 30, CandyDimColor);

            var claimBtn = CreateModernPanelButton(card.transform, "ClaimButton", "ÖDÜLÜ AL",
                new Vector2(0.23f, 0.16f), new Vector2(0.77f, 0.30f),
                new Color(0.30f, 0.78f, 0.36f, 1f),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_green_v2.png"));

            BuildCloseXButton(card.transform, new Vector2(0.80f, 0.86f), new Vector2(0.97f, 0.98f), overlay);

            var daily = overlay.AddComponent<DailyRewardPanel>();
            var so = new SerializedObject(daily);
            so.FindProperty("claimButton").objectReferenceValue = claimBtn;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("streakText").objectReferenceValue = streakText;
            so.ApplyModifiedPropertiesWithoutUndo();
            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildStatsPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "StatsPanel_Menu", tapOutsideToClose: true);
            var card = CreateCandyPanelCard(overlay.transform, "Card",
                new Vector2(0.13f, 0.23f), new Vector2(0.87f, 0.77f));

            var title = CreateText(card.transform, "Title", "İSTATİSTİKLER",
                new Vector2(0.16f, 0.84f), new Vector2(0.84f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 50, CandyTitleColor);
            StyleCandyTitle(title);

            var balloonsText = AddStatRow(card.transform, "Patlatılan Balon", 0.72f);
            var bombsText = AddStatRow(card.transform, "Patlayan Bomba", 0.63f);
            var comboText = AddStatRow(card.transform, "En Uzun Combo", 0.54f);
            var gamesText = AddStatRow(card.transform, "Oynanan Oyun", 0.45f);
            var winsText = AddStatRow(card.transform, "Kazanılan Seviye", 0.36f);
            var starsText = AddStatRow(card.transform, "Toplam Yıldız", 0.27f);
            var endlessText = AddStatRow(card.transform, "Endless Skor", 0.18f);

            BuildCloseXButton(card.transform, new Vector2(0.80f, 0.86f), new Vector2(0.97f, 0.98f), overlay);

            var stats = overlay.AddComponent<StatsPanel>();
            var so = new SerializedObject(stats);
            so.FindProperty("balloonsText").objectReferenceValue = balloonsText;
            so.FindProperty("bombsText").objectReferenceValue = bombsText;
            so.FindProperty("comboText").objectReferenceValue = comboText;
            so.FindProperty("gamesText").objectReferenceValue = gamesText;
            so.FindProperty("winsText").objectReferenceValue = winsText;
            so.FindProperty("starsText").objectReferenceValue = starsText;
            so.FindProperty("endlessText").objectReferenceValue = endlessText;
            so.ApplyModifiedPropertiesWithoutUndo();
            overlay.SetActive(false);
            return overlay;
        }

        private static TMP_Text AddStatRow(Transform parent, string label, float yCenter)
        {
            var row = new GameObject($"Row_{label}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            var rt = (RectTransform)row.transform;
            rt.anchorMin = new Vector2(0.14f, yCenter - 0.036f);
            rt.anchorMax = new Vector2(0.86f, yCenter + 0.036f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = row.GetComponent<Image>();
            bg.sprite = roundedSm;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 0.92f, 0.72f, 0.56f);
            bg.raycastTarget = false;

            CreateText(row.transform, "Label", label,
                new Vector2(0.04f, 0), new Vector2(0.68f, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Left, 30, CandyDimColor);

            return CreateText(row.transform, "Value", "0",
                new Vector2(0.68f, 0), new Vector2(0.95f, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Right, 34, CandyAccentColor);
        }

        private static GameObject BuildAchievementListPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "AchievementListPanel", tapOutsideToClose: true);
            var card = CreateCandyPanelCard(overlay.transform, "Card",
                new Vector2(0.03f, 0.14f), new Vector2(0.97f, 0.86f), preserveAspect: false);

            var title = CreateText(card.transform, "Title", "BAŞARIMLAR",
                new Vector2(0.18f, 0.88f), new Vector2(0.82f, 0.97f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 52, CandyTitleColor);
            StyleCandyTitle(title);

            var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(RectMask2D));
            scrollGO.transform.SetParent(card.transform, false);
            var srt = (RectTransform)scrollGO.transform;
            srt.anchorMin = new Vector2(0.09f, 0.06f);
            srt.anchorMax = new Vector2(0.91f, 0.82f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            var scrollBg = scrollGO.GetComponent<Image>();
            scrollBg.sprite = roundedMd;
            scrollBg.type = Image.Type.Sliced;
            scrollBg.color = new Color(0.76f, 0.48f, 0.22f, 0.10f);
            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.horizontal = false;

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var vrt = (RectTransform)viewportGO.transform;
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = vrt.offsetMax = Vector2.zero;
            scroll.viewport = vrt;

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var crt = (RectTransform)contentGO.transform;
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 15;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            var csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;

            var achItemPrefab = CreateAchievementItemPrefab();

            // Modern glossy X kapatma butonu — geniş hit area (mobil dokunma payı)
            BuildCloseXButton(card.transform, new Vector2(0.84f, 0.87f), new Vector2(0.98f, 0.98f), overlay);

            var list = overlay.AddComponent<AchievementListPanel>();
            var so = new SerializedObject(list);
            so.FindProperty("itemContainer").objectReferenceValue = contentGO.transform;
            so.FindProperty("itemPrefab").objectReferenceValue = achItemPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject CreateAchievementItemPrefab()
        {
            const string path = "Assets/Prefabs/AchievementItem.prefab";

            // Root — tüm item. AchievementListPanel script'i bu Image'ı (images[0]) renklendiriyor.
            var go = new GameObject("AchievementItem", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(0, 160);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 160;
            var img = go.GetComponent<Image>();
            img.sprite = roundedMd; img.type = Image.Type.Sliced;
            img.color = new Color(1f, 0.93f, 0.74f, 0.88f); // script unlocked durumunda sıcak altına çevirir

            // Shine (üst parlaklık - cam efekti)
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(go.transform, false);
            var shrt = (RectTransform)shine.transform;
            shrt.anchorMin = new Vector2(0.02f, 0.55f);
            shrt.anchorMax = new Vector2(0.98f, 0.92f);
            shrt.offsetMin = shrt.offsetMax = Vector2.zero;
            var shImg = shine.GetComponent<Image>();
            shImg.sprite = roundedMd; shImg.type = Image.Type.Sliced;
            shImg.color = new Color(1f, 1f, 1f, 0.28f);
            shImg.raycastTarget = false;

            // İkon arkasındaki yuvarlak glow disk
            var glow = new GameObject("IconGlow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(go.transform, false);
            var glRT = (RectTransform)glow.transform;
            glRT.anchorMin = new Vector2(0, 0.5f); glRT.anchorMax = new Vector2(0, 0.5f);
            glRT.sizeDelta = new Vector2(135, 135);
            glRT.anchoredPosition = new Vector2(85, 0);
            var glImg = glow.GetComponent<Image>();
            glImg.sprite = circle;
            glImg.color = new Color(1f, 0.94f, 0.40f, 0.28f);
            glImg.raycastTarget = false;

            // Yıldız ikonu (büyük, drop shadow ile)
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(go.transform, false);
            var irt = (RectTransform)iconGO.transform;
            irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f);
            irt.sizeDelta = new Vector2(105, 105);
            irt.anchoredPosition = new Vector2(85, 0);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = star;
            iconImg.color = new Color(1f, 0.85f, 0.20f, 1f);
            iconImg.raycastTarget = false;
            var iconShadow = iconGO.AddComponent<UnityEngine.UI.Shadow>();
            iconShadow.effectColor = new Color(0.30f, 0.15f, 0f, 0.65f);
            iconShadow.effectDistance = new Vector2(2, -3);

            // Title — büyük, outlined, drop shadow
            var titleText = CreateText(go.transform, "Title", "Title",
                new Vector2(0, 0.50f), new Vector2(0.78f, 0.95f),
                new Vector2(170, 0), new Vector2(0, 0),
                TextAlignmentOptions.Left, 38, CandyTextColor);
            titleText.fontStyle = FontStyles.Bold;
            titleText.outlineWidth = 0f;
            titleText.raycastTarget = false;
            var titleShadow = titleText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            titleShadow.effectColor = new Color(1f, 1f, 1f, 0.38f);
            titleShadow.effectDistance = new Vector2(1, 1);

            // Description — daha açık, sub-text
            var descText = CreateText(go.transform, "Desc", "Desc",
                new Vector2(0, 0.10f), new Vector2(0.78f, 0.50f),
                new Vector2(170, 0), new Vector2(0, 0),
                TextAlignmentOptions.Left, 28, CandyDimColor);
            descText.raycastTarget = false;

            // Progress — sağ tarafta dikey ortalı, büyük accent renkte
            var progressText = CreateText(go.transform, "Progress", "0/0",
                new Vector2(0.78f, 0), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(-25, 0),
                TextAlignmentOptions.Right, 34, CandyAccentColor);
            progressText.fontStyle = FontStyles.Bold;
            progressText.outlineWidth = 0f;
            progressText.raycastTarget = false;
            var progShadow = progressText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            progShadow.effectColor = new Color(1f, 1f, 1f, 0.35f);
            progShadow.effectDistance = new Vector2(1, 1);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void BuildHeaderHUD(Transform parent, GameObject shopPanel)
        {
            var bar = new GameObject("HeaderHUD", typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            var brt = (RectTransform)bar.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0f, 1f);
            brt.pivot = new Vector2(0f, 1f);
            brt.anchoredPosition = new Vector2(22f, -22f);
            brt.sizeDelta = new Vector2(10f, 10f);

            var coinSprite = LoadFirstSprite("Assets/Sprites/cap_coin.png");
            var heartSprite = LoadFirstSprite("Assets/Sprites/cap_heart.png");
            var starSprite = LoadFirstSprite("Assets/Sprites/cap_star.png");

            // Keep each capsule at its source aspect ratio. Single-digit heart/star values use
            // shorter capsules, while coin keeps room for the embedded green + badge.
            var coinCard = BuildResourceCapsule(bar.transform, "CoinCard", coinSprite,
                new Vector2(0f, 0f), new Vector2(235f, 83f),
                new Vector2(0.34f, 0.16f), new Vector2(0.72f, 0.84f), "0");
            var coinText = coinCard.text;

            var coinDisplay = coinCard.root.AddComponent<CoinDisplay>();
            var so = new SerializedObject(coinDisplay);
            so.FindProperty("coinText").objectReferenceValue = coinText;
            so.ApplyModifiedPropertiesWithoutUndo();

            // The + badge is baked into cap_coin; this transparent hit area makes it functional.
            var shopHitArea = new GameObject("ShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
            shopHitArea.transform.SetParent(coinCard.root.transform.Find("Body"), false);
            var shopRT = (RectTransform)shopHitArea.transform;
            shopRT.anchorMin = new Vector2(0.74f, 0.05f);
            shopRT.anchorMax = new Vector2(0.995f, 0.95f);
            shopRT.offsetMin = shopRT.offsetMax = Vector2.zero;
            var shopImage = shopHitArea.GetComponent<Image>();
            shopImage.color = new Color(1f, 1f, 1f, 0f);
            shopImage.raycastTarget = true;
            shopHitArea.GetComponent<Button>().transition = Selectable.Transition.None;
            var shopHook = shopHitArea.AddComponent<ButtonOpenTarget>();
            shopHook.Target = shopPanel;

            var heartCard = BuildResourceCapsule(bar.transform, "HeartCard", heartSprite,
                new Vector2(0f, -93f), new Vector2(170f, 78f),
                new Vector2(0.43f, 0.16f), new Vector2(0.88f, 0.84f), "5");
            var heartCountText = heartCard.text;
            var heartTimerText = CreateText(heartCard.root.transform, "Timer", "TAM",
                new Vector2(0.58f, 0.16f), new Vector2(0.88f, 0.84f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 20, new Color(0.60f, 0.45f, 0.28f, 1f));
            heartTimerText.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PaytoneOne SDF.asset");
            heartTimerText.enableAutoSizing = true;
            heartTimerText.fontSizeMin = 12f;
            heartTimerText.fontSizeMax = 20f;
            heartTimerText.raycastTarget = false;
            heartTimerText.gameObject.SetActive(false);

            var heartDisplay = heartCard.root.AddComponent<HeartDisplay>();
            var so3 = new SerializedObject(heartDisplay);
            so3.FindProperty("countText").objectReferenceValue = heartCountText;
            so3.FindProperty("timerText").objectReferenceValue = heartTimerText;
            so3.ApplyModifiedPropertiesWithoutUndo();

            var starCard = BuildResourceCapsule(bar.transform, "StarCard", starSprite,
                new Vector2(0f, -181f), new Vector2(170f, 78.5f),
                new Vector2(0.43f, 0.16f), new Vector2(0.88f, 0.84f), "0");
            var starText = starCard.text;
            var stars = starCard.root.AddComponent<TotalStarsDisplay>();
            var so2 = new SerializedObject(stars);
            so2.FindProperty("starText").objectReferenceValue = starText;
            so2.ApplyModifiedPropertiesWithoutUndo();
        }

        private struct HudPill { public GameObject root; public TMP_Text text; }

        private static Sprite LoadFirstSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                var subSprite = asset as Sprite;
                if (subSprite != null) return subSprite;
            }
            return null;
        }

        private static HudPill BuildResourceCapsule(Transform parent, string name, Sprite sprite,
            Vector2 anchoredPosition, Vector2 size, Vector2 textAnchorMin, Vector2 textAnchorMax,
            string initialText)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(root.transform, false);
            var bRT = (RectTransform)body.transform;
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;
            var bImg = body.GetComponent<Image>();
            bImg.sprite = sprite;
            bImg.type = Image.Type.Simple;
            bImg.color = Color.white;
            bImg.preserveAspect = false;
            bImg.raycastTarget = false;

            var text = CreateText(body.transform, "Text", initialText,
                textAnchorMin, textAnchorMax, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 24, new Color(0.45f, 0.30f, 0.15f, 1f));
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PaytoneOne SDF.asset");
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = 16f;
            text.fontSizeMax = 34f;
            text.outlineWidth = 0f;
            text.raycastTarget = false;
            var txShadow = text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            txShadow.effectColor = new Color(0, 0, 0, 0.55f);
            txShadow.effectDistance = new Vector2(1, -2);

            return new HudPill { root = root, text = text };
        }

        private static GoalItemUI CreateGoalItemPrefab()
        {
            const string path = "Assets/Prefabs/GoalItem.prefab";

            var root = new GameObject("GoalItem", typeof(RectTransform));
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 110);

            var bgGO = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(root.transform, false);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = roundedSm;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.12f, 0.16f, 0.30f, 0.92f);

            AddGradientHighlight(root.transform);

            var iconGO = new GameObject("Icon", typeof(Image));
            iconGO.transform.SetParent(root.transform, false);
            var iconRT = (RectTransform)iconGO.transform;
            iconRT.anchorMin = new Vector2(0f, 0.5f); iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.sizeDelta = new Vector2(90, 90);
            iconRT.anchoredPosition = new Vector2(50, 0);

            var text = CreateText(root.transform, "Remaining", "0",
                new Vector2(0.45f, 0), new Vector2(1f, 1f),
                new Vector2(0, 0), new Vector2(-12, 0),
                TextAlignmentOptions.Right, 56, C_TextLight);
            text.fontStyle = FontStyles.Bold;
            text.outlineWidth = 0.2f;
            text.outlineColor = new Color(0, 0, 0, 0.55f);

            var checkGO = new GameObject("CheckMark", typeof(Image));
            checkGO.transform.SetParent(root.transform, false);
            var crt = (RectTransform)checkGO.transform;
            crt.anchorMin = new Vector2(0.6f, 0.55f); crt.anchorMax = new Vector2(0.95f, 0.95f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            checkGO.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_star.png");
            checkGO.GetComponent<Image>().color = new Color(0.32f, 0.92f, 0.45f);
            checkGO.SetActive(false);

            var ui = root.AddComponent<GoalItemUI>();
            var iconSprites = new Sprite[7];
            string[] names = { "balloon_red","balloon_blue","balloon_green","balloon_yellow",
                               "balloon_purple","balloon_orange","balloon_pink" };
            for (int i = 0; i < names.Length; i++)
                iconSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{names[i]}.png");

            var so = new SerializedObject(ui);
            so.FindProperty("icon").objectReferenceValue = iconGO.GetComponent<Image>();
            so.FindProperty("remainingText").objectReferenceValue = text;
            so.FindProperty("checkMark").objectReferenceValue = checkGO;
            var arr = so.FindProperty("colorIcons");
            arr.arraySize = iconSprites.Length;
            for (int i = 0; i < iconSprites.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = iconSprites[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<GoalItemUI>();
        }

        private static GameObject BuildWinPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "WinPanel");

            var confettiGO = new GameObject("ConfettiRain", typeof(RectTransform));
            confettiGO.transform.SetParent(overlay.transform, false);
            var crt = (RectTransform)confettiGO.transform;
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var confetti = confettiGO.AddComponent<ConfettiRain>();
            var cso = new SerializedObject(confetti);
            cso.FindProperty("particleSprite").objectReferenceValue = circle;
            cso.FindProperty("starSprite").objectReferenceValue = star;
            cso.ApplyModifiedPropertiesWithoutUndo();

            // Maskot kutlama pozisyonu (eğer mascot sprite mevcutsa)
            var mascotSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/moscot.png")
                        ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mascot.png");
            if (mascotSp != null)
            {
                var mascotGO = new GameObject("WinMascot", typeof(RectTransform), typeof(Image));
                mascotGO.transform.SetParent(overlay.transform, false);
                var mrt = (RectTransform)mascotGO.transform;
                mrt.anchorMin = new Vector2(0.05f, 0.62f);
                mrt.anchorMax = new Vector2(0.30f, 0.95f);
                mrt.offsetMin = mrt.offsetMax = Vector2.zero;
                var mImg = mascotGO.GetComponent<Image>();
                mImg.sprite = mascotSp;
                mImg.preserveAspect = true;
                mImg.raycastTarget = false;
                mascotGO.AddComponent<BalloonPop.Effects.LogoBob>();
            }

            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.07f, 0.18f), new Vector2(0.93f, 0.82f), C_PanelBg);

            // TEBRİKLER başlığı — büyük, outlined, drop shadow ile
            var title = CreateText(card.transform, "Title", "TEBRİKLER!",
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 110, C_Accent);
            title.fontStyle = FontStyles.Bold;
            title.outlineWidth = 0.32f;
            title.outlineColor = new Color(0.18f, 0.05f, 0.30f, 1f);
            var winTitleShadow = title.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            winTitleShadow.effectColor = new Color(0, 0, 0, 0.6f);
            winTitleShadow.effectDistance = new Vector2(3, -5);

            CreateText(card.transform, "Sub", "Seviye Tamamlandı",
                new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.81f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 42, new Color(1, 1, 1, 0.85f)).fontStyle = FontStyles.Bold;

            // 3 yıldız — büyük + her birine glow disk + shadow
            var stars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                // Glow disk
                var glow = new GameObject($"StarGlow{i}", typeof(Image));
                glow.transform.SetParent(card.transform, false);
                var grt = (RectTransform)glow.transform;
                grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.58f);
                grt.sizeDelta = new Vector2(170, 170);
                grt.anchoredPosition = new Vector2((i - 1) * 160, 0);
                var gImg = glow.GetComponent<Image>();
                gImg.sprite = circle;
                gImg.color = new Color(1f, 0.92f, 0.30f, 0.30f);
                gImg.raycastTarget = false;

                // Yıldız sprite
                var s = new GameObject($"Star{i}", typeof(Image));
                s.transform.SetParent(card.transform, false);
                var srt = (RectTransform)s.transform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.58f);
                srt.sizeDelta = new Vector2(135, 135);
                srt.anchoredPosition = new Vector2((i - 1) * 160, 0);
                var img = s.GetComponent<Image>();
                img.sprite = star;
                img.color = C_StarOn;
                var sShadow = s.AddComponent<UnityEngine.UI.Shadow>();
                sShadow.effectColor = new Color(0.40f, 0.20f, 0f, 0.65f);
                sShadow.effectDistance = new Vector2(2, -4);
                stars[i] = s;
            }

            // Skor pill — outlined büyük
            var scorePill = new GameObject("ScorePill", typeof(RectTransform), typeof(Image));
            scorePill.transform.SetParent(card.transform, false);
            var spRT = (RectTransform)scorePill.transform;
            spRT.anchorMin = new Vector2(0.18f, 0.32f); spRT.anchorMax = new Vector2(0.82f, 0.46f);
            spRT.offsetMin = spRT.offsetMax = Vector2.zero;
            var spImg = scorePill.GetComponent<Image>();
            spImg.sprite = roundedMd; spImg.type = Image.Type.Sliced;
            spImg.color = new Color(0.10f, 0.06f, 0.22f, 0.85f);
            spImg.raycastTarget = false;
            var spShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            spShine.transform.SetParent(scorePill.transform, false);
            var spsRT = (RectTransform)spShine.transform;
            spsRT.anchorMin = new Vector2(0.05f, 0.55f); spsRT.anchorMax = new Vector2(0.95f, 0.92f);
            spsRT.offsetMin = spsRT.offsetMax = Vector2.zero;
            var spsImg = spShine.GetComponent<Image>();
            spsImg.sprite = roundedMd; spsImg.type = Image.Type.Sliced;
            spsImg.color = new Color(1f, 1f, 1f, 0.12f);
            spsImg.raycastTarget = false;
            CreateText(scorePill.transform, "ScoreLabel", "SKOR",
                new Vector2(0, 0.55f), new Vector2(1, 1f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 32, new Color(1, 1, 1, 0.7f)).raycastTarget = false;
            var scoreText = CreateText(scorePill.transform, "ScoreText", "0",
                new Vector2(0, 0f), new Vector2(1, 0.60f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 74, C_Accent);
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.outlineWidth = 0.28f;
            scoreText.outlineColor = new Color(0.30f, 0.18f, 0f, 1f);
            scoreText.raycastTarget = false;
            var sctShadow = scoreText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            sctShadow.effectColor = new Color(0, 0, 0, 0.5f);
            sctShadow.effectDistance = new Vector2(2, -3);

            // 3 modern glossy buton
            var nextBtn = CreateModernPanelButton(card.transform, "NextLevelButton", "SONRAKİ",
                new Vector2(0.06f, 0.16f), new Vector2(0.49f, 0.27f),
                new Color(0.30f, 0.78f, 0.36f, 1f));
            var replayBtn = CreateModernPanelButton(card.transform, "ReplayButton", "TEKRAR",
                new Vector2(0.51f, 0.16f), new Vector2(0.94f, 0.27f),
                new Color(0.30f, 0.70f, 0.95f, 1f));
            var menuBtn = CreateModernPanelButton(card.transform, "MenuButton", "MENÜ",
                new Vector2(0.22f, 0.03f), new Vector2(0.78f, 0.14f),
                new Color(0.62f, 0.42f, 0.95f, 1f));

            var winComp = overlay.AddComponent<WinPanel>();
            var so = new SerializedObject(winComp);
            so.FindProperty("finalScoreText").objectReferenceValue = scoreText;
            so.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("replayButton").objectReferenceValue = replayBtn;
            var arr = so.FindProperty("stars");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildLosePanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "LosePanel");

            // WinPanel ile aynı premium kart boyutu
            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.07f, 0.22f), new Vector2(0.93f, 0.78f), C_PanelBg);

            // Üst — kırık kalp ikon (büyük daire + simge)
            var iconWrap = new GameObject("IconWrap", typeof(RectTransform));
            iconWrap.transform.SetParent(card.transform, false);
            var iwRT = (RectTransform)iconWrap.transform;
            iwRT.anchorMin = new Vector2(0.5f, 0.65f);
            iwRT.anchorMax = new Vector2(0.5f, 0.65f);
            iwRT.sizeDelta = new Vector2(180, 180);
            iwRT.anchoredPosition = Vector2.zero;

            var iconGlow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            iconGlow.transform.SetParent(iconWrap.transform, false);
            var igRT = (RectTransform)iconGlow.transform;
            igRT.anchorMin = Vector2.zero; igRT.anchorMax = Vector2.one;
            igRT.offsetMin = new Vector2(-15, -15); igRT.offsetMax = new Vector2(15, 15);
            var igImg = iconGlow.GetComponent<Image>();
            igImg.sprite = circle; igImg.color = new Color(1f, 0.30f, 0.42f, 0.30f);
            igImg.raycastTarget = false;

            var iconBg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            iconBg.transform.SetParent(iconWrap.transform, false);
            var ibRT = (RectTransform)iconBg.transform;
            ibRT.anchorMin = Vector2.zero; ibRT.anchorMax = Vector2.one;
            ibRT.offsetMin = ibRT.offsetMax = Vector2.zero;
            var ibImg = iconBg.GetComponent<Image>();
            ibImg.sprite = circle; ibImg.color = new Color(0.95f, 0.30f, 0.42f);
            ibImg.raycastTarget = false;

            var iconShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            iconShine.transform.SetParent(iconBg.transform, false);
            var isRT = (RectTransform)iconShine.transform;
            isRT.anchorMin = new Vector2(0.18f, 0.50f); isRT.anchorMax = new Vector2(0.82f, 0.92f);
            isRT.offsetMin = isRT.offsetMax = Vector2.zero;
            var isImg = iconShine.GetComponent<Image>();
            isImg.sprite = circle; isImg.color = new Color(1f, 1f, 1f, 0.40f);
            isImg.raycastTarget = false;

            var iconEmoji = CreateText(iconBg.transform, "X", "✕",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 130, Color.white);
            iconEmoji.fontStyle = FontStyles.Bold;
            iconEmoji.raycastTarget = false;
            var ieShadow = iconEmoji.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            ieShadow.effectColor = new Color(0.3f, 0.05f, 0.10f, 0.7f);
            ieShadow.effectDistance = new Vector2(2, -3);

            // Başlık — outlined, drop shadow
            var title = CreateText(card.transform, "Title", "BAŞARISIZ!",
                new Vector2(0.05f, 0.43f), new Vector2(0.95f, 0.57f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 96, new Color(1f, 0.32f, 0.42f));
            title.fontStyle = FontStyles.Bold;
            title.outlineWidth = 0.32f;
            title.outlineColor = new Color(0.30f, 0.05f, 0.12f, 1f);
            var loseShadow = title.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            loseShadow.effectColor = new Color(0, 0, 0, 0.6f);
            loseShadow.effectDistance = new Vector2(3, -5);

            // Alt yazı
            var sub = CreateText(card.transform, "Sub", "Hamleler bitti — bir daha dene!",
                new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.42f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 36, new Color(1f, 1f, 1f, 0.80f));
            sub.fontStyle = FontStyles.Bold;
            var loc = sub.gameObject.AddComponent<LocalizedText>();
            var locSO = new SerializedObject(loc);
            locSO.FindProperty("key").stringValue = "lose.sub";
            locSO.ApplyModifiedPropertiesWithoutUndo();

            // Modern glossy butonlar (WinPanel ile aynı stil)
            var retryBtn = CreateModernPanelButton(card.transform, "RetryButton", "TEKRAR DENE",
                new Vector2(0.08f, 0.13f), new Vector2(0.55f, 0.25f),
                new Color(0.30f, 0.78f, 0.36f, 1f));   // yeşil → tekrar dene
            var menuBtn = CreateModernPanelButton(card.transform, "MenuButton", "MENÜ",
                new Vector2(0.57f, 0.13f), new Vector2(0.92f, 0.25f),
                new Color(0.62f, 0.42f, 0.95f, 1f));   // mor → menü

            // Button label'ları lokalize et
            var retryLabel = retryBtn.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (retryLabel != null) {
                var rLoc = retryLabel.gameObject.AddComponent<LocalizedText>();
                var rSO = new SerializedObject(rLoc);
                rSO.FindProperty("key").stringValue = "lose.retry";
                rSO.ApplyModifiedPropertiesWithoutUndo();
            }
            var menuLabel = menuBtn.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (menuLabel != null) {
                var mLoc = menuLabel.gameObject.AddComponent<LocalizedText>();
                var mSO = new SerializedObject(mLoc);
                mSO.FindProperty("key").stringValue = "pause.menu";
                mSO.ApplyModifiedPropertiesWithoutUndo();
            }

            var titleLoc = title.gameObject.AddComponent<LocalizedText>();
            var tSO = new SerializedObject(titleLoc);
            tSO.FindProperty("key").stringValue = "lose.title";
            tSO.ApplyModifiedPropertiesWithoutUndo();

            var lose = overlay.AddComponent<LosePanel>();
            var so = new SerializedObject(lose);
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildMysteryBoxPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "MysteryBoxPanel");
            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.75f), C_PanelBg);

            var title = CreateText(card.transform, "Title", "MYSTERY BOX!",
                new Vector2(0, 0.83f), new Vector2(1, 0.95f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 75, C_Accent);
            title.fontStyle = FontStyles.Bold;

            var boxGroup = new GameObject("BoxGroup", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            boxGroup.transform.SetParent(card.transform, false);
            var bgrt = (RectTransform)boxGroup.transform;
            bgrt.anchorMin = new Vector2(0.3f, 0.45f); bgrt.anchorMax = new Vector2(0.7f, 0.78f);
            bgrt.offsetMin = bgrt.offsetMax = Vector2.zero;
            var bgImg = boxGroup.GetComponent<Image>();
            bgImg.sprite = roundedMd; bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.95f, 0.6f, 0.2f);
            var question = CreateText(boxGroup.transform, "Q", "?",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 200, Color.white);
            question.fontStyle = FontStyles.Bold;

            var rewardText = CreateText(card.transform, "Reward", "Hazine kutusunu aç!",
                new Vector2(0, 0.30f), new Vector2(1, 0.42f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 50, C_TextLight);
            rewardText.fontStyle = FontStyles.Bold;

            var openBtn = CreatePrimaryButton(card.transform, "Open", "AÇ!",
                new Vector2(0.1f, 0.10f), new Vector2(0.49f, 0.22f));
            var closeBtn = CreateSecondaryButton(card.transform, "Close", "KAPAT",
                new Vector2(0.51f, 0.10f), new Vector2(0.9f, 0.22f));

            var mb = overlay.AddComponent<MysteryBoxPanel>();
            var so = new SerializedObject(mb);
            so.FindProperty("openButton").objectReferenceValue = openBtn;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("rewardText").objectReferenceValue = rewardText;
            so.FindProperty("boxGroup").objectReferenceValue = boxGroup.GetComponent<CanvasGroup>();
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildPausePanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "PausePanel");
            var pausePanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/panel_pause_v2.png");
            var pauseGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_green_v2.png");
            var pauseBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_blue_v2.png");
            var pauseRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_red_v2.png");

            // Card wrapper — glossy 3D katmanlı
            var cardWrap = new GameObject("CardWrap", typeof(RectTransform));
            cardWrap.transform.SetParent(overlay.transform, false);
            var cwrt = (RectTransform)cardWrap.transform;
            cwrt.anchorMin = new Vector2(0.13f, 0.23f);
            cwrt.anchorMax = new Vector2(0.87f, 0.77f);
            cwrt.offsetMin = cwrt.offsetMax = Vector2.zero;

            // Drop shadow
            var pShadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            pShadow.transform.SetParent(cardWrap.transform, false);
            var psRT = (RectTransform)pShadow.transform;
            psRT.anchorMin = Vector2.zero; psRT.anchorMax = Vector2.one;
            psRT.offsetMin = new Vector2(-5, -22);
            psRT.offsetMax = new Vector2(5, -8);
            var psImg = pShadow.GetComponent<Image>();
            psImg.sprite = roundedLg; psImg.type = Image.Type.Sliced;
            psImg.color = new Color(0, 0, 0, 0.5f);
            psImg.raycastTarget = false;
            pShadow.SetActive(false);

            // Dark border
            var pBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
            pBorder.transform.SetParent(cardWrap.transform, false);
            var pbRT = (RectTransform)pBorder.transform;
            pbRT.anchorMin = Vector2.zero; pbRT.anchorMax = Vector2.one;
            pbRT.offsetMin = pbRT.offsetMax = Vector2.zero;
            var pbImg = pBorder.GetComponent<Image>();
            pbImg.sprite = roundedLg; pbImg.type = Image.Type.Sliced;
            pbImg.color = new Color(0.05f, 0.10f, 0.22f, 1f);
            pbImg.raycastTarget = false;
            pBorder.SetActive(false);

            // Body (deep navy/purple gradient look via Image + Inner shade)
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(cardWrap.transform, false);
            var cardRT = (RectTransform)card.transform;
            cardRT.anchorMin = Vector2.zero; cardRT.anchorMax = Vector2.one;
            cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;
            var cardImg = card.GetComponent<Image>();
            cardImg.sprite = pausePanelSprite != null ? pausePanelSprite : roundedLg;
            cardImg.type = pausePanelSprite != null ? Image.Type.Simple : Image.Type.Sliced;
            cardImg.preserveAspect = pausePanelSprite != null;
            cardImg.color = pausePanelSprite != null ? Color.white : new Color(0.20f, 0.27f, 0.55f, 1f);

            // Üst parıltı
            var cardShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            cardShine.transform.SetParent(card.transform, false);
            var csRT = (RectTransform)cardShine.transform;
            csRT.anchorMin = new Vector2(0, 0.55f); csRT.anchorMax = new Vector2(1, 1);
            csRT.offsetMin = new Vector2(15, 0); csRT.offsetMax = new Vector2(-15, -10);
            var csImg = cardShine.GetComponent<Image>();
            csImg.sprite = roundedLg; csImg.type = Image.Type.Sliced;
            csImg.color = new Color(1f, 1f, 1f, 0.10f);
            csImg.raycastTarget = false;
            cardShine.SetActive(false);

            // Title with strong outline + drop shadow
            var title = CreateText(card.transform, "Title", "DURDURULDU",
                new Vector2(0.17f, 0.82f), new Vector2(0.83f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 56, new Color(0.66f, 0.43f, 0.10f));
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 30f;
            title.fontSizeMax = 58f;
            title.outlineWidth = 0.30f;
            title.outlineColor = new Color(0.10f, 0.05f, 0.22f, 1f);
            var titleShadow = title.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            titleShadow.effectColor = new Color(0, 0, 0, 0.55f);
            titleShadow.effectDistance = new Vector2(3, -5);

            // 3 buton — modern glossy 3D, score card stilinde
            var resumeBtn = CreateModernPanelButton(card.transform, "ResumeButton", "DEVAM ET",
                new Vector2(0.12f, 0.56f), new Vector2(0.88f, 0.75f),
                new Color(0.30f, 0.78f, 0.36f, 1f), pauseGreen);
            var resumeHook = resumeBtn.gameObject.AddComponent<PauseResumeHook>();
            resumeHook.SetPanel(overlay);

            var replayBtn = CreateModernPanelButton(card.transform, "ReplayButton", "TEKRAR DENE",
                new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.53f),
                new Color(0.32f, 0.68f, 0.92f, 1f), pauseBlue);
            replayBtn.gameObject.AddComponent<PauseReplayHook>();

            var menuBtn = CreateModernPanelButton(card.transform, "MenuButton", "ANA MENÜ",
                new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.31f),
                new Color(0.85f, 0.42f, 0.28f, 1f), pauseRed);
            menuBtn.gameObject.AddComponent<PauseMenuHook>();

            overlay.SetActive(false);
            return overlay;
        }

        /// <summary>
        /// Glossy 3D modern buton (panel içlerinde kullanılır): shadow + border + body + shine + outline label.
        /// </summary>
        private static Button CreateModernPanelButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color color, Sprite customSprite = null)
        {
            var wrap = new GameObject(name + "Wrap", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrap.transform;
            wrt.anchorMin = anchorMin; wrt.anchorMax = anchorMax;
            wrt.offsetMin = wrt.offsetMax = Vector2.zero;

            // Drop shadow
            var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadow.transform.SetParent(wrap.transform, false);
            var srt = (RectTransform)shadow.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(-3, -14); srt.offsetMax = new Vector2(3, -4);
            var shadowImg = shadow.GetComponent<Image>();
            shadowImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            shadowImg.type = Image.Type.Sliced;
            shadowImg.color = new Color(0, 0, 0, 0.55f);
            shadowImg.raycastTarget = false;
            if (customSprite != null) shadow.SetActive(false);

            // Dark border
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(wrap.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var borderImg = border.GetComponent<Image>();
            borderImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            borderImg.type = Image.Type.Sliced;
            borderImg.color = Color.Lerp(color, Color.black, 0.55f);
            borderImg.raycastTarget = false;
            if (customSprite != null) border.SetActive(false);

            // Body
            var body = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            body.transform.SetParent(wrap.transform, false);
            var bodyRT = (RectTransform)body.transform;
            bodyRT.anchorMin = Vector2.zero; bodyRT.anchorMax = Vector2.one;
            bodyRT.offsetMin = customSprite != null ? Vector2.zero : new Vector2(7, 7);
            bodyRT.offsetMax = customSprite != null ? Vector2.zero : new Vector2(-7, -7);
            var bodyImg = body.GetComponent<Image>();
            bodyImg.sprite = customSprite != null ? customSprite : (roundedMd != null ? roundedMd : roundedSm);
            bodyImg.type = customSprite != null ? Image.Type.Simple : Image.Type.Sliced;
            bodyImg.preserveAspect = customSprite != null;
            bodyImg.color = customSprite != null ? Color.white : color;

            // Üst parıltı
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(body.transform, false);
            var sh = (RectTransform)shine.transform;
            sh.anchorMin = new Vector2(0, 0.55f); sh.anchorMax = new Vector2(1, 1);
            sh.offsetMin = new Vector2(8, 0); sh.offsetMax = new Vector2(-8, -5);
            var shineImg = shine.GetComponent<Image>();
            shineImg.sprite = roundedSm; shineImg.type = Image.Type.Sliced;
            shineImg.color = new Color(1f, 1f, 1f, 0.30f);
            shineImg.raycastTarget = false;
            if (customSprite != null) shine.SetActive(false);

            // Inner shade alt
            var innerShade = new GameObject("InnerShade", typeof(RectTransform), typeof(Image));
            innerShade.transform.SetParent(body.transform, false);
            var ish = (RectTransform)innerShade.transform;
            ish.anchorMin = new Vector2(0, 0); ish.anchorMax = new Vector2(1, 0.45f);
            ish.offsetMin = new Vector2(8, 5); ish.offsetMax = new Vector2(-8, 0);
            var innerImg = innerShade.GetComponent<Image>();
            innerImg.sprite = roundedSm; innerImg.type = Image.Type.Sliced;
            innerImg.color = new Color(0, 0, 0, 0.18f);
            innerImg.raycastTarget = false;
            if (customSprite != null) innerShade.SetActive(false);

            // Label
            var labelTxt = CreateText(body.transform, "Label", label,
                customSprite != null ? new Vector2(0.08f, 0.14f) : Vector2.zero,
                customSprite != null ? new Vector2(0.92f, 0.86f) : Vector2.one,
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center,
                customSprite != null ? 44 : 56, Color.white);
            labelTxt.fontStyle = FontStyles.Bold;
            if (customSprite != null)
            {
                labelTxt.enableAutoSizing = true;
                labelTxt.fontSizeMin = 24f;
                labelTxt.fontSizeMax = 46f;
            }
            labelTxt.outlineWidth = 0.30f;
            labelTxt.outlineColor = Color.Lerp(color, Color.black, 0.7f);
            labelTxt.raycastTarget = false; // tıklamayı bloke etmesin
            var labelShadow = labelTxt.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            labelShadow.effectColor = new Color(0, 0, 0, 0.55f);
            labelShadow.effectDistance = new Vector2(2, -3);

            return body.GetComponent<Button>();
        }

        private static void BuildPauseButton(Transform parent, GameObject pausePanel)
        {
            // Wrapper for shadow + border + body
            var wrap = new GameObject("PauseButton", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrap.transform;
            wrt.anchorMin = new Vector2(0, 1);
            wrt.anchorMax = new Vector2(0, 1);
            wrt.pivot = new Vector2(0, 1);
            wrt.sizeDelta = new Vector2(120, 120);
            wrt.anchoredPosition = new Vector2(28, -28);

            // Drop shadow
            var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadow.transform.SetParent(wrap.transform, false);
            var srt = (RectTransform)shadow.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(-2, -12);
            srt.offsetMax = new Vector2(2, -4);
            var shadowImg = shadow.GetComponent<Image>();
            shadowImg.sprite = circle;
            shadowImg.color = new Color(0, 0, 0, 0.45f);
            shadowImg.raycastTarget = false;

            // Dark outer border ring
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(wrap.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var borderImg = border.GetComponent<Image>();
            borderImg.sprite = circle;
            borderImg.color = new Color(0.06f, 0.20f, 0.10f, 1f);
            borderImg.raycastTarget = false;

            // Body (glossy green)
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(Button));
            body.transform.SetParent(wrap.transform, false);
            var bodyRT = (RectTransform)body.transform;
            bodyRT.anchorMin = Vector2.zero; bodyRT.anchorMax = Vector2.one;
            bodyRT.offsetMin = new Vector2(8, 8); bodyRT.offsetMax = new Vector2(-8, -8);
            var bodyImg = body.GetComponent<Image>();
            bodyImg.sprite = circle;
            bodyImg.color = new Color(0.20f, 0.78f, 0.30f, 1f);

            // Top shine
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(body.transform, false);
            var sh = (RectTransform)shine.transform;
            sh.anchorMin = new Vector2(0.15f, 0.55f);
            sh.anchorMax = new Vector2(0.85f, 0.92f);
            sh.offsetMin = sh.offsetMax = Vector2.zero;
            var shineImg = shine.GetComponent<Image>();
            shineImg.sprite = circle;
            shineImg.color = new Color(1f, 1f, 1f, 0.40f);
            shineImg.raycastTarget = false;

            // 2 dikey beyaz çizgi (pause ikonu)
            for (int i = 0; i < 2; i++)
            {
                var barGO = new GameObject($"Bar{i}", typeof(RectTransform), typeof(Image));
                barGO.transform.SetParent(body.transform, false);
                var bRT = (RectTransform)barGO.transform;
                bRT.anchorMin = new Vector2(0.5f, 0.5f);
                bRT.anchorMax = new Vector2(0.5f, 0.5f);
                bRT.pivot = new Vector2(0.5f, 0.5f);
                bRT.sizeDelta = new Vector2(16, 52);
                bRT.anchoredPosition = new Vector2(i == 0 ? -14 : 14, 0);
                var bi = barGO.GetComponent<Image>();
                bi.sprite = roundedXs; bi.type = Image.Type.Sliced;
                bi.color = Color.white;
                bi.raycastTarget = false;
            }

            var btn = body.GetComponent<Button>();
            var hook = body.gameObject.AddComponent<ButtonOpenTarget>();
            hook.Target = pausePanel;
        }

        private static GameObject BuildLevelSelectPanel(Transform parent, LevelDatabase db)
        {
            var overlay = CreateOverlay(parent, "LevelSelectPanel", tapOutsideToClose: true);

            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f), C_PanelBg);

            var title = CreateText(card.transform, "Title", "SEVİYE SEÇ",
                new Vector2(0.05f, 0.88f), new Vector2(0.85f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 76, C_Accent);
            title.fontStyle = FontStyles.Bold;
            title.outlineWidth = 0.28f;
            title.outlineColor = new Color(0.18f, 0.10f, 0.40f, 1f);
            var titleShadow = title.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            titleShadow.effectColor = new Color(0, 0, 0, 0.5f);
            titleShadow.effectDistance = new Vector2(2, -4);

            // X kapatma butonu (sağ-üst)
            BuildCloseXButton(card.transform, new Vector2(0.86f, 0.88f), new Vector2(0.96f, 0.97f), overlay);

            var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(RectMask2D));
            scrollGO.transform.SetParent(card.transform, false);
            var srt = (RectTransform)scrollGO.transform;
            srt.anchorMin = new Vector2(0.05f, 0.08f);
            srt.anchorMax = new Vector2(0.95f, 0.84f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            var sImg = scrollGO.GetComponent<Image>();
            sImg.sprite = roundedMd; sImg.type = Image.Type.Sliced;
            sImg.color = new Color(0,0,0,0.20f);
            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.horizontal = false;

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var vrt = (RectTransform)viewportGO.transform;
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = vrt.offsetMax = Vector2.zero;
            scroll.viewport = vrt;

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            var crt = (RectTransform)contentGO.transform;
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var glg = contentGO.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(230, 230);
            glg.spacing = new Vector2(28, 36);
            glg.padding = new RectOffset(28, 28, 36, 36);
            glg.childAlignment = TextAnchor.UpperCenter;
            var csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;

            var buttonPrefab = CreateLevelButtonPrefab();

            var lsUI = overlay.AddComponent<LevelSelectUI>();
            var so = new SerializedObject(lsUI);
            so.FindProperty("database").objectReferenceValue = db;
            so.FindProperty("buttonContainer").objectReferenceValue = contentGO.transform;
            so.FindProperty("buttonPrefab").objectReferenceValue = buttonPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        private static LevelButton CreateLevelButtonPrefab()
        {
            const string path = "Assets/Prefabs/LevelButton.prefab";

            // Root: invisible wrapper with Button component
            var root = new GameObject("LevelButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rootImg = root.GetComponent<Image>();
            rootImg.sprite = roundedMd;
            rootImg.type = Image.Type.Sliced;
            rootImg.color = new Color(0, 0, 0, 0); // şeffaf — sadece raycast için

            // 1) Gölge katmanı (alt offset)
            var shadow = new GameObject("Shadow", typeof(Image));
            shadow.transform.SetParent(root.transform, false);
            var shRT = (RectTransform)shadow.transform;
            shRT.anchorMin = Vector2.zero; shRT.anchorMax = Vector2.one;
            shRT.offsetMin = new Vector2(0, -8); shRT.offsetMax = new Vector2(0, -8);
            var shImg = shadow.GetComponent<Image>();
            shImg.sprite = roundedMd; shImg.type = Image.Type.Sliced;
            shImg.color = new Color(0, 0, 0, 0.35f);
            shImg.raycastTarget = false;

            // 2) Çerçeve (koyu kenar)
            var border = new GameObject("Border", typeof(Image));
            border.transform.SetParent(root.transform, false);
            var brRT = (RectTransform)border.transform;
            brRT.anchorMin = Vector2.zero; brRT.anchorMax = Vector2.one;
            brRT.offsetMin = brRT.offsetMax = Vector2.zero;
            var brImg = border.GetComponent<Image>();
            brImg.sprite = roundedMd; brImg.type = Image.Type.Sliced;
            brImg.color = new Color(0.62f, 0.18f, 0.22f, 1f);
            brImg.raycastTarget = false;

            // 3) Body (ana renk — glossy candy pembe)
            var body = new GameObject("Body", typeof(Image));
            body.transform.SetParent(root.transform, false);
            var bdRT = (RectTransform)body.transform;
            bdRT.anchorMin = Vector2.zero; bdRT.anchorMax = Vector2.one;
            bdRT.offsetMin = new Vector2(6, 6); bdRT.offsetMax = new Vector2(-6, -6);
            var bdImg = body.GetComponent<Image>();
            bdImg.sprite = roundedMd; bdImg.type = Image.Type.Sliced;
            bdImg.color = new Color(1f, 0.42f, 0.45f, 1f);
            bdImg.raycastTarget = false;

            // 4) Üst parlaklık (shine) - daha açık üst yarı
            var shine = new GameObject("Shine", typeof(Image));
            shine.transform.SetParent(root.transform, false);
            var snRT = (RectTransform)shine.transform;
            snRT.anchorMin = new Vector2(0.10f, 0.55f); snRT.anchorMax = new Vector2(0.90f, 0.88f);
            snRT.offsetMin = snRT.offsetMax = Vector2.zero;
            var snImg = shine.GetComponent<Image>();
            snImg.sprite = roundedMd; snImg.type = Image.Type.Sliced;
            snImg.color = new Color(1f, 1f, 1f, 0.32f);
            snImg.raycastTarget = false;

            // 5) Sayı yazısı — üstte, büyük & outlined
            var numText = CreateText(root.transform, "Number", "1",
                new Vector2(0, 0.35f), new Vector2(1, 0.92f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 96, Color.white);
            numText.fontStyle = FontStyles.Bold;
            numText.outlineWidth = 0.30f;
            numText.outlineColor = new Color(0.55f, 0.10f, 0.15f, 1f);
            numText.raycastTarget = false;
            var numShadow = numText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            numShadow.effectColor = new Color(0, 0, 0, 0.45f);
            numShadow.effectDistance = new Vector2(2, -3);

            // 6) 3 yıldız (alt) — büyük ve belirgin
            var stars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var s = new GameObject($"Star{i}", typeof(Image));
                s.transform.SetParent(root.transform, false);
                var srt = (RectTransform)s.transform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.18f);
                srt.sizeDelta = new Vector2(50, 50);
                srt.anchoredPosition = new Vector2((i - 1) * 52, 0);
                var im = s.GetComponent<Image>();
                im.sprite = star;
                im.color = C_StarOn;
                im.raycastTarget = false;
                // Yıldızlara hafif drop shadow
                var sShadow = s.AddComponent<UnityEngine.UI.Shadow>();
                sShadow.effectColor = new Color(0.40f, 0.20f, 0f, 0.55f);
                sShadow.effectDistance = new Vector2(1, -2);
                stars[i] = s;
            }

            // 7) Kilit overlay — locked durumda aktif olur, kilit ikonu içerir
            var locked = new GameObject("Lock", typeof(Image));
            locked.transform.SetParent(root.transform, false);
            var lockRT = (RectTransform)locked.transform;
            lockRT.anchorMin = Vector2.zero; lockRT.anchorMax = Vector2.one;
            lockRT.offsetMin = lockRT.offsetMax = Vector2.zero;
            var lockImg = locked.GetComponent<Image>();
            lockImg.sprite = roundedMd; lockImg.type = Image.Type.Sliced;
            lockImg.color = new Color(0.08f, 0.08f, 0.12f, 0.72f);
            lockImg.raycastTarget = false;

            // Kilit ikonu (procedural: gövde + sap)
            var padlock = new GameObject("Padlock", typeof(RectTransform));
            padlock.transform.SetParent(locked.transform, false);
            var pRT = (RectTransform)padlock.transform;
            pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
            pRT.sizeDelta = new Vector2(80, 80);
            pRT.anchoredPosition = Vector2.zero;

            // Sap (ring)
            var lockRing = new GameObject("Ring", typeof(Image));
            lockRing.transform.SetParent(padlock.transform, false);
            var lrRT = (RectTransform)lockRing.transform;
            lrRT.anchorMin = new Vector2(0.5f, 0.55f); lrRT.anchorMax = new Vector2(0.5f, 1f);
            lrRT.sizeDelta = new Vector2(42, 36);
            lrRT.anchoredPosition = Vector2.zero;
            var lrImg = lockRing.GetComponent<Image>();
            lrImg.sprite = roundedSm; lrImg.type = Image.Type.Sliced;
            lrImg.color = new Color(0.85f, 0.85f, 0.88f, 1f);
            lrImg.raycastTarget = false;

            // Sap iç boşluk
            var lockRingHole = new GameObject("RingHole", typeof(Image));
            lockRingHole.transform.SetParent(lockRing.transform, false);
            var lrhRT = (RectTransform)lockRingHole.transform;
            lrhRT.anchorMin = Vector2.zero; lrhRT.anchorMax = Vector2.one;
            lrhRT.offsetMin = new Vector2(8, 8); lrhRT.offsetMax = new Vector2(-8, 4);
            var lrhImg = lockRingHole.GetComponent<Image>();
            lrhImg.sprite = roundedSm; lrhImg.type = Image.Type.Sliced;
            lrhImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            lrhImg.raycastTarget = false;

            // Kilit gövdesi
            var lockBody = new GameObject("Body", typeof(Image));
            lockBody.transform.SetParent(padlock.transform, false);
            var lbRT = (RectTransform)lockBody.transform;
            lbRT.anchorMin = new Vector2(0.1f, 0f); lbRT.anchorMax = new Vector2(0.9f, 0.55f);
            lbRT.offsetMin = lbRT.offsetMax = Vector2.zero;
            var lbImg = lockBody.GetComponent<Image>();
            lbImg.sprite = roundedSm; lbImg.type = Image.Type.Sliced;
            lbImg.color = new Color(1f, 0.82f, 0.18f, 1f);
            lbImg.raycastTarget = false;

            // Button setup
            var btn = root.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f);
            btn.colors = colors;

            var lb = root.AddComponent<LevelButton>();
            var so = new SerializedObject(lb);
            so.FindProperty("button").objectReferenceValue = btn;
            so.FindProperty("numberText").objectReferenceValue = numText;
            so.FindProperty("lockedIcon").objectReferenceValue = locked;
            var arr = so.FindProperty("starIcons");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<LevelButton>();
        }

        private static GameObject BuildSettingsPanel(Transform parent)
        {
            var overlay = CreateOverlay(parent, "SettingsPanel", tapOutsideToClose: true);
            var settingsPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/panel_pause_v2.png");
            var settingsBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_blue_v2.png");
            var settingsGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_green_v2.png");
            var settingsRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pausebtn_red_v2.png");
            var card = CreateRoundedCard(overlay.transform, "Card",
                new Vector2(0.11f, 0.22f), new Vector2(0.89f, 0.78f), C_PanelBg);
            var cardImage = card.GetComponent<Image>();
            if (settingsPanelSprite != null)
            {
                cardImage.sprite = settingsPanelSprite;
                cardImage.type = Image.Type.Simple;
                cardImage.preserveAspect = true;
                cardImage.color = Color.white;
            }

            var title = CreateText(card.transform, "Title", "AYARLAR",
                new Vector2(0.17f, 0.82f), new Vector2(0.83f, 0.96f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 56, new Color(0.66f, 0.43f, 0.10f));
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 30f;
            title.fontSizeMax = 58f;
            title.outlineWidth = 0.28f;
            title.outlineColor = new Color(0.18f, 0.10f, 0.40f, 1f);
            var settingsTitleShadow = title.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            settingsTitleShadow.effectColor = new Color(0, 0, 0, 0.5f);
            settingsTitleShadow.effectDistance = new Vector2(2, -4);

            // Modern X kapatma butonu (kırmızı glossy daire)
            BuildCloseXButton(card.transform, new Vector2(0.82f, 0.88f), new Vector2(0.95f, 0.98f), overlay);

            CreateText(card.transform, "MusicLabel", "Müzik",
                new Vector2(0.12f, 0.68f), new Vector2(0.27f, 0.74f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 34, new Color(0.45f, 0.30f, 0.15f));
            var musicSlider = CreateSlider(card.transform, "MusicSlider",
                new Vector2(0.29f, 0.68f), new Vector2(0.82f, 0.74f));

            CreateText(card.transform, "SfxLabel", "Efektler",
                new Vector2(0.12f, 0.57f), new Vector2(0.27f, 0.63f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 34, new Color(0.45f, 0.30f, 0.15f));
            var sfxSlider = CreateSlider(card.transform, "SfxSlider",
                new Vector2(0.29f, 0.57f), new Vector2(0.82f, 0.63f));

            // Dil seçimi (TR / EN toggle) — modern glossy buton
            CreateText(card.transform, "DilCaption", "Dil",
                new Vector2(0.12f, 0.43f), new Vector2(0.25f, 0.49f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 34, new Color(0.45f, 0.30f, 0.15f));
            var langBtn = CreateModernPanelButton(card.transform, "LangButton", "TR  ⇄  EN",
                new Vector2(0.29f, 0.39f), new Vector2(0.83f, 0.52f),
                new Color(0.32f, 0.68f, 0.92f, 1f), settingsBlue);
            langBtn.gameObject.AddComponent<LanguageToggleHook>();

            // KAPAT (yeşil)
            var closeBtn = CreateModernPanelButton(card.transform, "CloseButton", "KAPAT",
                new Vector2(0.24f, 0.20f), new Vector2(0.77f, 0.35f),
                new Color(0.30f, 0.78f, 0.36f, 1f), settingsGreen);
            var settingsCloser = closeBtn.gameObject.AddComponent<ButtonCloseTarget>();
            settingsCloser.Target = overlay;

            // GM/development shortcut: keep the label visible, but do not expose a clickable control.
            var unlockBtn = CreateModernPanelButton(card.transform, "UnlockAllButton", "TÜM SEVİYELERİ AÇ",
                new Vector2(0.24f, 0.10f), new Vector2(0.77f, 0.24f),
                new Color(0.90f, 0.25f, 0.28f, 1f), settingsRed);
            var unlockWrap = unlockBtn.transform.parent as RectTransform;
            if (unlockWrap != null)
            {
                unlockWrap.gameObject.SetActive(true);
                unlockWrap.anchorMin = new Vector2(0.24f, 0.145f);
                unlockWrap.anchorMax = new Vector2(0.77f, 0.185f);
                unlockWrap.offsetMin = Vector2.zero;
                unlockWrap.offsetMax = Vector2.zero;
            }

            var unlockBody = unlockBtn.transform as RectTransform;
            if (unlockBody != null)
            {
                unlockBody.anchorMin = Vector2.zero;
                unlockBody.anchorMax = Vector2.one;
                unlockBody.offsetMin = Vector2.zero;
                unlockBody.offsetMax = Vector2.zero;
            }

            var unlockImage = unlockBtn.GetComponent<Image>();
            if (unlockImage != null)
            {
                unlockImage.color = Color.clear;
                unlockImage.preserveAspect = false;
            }
            unlockBtn.interactable = false;

            var unlockLabel = unlockBtn.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (unlockLabel != null)
            {
                var labelRect = unlockLabel.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                unlockLabel.color = new Color(0.66f, 0.28f, 0.18f, 0.92f);
                unlockLabel.fontSize = 24f;
                unlockLabel.enableAutoSizing = true;
                unlockLabel.fontSizeMin = 16f;
                unlockLabel.fontSizeMax = 26f;
            }

            // İLERLEMEYİ SIFIRLA — sade ghost (tehlikeli, küçük)
            var resetBtn = CreateGhostButton(card.transform, "ResetButton", "İLERLEMEYİ SIFIRLA",
                new Vector2(0.22f, 0.09f), new Vector2(0.78f, 0.13f));
            var resetBody = resetBtn.transform;
            if (resetBody != null)
            {
                var resetBodyImage = resetBody.GetComponent<Image>();
                if (resetBodyImage != null) resetBodyImage.color = Color.clear;
                var resetLabel = resetBody.Find("Label") != null ? resetBody.Find("Label").GetComponent<TMP_Text>() : null;
                if (resetLabel != null)
                {
                    resetLabel.color = new Color(0.62f, 0.30f, 0.20f, 0.90f);
                    resetLabel.fontSize = 24f;
                    resetLabel.enableAutoSizing = true;
                    resetLabel.fontSizeMin = 16f;
                    resetLabel.fontSizeMax = 26f;
                }
            }

            var settings = overlay.AddComponent<SettingsPanel>();
            var so = new SerializedObject(settings);
            so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("resetProgressButton").objectReferenceValue = resetBtn;
            so.FindProperty("unlockAllButton").objectReferenceValue = unlockBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject CreateStatCard(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, string label, Color color, out TMP_Text valueText)
        {
            // Wrapper (drop shadow için)
            var wrap = new GameObject(name + "Wrap", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrap.transform;
            wrt.anchorMin = anchorMin; wrt.anchorMax = anchorMax;
            wrt.offsetMin = wrt.offsetMax = Vector2.zero;

            // Drop shadow
            var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadow.transform.SetParent(wrap.transform, false);
            var srt = (RectTransform)shadow.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(-3, -14);
            srt.offsetMax = new Vector2(3, -6);
            var shadowImg = shadow.GetComponent<Image>();
            shadowImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            shadowImg.type = Image.Type.Sliced;
            shadowImg.color = new Color(0, 0, 0, 0.45f);
            shadowImg.raycastTarget = false;

            // Dark border
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(wrap.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var borderImg = border.GetComponent<Image>();
            borderImg.sprite = roundedMd != null ? roundedMd : roundedSm;
            borderImg.type = Image.Type.Sliced;
            borderImg.color = Color.Lerp(color, Color.black, 0.65f);
            borderImg.raycastTarget = false;

            // Body fill
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(wrap.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6, 6); rt.offsetMax = new Vector2(-6, -6);
            var img = go.GetComponent<Image>();
            img.sprite = roundedSm; img.type = Image.Type.Sliced;
            img.color = color;

            // Üst parıltı (glossy 3D)
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(go.transform, false);
            var sh = (RectTransform)shine.transform;
            sh.anchorMin = new Vector2(0, 0.55f);
            sh.anchorMax = new Vector2(1, 1);
            sh.offsetMin = new Vector2(8, 0);
            sh.offsetMax = new Vector2(-8, -4);
            var shineImg = shine.GetComponent<Image>();
            shineImg.sprite = roundedSm;
            shineImg.type = Image.Type.Sliced;
            shineImg.color = new Color(1f, 1f, 1f, 0.30f);
            shineImg.raycastTarget = false;

            // Alt inner shade
            var innerShade = new GameObject("InnerShade", typeof(RectTransform), typeof(Image));
            innerShade.transform.SetParent(go.transform, false);
            var ish = (RectTransform)innerShade.transform;
            ish.anchorMin = new Vector2(0, 0);
            ish.anchorMax = new Vector2(1, 0.45f);
            ish.offsetMin = new Vector2(8, 4);
            ish.offsetMax = new Vector2(-8, 0);
            var innerImg = innerShade.GetComponent<Image>();
            innerImg.sprite = roundedSm;
            innerImg.type = Image.Type.Sliced;
            innerImg.color = new Color(0f, 0f, 0f, 0.18f);
            innerImg.raycastTarget = false;

            var labelText = CreateText(go.transform, "Label", label,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.95f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 30, new Color(1f, 1f, 1f, 0.95f));
            labelText.fontStyle = FontStyles.Bold;
            labelText.outlineWidth = 0.22f;
            labelText.outlineColor = Color.Lerp(color, Color.black, 0.7f);

            valueText = CreateText(go.transform, "Value", "0",
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.58f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 62, Color.white);
            valueText.fontStyle = FontStyles.Bold;
            valueText.outlineWidth = 0.28f;
            valueText.outlineColor = Color.Lerp(color, Color.black, 0.7f);
            var valShadow = valueText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            valShadow.effectColor = new Color(0, 0, 0, 0.55f);
            valShadow.effectDistance = new Vector2(2, -3);

            return go;
        }

        private static void BuildBoosterPanel(Transform parent)
        {
            var card = new GameObject("BoosterCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            var crt = (RectTransform)card.transform;
            crt.anchorMin = new Vector2(0.03f, 0.03f);
            crt.anchorMax = new Vector2(0.97f, 0.17f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var cardImg = card.GetComponent<Image>();
            cardImg.sprite = roundedSm;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0f, 0f, 0f, 0.42f);

            UnityEngine.UI.Button hammerBtn = null, shuffleBtn = null, moveBtn = null;
            TMP_Text hammerCount = null, shuffleCount = null, moveCount = null;

            float gap = 0.02f;
            float slot = (1f - gap * 4f) / 3f;
            float x0 = gap;

            var hammerIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_hammer.png");
            var shuffleIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_shuffle.png");
            var plusIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_plus.png");

            hammerBtn = MakeBoosterButton(card.transform, "Hammer", "ÇEKİÇ", hammerIcon, new Color(0.96f, 0.56f, 0.28f),
                new Vector2(x0, 0.12f), new Vector2(x0 + slot, 0.88f), out hammerCount);
            x0 += slot + gap;
            shuffleBtn = MakeBoosterButton(card.transform, "Shuffle", "KARIŞTIR", shuffleIcon, new Color(0.30f, 0.78f, 0.75f),
                new Vector2(x0, 0.12f), new Vector2(x0 + slot, 0.88f), out shuffleCount);
            x0 += slot + gap;
            moveBtn = MakeBoosterButton(card.transform, "MovePack", "+5 HAMLE", plusIcon, new Color(0.66f, 0.38f, 0.93f),
                new Vector2(x0, 0.12f), new Vector2(x0 + slot, 0.88f), out moveCount);

            var panel = card.AddComponent<BoosterPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("hammerButton").objectReferenceValue = hammerBtn;
            so.FindProperty("shuffleButton").objectReferenceValue = shuffleBtn;
            so.FindProperty("movePackButton").objectReferenceValue = moveBtn;
            so.FindProperty("hammerCount").objectReferenceValue = hammerCount;
            so.FindProperty("shuffleCount").objectReferenceValue = shuffleCount;
            so.FindProperty("movePackCount").objectReferenceValue = moveCount;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UnityEngine.UI.Button MakeBoosterButton(Transform parent, string name, string label, Sprite iconSprite, Color color,
            Vector2 anchorMin, Vector2 anchorMax, out TMP_Text countText)
        {
            var wrapper = new GameObject(name, typeof(RectTransform));
            wrapper.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrapper.transform;
            wrt.anchorMin = anchorMin; wrt.anchorMax = anchorMax;
            wrt.offsetMin = wrt.offsetMax = Vector2.zero;

            var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadow.transform.SetParent(wrapper.transform, false);
            var sht = (RectTransform)shadow.transform;
            sht.anchorMin = Vector2.zero; sht.anchorMax = Vector2.one;
            sht.offsetMin = new Vector2(0, -12);
            sht.offsetMax = new Vector2(0, -6);
            var shi = shadow.GetComponent<Image>();
            shi.sprite = roundedXs; shi.type = Image.Type.Sliced;
            shi.color = Color.Lerp(color, Color.black, 0.6f);
            shi.raycastTarget = false;

            var go = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(wrapper.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = roundedXs;
            img.type = Image.Type.Sliced;
            img.color = color;

            var topHighlight = new GameObject("TopShine", typeof(RectTransform), typeof(Image));
            topHighlight.transform.SetParent(go.transform, false);
            var thrt = (RectTransform)topHighlight.transform;
            thrt.anchorMin = new Vector2(0.06f, 0.55f);
            thrt.anchorMax = new Vector2(0.94f, 0.94f);
            thrt.offsetMin = thrt.offsetMax = Vector2.zero;
            var thi = topHighlight.GetComponent<Image>();
            thi.sprite = roundedXs; thi.type = Image.Type.Sliced;
            thi.color = new Color(1f, 1f, 1f, 0.30f);
            thi.raycastTarget = false;

            var bottomShade = new GameObject("BottomShade", typeof(RectTransform), typeof(Image));
            bottomShade.transform.SetParent(go.transform, false);
            var bsrt = (RectTransform)bottomShade.transform;
            bsrt.anchorMin = new Vector2(0.06f, 0.06f);
            bsrt.anchorMax = new Vector2(0.94f, 0.30f);
            bsrt.offsetMin = bsrt.offsetMax = Vector2.zero;
            var bsi = bottomShade.GetComponent<Image>();
            bsi.sprite = roundedXs; bsi.type = Image.Type.Sliced;
            bsi.color = new Color(0f, 0f, 0f, 0.18f);
            bsi.raycastTarget = false;

            if (iconSprite != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(go.transform, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.5f, 0.62f);
                iconRT.anchorMax = new Vector2(0.5f, 0.62f);
                iconRT.sizeDelta = new Vector2(110, 110);
                iconRT.anchoredPosition = Vector2.zero;
                var iconImg = iconGO.GetComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
            }

            var labelText = CreateText(go.transform, "Label", label,
                new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.30f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 36, Color.white);
            labelText.fontStyle = FontStyles.Bold;
            labelText.outlineWidth = 0.22f;
            labelText.outlineColor = new Color(0, 0, 0, 0.7f);

            // Booster sprites already contain the translucent circle in their
            // top-right corner. Only overlay the inventory count on that circle.
            countText = CreateText(go.transform, "Count", "1",
                new Vector2(0.695f, 0.762f), new Vector2(0.875f, 0.942f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 26,
                new Color(0.24f, 0.16f, 0.28f));
            countText.fontStyle = FontStyles.Bold;
            countText.enableAutoSizing = true;
            countText.fontSizeMin = 12;
            countText.fontSizeMax = 26;

            return go.GetComponent<UnityEngine.UI.Button>();
        }

        private static void BuildComboTextOverlay(Transform parent)
        {
            var go = new GameObject("ComboText", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.1f, 0.55f);
            rt.anchorMax = new Vector2(0.9f, 0.65f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false;

            var tmp = CreateText(go.transform, "Label", "COMBO 2x!",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 130, new Color(1f, 0.83f, 0.24f));
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color(0.55f, 0.05f, 0.05f);

            var combo = go.AddComponent<ComboText>();
            var so = new SerializedObject(combo);
            so.FindProperty("text").objectReferenceValue = tmp;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateOverlay(Transform parent, string name, bool tapOutsideToClose = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.72f);

            go.AddComponent<PanelAnimator>();

            if (tapOutsideToClose)
            {
                var btn = go.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                var closer = go.AddComponent<ButtonCloseTarget>();
                closer.Target = go;
            }
            return go;
        }

        private static GameObject CreateRoundedCard(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color fill)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = roundedLg;
            img.type = Image.Type.Sliced;
            img.color = fill;
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            TextAlignmentOptions alignment, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private static Button CreatePrimaryButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateStyledButton(parent, name, label, anchorMin, anchorMax,
                C_Primary, Color.white, 56, true);
        }

        private static Button CreateSecondaryButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateStyledButton(parent, name, label, anchorMin, anchorMax,
                C_Secondary, Color.white, 48, true);
        }

        private static Button CreateGhostButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateStyledButton(parent, name, label, anchorMin, anchorMax,
                new Color(1f, 1f, 1f, 0.12f), C_TextLight, 42, false);
        }

        private static Button CreateStyledButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Color textColor, int textSize, bool bold)
        {
            var wrapper = new GameObject(name, typeof(RectTransform));
            wrapper.transform.SetParent(parent, false);
            var wrt = (RectTransform)wrapper.transform;
            wrt.anchorMin = anchorMin; wrt.anchorMax = anchorMax;
            wrt.offsetMin = wrt.offsetMax = Vector2.zero;

            var btnGO = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(wrapper.transform, false);
            var brt = (RectTransform)btnGO.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var img = btnGO.GetComponent<Image>();
            // Yeni glossy cartoon buton sprite'ı (içinde drop shadow + gloss + border var)
            if (buttonPrimary != null)
            {
                img.sprite = buttonPrimary;
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.sprite = roundedMd != null ? roundedMd : roundedXs;
                img.type = Image.Type.Sliced;
            }
            img.color = bgColor;

            // (Üst parıltı overlay kapatıldı — button_primary sprite'ı zaten gloss içeriyor)

            var tmp = CreateText(btnGO.transform, "Label", label,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, textSize, textColor);
            if (bold) tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color(0, 0, 0, 0.75f);
            var labelShadow = tmp.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            labelShadow.effectColor = new Color(0, 0, 0, 0.55f);
            labelShadow.effectDistance = new Vector2(3, -4);

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            return btn;
        }

        private static Button CreateCircleButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = circle;
            img.color = new Color(1f, 1f, 1f, 0.15f);

            var tmp = CreateText(go.transform, "Label", label,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 50, C_TextLight);
            tmp.fontStyle = FontStyles.Bold;

            return go.GetComponent<Button>();
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Track shadow (alt offset)
            var trackShadow = new GameObject("TrackShadow", typeof(RectTransform), typeof(Image));
            trackShadow.transform.SetParent(go.transform, false);
            var tsRT = (RectTransform)trackShadow.transform;
            tsRT.anchorMin = new Vector2(0, 0.30f); tsRT.anchorMax = new Vector2(1, 0.60f);
            tsRT.offsetMin = new Vector2(0, -6); tsRT.offsetMax = new Vector2(0, -2);
            var tsImg = trackShadow.GetComponent<Image>();
            tsImg.sprite = roundedSm; tsImg.type = Image.Type.Sliced;
            tsImg.color = new Color(0, 0, 0, 0.45f);
            tsImg.raycastTarget = false;

            // Track border (koyu)
            var trackBorder = new GameObject("TrackBorder", typeof(RectTransform), typeof(Image));
            trackBorder.transform.SetParent(go.transform, false);
            var tbRT = (RectTransform)trackBorder.transform;
            tbRT.anchorMin = new Vector2(0, 0.30f); tbRT.anchorMax = new Vector2(1, 0.60f);
            tbRT.offsetMin = tbRT.offsetMax = Vector2.zero;
            var tbImg = trackBorder.GetComponent<Image>();
            tbImg.sprite = roundedSm; tbImg.type = Image.Type.Sliced;
            tbImg.color = new Color(0.10f, 0.05f, 0.20f, 1f);
            tbImg.raycastTarget = false;

            // Track background (koyu mavi)
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var brt = (RectTransform)bg.transform;
            brt.anchorMin = new Vector2(0, 0.30f); brt.anchorMax = new Vector2(1, 0.60f);
            brt.offsetMin = new Vector2(4, 4); brt.offsetMax = new Vector2(-4, -4);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = roundedSm; bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.20f, 0.16f, 0.36f, 1f);

            // Fill area (parlak gradient)
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fart = (RectTransform)fillArea.transform;
            fart.anchorMin = new Vector2(0, 0.30f); fart.anchorMax = new Vector2(1, 0.60f);
            fart.offsetMin = new Vector2(8, 4); fart.offsetMax = new Vector2(-8, -4);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = roundedSm; fillImg.type = Image.Type.Sliced;
            fillImg.color = new Color(0.30f, 0.78f, 0.95f, 1f); // parlak cyan-mavi
            var frt = (RectTransform)fill.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = frt.offsetMax = Vector2.zero;

            // Handle area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var hart = (RectTransform)handleArea.transform;
            hart.anchorMin = new Vector2(0, 0); hart.anchorMax = new Vector2(1, 1);
            hart.offsetMin = new Vector2(20, 0); hart.offsetMax = new Vector2(-20, 0);

            // Handle: shadow + border + body + shine — glossy 3D top
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hrt = (RectTransform)handle.transform;
            hrt.sizeDelta = new Vector2(70, 70);

            // Handle shadow
            var hShadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            hShadow.transform.SetParent(handle.transform, false);
            var hsRT = (RectTransform)hShadow.transform;
            hsRT.anchorMin = Vector2.zero; hsRT.anchorMax = Vector2.one;
            hsRT.offsetMin = new Vector2(-2, -8); hsRT.offsetMax = new Vector2(2, -2);
            var hsImg = hShadow.GetComponent<Image>();
            hsImg.sprite = circle;
            hsImg.color = new Color(0, 0, 0, 0.55f);
            hsImg.raycastTarget = false;

            // Handle border
            var hBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
            hBorder.transform.SetParent(handle.transform, false);
            var hbRT = (RectTransform)hBorder.transform;
            hbRT.anchorMin = Vector2.zero; hbRT.anchorMax = Vector2.one;
            hbRT.offsetMin = hbRT.offsetMax = Vector2.zero;
            var hbImg = hBorder.GetComponent<Image>();
            hbImg.sprite = circle;
            hbImg.color = new Color(0.10f, 0.05f, 0.25f, 1f);
            hbImg.raycastTarget = false;

            // Handle body (button target)
            var hBody = new GameObject("Body", typeof(RectTransform), typeof(Image));
            hBody.transform.SetParent(handle.transform, false);
            var hbdRT = (RectTransform)hBody.transform;
            hbdRT.anchorMin = Vector2.zero; hbdRT.anchorMax = Vector2.one;
            hbdRT.offsetMin = new Vector2(5, 5); hbdRT.offsetMax = new Vector2(-5, -5);
            var hbdImg = hBody.GetComponent<Image>();
            hbdImg.sprite = circle;
            hbdImg.color = Color.white;

            // Handle shine
            var hShine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            hShine.transform.SetParent(hBody.transform, false);
            var hshRT = (RectTransform)hShine.transform;
            hshRT.anchorMin = new Vector2(0.18f, 0.50f); hshRT.anchorMax = new Vector2(0.82f, 0.92f);
            hshRT.offsetMin = hshRT.offsetMax = Vector2.zero;
            var hshImg = hShine.GetComponent<Image>();
            hshImg.sprite = circle;
            hshImg.color = new Color(1f, 1f, 1f, 0.55f);
            hshImg.raycastTarget = false;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = frt;
            slider.handleRect = hrt;
            slider.targetGraphic = hbdImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.value = 0.7f;

            return slider;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
#endif
