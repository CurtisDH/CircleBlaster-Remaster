using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Particle_Scripts
{
    public abstract class DeathParticleBase : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _networkObject;

        void OnEnable()
        {
            if (!IsServer) return;

            if (_networkObject == null)
            {
                _networkObject = GetComponent<NetworkObject>();
            }

            StartCoroutine(DespawnTimer());
        }

        IEnumerator DespawnTimer()
        {
            yield return new WaitForSeconds(2);
            DespawnParticle();
        }

        private void OnDisable()
        {
            ObjectPooling.Instance.PoolObject(this);
        }

        void DespawnParticle()
        {
            gameObject.SetActive(false);
        }
    }
}