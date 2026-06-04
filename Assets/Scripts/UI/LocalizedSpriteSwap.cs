using UnityEngine;
using UnityEngine.UI;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    /// <summary>
    /// Image component'ine ekle, başlangıçta ve dil değişiminde TR/EN sprite swap eder.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LocalizedSpriteSwap : MonoBehaviour
    {
        public Sprite TrSprite;
        public Sprite EnSprite;

        private Image img;

        private void Awake()
        {
            img = GetComponent<Image>();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= Apply;
        }

        private void Apply()
        {
            if (img == null) img = GetComponent<Image>();
            if (img == null) return;
            var lang = LocalizationManager.Current;
            Sprite chosen;
            if (lang == LocalizationManager.Lang.TR)
                chosen = TrSprite != null ? TrSprite : EnSprite;
            else
                chosen = EnSprite != null ? EnSprite : TrSprite;
            if (chosen != null) img.sprite = chosen;
        }
    }
}
