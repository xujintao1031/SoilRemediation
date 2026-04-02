using UnityEngine;

namespace GasChromatography.Manager
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();
                // if (_instance == null)
                // {
                //     GameObject singletonObject = new GameObject();
                //     _instance = singletonObject.AddComponent<T>();
                //     singletonObject.name = typeof(T).ToString() + " (Singleton)";
                // }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
                _instance = this as T;
            else if (_instance != this) Destroy(gameObject);
        }
    }
}