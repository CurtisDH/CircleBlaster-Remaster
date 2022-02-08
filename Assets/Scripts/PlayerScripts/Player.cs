using System;
using System.Collections;
using Managers;
using Particle_Scripts;
using PlayerScripts.Camera;
using Unity.Netcode;
using UnityEngine;
using Utility;
using Utility.Text;

namespace PlayerScripts
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private int teamID;
        [SerializeField] private SpriteRenderer teamColourSpriteRenderer;
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField] private float health;

        //Camera script instead.
        [SerializeField] private CameraController playerCam;

        //Used for retrieving the network pool.
        [SerializeField] private GameObject projectilePrefab;

        [SerializeField] float moveSpeed = 10;

        [SerializeField] private ulong clientID;

        private bool _playerReady;
        private bool _playerCanMove;

        private void OnEnable()
        {
            SubscribeEvents();
            if (!_playerReady)
            {
                StartCoroutine(InitialisePlayer());
                return;
            }
            //TODO Allow customisation within a UI menu.
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents()
        {
            EventManager.Instance.OnEnemyHitEvent -= OnPlayerEnemyHitEvent;
        }

        private void SubscribeEvents()
        {
            EventManager.Instance.OnEnemyHitEvent += OnPlayerEnemyHitEvent;
            EventManager.Instance.InvokeOnPlayerSpawn(this);
        }

        private void OnPlayerEnemyHitEvent(GameObject obj, float damage)
        {
            if (obj != gameObject) return;

            health -= damage;
            if (health <= 0)
            {
                PlayerDeath();
                return;
            }

            DamagePlayerEffect();
        }

        private void DamagePlayerEffect()
        {
            var damageParticle = ObjectPooling.Instance.RequestComponentFromPool<DamagePlayerParticle>();
            damageParticle.transform.position = transform.position;
            damageParticle.gameObject.SetActive(true);
        }


        private void PlayerDeath()
        {
            if (!IsOwner)
            {
                return;
            }

            _playerReady = false;
            _playerCanMove = false;
            //Not sure how this works with network objects
            var localID = NetworkManager.Singleton.LocalClientId;
            Debug.Log($"Invoking LocalclientID:{localID}");
            EventManager.Instance.InvokeOnPlayerDeath(this);
            playerCam.SetTargetTransform(null);
            gameObject.SetActive(false);

            //StartCoroutine(RespawnPlayer());

            //Camera will do nothing until reassigned at OnEnable

            //gameObject.SetActive(false);
            //TODO respawn the player
            //Round detection - Event -> respawn?
            // Revive system? - Create a circle area that the other player/s have to enter to revive the downed player.
            //TODO change camera tracking to nearest ally (also allow for switching between players)
        }

        private IEnumerator InitialisePlayer()
        {
            while (!_playerReady)
            {
                GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Spawning...",
                    transform.position, 1.5f, 2, Color.yellow,
                    0.25f);

                yield return new WaitForSeconds(2);
                SetupCamera();
                SetPlayerColours();

                _playerReady = true;
            }

            GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Spawned",
                transform.position, 1.5f, 2, Color.green, 0.25f);
            _playerCanMove = true;
        }

        private void SetPlayerColours()
        {
            teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
            weapon.SetOuterCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 1));
            weapon.SetInnerCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 2));
        }


        private void SetupCamera()
        {
            if (!IsLocalPlayer) return;

            playerCam.SetTargetTransform(this.transform);
            playerCam.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!IsClient || !IsOwner || !_playerCanMove) return;
            weapon.UpdatePosition();
            PlayerMovement();
            Shoot();
        }

        private void Shoot()
        {
            if (Input.GetKeyDown(KeybindManager.Instance.shootPrimary))
            {
                //Client
                var projectile = ObjectPooling.Instance.RequestComponentFromPool<Projectile>();
                projectile.ProjectileSetup(teamID, moveSpeed, 1);

                projectile.ProjectileSetup(teamID, moveSpeed, 1);
                projectile.SetServerProjectileStatus(false);
                SetupProjectilePosition(projectile.gameObject);
                //Server
                var id = NetworkManager.LocalClientId;
                if (UIManager.Instance.IsHosting())
                {
                    id = UInt64.MaxValue;
                }

                RequestProjectileSpawnServerRPC(id);
            }
        }

        [ServerRpc]
        private void RequestProjectileSpawnServerRPC(ulong clientID)
        {
            var id = clientID;
            var projectile = NetworkObjectPooling.Instance.GetNetworkObject(projectilePrefab);
            SetupProjectilePosition(projectile.gameObject);
            projectile.Spawn();
            //Stops the client from seeing the server sided projectile they just fired.
            if (id != ulong.MaxValue)
            {
                projectile.NetworkHide(id);
            }
        }

        private void SetupProjectilePosition(GameObject projectile)
        {
            projectile.SetActive(true);
            var projectileTransform = projectile.transform;
            var weaponTransform = weapon.transform;
            projectileTransform.rotation = weaponTransform.rotation;
            projectileTransform.position = weaponTransform.position;
        }

        private void PlayerMovement()
        {
            float hoz = 0;
            float vert = 0;
            hoz = KeyDown(hoz, ref vert);
            vert = ReleaseKey(vert, ref hoz);
            var direction = new Vector2(hoz, vert);
            direction = Vector3.ClampMagnitude(direction, 1f);
            transform.Translate(direction * (Time.deltaTime * moveSpeed));
        }


        private float KeyDown(float hoz, ref float vert)
        {
            if (Input.GetKey(KeybindManager.Instance.moveLeft))
            {
                hoz = -1;
            }

            if (Input.GetKey(KeybindManager.Instance.moveRight))
            {
                hoz = 1;
            }

            if (Input.GetKey(KeybindManager.Instance.moveDown))
            {
                vert = -1;
            }

            if (Input.GetKey(KeybindManager.Instance.moveUp))
            {
                vert = 1;
            }

            return hoz;
        }

        private float ReleaseKey(float vert, ref float hoz)
        {
            if (Input.GetKeyUp(KeybindManager.Instance.moveUp) || Input.GetKeyUp(KeybindManager.Instance.moveDown))
            {
                vert = 0;
            }

            if (Input.GetKeyUp(KeybindManager.Instance.moveLeft) || Input.GetKeyUp(KeybindManager.Instance.moveRight))
            {
                hoz = 0;
            }

            return vert;
        }
    }
}