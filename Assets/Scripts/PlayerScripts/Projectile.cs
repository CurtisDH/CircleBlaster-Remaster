using System.Collections.Generic;
using Managers;
using Particle_Scripts;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace PlayerScripts
{
    public class Projectile : NetworkBehaviour
    {
        private int _teamID;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float projectileDamage = 1;

        [SerializeField] public NetworkObject networkObject;
        [SerializeField] private GameObject hitTarget;

        /*public delegate void ProjectileHitEvent(GameObject target, float damage);

    public static event ProjectileHitEvent OnProjectileHitEvent;*/

        //This currently only determines whether or not it should be in the client pool. It will influence hit reg later on.
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        [SerializeField] private List<Color> colours;

        [SerializeField] private string projectileUniqueID;


        private int _pierceLevel;
        [SerializeField] private Vector2 startPos;
        private Transform _weaponTransform;
        private bool _initialSetupCompleted;
        private float scale;

        private const int DespawnRange = 50;

        private const int SpeedIncrement = 2;


        private void Update()
        {
            if (transform.position.y >= startPos.y + DespawnRange ||
                transform.position.y <= startPos.y - DespawnRange ||
                transform.position.x >= startPos.x + DespawnRange ||
                transform.position.x <= startPos.x - DespawnRange)
            {
                gameObject.SetActive(false);
            }

            transform.Translate(Vector2.left * (projectileSpeed * Time.deltaTime));
        }

        private void OnEnable()
        {
            startPos = transform.position;
            //We create an object pool before listening. This determines if its a server or a client pool. 
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Enemy")) return;


            var target = col.gameObject;
            hitTarget = target;

            EventManager.Instance.InvokeOnProjectileHitEvent(target, projectileDamage);


            OnDeath();
        }

        private void OnDeath()
        {
            var deathParticle = ObjectPooling.Instance.RequestComponentFromPool<PlayerCollisionProjectileParticle>();
            deathParticle.transform.position = transform.position;
            deathParticle.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }


        private void OnDisable()
        {
            ObjectPooling.Instance.PoolUniqueIDObject(projectileUniqueID, this.gameObject);
        }


        public void InitialSetup(float pSpeed, float pDamage, List<Color> pColours, int pPierce, float pScale,
            string pUniqueID)
        {
            if (_initialSetupCompleted)
            {
                return;
            }

            projectileSpeed = pSpeed;
            projectileDamage = pDamage;
            colours = pColours;
            _pierceLevel = pPierce;
            scale = pScale;
            projectileUniqueID = pUniqueID;
            foreach (var sr in spriteRenderers)
            {
                if (colours.Count <= 0) continue;

                sr.color = colours[^1];

                colours.RemoveAt(colours.Count - 1);
                sr.transform.localScale += new Vector3(scale, scale, scale);
                continue;
            }

            _initialSetupCompleted = true;
        }
    }
}