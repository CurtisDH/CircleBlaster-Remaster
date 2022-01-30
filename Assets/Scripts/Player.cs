using System;
using System.Collections;
using Managers;
using Unity.Netcode;
using UnityEngine;
using Utility;
using Utility.Text;

public class Player : NetworkBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] private SpriteRenderer teamColourSpriteRenderer;
    [SerializeField] private PlayerWeapon weapon;


    //Used for retrieving the network pool.
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] float moveSpeed = 10;

    [SerializeField] private ulong clientID;

    private bool _playerReady;
    private bool _playerCanMove;

    private void OnEnable()
    {
        StartCoroutine(InitialisePlayer());
        //TODO Allow customisation within a UI menu.
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
        weapon.SetOuterCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 1));
        weapon.SetInnerCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 2));
    }
    
    IEnumerator InitialisePlayer()
    {
        while (!_playerReady)
        {
            GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Loading...",
                transform.position, 1.5f, 2, Color.yellow,
                0.25f);
            yield return new WaitForSeconds(2);
            //Only updates for the server
            clientID = (ulong)PlayerConnectionManager.Instance.mostRecentClientConnectionID.Value;
            _playerReady = true;
        }

        GenerateWorldSpaceText.CreateWorldSpaceTextPopup("Loaded",
            transform.position, 1.5f, 2, Color.green, 0.25f);
        _playerCanMove = true;
    }


    private void Update()
    {
        if (IsClient && IsOwner && _playerCanMove)
        {
            weapon.UpdatePosition();
            PlayerMovement();
            Shoot();
        }
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
        SpawnManager.Instance.SpawnEnemy();
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