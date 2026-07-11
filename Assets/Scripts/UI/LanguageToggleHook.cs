using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Core;

namespace BalloonPop.UI
{
    [RequireComponent(typeof(Button))]
    public class LanguageToggleHook : MonoBehaviour
    {
        private TMP_Text label;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                LocalizationManager.Toggle();
                Refresh();
            });
        }

        private void OnEnable()
        {
            label = GetComponentInChildren<TMP_Text>();
            Refresh();
        }

        private void Refresh()
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>();
            if (label == null) return;
            label.text = LocalizationManager.Current == LocalizationManager.Lang.TR ? "TR / EN" : "EN / TR";
        }
    }
}
