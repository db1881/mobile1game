using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Audio;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button unlockAllButton;

        private void OnEnable()
        {
            if (AudioManager.Instance != null)
            {
                musicSlider.value = AudioManager.Instance.MusicVolume;
                sfxSlider.value = AudioManager.Instance.SfxVolume;
            }

            musicSlider.onValueChanged.AddListener(OnMusic);
            sfxSlider.onValueChanged.AddListener(OnSfx);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(ConfirmReset);
            if (unlockAllButton != null) unlockAllButton.onClick.AddListener(UnlockAll);
        }

        private void OnDisable()
        {
            musicSlider.onValueChanged.RemoveListener(OnMusic);
            sfxSlider.onValueChanged.RemoveListener(OnSfx);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (resetProgressButton != null) resetProgressButton.onClick.RemoveListener(ConfirmReset);
            if (unlockAllButton != null) unlockAllButton.onClick.RemoveListener(UnlockAll);
        }

        private void UnlockAll()
        {
            SaveSystem.Data.HighestUnlockedLevel = 60;
            SaveSystem.Data.Coins += 1000;
            SaveSystem.Data.Hammers += 5;
            SaveSystem.Data.Shuffles += 5;
            SaveSystem.Data.MovePacks += 5;
            SaveSystem.Save();
        }

        private void OnMusic(float v)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = v;
            SaveSystem.MusicVolume = v;
        }

        private void OnSfx(float v)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SfxVolume = v;
            SaveSystem.SfxVolume = v;
        }

        private void Close() => gameObject.SetActive(false);

        private void ConfirmReset()
        {
            SaveSystem.ResetAll();
        }
    }
}
