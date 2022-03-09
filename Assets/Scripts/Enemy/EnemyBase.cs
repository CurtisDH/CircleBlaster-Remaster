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
        [SerializeField] private float health;

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        [SerializeField] private Transform closestPlayerTransform;
        private bool _colourConfigured;

        [SerializeField] private float _scale;
        [SerializeField] private string _uniqueID;
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
            ObjectPooling.Instance.PoolUniqueIDObject(_uniqueID, this.gameObject, false);

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
            EventManager.Instance.OnProjectileHitEvent += OnProjectileHit;
            //Adds enemy to active enemy list (GameManager)
            EventManager.Instance.OnEndGameEvent += EndGame;
            EventManager.Instance.InvokeEnemySpawnEvent(this, true);
        }

        private void UnsubscribeEvents()
        {
            EventManager.Instance.OnEndGameEvent -= EndGame;
            EventManager.Instance.OnProjectileHitEvent -= OnProjectileHit;
            EventManager.Instance.InvokeEnemySpawnEvent(this, false);

            //Removes enemy from active enemy list (GameManager)
        }


        private void OnDisable()
        {
            if (!_initialSetupCompleted)
            {
                Debug.Log("Initial Setup not completed.");
                return;
            }

            //OnDeath();
            UnsubscribeEvents();
        }


        private void CheckIfDead()
        {
            if (health <= 0)
            {
                health = initialHealth;
                OnDeath();
            }
        }

        private void OnProjectileHit(GameObject obj, float damage)
        {
            if (obj == gameObject && damage > 0)
            {
                ReceiveDamage(damage);
            }
        }


        private void ReceiveDamage(float damage)
        {
            health -= damage;
            CheckIfDead();
        }

        private void OnDeath()
        {
            var deathParticle = ObjectPooling.Instance.RequestComponentFromPool<EnemyDeathParticle>();
            deathParticle.transform.position = transform.position;
            deathParticle.gameObject.SetActive(true);
            ObjectPooling.Instance.PoolUniqueIDObject(_uniqueID, this.gameObject);
            gameObject.SetActive(false);
        }

        public void EndGame()
        {
            OnDeath();
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


            health = 0;
            CheckIfDead();


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

            health = initialHealth;


            _initialSetupCompleted = true;
        }
    }
}