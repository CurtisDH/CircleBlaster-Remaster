using System;
using System.Collections.Generic;
using System.Linq;
using Enemy;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;
using Random = UnityEngine.Random;


namespace Managers
{
    public class SpawnManager : NetworkSingleton<SpawnManager>
    {
        [SerializeField] private GameObject enemyBlankPrefab;
        [SerializeField] private GameObject projectileBlankPrefab;
        public Dictionary<string, GameObject> AllPrefabs { get; protected set; }

        private IReadOnlyList<NetworkClient> _activePlayerClients;

        [SerializeField] private List<Player> deadPlayers;
        [SerializeField] private List<GameObject> alivePlayers;
        [SerializeField] private float minSpawnRadiusSize = 25f;


        private int _seedCount;

        public int GetSeed()
        {
            _seedCount++;
            return _seedCount;
        }


        // public void SpawnNetworkObjectFromPrefabObject(GameObject prefab, Vector3 position, bool enemyType = true)
        // {
        //     var prefabObject = NetworkObjectPooling.Instance.GetNetworkObject(prefab);
        //     prefabObject.transform.position = position;
        //     //prefabObject.gameObject.SetActive(true);
        //     //prefabObject.Spawn();
        // }

        public void SpawnObjectFromUniqueID<T>(string id, Vector3 position) where T : Component
        {
            var prefabObject = ObjectPooling.Instance.RequestComponentUsingUniqueID(id);
            prefabObject.transform.position = position;
            prefabObject.gameObject.SetActive(true);
            //prefabObject.Spawn();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && UIManager.Instance.IsHosting())
            {
                //SpawnNetworkObjectFromPrefabObject(GetEnemyPrefabFromUniqueID("temp"), SetSpawnPosition());

                SpawnEnemyIdAtPositionClientRpc("enemy_slow_tank", (_seedCount));
            }
        }

        [ClientRpc]
        private void SpawnEnemyIdAtPositionClientRpc(string uniqueID, int seed)
        {
            SpawnObjectFromUniqueID<EnemyBase>(uniqueID, SetSpawnPosition(seed));
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents()
        {
            EventManager.Instance.OnPlayerDeath -= OnPlayerDeath;
            EventManager.Instance.OnPlayerSpawn -= OnPlayerSpawn;
            EventManager.Instance.OnServerStart -= OnServerStart;
        }

        private void SubscribeEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnection;
            EventManager.Instance.OnPlayerDeath += OnPlayerDeath;
            EventManager.Instance.OnPlayerSpawn += OnPlayerSpawn;
            EventManager.Instance.OnServerStart += OnServerStart;
        }

        private void OnServerStart()
        {
            //Deserialize data
            //Create enemy prefab from the given information
            AllPrefabs = new Dictionary<string, GameObject>();
            //GeneratePrefabsFromDeserializedXML();
        }

        private void GeneratePrefabsFromDeserializedXML()
        {
            var allEnemyData = XmlManager.DeserializeEnemyData();
            foreach (var e in allEnemyData)
            {
                var blankPrefab = Instantiate(enemyBlankPrefab);
                var component = blankPrefab.GetComponent<EnemyBase>();
                
                component.InitialSetup(e.speed, e.damage, e.colours, e.health, e.scale, e.uniqueID);
                component.transform.parent = this.transform;
                if (AllPrefabs == null)
                {
                    AllPrefabs = new Dictionary<string, GameObject>();
                }

                AllPrefabs.Add(e.uniqueID, component.gameObject);
                blankPrefab.name = $"Enemy:{e.uniqueID}";
                blankPrefab.SetActive(false);
            }

            var projectileData = XmlManager.DeserializeProjectileData();
            //TODO why don't we just dynamically create them instead of having a blank prefab..
            foreach (var p in projectileData)
            {
                var blankPrefab = Instantiate(projectileBlankPrefab);
                var component = blankPrefab.GetComponent<Projectile>();

                component.InitialSetup(p.speed, p.damage, p.colours, p.pierce,
                    p.scale, p.uniqueID);
                AllPrefabs.Add(p.uniqueID, component.gameObject);
                blankPrefab.name = $"Projectile:{p.uniqueID}";
                if (IsServer)
                    blankPrefab.GetComponent<NetworkObject>().Spawn();
                blankPrefab.SetActive(false);
            }

            NetworkInformationManager.Instance.SetPrefabData(AllPrefabs);
        }

        public void LoadSpawnManagerDataXml()
        {
            GeneratePrefabsFromDeserializedXML();
        }


        public GameObject GetObjectFromUniqueID(string uniqueID)
        {
            if (AllPrefabs.ContainsKey(uniqueID))
            {
                //Debug.Log($"returning:{AllPrefabs[uniqueID]}");
                return AllPrefabs[uniqueID];
            }
            Debug.LogError($"Provided uniqueID:{uniqueID} was not found in the dictionary.");
            return null;
        }

        private void OnPlayerSpawn(Player playercomponent)
        {
            alivePlayers.Add(playercomponent.gameObject);
        }


        private void ClientConnection(ulong obj)
        {
            if (IsServer)
            {
                _activePlayerClients = NetworkManager.Singleton.ConnectedClientsList;
            }
        }

        public Vector3 SetSpawnPosition(int seed)
        {
            _seedCount++;
            return RandomPointOnCircleEdge(FindCenterBetweenPlayers(),
                FindRadiusBetweenPlayers(), seed);
        }

        private Vector3 RandomPointOnCircleEdge(Vector3 initialPos, float radius, int seed)
        {
            Random.InitState(seed);
            var v2 = Random.insideUnitCircle.normalized * radius;
            var spawnPosition = new Vector3(v2.x, v2.y, 0);
            spawnPosition += initialPos;
            return spawnPosition;
        }

        private float FindRadiusBetweenPlayers()
        {
            float distance = 0;
            for (int i = 0; i < alivePlayers.Count; i++)
            {
                if (i + 1 > alivePlayers.Count - 1)
                {
                    break;
                }

                distance += Vector3.Distance(alivePlayers[i].transform.position
                    , alivePlayers[i + 1].transform.position);
            }

            var radius = distance * (alivePlayers.Count);
            if (radius < minSpawnRadiusSize)
            {
                radius = minSpawnRadiusSize;
            }

            return radius;
        }

        private Vector3 FindCenterBetweenPlayers()
        {
            Vector3 centerPos = new Vector3();

            foreach (var client in alivePlayers)
            {
                var clientPos = client.transform.position;
                var newPos = new Vector3(clientPos.x, clientPos.y, clientPos.z);
                centerPos += newPos / alivePlayers.Count;
            }

            return centerPos;
        }

        public Transform GetClosestPlayer(Vector3 initialPos, List<Transform> compareTransforms)
        {
            float distance = float.MaxValue;
            Transform closestPlayer = null;
            foreach (var player in compareTransforms)
            {
                if (player == null)
                {
                    continue;
                }

                float dist = Vector3.Distance(initialPos, player.position);
                if (dist < distance)
                {
                    distance = dist;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }


        #region Player related

        private void OnPlayerDeath(Player playerComponent)
        {
            Debug.Log("SpawnManager::OnPlayerDeath");
            deadPlayers.Add(playerComponent);

            alivePlayers.Remove(playerComponent.gameObject);

            //HideAllDeadClientsServerRpc();
            if (!IsServer) return;
            if (deadPlayers.Count == _activePlayerClients.Count)
            {
                GameManager.Instance.EndGame();
            }
        }

        public GameObject GetActivePlayer()
        {
            return alivePlayers.Count > 0 ? alivePlayers[0] : null;
        }

        public List<GameObject> GetAllAlivePlayers()
        {
            return alivePlayers;
        }


        public void SpawnDeadPlayers()
        {
            foreach (var deadPlayer in deadPlayers)
            {
                deadPlayer.gameObject.SetActive(true);
                deadPlayer.SetDeathValue(false);
                // may need to be set on the player itself.
                deadPlayer.transform.position = Vector3.zero;
            }

            deadPlayers.Clear();
        }

        public void SpawnPendingConnectionPlayers()
        {
            //Probably just filter them through into dead players aswell. 
        }

        #endregion
    }
}