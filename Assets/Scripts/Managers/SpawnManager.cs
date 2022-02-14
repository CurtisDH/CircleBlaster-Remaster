using System;
using System.Collections.Generic;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;


namespace Managers
{
    public class SpawnManager : NetworkSingleton<SpawnManager>
    {
        [SerializeField] private GameObject enemyTankPrefab;
        public GameObject GetEnemyTankPrefab => enemyTankPrefab;

        private IReadOnlyList<NetworkClient> _activePlayerClients;

        [SerializeField] private List<Player> deadPlayers;
        [SerializeField] private List<GameObject> alivePlayers;
        [SerializeField] private float minSpawnRadiusSize = 25f;


        private void SpawnNetworkObjectFromPrefabObject(GameObject prefab, Vector3 position, bool enemyType = true)
        {
            var prefabObject = NetworkObjectPooling.Instance.GetNetworkObject(prefab);
            prefabObject.transform.position = position;
            prefabObject.Spawn();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && UIManager.Instance.IsHosting())
            {
                SpawnNetworkObjectFromPrefabObject(GetEnemyTankPrefab, SetSpawnPosition());
            }
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
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnection;
            EventManager.Instance.OnPlayerDeath -= OnPlayerDeath;
            EventManager.Instance.OnPlayerSpawn -= OnPlayerSpawn;
        }

        private void SubscribeEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnection;
            EventManager.Instance.OnPlayerDeath += OnPlayerDeath;
            EventManager.Instance.OnPlayerSpawn += OnPlayerSpawn;
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

        private Vector3 SetSpawnPosition()
        {
            return RandomPointOnCircleEdge(FindCenterBetweenPlayers(),
                FindRadiusBetweenPlayers());
        }

        private Vector3 RandomPointOnCircleEdge(Vector3 initialPos, float radius)
        {
            var v2 = Random.insideUnitCircle.normalized * radius;
            var spawnPosition = new Vector3(v2.x, v2.y, 0);
            spawnPosition += initialPos;
            return spawnPosition;
        }

        private float FindRadiusBetweenPlayers()
        {
            float distance = 0;
            for (int i = 0; i < _activePlayerClients.Count; i++)
            {
                if (i + 1 > _activePlayerClients.Count - 1)
                {
                    break;
                }

                distance += Vector3.Distance(_activePlayerClients[i].PlayerObject.transform.position
                    , _activePlayerClients[i + 1].PlayerObject.transform.position);
            }

            var radius = distance * (_activePlayerClients.Count);
            if (radius < minSpawnRadiusSize)
            {
                radius = minSpawnRadiusSize;
            }

            return radius;
        }

        private Vector3 FindCenterBetweenPlayers()
        {
            Vector3 centerPos = new Vector3();

            foreach (var client in _activePlayerClients)
            {
                var clientPos = client.PlayerObject.transform.position;
                var newPos = new Vector3(clientPos.x, clientPos.y, clientPos.z);
                centerPos += newPos / _activePlayerClients.Count;
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