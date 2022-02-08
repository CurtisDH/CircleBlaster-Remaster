using System;
using System.Collections.Generic;
using Managers;
using Particle_Scripts;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Enemy
{
    public abstract class EnemyBase : NetworkBehaviour
    {
        [SerializeField] public NetworkObject networkObject;

        //[SerializeField] private float health;
        [SerializeField] private float speed;
        [SerializeField] private float damage;
        [SerializeField] private List<Color> colours;
        [SerializeField] private float initialHealth;
        [SerializeField] private NetworkVariable<float> health;

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        [SerializeField] private Transform closestPlayerTransform;
        private bool _colourConfigured;

        public void SetClosestPlayerTransform(Transform transform)
        {
            closestPlayerTransform = transform;
        }
        
        private void OnEnable()
        {
            SubscribeEvents();
            
            if (_colourConfigured)
            {
                return;
            }
            foreach (var sr in spriteRenderers)
            {
                if (colours.Count <= 0) continue;

                sr.color = colours[^1];
                colours.RemoveAt(colours.Count - 1);
                continue;
            }

            _colourConfigured = true;
        }

        private void SubscribeEvents()
        {
            if (!IsServer) return;

            health.OnValueChanged += CheckIfDead;
            EventManager.Instance.OnProjectileHitEvent += OnProjectileHit;
            //Adds enemy to active enemy list (GameManager)
            EventManager.Instance.InvokeEnemySpawnEvent(this, true);
        }

        private void UnsubscribeEvents()
        {
            health.OnValueChanged -= CheckIfDead;
            EventManager.Instance.OnProjectileHitEvent -= OnProjectileHit;
            EventManager.Instance.InvokeEnemySpawnEvent(this, false);

            //Removes enemy from active enemy list (GameManager)
        }

        [ServerRpc]
        private void OnHitServerRpc(float damage)
        {
            health.Value -= damage;
        }

        private void OnDisable()
        {
            OnDeath();
            if (!IsServer) return;

            UnsubscribeEvents();
        }


        private void CheckIfDead(float previousvalue, float newvalue)
        {
            if (newvalue <= 0)
            {
                health.Value = initialHealth;
                DespawnEnemyServerRpc();
            }
        }

        private void OnProjectileHit(GameObject obj, float damage)
        {
            if (obj == gameObject && damage > 0)
            {
                OnHitServerRpc(damage);
            }
        }

        [ServerRpc]
        private void DespawnEnemyServerRpc()
        {
            networkObject.Despawn();
        }


        private void OnDeath()
        {
            var deathParticle = ObjectPooling.Instance.RequestComponentFromPool<EnemyDeathParticle>();
            deathParticle.transform.position = transform.position;
            deathParticle.gameObject.SetActive(true);
        }


        private void Update()
        {
            if (closestPlayerTransform != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, closestPlayerTransform.position,
                    speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;


            if (IsServer)
            {
                health.Value = 0;
            }

            EventManager.Instance.InvokeOnEnemyHitEvent(col.gameObject, damage);
        }
    }
}