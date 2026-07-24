#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BalloonPop.EditorTools
{
    public static class AndroidBuildScript
    {
        private const string OutputDir = "Build";
        private const string ApkOutputName = "BalloonPop.apk";
        private const string AabOutputName = "BalloonPop.aab";
        private const string PackageId = "com.triogames.balloonpop";
        private const string UploadKeyAlias = "balloonpop";
        private const string VersionName = "0.1.6";
        private const int VersionCode = 7;

        [MenuItem("BalloonPop/Build APK")]
        public static void BuildAPK_Menu() => DoBuild(false);

        [MenuItem("BalloonPop/Build AAB for Play")]
        public static void BuildAAB_Menu() => DoBuild(true);

        // Batch mode entry-point
        public static void BatchBuildAPK() => DoBuild(false);

        // Batch mode entry-point for Google Play uploads.
        public static void BatchBuildAAB() => DoBuild(true);

        private static void DoBuild(bool appBundle)
        {
            Debug.Log($"[BalloonPop] {(appBundle ? "AAB" : "APK")} build başlıyor...");

            // Önce sahnelerin kurulu olduğundan emin ol
            if (!File.Exists("Assets/Scenes/MainMenu.unity") || !File.Exists("Assets/Scenes/Game.unity"))
            {
                Debug.Log("[BalloonPop] Sahneler yok, AutoSetup çalıştırılıyor...");
                AutoSetup.BatchSetup();
            }

            // Android target'a geç
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[BalloonPop] Build target Android'e çevriliyor...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // Player settings
            PlayerSettings.companyName  = "BalloonPop";
            PlayerSettings.productName  = "Balloon Pop";
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 64-bit: IL2CPP backend + sadece ARM64 (RAM düşük, ARMv7 atlandı).
            // Modern Android cihazlar zaten ARM64. Play Store da ARM64 istiyor.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Output path
            Directory.CreateDirectory(OutputDir);
            if (appBundle) ConfigureUploadSigning();

            string outPath = Path.Combine(OutputDir, appBundle ? AabOutputName : ApkOutputName);

            var scenes = new[] {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Game.unity"
            };

            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.buildAppBundle = appBundle;

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[BalloonPop] Build hedefi: {outPath}");
            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                double mb = summary.totalSize / 1024.0 / 1024.0;
                Debug.Log($"[BalloonPop] {(appBundle ? "AAB" : "APK")} build BAŞARILI: {outPath} ({mb:F1} MB, {summary.totalTime})");
            }
            else
            {
                Debug.LogError($"[BalloonPop] {(appBundle ? "AAB" : "APK")} build BAŞARISIZ: {summary.result} • {summary.totalErrors} hata, {summary.totalWarnings} uyarı");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static void ConfigureUploadSigning()
        {
            string androidDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".android");
            string credentialsPath = Environment.GetEnvironmentVariable("BALLOON_POP_UPLOAD_CREDENTIALS");
            if (string.IsNullOrWhiteSpace(credentialsPath))
                credentialsPath = Path.Combine(androidDir, "balloon-pop-upload-credentials.txt");

            if (!File.Exists(credentialsPath))
                throw new FileNotFoundException("Balloon Pop upload key credentials file bulunamadı.", credentialsPath);

            string keystorePath = null;
            string keystorePassword = null;
            string keyAliasPassword = null;
            foreach (string rawLine in File.ReadAllLines(credentialsPath))
            {
                int separator = rawLine.IndexOf('=');
                if (separator <= 0) continue;
                string key = rawLine.Substring(0, separator).Trim();
                string value = rawLine.Substring(separator + 1).Trim();
                if (key == "KEYSTORE_PATH") keystorePath = value;
                else if (key == "KEYSTORE_PASSWORD") keystorePassword = value;
                else if (key == "KEY_ALIAS_PASSWORD") keyAliasPassword = value;
            }

            if (string.IsNullOrWhiteSpace(keystorePath) || !File.Exists(keystorePath) ||
                string.IsNullOrWhiteSpace(keystorePassword) || string.IsNullOrWhiteSpace(keyAliasPassword))
                throw new InvalidDataException("Balloon Pop upload key bilgileri eksik veya geçersiz.");

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = UploadKeyAlias;
            PlayerSettings.Android.keyaliasPass = keyAliasPassword;
        }
    }
}
#endif
