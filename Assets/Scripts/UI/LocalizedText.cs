using UnityEngine;
using TMPro;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    /// <summary> TMP_Text'e ekle, başlangıçta ve dil değiştiğinde otomatik çevirir. </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            Apply();
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

        public void SetKey(string newKey)
        {
            key = newKey;
            Apply();
        }

        public void Apply()
        {
            if (text == null) text = GetComponent<TMP_Text>();
            if (text == null || string.IsNullOrEmpty(key)) return;
            text.text = LocalizationManager.Get(key);
        }
    }
}
