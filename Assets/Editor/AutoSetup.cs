#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TMPro;
using BalloonPop.Audio;
using BalloonPop.Core;
using BalloonPop.Data;
using BalloonPop.Effects;
using BalloonPop.Gameplay;
using BalloonPop.Grid;
using BalloonPop.InputSystem;
using BalloonPop.UI;

namespace BalloonPop.EditorTools
{
    [InitializeOnLoad]
    public static class AutoSetup
    {
        private const string SetupMarkerKey  = "BalloonPop_AutoSetupDone_v51";
        private const string PrefabPath      = "Assets/Prefabs/Balloon.prefab";
        private const string LevelDbPath     = "Assets/Resources/LevelDatabase.asset";
        private const string MenuScenePath   = "Assets/Scenes/MainMenu.unity";
        private const string GameScenePath   = "Assets/Scenes/Game.unity";

        static AutoSetup()
        {
            EditorApplication.delayCall += MaybeRunSetup;
        }

        [MenuItem("BalloonPop/Run Full Auto-Setup")]
        public static void ForceSetup()
        {
            EditorPrefs.DeleteKey(SetupMarkerKey);
            MaybeRunSetup();
        }

        public static void BatchSetup()
        {
            EditorPrefs.DeleteKey(SetupMarkerKey);
            RunSetupNow();
        }

        public static void BatchSliceAndSetup()
        {
            if (System.IO.File.Exists("Assets/Sprites/balloons_sheet.png"))
                BalloonSheetSlicer.Slice();
            if (System.IO.File.Exists("Assets/Sprites/icons_sheet.png"))
                IconSheetSlicer.Slice();
            if (System.IO.File.Exists("Assets/Sprites/menu_icons_sheet.png"))
                MenuIconSheetSlicer.Slice();
            EditorPrefs.DeleteKey(SetupMarkerKey);
            RunSetupNow();
        }

        public static void BatchSetupWithTMP()
        {
            TMPEssentialsImporter.ImportEssentials();
            AssetDatabase.Refresh();
            EditorPrefs.DeleteKey(SetupMarkerKey);
            RunSetupNow();
        }

        private static void RunSetupNow()
        {
            try
            {
                Debug.Log("[BalloonPop] Batch setup starting...");

                EnsureFolders();
                BalloonSpriteGenerator.Generate();
                UISpriteGenerator.Generate();
                BackgroundSpriteGenerator.Generate();
                // GPT'den gelen pop sprite'larının beyaz arka planını alpha'ya çevir
                PopSpriteAlphaFix.BatchFix();
                // Yeni glossy buton sprite'larının import ayarlarını uygula (9-slice border, PPU)
                ButtonSpriteImporter.BatchConfigure();
                // User-supplied btn_*.png sprite'larının etrafındaki gradient bg'yi crop ile at
                ButtonAutoCrop.BatchRun();
                // Alt 4 mini butonu eşit kare boyuta normalize et
                MiniButtonNormalize.BatchRun();
                CreateSampleLevels();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDbPath);
                if (db == null) throw new System.Exception("LevelDatabase asset could not be loaded after creation.");

                var prefab = CreateBalloonPrefab();
                var boomPrefab = CreateBoomEffectPrefab();
                var particlePrefab = CreatePopParticlePrefab();
                var scorePopupPrefab = CreateScorePopupPrefab();
                var flashPrefab = CreateFlashPrefab();
                var shockwavePrefab = CreateShockwavePrefab();
                var handPrefab = CreateHandIconPrefab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDbPath);
                CreateGameScene(prefab, db, boomPrefab, particlePrefab, scorePopupPrefab, flashPrefab, shockwavePrefab, handPrefab);

                db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDbPath);
                CreateMainMenuScene(db);

                AddScenesToBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorPrefs.SetBool(SetupMarkerKey, true);
                Debug.Log("[BalloonPop] Batch setup complete!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BalloonPop] Batch setup failed: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private static void MaybeRunSetup()
        {
            if (EditorPrefs.GetBool(SetupMarkerKey, false)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += MaybeRunSetup;
                return;
            }

            try
            {
                RunSetupNow();
                EditorSceneManager.OpenScene(MenuScenePath);
                Debug.Log("[BalloonPop] Auto-setup tamamlandı! MainMenu sahnesi açıldı, Play'e bas.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BalloonPop] Auto-setup başarısız: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void EnsureFolders()
        {
            string[] folders = {
                "Assets/Prefabs", "Assets/Scenes", "Assets/Sprites",
                "Assets/Audio", "Assets/Materials",
                "Assets/ScriptableObjects", "Assets/ScriptableObjects/Levels",
                "Assets/Resources"
            };
            foreach (var f in folders)
            {
                if (!AssetDatabase.IsValidFolder(f))
                {
                    Directory.CreateDirectory(f);
                }
            }
            AssetDatabase.Refresh();
        }

        private static LevelDatabase CreateSampleLevels()
        {
            SampleLevelCreator.CreateSamples();
            return AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDbPath);
        }

        private static GameObject CreateBalloonPrefab()
        {
            // Eski prefab'ı sil (sprite GUID'leri değişmiş olabilir → NULL referans riski).
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            var go = new GameObject("Balloon");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Balloons";

            var balloon = go.AddComponent<Balloon>();
            go.AddComponent<BalloonPop.Effects.BalloonIdleBob>();

            var sprites = new Sprite[7];
            string[] names = { "balloon_red","balloon_blue","balloon_green","balloon_yellow",
                               "balloon_purple","balloon_orange","balloon_pink" };
            for (int i = 0; i < names.Length; i++)
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{names[i]}.png");

            var bombSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/balloon_bomb.png");
            var lineHSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/balloon_lineH.png");
            var lineVSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/balloon_lineV.png");
            var rainbowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/balloon_rainbow.png");
            var goldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/balloon_gold.png");

            var so = new SerializedObject(balloon);
            var colorArr = so.FindProperty("colorSprites");
            colorArr.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                colorArr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.FindProperty("bombSprite").objectReferenceValue = bombSprite;
            so.FindProperty("lineHSprite").objectReferenceValue = lineHSprite;
            so.FindProperty("lineVSprite").objectReferenceValue = lineVSprite;
            so.FindProperty("rainbowSprite").objectReferenceValue = rainbowSprite;
            so.FindProperty("goldSprite").objectReferenceValue = goldSprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBoomEffectPrefab()
        {
            const string path = "Assets/Prefabs/BoomText.prefab";

            var go = new GameObject("BoomText");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = "BOOM!";
            // AUTO-SIZE: max 4.5'te başlar, taşma riski varsa min 2.4'e kadar küçülür.
            // Kamera orthoSize=7, viewport genişliği ≈ 14 birim → autosize 4.5 × peak 1.2 ≈ 5.4 birim,
            // "AMAZING!" gibi uzun kelimeler de rahat sığar.
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 2.4f;
            tmp.fontSizeMax = 4.5f;
            tmp.fontSize = 4.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 4f;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            // Şık vertex gradient: üstte parlak altın, altta turuncu (TMP'nin premium görünümü)
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(
                new Color(1.00f, 0.96f, 0.55f),   // top-left  açık altın
                new Color(1.00f, 0.96f, 0.55f),   // top-right
                new Color(1.00f, 0.55f, 0.10f),   // bot-left  turuncu
                new Color(1.00f, 0.55f, 0.10f)    // bot-right
            );

            // Daha güçlü dış çizgi + daha kalın font feel
            tmp.outlineWidth = 0.32f;
            tmp.outlineColor = new Color(0.30f, 0.05f, 0.10f, 1f);

            // Rect boyutu sınırlı tut — viewport'a sığsın (rect'i sabit yap, içerik scale ile büyüsün)
            var rt = tmp.rectTransform;
            rt.sizeDelta = new Vector2(11f, 3.5f);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) { mr.sortingOrder = 100; mr.sortingLayerName = "Default"; }

            var effect = go.AddComponent<BoomEffect>();
            var so = new SerializedObject(effect);
            so.FindProperty("text").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePopParticlePrefab()
        {
            const string path = "Assets/Prefabs/PopParticle.prefab";
            const string matPath = "Assets/Materials/PopParticleMat.mat";

            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_circle.png");
            var spritesShader = Shader.Find("Sprites/Default");

            // Material asset olarak diske kaydet (prefab in-memory material'i serialize etmiyor)
            var matDir = System.IO.Path.GetDirectoryName(matPath);
            if (!AssetDatabase.IsValidFolder(matDir))
            {
                System.IO.Directory.CreateDirectory(matDir);
                AssetDatabase.Refresh();
            }
            var matAsset = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (matAsset == null)
            {
                matAsset = new Material(spritesShader);
                if (circleSprite != null) matAsset.mainTexture = circleSprite.texture;
                AssetDatabase.CreateAsset(matAsset, matPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                matAsset.shader = spritesShader;
                if (circleSprite != null) matAsset.mainTexture = circleSprite.texture;
                EditorUtility.SetDirty(matAsset);
            }

            var go = new GameObject("PopParticle");

            // ─── KATMAN 1: ANA RENKLI BURST (büyük, hızlı, balon rengiyle boyanır) ───
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.6f;
            main.playOnAwake = true;
            main.startColor = Color.white;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.20f;

            var velOverLife = ps.velocityOverLifetime;
            velOverLife.enabled = true;
            velOverLife.radial = new ParticleSystem.MinMaxCurve(2.0f);

            var rotOverLife = ps.rotationOverLifetime;
            rotOverLife.enabled = true;
            rotOverLife.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.95f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var sizeCurve = new AnimationCurve(new Keyframe(0, 1.2f), new Keyframe(0.4f, 1.0f), new Keyframe(1, 0f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = matAsset;
            psr.trailMaterial = null;
            psr.sortingOrder = 50;
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            var trails = ps.trails; trails.enabled = false;

            // ─── KATMAN 2: PARLAK SPARKLE'lar (küçük, hızlı, daha uzun ömür, beyaz) ───
            var sparkleGO = new GameObject("Sparkle");
            sparkleGO.transform.SetParent(go.transform, false);
            var sps = sparkleGO.AddComponent<ParticleSystem>();
            sps.Stop();
            var spsMain = sps.main;
            spsMain.startLifetime = new ParticleSystem.MinMaxCurve(0.40f, 0.70f);
            spsMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7.5f);
            spsMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            spsMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2);
            spsMain.maxParticles = 40;
            spsMain.simulationSpace = ParticleSystemSimulationSpace.World;
            spsMain.gravityModifier = 0.3f;
            spsMain.playOnAwake = true;
            spsMain.startColor = new Color(1f, 1f, 1f, 1f);

            var spsEm = sps.emission;
            spsEm.enabled = true;
            spsEm.rateOverTime = 0;
            spsEm.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            var spsShape = sps.shape;
            spsShape.enabled = true;
            spsShape.shapeType = ParticleSystemShapeType.Circle;
            spsShape.radius = 0.12f;

            var spsVel = sps.velocityOverLifetime;
            spsVel.enabled = true;
            spsVel.radial = new ParticleSystem.MinMaxCurve(2.5f);

            var spsColor = sps.colorOverLifetime;
            spsColor.enabled = true;
            var spsGrad = new Gradient();
            spsGrad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.95f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            spsColor.color = spsGrad;

            var spsSize = sps.sizeOverLifetime;
            spsSize.enabled = true;
            spsSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 0f)));

            var spsr = sparkleGO.GetComponent<ParticleSystemRenderer>();
            spsr.sharedMaterial = matAsset;
            spsr.sortingOrder = 52;
            spsr.renderMode = ParticleSystemRenderMode.Billboard;
            var spsTrails = sps.trails; spsTrails.enabled = false;

            // ─── KATMAN 3: MERKEZ FLASH (1 büyük disk hızla genişler ve solar) ───
            var flashGO = new GameObject("Flash");
            flashGO.transform.SetParent(go.transform, false);
            var fps = flashGO.AddComponent<ParticleSystem>();
            fps.Stop();
            var fpsMain = fps.main;
            fpsMain.startLifetime = 0.22f;
            fpsMain.startSpeed = 0f;
            fpsMain.startSize = 0.45f;
            fpsMain.maxParticles = 3;
            fpsMain.simulationSpace = ParticleSystemSimulationSpace.World;
            fpsMain.playOnAwake = true;
            fpsMain.startColor = new Color(1f, 1f, 1f, 0.85f);

            var fpsEm = fps.emission;
            fpsEm.enabled = true;
            fpsEm.rateOverTime = 0;
            fpsEm.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

            var fpsShape = fps.shape;
            fpsShape.enabled = false;

            var fpsSize = fps.sizeOverLifetime;
            fpsSize.enabled = true;
            fpsSize.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 0.4f), new Keyframe(0.5f, 2.2f), new Keyframe(1, 3.0f)));

            var fpsColor = fps.colorOverLifetime;
            fpsColor.enabled = true;
            var fpsGrad = new Gradient();
            fpsGrad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0.40f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            fpsColor.color = fpsGrad;

            var fpsr = flashGO.GetComponent<ParticleSystemRenderer>();
            fpsr.sharedMaterial = matAsset;
            fpsr.sortingOrder = 49; // burst altında
            fpsr.renderMode = ParticleSystemRenderMode.Billboard;
            var fpsTrails = fps.trails; fpsTrails.enabled = false;

            go.AddComponent<ParticleEffect>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateScorePopupPrefab()
        {
            const string path = "Assets/Prefabs/ScorePopup.prefab";

            var go = new GameObject("ScorePopup");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = "+100";
            tmp.fontSize = 4;
            tmp.color = new Color(1f, 1f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color(0.2f, 0.3f, 0.5f);
            tmp.enableWordWrapping = false;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) { mr.sortingOrder = 90; mr.sortingLayerName = "Default"; }

            var sp = go.AddComponent<ScorePopup>();
            var so = new SerializedObject(sp);
            so.FindProperty("text").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreatePersistentSystems()
        {
            var faderGO = new GameObject("SceneTransitionFader");
            var faderCanvas = faderGO.AddComponent<Canvas>();
            faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            faderCanvas.sortingOrder = 1000;
            faderGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            faderGO.AddComponent<GraphicRaycaster>();

            var overlayGO = new GameObject("Overlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlayGO.transform.SetParent(faderGO.transform, false);
            var ort = (RectTransform)overlayGO.transform;
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
            ort.offsetMin = ort.offsetMax = Vector2.zero;
            overlayGO.GetComponent<Image>().color = Color.black;
            overlayGO.GetComponent<Image>().raycastTarget = false;
            var cg = overlayGO.GetComponent<CanvasGroup>();
            cg.alpha = 0; cg.blocksRaycasts = false;

            var fader = faderGO.AddComponent<BalloonPop.Effects.SceneTransitionFader>();
            var sf = new SerializedObject(fader);
            sf.FindProperty("overlay").objectReferenceValue = cg;
            sf.ApplyModifiedPropertiesWithoutUndo();

            var achGO = new GameObject("AchievementManager");
            achGO.AddComponent<BalloonPop.Gameplay.AchievementManager>();

            var statsGO = new GameObject("StatsTracker");
            statsGO.AddComponent<BalloonPop.Gameplay.StatsTracker>();

            CreateAchievementToast();
        }

        private static void CreateAchievementToast()
        {
            // Toast canvas — raycaster YOK çünkü toast click almıyor, sadece görsel notification.
            // Eskiden raycaster vardı ve toast'un Card/Desc/Reward elementleri ekranın üst alanındaki
            // panel close (X) butonlarının tıklamasını yutuyordu.
            var canvasGO = new GameObject("AchievementToastCanvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            card.transform.SetParent(canvasGO.transform, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(900, 200);
            rt.anchoredPosition = new Vector2(0, -50);
            var cardImg = card.GetComponent<Image>();
            var roundedXsSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_16.png");
            cardImg.sprite = roundedXsSp;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0.20f, 0.27f, 0.46f, 0.97f);
            var cardCg = card.GetComponent<CanvasGroup>();
            cardCg.alpha = 0f;

            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(card.transform, false);
            var irt = (RectTransform)iconGO.transform;
            irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f);
            irt.sizeDelta = new Vector2(140, 140);
            irt.anchoredPosition = new Vector2(100, 0);
            iconGO.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_star.png");
            iconGO.GetComponent<Image>().color = new Color(1f, 0.83f, 0.24f);

            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(card.transform, false);
            var trt = (RectTransform)titleGO.transform;
            trt.anchorMin = new Vector2(0, 0.55f); trt.anchorMax = new Vector2(1, 0.95f);
            trt.offsetMin = new Vector2(200, 0); trt.offsetMax = new Vector2(-30, 0);
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "BAŞARIM";
            titleTmp.fontSize = 52;
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.color = new Color(1f, 0.85f, 0.24f);
            titleTmp.fontStyle = FontStyles.Bold;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(card.transform, false);
            var drt = (RectTransform)descGO.transform;
            drt.anchorMin = new Vector2(0, 0.15f); drt.anchorMax = new Vector2(1, 0.55f);
            drt.offsetMin = new Vector2(200, 0); drt.offsetMax = new Vector2(-30, 0);
            var descTmp = descGO.AddComponent<TextMeshProUGUI>();
            descTmp.text = "Açıklama";
            descTmp.fontSize = 36;
            descTmp.alignment = TextAlignmentOptions.Left;
            descTmp.color = Color.white;

            var rewardGO = new GameObject("Reward", typeof(RectTransform));
            rewardGO.transform.SetParent(card.transform, false);
            var rrt = (RectTransform)rewardGO.transform;
            rrt.anchorMin = new Vector2(1, 0); rrt.anchorMax = new Vector2(1, 1);
            rrt.pivot = new Vector2(1, 0.5f);
            rrt.sizeDelta = new Vector2(180, 0);
            rrt.anchoredPosition = new Vector2(-30, 0);
            var rewardTmp = rewardGO.AddComponent<TextMeshProUGUI>();
            rewardTmp.text = "+0";
            rewardTmp.fontSize = 56;
            rewardTmp.alignment = TextAlignmentOptions.Right;
            rewardTmp.color = new Color(1f, 0.83f, 0.24f);
            rewardTmp.fontStyle = FontStyles.Bold;

            var toast = canvasGO.AddComponent<AchievementToast>();
            var so = new SerializedObject(toast);
            so.FindProperty("canvasGroup").objectReferenceValue = card.GetComponent<CanvasGroup>();
            so.FindProperty("card").objectReferenceValue = (RectTransform)card.transform;
            so.FindProperty("titleText").objectReferenceValue = titleTmp;
            so.FindProperty("descText").objectReferenceValue = descTmp;
            so.FindProperty("rewardText").objectReferenceValue = rewardTmp;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateAnimatedBackground(Camera cam)
        {
            var bgGO = new GameObject("AnimatedBackground");
            var bg = bgGO.AddComponent<BalloonPop.Effects.AnimatedBackground>();

            string[] names = { "balloon_red","balloon_blue","balloon_green","balloon_yellow",
                               "balloon_purple","balloon_orange","balloon_pink" };
            var sprites = new Sprite[names.Length];
            for (int i = 0; i < names.Length; i++)
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{names[i]}.png");

            var so = new SerializedObject(bg);
            var arr = so.FindProperty("balloonSprites");
            arr.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.FindProperty("targetCamera").objectReferenceValue = cam;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateHandIconPrefab()
        {
            const string path = "Assets/Prefabs/HandIcon.prefab";
            var go = new GameObject("HandIcon");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_tap.png");
            sr.color = Color.white;
            sr.sortingOrder = 80;
            sr.sortingLayerName = "Default";
            go.transform.localScale = Vector3.one * 0.8f;
            go.AddComponent<BalloonPop.Effects.HandIcon>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateFlashPrefab()
        {
            const string path = "Assets/Prefabs/PopFlash.prefab";
            var go = new GameObject("PopFlash");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_glow.png");
            sr.color = Color.white;
            sr.sortingOrder = 60;
            sr.sortingLayerName = "Default";
            go.transform.localScale = Vector3.one * 0.5f;
            go.AddComponent<FlashEffect>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateShockwavePrefab()
        {
            const string path = "Assets/Prefabs/Shockwave.prefab";
            var go = new GameObject("Shockwave");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_ring.png");
            sr.color = new Color(1f, 0.85f, 0.4f);
            sr.sortingOrder = 65;
            sr.sortingLayerName = "Default";
            go.transform.localScale = Vector3.one * 0.5f;
            go.AddComponent<ShockwaveEffect>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateGameScene(GameObject balloonPrefab, LevelDatabase db, GameObject boomPrefab, GameObject particlePrefab, GameObject scorePopupPrefab, GameObject flashPrefab, GameObject shockwavePrefab, GameObject handPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGO = new GameObject("Main Camera");
            cameraGO.transform.position = new Vector3(0, 0, -10);
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.07f, 0.09f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographicSize = 6f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<CameraFitter>();

            var bootGO = new GameObject("Bootstrap");
            bootGO.AddComponent<GameSceneBootstrap>();

            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            var loaderGO = new GameObject("LevelLoader");
            var loader = loaderGO.AddComponent<LevelLoader>();
            var sl = new SerializedObject(loader);
            sl.FindProperty("database").objectReferenceValue = db;
            sl.FindProperty("gameSceneName").stringValue = "Game";
            sl.FindProperty("menuSceneName").stringValue = "MainMenu";
            sl.ApplyModifiedPropertiesWithoutUndo();

            var gridGO = new GameObject("GridManager");
            var grid = gridGO.AddComponent<GridManager>();
            var containerGO = new GameObject("BalloonContainer");
            containerGO.transform.SetParent(gridGO.transform);

            var sg = new SerializedObject(grid);
            sg.FindProperty("balloonPrefab").objectReferenceValue = balloonPrefab.GetComponent<Balloon>();
            sg.FindProperty("balloonContainer").objectReferenceValue = containerGO.transform;
            sg.FindProperty("boomEffectPrefab").objectReferenceValue = boomPrefab;
            sg.FindProperty("particlePrefab").objectReferenceValue = particlePrefab;
            sg.FindProperty("scorePopupPrefab").objectReferenceValue = scorePopupPrefab;
            sg.FindProperty("flashPrefab").objectReferenceValue = flashPrefab;
            sg.FindProperty("shockwavePrefab").objectReferenceValue = shockwavePrefab;
            sg.ApplyModifiedPropertiesWithoutUndo();

            gridGO.AddComponent<GameplayController>();
            gridGO.AddComponent<ScoreManager>();

            var inputGO = new GameObject("InputManager");
            var im = inputGO.AddComponent<InputManager>();
            var si = new SerializedObject(im);
            si.FindProperty("mainCamera").objectReferenceValue = cam;
            si.ApplyModifiedPropertiesWithoutUndo();

            var boosterGO = new GameObject("BoosterManager");
            boosterGO.AddComponent<BalloonPop.Gameplay.BoosterManager>();

            var tutorialGO = new GameObject("TutorialManager");
            var tutorial = tutorialGO.AddComponent<BalloonPop.Effects.TutorialManager>();
            var stTut = new SerializedObject(tutorial);
            stTut.FindProperty("handPrefab").objectReferenceValue = handPrefab;
            stTut.ApplyModifiedPropertiesWithoutUndo();

            var idleGO = new GameObject("IdleHintTrigger");
            var idle = idleGO.AddComponent<BalloonPop.Effects.IdleHintTrigger>();
            var stIdle = new SerializedObject(idle);
            stIdle.FindProperty("handPrefab").objectReferenceValue = handPrefab;
            stIdle.ApplyModifiedPropertiesWithoutUndo();

            var obstacleGO = new GameObject("ObstacleVisualizer");
            var obstacle = obstacleGO.AddComponent<BalloonPop.Effects.ObstacleVisualizer>();
            var stObs = new SerializedObject(obstacle);
            stObs.FindProperty("stoneSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_stone.png");
            stObs.FindProperty("iceSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_ice.png");
            stObs.ApplyModifiedPropertiesWithoutUndo();

            var cinemaGO = new GameObject("CinematicWin");
            cinemaGO.AddComponent<BalloonPop.Effects.CinematicWin>();

            // Grid arkasında DOLU bir panel — bg_beach'in deniz/gökyüzü hattı
            // balonların arkasından geçip kontrast bozmasın diye tek-renk altlık
            var solidBgGO = new GameObject("GridSolidBackground", typeof(SpriteRenderer));
            var solidBgSR = solidBgGO.GetComponent<SpriteRenderer>();
            solidBgSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_rounded_56.png");
            solidBgSR.color = new Color(0.92f, 0.96f, 0.98f, 1.0f); // tam opak: bg horizon hattı sızmasın
            solidBgSR.sortingOrder = -6;
            solidBgSR.sortingLayerName = "Default";
            solidBgSR.drawMode = SpriteDrawMode.Sliced;
            solidBgGO.AddComponent<BalloonPop.Effects.GridSolidBackground>();

            // Grid alanı için "tablo" görünümlü tek bir panel
            var frameGO = new GameObject("GameAreaFrame", typeof(SpriteRenderer));
            var frameSR = frameGO.GetComponent<SpriteRenderer>();
            frameSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_game_frame.png");
            frameSR.color = new Color(1f, 1f, 1f, 0.55f);
            frameSR.sortingOrder = -5;
            frameSR.sortingLayerName = "Default";
            frameSR.drawMode = SpriteDrawMode.Sliced;
            frameGO.AddComponent<BalloonPop.Effects.GameAreaFrame>();

            // Her hücre için cam panel tile'ı — referans tasarımı: parlak kenar + faint iç
            var cellGridGO = new GameObject("CellTileGrid");
            var cellTileGrid = cellGridGO.AddComponent<BalloonPop.Effects.CellTileGrid>();
            var cellTileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_cell_tile.png");
            var sCell = new SerializedObject(cellTileGrid);
            sCell.FindProperty("tileSprite").objectReferenceValue = cellTileSprite;
            sCell.ApplyModifiedPropertiesWithoutUndo();

            var themeGO = new GameObject("ThemeBackground", typeof(SpriteRenderer));
            var theme = themeGO.AddComponent<BalloonPop.Effects.ThemeBackground>();
            var bgSprites = new Sprite[] {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_beach.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_winter.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_space.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_candy.png"),
            };
            var stTheme = new SerializedObject(theme);
            var arrTheme = stTheme.FindProperty("worldBgs");
            arrTheme.arraySize = bgSprites.Length;
            for (int i = 0; i < bgSprites.Length; i++)
                arrTheme.GetArrayElementAtIndex(i).objectReferenceValue = bgSprites[i];
            stTheme.ApplyModifiedPropertiesWithoutUndo();

            var audioGO = new GameObject("AudioManager");
            var am = audioGO.AddComponent<AudioManager>();
            var musicSrc = audioGO.AddComponent<AudioSource>();
            musicSrc.playOnAwake = false;
            var sfxSrc = audioGO.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            var sa = new SerializedObject(am);
            sa.FindProperty("musicSource").objectReferenceValue = musicSrc;
            sa.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
            sa.ApplyModifiedPropertiesWithoutUndo();

            var hud = SceneUIBuilder.BuildGameHUD();

            EditorSceneManager.SaveScene(scene, GameScenePath);
            Debug.Log("[BalloonPop] Game sahnesi oluşturuldu");
        }

        private static void CreateMainMenuScene(LevelDatabase db)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGO = new GameObject("Main Camera");
            cameraGO.transform.position = new Vector3(0, 0, -10);
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.30f, 0.55f, 0.85f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();

            CreateAnimatedBackground(cam);
            CreatePersistentSystems();

            var loaderGO = new GameObject("LevelLoader");
            var loader = loaderGO.AddComponent<LevelLoader>();
            var sl = new SerializedObject(loader);
            sl.FindProperty("database").objectReferenceValue = db;
            sl.FindProperty("gameSceneName").stringValue = "Game";
            sl.FindProperty("menuSceneName").stringValue = "MainMenu";
            sl.ApplyModifiedPropertiesWithoutUndo();

            SceneUIBuilder.BuildMainMenu(db);

            EditorSceneManager.SaveScene(scene, MenuScenePath);
            Debug.Log("[BalloonPop] MainMenu sahnesi oluşturuldu");
        }

        private static void AddScenesToBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true),
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
#endif
