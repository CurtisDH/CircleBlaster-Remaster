using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utility
{
    /// <summary>
    /// Object Pool for networked objects, used for controlling how objects are spawned by Netcode. Netcode by default will allocate new memory when spawning new
    /// objects. With this Networked Pool, we're using custom spawning to reuse objects.
    /// Boss Room uses this for projectiles. In theory it should use this for imps too, but we wanted to show vanilla spawning vs pooled spawning.
    /// Hooks to NetworkManager's prefab handler to intercept object spawning and do custom actions
    /// </summary>
    public class NetworkObjectPooling : NetworkSingleton<NetworkObjectPooling>
    {
        [SerializeField] private List<PoolConfigObject> pooledPrefabsList;

        private HashSet<GameObject> _prefabs = new HashSet<GameObject>();

        private Dictionary<GameObject, Queue<NetworkObject>> pooledObjects =
            new Dictionary<GameObject, Queue<NetworkObject>>();

        private bool _hasInitialized;

        private void OnEnable()
        {
            EventManager.Instance.OnDataDeserialization += OnDataDeserializationReady;
            // Need an event for Deserialization
            //Wait for XML's to be deserialized
            //Then populate the network pool
            //And initialize it.
        }

        private void SetupPoolFromDeserializedXML()
        {
            foreach (var valuePair in SpawnManager.Instance.AllPrefabs)
            {
                PoolConfigObject newObjectConfig = new PoolConfigObject();
                newObjectConfig.uniqueID = valuePair.Key;
                newObjectConfig.PrewarmCount = 500; //TODO replace with generic cache count
                pooledPrefabsList.Add(newObjectConfig);
            }
        }

        private void OnDataDeserializationReady()
        {
            SetupPoolFromDeserializedXML();
            InitializePool();
        }

        public override void OnNetworkDespawn()
        {
            ClearPool();
        }

        public void OnValidate()
        {
            for (var i = 0; i < pooledPrefabsList.Count; i++)
            {
                var prefab = SpawnManager.Instance.AllPrefabs[pooledPrefabsList[i].uniqueID];
                if (prefab != null)
                {
                    Assert.IsNotNull(prefab.GetComponent<NetworkObject>(),
                        $"" +
                        $"{nameof(NetworkObjectPooling)}" +
                        $": Pooled prefab \"{prefab.name}\"" +
                        $" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
                }
            }
        }

        /// <summary>
        /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(GameObject prefab)
        {
            return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position">The position to spawn the object at.</param>
        /// <param name="rotation">The rotation to spawn the object with.</param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return GetNetworkObjectInternal(prefab, position, rotation);
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
        {
            var go = networkObject.gameObject;
            go.SetActive(false);

            pooledObjects[prefab].Enqueue(networkObject);
        }

        /// <summary>
        /// Adds a prefab to the list of spawnable prefabs.
        /// </summary>
        /// <param name="prefab">The prefab to add.</param>
        /// <param name="prewarmCount"></param>
        public void AddPrefab(GameObject prefab, int prewarmCount = 0)
        {
            var networkObject = prefab.GetComponent<NetworkObject>();

            Assert.IsNotNull(networkObject, $"{nameof(prefab)} must have {nameof(networkObject)} component.");
            Assert.IsFalse(_prefabs.Contains(prefab), $"Prefab {prefab.name} is already registered in the pool.");

            RegisterPrefabInternal(prefab, prewarmCount);
        }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            _prefabs.Add(prefab);

            var prefabQueue = new Queue<NetworkObject>();
            pooledObjects[prefab] = prefabQueue;
            for (int i = 0; i < prewarmCount; i++)
            {
                var go = CreateInstance(prefab);
                go.name += "SERVER";
                ReturnNetworkObject(go.GetComponent<NetworkObject>(), prefab);
            }

            // Register Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject CreateInstance(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private NetworkObject GetNetworkObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var queue = pooledObjects[prefab];

            NetworkObject networkObject;
            if (queue.Count > 0)
            {
                networkObject = queue.Dequeue();
            }
            else
            {
                networkObject = CreateInstance(prefab).GetComponent<NetworkObject>();
            }

            // Here we must reverse the logic in ReturnNetworkObject.
            var go = networkObject.gameObject;
            go.SetActive(true);

            go.transform.position = position;
            go.transform.rotation = rotation;
            return networkObject;
        }

        /// <summary>
        /// Registers all objects in <see cref="pooledPrefabsList"/> to the cache.
        /// </summary>
        private void InitializePool()
        {
            if (_hasInitialized) return;
            foreach (var configObject in pooledPrefabsList)
            {
                RegisterPrefabInternal(SpawnManager.Instance.AllPrefabs[configObject.uniqueID],
                    configObject.PrewarmCount);
            }

            _hasInitialized = true;
        }

        /// <summary>
        /// Unregisters all objects in <see cref="pooledPrefabsList"/> from the cache.
        /// </summary>
        private void ClearPool()
        {
            foreach (var prefab in _prefabs)
            {
                // Unregister Netcode Spawn handlers
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            }

            pooledObjects.Clear();
        }
    }

    [Serializable]
    internal struct PoolConfigObject
    {
        public string uniqueID;
        public int PrewarmCount;
    }

    internal class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        GameObject m_Prefab;
        NetworkObjectPooling m_Pool;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPooling pool)
        {
            m_Prefab = prefab;
            m_Pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position,
            Quaternion rotation)
        {
            var netObject = m_Pool.GetNetworkObject(m_Prefab, position, rotation);
            return netObject;
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(networkObject, m_Prefab);
        }
    }
}