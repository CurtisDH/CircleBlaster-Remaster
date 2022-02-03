using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public abstract class EnemyBase : NetworkBehaviour
    {
        [SerializeField] public NetworkObject networkObject;

        //[SerializeField] private float health;
        [SerializeField] private float speed;
        [SerializeField] private float damage;
        [SerializeField] private List<Color> colours;

        [SerializeField] private NetworkVariable<float> health;

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        [SerializeField] public Transform closestPlayerTransform;
        private bool colourConfigured;

        private void OnEnable()
        {
            if (IsServer)
            {
                health.OnValueChanged += CheckIfDead;
                Projectile.OnProjectileHitEvent += OnProjectileHit;
            }

            if (colourConfigured)
            {
                return;
            }

            foreach (var sr in spriteRenderers)
            {
                if (colours.Count > 0)
                {
                    sr.color = colours[^1];
                    colours.RemoveAt(colours.Count - 1);
                    continue;
                }
            }

            colourConfigured = true;
        }

        [ServerRpc]
        private void OnHitServerRpc(float damage)
        {
            health.Value -= damage;
        }

        private void OnDisable()
        {
            if (!IsServer) return;


            health.OnValueChanged -= CheckIfDead;
            Projectile.OnProjectileHitEvent -= OnProjectileHit;
        }

        private void CheckIfDead(float previousvalue, float newvalue)
        {
            if (newvalue <= 0)
            {
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
            //Spawn Manager particles?

            networkObject.Despawn();
        }


        private void Update()
        {
            if (closestPlayerTransform != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, closestPlayerTransform.position,
                    speed * Time.deltaTime);
            }
        }
    }
}