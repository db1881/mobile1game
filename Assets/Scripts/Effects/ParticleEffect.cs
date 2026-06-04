using UnityEngine;

namespace BalloonPop.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleEffect : MonoBehaviour
    {
        private ParticleSystem ps;

        public static ParticleEffect Spawn(GameObject prefab, Vector3 pos, Color color)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, pos, Quaternion.identity);
            var pe = go.GetComponent<ParticleEffect>();
            if (pe != null) pe.SetColor(color);
            Destroy(go, 1.0f);
            return pe;
        }

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public void SetColor(Color color)
        {
            if (ps == null) ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            ps.Clear();
            ps.Play();
        }
    }
}
