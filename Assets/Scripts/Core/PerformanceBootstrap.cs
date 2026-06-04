using UnityEngine;

namespace BalloonPop.Core
{
    /// <summary>
    /// Uygulama açıldığında bir kez çalışır; hedef FPS'i 60'a sabitler,
    /// V-Sync'i kapatır (mobilde target framerate ile çakışmasın).
    /// Android default 30 FPS'tir — bu olmadan oyun "kasıyormuş" gibi hissedilir.
    /// </summary>
    public static class PerformanceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;        // mobilde manuel target framerate kullan
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Debug.Log("[PerformanceBootstrap] targetFrameRate=60, vSync=off");
        }
    }
}
