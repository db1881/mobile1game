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
        private const string OutputDir  = "Build";
        private const string OutputName = "BalloonPop.apk";
        private const string PackageId  = "com.triogames.balloonpop";

        [MenuItem("BalloonPop/Build APK")]
        public static void BuildAPK_Menu() => DoBuild();

        // Batch mode entry-point
        public static void BatchBuildAPK() => DoBuild();

        private static void DoBuild()
        {
            Debug.Log("[BalloonPop] APK build başlıyor...");

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
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.Android.bundleVersionCode = 1;
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
            string outPath = Path.Combine(OutputDir, OutputName);

            var scenes = new[] {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Game.unity"
            };

            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.buildAppBundle = false; // .aab değil .apk

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
                Debug.Log($"[BalloonPop] APK build BAŞARILI: {outPath} ({mb:F1} MB, {summary.totalTime})");
            }
            else
            {
                Debug.LogError($"[BalloonPop] APK build BAŞARISIZ: {summary.result} • {summary.totalErrors} hata, {summary.totalWarnings} uyarı");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}
#endif
