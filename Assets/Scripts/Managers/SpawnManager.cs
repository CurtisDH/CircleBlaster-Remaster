using System;
using System.Collections.Generic;
using Enemy;
using Unity.Netcode;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;


namespace Managers
{
    public class SpawnManager : NetworkSingleton<SpawnManager>
    {
        [SerializeField] private GameObject enemyTankPrefab;

        [SerializeField] private IReadOnlyList<NetworkClient> activePlayerClients;

        [SerializeField] private Transform visualRadius;

        [SerializeField] private float minSpawnRadiusSize = 25f;

        public void SpawnEnemy()
        {
            var enemy = NetworkObjectPooling.Instance.GetNetworkObject(enemyTankPrefab);
            enemy.transform.position = transform.position = SetSpawnPosition();

            enemy.Spawn();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && UIManager.Instance.IsHosting())
            {
                SpawnEnemy();
            }
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnection;
        }

        private void ClientConnection(ulong obj)
        {
            if (UIManager.Instance.IsHosting())
            {
                activePlayerClients = NetworkManager.Singleton.ConnectedClientsList;
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
            for (int i = 0; i < activePlayerClients.Count; i++)
            {
                if (i + 1 > activePlayerClients.Count - 1)
                {
                    break;
                }

                distance += Vector3.Distance(activePlayerClients[i].PlayerObject.transform.position
                    , activePlayerClients[i + 1].PlayerObject.transform.position);
            }

            var radius = distance * (activePlayerClients.Count);
            if (radius < minSpawnRadiusSize)
            {
                radius = minSpawnRadiusSize;
            }

            visualRadius.transform.localScale = new Vector3(radius,
                radius,
                radius);
            visualRadius.transform.position = FindCenterBetweenPlayers();

            return radius;
        }

        private Vector3 FindCenterBetweenPlayers()
        {
            Vector3 centerPos = new Vector3();

            foreach (var client in activePlayerClients)
            {
                var clientPos = client.PlayerObject.transform.position;
                var newPos = new Vector3(clientPos.x, clientPos.y, clientPos.z);
                centerPos += newPos / activePlayerClients.Count;
            }

            // visualRadius.transform.localScale = Vector3.one;
            // visualRadius.transform.position = centerPos;
            return centerPos;
        }
        
        public Transform GetClosestPlayer(Vector3 pos)
        {
            float distance = float.MaxValue;
            Transform t = null;
            foreach (var client in activePlayerClients)
            {
                float d = Vector3.Distance(pos, client.PlayerObject.transform.position);
                if (d < distance)
                {
                    distance = d;
                    t = client.PlayerObject.transform;
                }
            }

            return t;
        }
    }
}