using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    [RequireComponent(typeof(ObjectPooling))]
    public class ObjectPooling : MonoSingleton<ObjectPooling>
    {
        private Dictionary<Type, List<Component>> _pooledObjects = new();
        public List<GameObject> prefabs;

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
                    PoolObject(objectToReturn,false);
                    return objectToReturn;
                }
            }

            //If we don't have any available in the pool we create one and return it to the original
            for (var i = 0; i <= prefabs.Count; i++)
            {
                var prefabComp = prefabs[i].GetComponent<T>();
                if (prefabComp is null || prefabComp.GetType() != componentType) continue;
                Instantiate(prefabComp.gameObject);
                return prefabComp;
            }

            return null; // TODO 
        }
    }
}