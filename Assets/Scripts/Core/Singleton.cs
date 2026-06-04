using UnityEngine;

namespace BalloonPop.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool isQuitting;

        public static T Instance
        {
            get
            {
                if (isQuitting) return null;
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));
                        if (instance == null)
                        {
                            var go = new GameObject(typeof(T).Name);
                            instance = go.AddComponent<T>();
                        }
                    }
                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this as T;
        }

        protected virtual void OnApplicationQuit() => isQuitting = true;
    }
}
