using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Utility
{
    [RequireComponent(typeof(ObjectPooling))]
    public class ObjectPooling : NetworkSingleton<ObjectPooling>
    {
        private Dictionary<Type, List<Component>> _pooledObjects = new();
        private Dictionary<string, List<Component>> _uniqueIdPooledObjects = new();
        [SerializeField] private List<PoolConfig> prefabs;

        [Serializable]
        public struct PoolConfig
        {
            public GameObject Prefab; //TODO this is for the client sided only
            public string uniqueID;
            public int PreCacheCount;
        }

        //I think this is obsolete 
        // public IEnumerable<GameObject> GetPrefabs()
        // {
        //     return prefabs.Select(poolConfig => poolConfig.GameObject).ToList();
        // }

        private void OnEnable()
        {
            //TODO create XML particle effects
            //TODO XML -> When enemy dies effect_id to be played
            //TODO XML -> When projectile gets destroyed effect_id to be played
            EventManager.Instance.OnDataDeserialization += DataSerialization;
        }

        private void DataSerialization()
        {
            foreach (var p in SpawnManager.Instance.AllPrefabs)
            {
                var poolConfig = new PoolConfig
                {
                    Prefab = null,
                    uniqueID = p.Key,
                    PreCacheCount = 250 // TODO get rid of magic number
                };
                prefabs.Add(poolConfig);
            }

            PreCache();
        }

        private void PreCache()
        {
            foreach (var config in prefabs)
            {
                for (int i = 0; i <= config.PreCacheCount; i++)
                {
                    GameObject prefab = SpawnManager.Instance.GetObjectFromUniqueID(config.uniqueID);


                    if (config.Prefab != null)
                    {
                        prefab = config.Prefab;
                    }

                    var obj = Instantiate(prefab,
                        this.transform,
                        true);
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

        public void PoolUniqueIDObject(string uniqueID, Component component, bool add = true)
        {
            if (add)
            {
                if (!_uniqueIdPooledObjects.ContainsKey(uniqueID))
                {
                    var componentList = new List<Component>();
                    componentList.Add(component);
                    _uniqueIdPooledObjects.Add(uniqueID, componentList);
                }

                _uniqueIdPooledObjects[uniqueID].Add(component);
                return;
            }

            _uniqueIdPooledObjects[uniqueID].Remove(component);
        }

        public T RequestComponentUsingUniqueID<T>(string uniqueID) where T : Component
        {
            if (_uniqueIdPooledObjects.ContainsKey(uniqueID))
            {
                if (_uniqueIdPooledObjects[uniqueID].Count > 0)
                {
                    var objToReturn = _uniqueIdPooledObjects[uniqueID][0] as T;
                    PoolUniqueIDObject(uniqueID, objToReturn, false);
                    Debug.Log("returned object from pool");
                    return objToReturn;
                }
            }

            if (CreateObjectIfMissing(typeof(T), out T component,uniqueID)) return component; //TODO incorrect projectile

            return null;
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
            if (CreateObjectIfMissing(componentType, out T component)) return component;

            return null; // TODO 
        }

        private bool CreateObjectIfMissing<T>(Type componentType ,out T component, string uniqueID = "NULL") where T : Component
        {
            Debug.Log("Creating object as it is missing");
            for (var i = 0; i <= prefabs.Count; i++)
            {
                GameObject prefab = null;
                if (uniqueID != "NULL")
                {
                    prefab = SpawnManager.Instance.GetObjectFromUniqueID(uniqueID);
                }
                else
                {
                    prefab = prefabs[i].Prefab;
                }

                var prefabComp = prefab.GetComponent<T>();
                if (prefabComp is null || prefabComp.GetType() != componentType) continue;
                var obj = Instantiate(prefabComp.gameObject);
                obj.name = $"{componentType} CLIENTPOOL";
                var objComp = obj.GetComponent<T>();
                {
                    component = objComp;
                    return true;
                }
            }

            component = null;
            return false;
        }
    }
}