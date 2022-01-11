using System;
using Unity.Netcode;
using UnityEngine;
using Utility;

public class Projectile : NetworkBehaviour
{
    private int _teamID;
    [SerializeField] private float projectileSpeed;

    [SerializeField] public NetworkObject networkObject;
    private int _pierceLevel;
    [SerializeField] private Vector2 startPos;

    private const int DespawnRange = 50;

    private const int SpeedIncrement = 2;

    public void ProjectileSetup(int teamId, float speed, int pierce, Transform playerWeaponTransform)
    {
        _teamID = teamId;
        projectileSpeed = speed + SpeedIncrement;
        _pierceLevel = pierce;

        var projectileTransform = transform;
        var weaponPosition = playerWeaponTransform.position;

        projectileTransform.rotation = playerWeaponTransform.rotation;
        projectileTransform.position = weaponPosition;
        startPos = weaponPosition;
    }

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
        if (!networkObject.IsSpawned && IsServer)
            networkObject.Spawn();
    }

    private void OnDisable()
    {       
        //TODO fix
        //I don't think pooling works properly like this..
        DespawnProjectileServerRPC();
        ObjectPooling.Instance.PoolObject(this);
    }

    [ServerRpc]
    private void DespawnProjectileServerRPC()
    {
        networkObject.Despawn();
    }
}