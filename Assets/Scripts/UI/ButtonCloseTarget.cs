using UnityEngine;
using UnityEngine.UI;

namespace BalloonPop.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonCloseTarget : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        public GameObject Target { get => target; set => target = value; }

        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(Close);
        }

        public void Close()
        {
            if (target != null) target.SetActive(false);
        }
    }
}
