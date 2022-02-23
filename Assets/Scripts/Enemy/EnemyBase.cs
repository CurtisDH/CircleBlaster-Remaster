using System.Collections;
using System.Collections.Generic;
using Managers;
using Particle_Scripts;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Enemy
{
    public class EnemyBase : NetworkBehaviour
    {
        [SerializeField] public NetworkObject networkObject;

        [SerializeField] private float speed;
        [SerializeField] private float damage;
        [SerializeField] private List<Color> colours;
        [SerializeField] private float initialHealth;
        [SerializeField] private NetworkVariable<float> health;

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        [SerializeField] private Transform closestPlayerTransform;
        private bool _colourConfigured;

        [SerializeField] private float _scale;
        private string _uniqueID;
        [SerializeField] private bool _initialSetupCompleted;

        public void SetClosestPlayerTransform(Transform transform)
        {
            closestPlayerTransform = transform;
        }

        private void OnEnable()
        {
            StartCoroutine(SetupEnemy());
        }

        IEnumerator SetupEnemy()
        {
            while (!_initialSetupCompleted)
            {
                yield return new WaitForSeconds(1);
            }

            SubscribeEvents();

            if (_colourConfigured)
            {
                yield return null;
            }

            gameObject.name = _uniqueID; //TODO should we use a public name here instead

            foreach (var sr in spriteRenderers)
            {
                if (colours.Count <= 0) continue;

                sr.color = colours[^1];

                colours.RemoveAt(colours.Count - 1);
                sr.transform.localScale += new Vector3(_scale, _scale, _scale);
                continue;
            }

            _colourConfigured = true;
            yield return null;
        }

        private void SubscribeEvents()
        {
            if (!IsServer) return;

            health.OnValueChanged += CheckIfDead;
            EventManager.Instance.OnProjectileHitEvent += OnProjectileHit;
            //Adds enemy to active enemy list (GameManager)
            EventManager.Instance.OnEndGameEvent += EndGame;
            EventManager.Instance.InvokeEnemySpawnEvent(this, true);
        }

        private void UnsubscribeEvents()
        {
            health.OnValueChanged -= CheckIfDead;
            EventManager.Instance.OnEndGameEvent -= EndGame;
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
            if (!_initialSetupCompleted)
            {
                return;
            }

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
            if (networkObject.IsSpawned)
                networkObject.Despawn();
        }


        private void OnDeath()
        {
            var deathParticle = ObjectPooling.Instance.RequestComponentFromPool<EnemyDeathParticle>();
            deathParticle.transform.position = transform.position;
            deathParticle.gameObject.SetActive(true);
        }

        public void EndGame()
        {
            DespawnEnemyServerRpc();
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

        public void InitialSetup(float eSpeed, float eDamage, List<Color> eColours, float eHealth,
            float eScale, string eUniqueID)
        {
            this.speed = eSpeed;
            this.damage = eDamage;
            this.colours = eColours;
            this.initialHealth = eHealth;
            this._scale = eScale;
            this._uniqueID = eUniqueID;
            if (IsServer)
            {
                health.Value = initialHealth;
            }

            _initialSetupCompleted = true;
        }
    }
}