using UnityEngine;
using UnityEngine.UI;

namespace BalloonPop.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonOpenTarget : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        public GameObject Target { get => target; set => target = value; }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Open);
        }

        public void Open()
        {
            if (target != null) target.SetActive(true);
        }
    }
}
