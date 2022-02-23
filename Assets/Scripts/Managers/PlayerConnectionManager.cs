using System;
using System.Collections.Generic;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Managers
{
    public class PlayerConnectionManager : NetworkSingleton<PlayerConnectionManager>
    {
        [SerializeField] private List<ulong> activeClientIDs;
        private IReadOnlyDictionary<ulong, NetworkClient> _connectedClientList;
        public List<Player> ConnectedPlayerComponents { get; protected set; }


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

            _connectedClientList = NetworkManager.Singleton.ConnectedClients;
            this.activeClientIDs.Add(clientID);
            if (ConnectedPlayerComponents == null)
            {
                ConnectedPlayerComponents = new List<Player>();
            }

            foreach (var client in _connectedClientList)
            {
                var comp = client.Value.PlayerObject.transform.gameObject.GetComponent<Player>();
                if (!ConnectedPlayerComponents.Contains(comp))
                {
                    ConnectedPlayerComponents.Add(comp);
                }
            }

            if (_connectedClientList.Count != activeClientIDs.Count)
            {
                activeClientIDs.Clear();
                foreach (var i in _connectedClientList)
                {
                    activeClientIDs.Add(i.Key);
                }
            }

            return;
        }

        public IReadOnlyDictionary<ulong, NetworkClient> GetConnectedClients()
        {
            return _connectedClientList;
        }
    }
}