using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility;

namespace Managers
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private TMP_InputField portInputField;
        [SerializeField] private PlayerConnectionManager connectionManager;

        [FormerlySerializedAs("_transport")] [SerializeField]
        private UNetTransport transport;


        private bool _hosting = false;
        private bool _connected = false;
        private bool _dedicated = false;

        private Button _stopButton;

        public void StartHost()
        {
            _hosting = !_hosting;
            Debug.Log(_hosting);
            Debug.Log(_connected);
            Debug.Log(_dedicated);
            SetPort();
            if (_hosting && !_connected && !_dedicated)
            {
                Debug.Log("Started");
                NetworkManager.Singleton.StartHost();
                connectionManager.gameObject.SetActive(true);
                return;
            }
        }
    
        public void Shutdown()
        {
            _hosting = false;
            _connected = false;
            _dedicated = false;
            Debug.Log("Shutdown");
            connectionManager.gameObject.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }

        public void StartConnected()
        {
            _connected = !_connected;
            if (ipAddressInputField.text != "")
            {
                transport.ConnectAddress = ipAddressInputField.text;
            }

            SetPort();
            Debug.Log(_hosting);
            Debug.Log(_connected);
            Debug.Log(_dedicated);
            if (!_hosting && _connected && !_dedicated)
            {
                NetworkManager.Singleton.StartClient();
                connectionManager.gameObject.SetActive(true);
                return;
            }
        }

        public void StartDedicated()
        {
            _dedicated = !_dedicated;
            Debug.Log(_hosting);
            Debug.Log(_connected);
            Debug.Log(_dedicated);
            SetPort();
            if (!_hosting && !_connected && _dedicated)
            {
                NetworkManager.Singleton.StartServer();
                return;
            }

            _dedicated = false;
            Debug.Log("Shutdown");
            NetworkManager.Singleton.Shutdown();
        }

        private void SetPort()
        {
            if (portInputField.text == "") return;
            if (portInputField.text.Length <= 5)
            {
                int port = int.Parse(portInputField.text);
                transport.ConnectPort = port;
                transport.ServerListenPort = port;
            }
        }
    }
}