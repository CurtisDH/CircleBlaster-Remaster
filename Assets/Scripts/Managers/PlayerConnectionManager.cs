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
        private IReadOnlyDictionary<ulong, NetworkClient> list;
        public NetworkVariable<int> mostRecentClientConnectionID = new NetworkVariable<int>();
        

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        }

        private void ClientDisconnected(ulong clientID)
        {
            if (!UIManager.Instance.IsHosting()) return;
            this.activeClientIDs.Remove(clientID);
        }

        private void ClientConnected(ulong clientID)
        {
            if (!UIManager.Instance.IsHosting()) return;

            list = NetworkManager.Singleton.ConnectedClients;

            this.activeClientIDs.Add(clientID);

            if (list.Count != activeClientIDs.Count)
            {
                activeClientIDs.Clear();
                foreach (var i in list)
                {
                    activeClientIDs.Add(i.Key);
                }
            }

            mostRecentClientConnectionID.Value = (int)activeClientIDs[^1];

            return;
        }
        
    }
}