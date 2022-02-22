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
        [SerializeField] private float initialHealth = 100f;
        [SerializeField] private NetworkVariable<float> health;
        [SerializeField] private NetworkVariable<bool> isDead;

        //Camera script instead.
        [SerializeField] private CameraController playerCam;

        //Used for retrieving the network pool.
        [SerializeField] private GameObject projectilePrefab;

        [SerializeField] float moveSpeed = 10;

        [SerializeField] private ulong clientID;

        private bool _playerReady;
        private bool _playerCanMove;
        [SerializeField]
        private string playerWeaponUniqueID = "weapon_fast";

        private void OnEnable()
        {
            EventManager.Instance.OnDataDeserialization += OnDataDeserialization;

            //TODO Allow customisation within a UI menu.
        }

        private void OnDataDeserialization()
        {
            if (!_playerReady)
            {
                StartCoroutine(InitialisePlayer());
                return;
            }
        }

        public void SetDeathValue(bool value)
        {
            if (IsServer)
                isDead.Value = value;
        }

        private IEnumerator InitialisePlayer()
        {
            while (!_playerReady)
            {
                GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Spawning...",
                    transform.position, 1.5f, 2, Color.yellow,
                    0.25f);
                ResetData();
                yield return new WaitForSeconds(2);
                if (isDead.Value) //Update any clients that join half way through? Not sure how this will function yet
                {
                    PlayerDeath();
                }

                if (GameManager.Instance.IsWaveActive())
                {
                    if (!isDead.Value)
                    {
                        //TODO do this better, currently the player connecting mid round will be left on a frozen screen
                        PlayerDeath(); // wont mess with the current session
                    }

                    continue;
                }

                SubscribeClientEvents();
                SetupPlayerTag("Player");
                SubscribeServerEvents();
                SetupCamera(this.transform, true);
                SetPlayerColours();

                SendPlayerSpawnEvent();
                _playerReady = true;
            }

            GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Spawned",
                transform.position, 1.5f, 2, Color.green, 0.25f);
            _playerCanMove = true;
        }

        private void ResetData()
        {
            if (IsServer)
            {
                health.Value = initialHealth;
            }

            SetupPlayerTag("Respawn");

            var alivePlayer = SpawnManager.Instance.GetActivePlayer();
            if (alivePlayer != null)
            {
                SetupCamera(alivePlayer.transform, true);
            }
            else
            {
                SetupCamera(null, false);
            }
        }

        private void SetupPlayerTag(string tag)
        {
            gameObject.tag = tag;
        }

        private void SubscribeClientEvents()
        {
            health.OnValueChanged += OnPlayerHealthChange;
        }

        private void SubscribeServerEvents()
        {
            if (IsServer)
                EventManager.Instance.OnEnemyHitEvent += OnPlayerEnemyHitEvent;
        }

        private void OnPlayerHealthChange(float previousvalue, float newvalue)
        {
            if (health.Value <= 0)
            {
                PlayerDeath();
                return;
            }

            DamagePlayerEffect();
        }

        private void SetupCamera(Transform followTransform, bool setActive)
        {
            if (!IsLocalPlayer) return;

            playerCam.SetTargetTransform(followTransform);
            playerCam.gameObject.SetActive(setActive);
        }


        private void UnsubscribeEvents()
        {
            UnsubscribeServerEvents();
            health.OnValueChanged -= OnPlayerHealthChange;
        }

        private void UnsubscribeServerEvents()
        {
            if (IsServer)
                EventManager.Instance.OnEnemyHitEvent -= OnPlayerEnemyHitEvent;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }


        private void OnPlayerEnemyHitEvent(GameObject obj, float damage)
        {
            if (obj != gameObject || !IsServer) return;

            health.Value -= damage;
        }

        private void DamagePlayerEffect()
        {
            var damageParticle = ObjectPooling.Instance.RequestComponentFromPool<DamagePlayerParticle>();
            damageParticle.transform.position = transform.position;
            damageParticle.gameObject.SetActive(true);
        }


        private void PlayerDeath()
        {
            if (IsServer)
            {
                isDead.Value = true;
            }

            _playerReady = false;
            _playerCanMove = false;
            //Tells the spawn manager we're dead
            EventManager.Instance.InvokeOnPlayerDeath(this);
            var activePlayers = SpawnManager.Instance.GetAllAlivePlayers();
            Transform activePlayerTransform = null;
            if (activePlayers.Count > 0)
            {
                activePlayerTransform = activePlayers[0].transform;
            }

            playerCam.SetTargetTransform(activePlayerTransform != null ? activePlayerTransform : null);
            gameObject.SetActive(false);

            //TODO respawn the player
            //Round detection - Event -> respawn?
            // Revive system? - Create a circle area that the other player/s have to enter to revive the downed player.
            //TODO change camera tracking to nearest ally (also allow for switching between players)
        }


        private void SetPlayerColours()
        {
            //TODO allow for player customisation
            //TODO rework this, after every wave the player colour is dependent on the death order.
            var id = SpawnManager.Instance.GetAllAlivePlayers().Count;
            teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(id);
            weapon.SetOuterCircleColour(PlayerManager.Instance.GetColourFromTeamID(id + 1));
            weapon.SetInnerCircleColour(PlayerManager.Instance.GetColourFromTeamID(id + 2));
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
                var projectile = ObjectPooling.Instance.RequestComponentUsingUniqueID<Projectile>(playerWeaponUniqueID);
                //TODO do this by id, otherwise we wont have customisable projectiles.
                //projectile.ProjectileSetup(teamID, moveSpeed, 1);
                //TODO projectile needs to be based on whatever weapon is equipped
                //TODO the entire weapon system
                projectile.SetServerProjectileStatus(false);
                SetupProjectilePosition(projectile.gameObject);
                //Server
                var id = NetworkManager.LocalClientId;
                if (UIManager.Instance.IsHosting())
                {
                    id = UInt64.MaxValue;
                }

                RequestProjectileSpawnServerRPC(id,playerWeaponUniqueID);
            }
        }

        [ServerRpc]
        private void RequestProjectileSpawnServerRPC(ulong clientID,string projectileID)
        {
            //TODO probably gonna error here
            var id = clientID;
            var projectile =
                NetworkObjectPooling.Instance.GetNetworkObject(
                    SpawnManager.Instance.GetObjectFromUniqueID(playerWeaponUniqueID));
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

        private void SendPlayerSpawnEvent()
        {
            EventManager.Instance.InvokeOnPlayerSpawn(this);
        }
    }
}