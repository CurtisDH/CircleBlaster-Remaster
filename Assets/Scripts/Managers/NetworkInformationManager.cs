using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Managers
{
    public class NetworkInformationManager : NetworkSingleton<NetworkInformationManager>
    {
        private Dictionary<string, GameObject> _allPrefabs = new();
        [SerializeField] private List<XmlManager.FullWaveInformation> _fullWaveInformation = new();


        //Soo i think we can send byte arrays over to the client.
        // with this information we should be able to then send over the ENTIRE XML data and deserialize it then

        public void SetPrefabData(Dictionary<string, GameObject> allPrefabs)
        {
            _allPrefabs = allPrefabs;
        }

        public void SetWaveData(List<XmlManager.FullWaveInformation> fullWaveInformation)
        {
            _fullWaveInformation = fullWaveInformation;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestClientDataServerRpc()
        {
            Debug.Log("Here!");
            var allConfigs = XmlManager.GetAllFileByteArrays();
            foreach (var keyValuePair in allConfigs)
            {
                SendXMLFilesToClientRpc(keyValuePair.Key, keyValuePair.Value);
            }

            //do we need to wait??
            XmlDataCompleteClientRpc();
        }

        [ClientRpc]
        public void XmlDataCompleteClientRpc()
        {
            if (!IsServer)
            {
                XmlManager.LoadAllXml();
                EventManager.Instance.InvokeOnDataDeserializationClient();
            }
        }

        [ClientRpc] //TODO also verify client id first so avoid resending significant amounts of data.
        //TODO probably need a bool to say "hey we've downloaded the xml, dont redownload and reload it"
        public void SendXMLFilesToClientRpc(string path, byte[] byteArray)
        {
            var fEx = Path.GetFileName(path);
            var dir = Path.GetDirectoryName(path);
            path = Path.Combine(dir, "ServerConfigs",fEx);
            XmlManager.VerifyConfigExists(path);
            Debug.Log($"Client received: PATH:{path}");
            if (!IsServer)
            {
                XmlManager.SetupPaths();
                File.WriteAllBytes(path, byteArray);
            }
        }


        //Get byte array of all XML
    }
}