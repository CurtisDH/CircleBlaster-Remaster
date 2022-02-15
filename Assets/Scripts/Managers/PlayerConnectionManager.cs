using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Managers
{
    public class PlayerConnectionManager : NetworkSingleton<PlayerConnectionManager>
    {
        [SerializeField] private List<ulong> activeClientIDs;
        private IReadOnlyDictionary<ulong, NetworkClient> connectedClientList;


        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        }

        [ClientRpc]
        public void TestingClientRpc()
        {
            if (!IsServer)
                UIManager.Instance.Shutdown();
        }

        private void ClientDisconnected(ulong clientID)
        {
            if (!UIManager.Instance.IsHosting()) return;
            this.activeClientIDs.Remove(clientID);
        }

        private void ClientConnected(ulong clientID)
        {
            if (!UIManager.Instance.IsHosting()) return;

            connectedClientList = NetworkManager.Singleton.ConnectedClients;

            this.activeClientIDs.Add(clientID);

            if (connectedClientList.Count != activeClientIDs.Count)
            {
                activeClientIDs.Clear();
                foreach (var i in connectedClientList)
                {
                    activeClientIDs.Add(i.Key);
                }
            }

            return;
        }

        public IReadOnlyDictionary<ulong, NetworkClient> GetConnectedClients()
        {
            return connectedClientList;
        }
    }
}