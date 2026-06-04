using UnityEngine;
using BalloonPop.Gameplay;

namespace BalloonPop.Effects
{
    public class BalloonIdleBob : MonoBehaviour
    {
        [SerializeField] private float bobAmount = 0.04f;
        [SerializeField] private float swayAmount = 0.02f;
        [SerializeField] private float bobSpeed = 1.8f;

        private Balloon balloon;
        private float seed;
        private Vector3 currentOffset = Vector3.zero;
        private bool wasMoving = true;

        private void Awake()
        {
            balloon = GetComponent<Balloon>();
            seed = Random.Range(0f, Mathf.PI * 2f);
        }

        private void LateUpdate()
        {
            if (balloon == null) return;

            if (balloon.IsMoving)
            {
                wasMoving = true;
                currentOffset = Vector3.zero;
                return;
            }

            Vector3 restingPos = transform.position - currentOffset;
            float t = Time.time * bobSpeed + seed;
            Vector3 newOffset = new Vector3(
                Mathf.Sin(t * 0.7f) * swayAmount,
                Mathf.Sin(t) * bobAmount,
                0f);

            transform.position = restingPos + newOffset;
            currentOffset = newOffset;
            wasMoving = false;
        }
    }
}
