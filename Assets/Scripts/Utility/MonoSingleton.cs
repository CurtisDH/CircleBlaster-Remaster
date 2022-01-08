using UnityEngine;

namespace Utility
{
    public class MonoSingleton<T>: MonoBehaviour where T:  MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance is { }) return _instance;
                Debug.LogError($"Instance of type:{typeof(T)} is null");
                return _instance;
            }
        }
        private void Awake()
        {
            //This may cause problems down the line. Haven't looked into it

            if(FindObjectOfType(typeof(T), true) is T { } findComponent)
            {
                _instance = findComponent;
                return;
            }
            Debug.LogError($"No GameObject of type {typeof(T)} found");
        }
    }
}