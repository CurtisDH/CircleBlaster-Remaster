using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utility
{
    [RequireComponent(typeof(ObjectPooling))]
    public class ObjectPooling : NetworkSingleton<ObjectPooling>
    {
        private Dictionary<Type, List<Component>> _pooledObjects = new();
        [SerializeField] private List<PoolConfig> prefabs;

        [Serializable]
        public struct PoolConfig
        {
            public GameObject GameObject;
            public int PreCacheCount;
        }

        public IEnumerable<GameObject> GetPrefabs()
        {
            return prefabs.Select(poolConfig => poolConfig.GameObject).ToList();
        }

        private void OnEnable()
        {
            foreach (var config in prefabs)
            {
                for (int i = 0; i <= config.PreCacheCount; i++)
                {
                    var obj = Instantiate(config.GameObject, this.transform, true);
                    obj.SetActive(false);
                }
            }
        }

        public void PoolObject(Component component, bool add = true)
        {
            var componentType = component.GetType();
            if (add)
            {
                if (_pooledObjects.ContainsKey(componentType))
                {
                    _pooledObjects[componentType].Add(component);
                    return;
                }

                _pooledObjects.Add(componentType, new List<Component> { component });
                return;
            }

            if (_pooledObjects.ContainsKey(componentType))
                _pooledObjects[componentType].Remove(component);
        }

        public T RequestComponentFromPool<T>() where T : Component
        {
            var componentType = typeof(T);
            if (_pooledObjects.ContainsKey(componentType))
            {
                if (_pooledObjects[componentType].Count > 0 && _pooledObjects[componentType][0] != null)
                {
                    var objectToReturn = _pooledObjects[componentType][0] as T;
                    PoolObject(objectToReturn, false);
                    return objectToReturn;
                }
            }

            //If we don't have any available in the pool we create one and return it to the original
            for (var i = 0; i <= prefabs.Count; i++)
            {
                var prefabComp = prefabs[i].GameObject.GetComponent<T>();
                if (prefabComp is null || prefabComp.GetType() != componentType) continue;
                var obj = Instantiate(prefabComp.gameObject);
                obj.name = $"{componentType} CLIENTPOOL";
                var objComp = obj.GetComponent<T>();
                return objComp;
            }

            return null; // TODO 
        }
    }
}