using System;
using Unity.Netcode;
using UnityEngine;
using Utility;

public class Projectile : NetworkBehaviour
{
    private int _teamID;
    [SerializeField] private float projectileSpeed;

    [SerializeField] public NetworkObject networkObject;
    private bool _isServerProjectile = true;
    public bool IsClientProjectile()
    {
        return IsClient && !IsServer;
    }

    private int _pierceLevel;
    [SerializeField] private Vector2 startPos;
    private Transform _weaponTransform;

    private const int DespawnRange = 50;

    private const int SpeedIncrement = 2;

    public void SetServerProjectileStatus(bool isServerProjectile)
    {
        _isServerProjectile = false;
    }
    
    public void ProjectileSetup(int teamId, float speed, int pierce, Transform playerWeaponTransform)
    {
        _weaponTransform = playerWeaponTransform;
        _teamID = teamId;
        projectileSpeed = speed + SpeedIncrement;
        _pierceLevel = pierce;

        var projectileTransform = transform;
        projectileTransform.rotation = _weaponTransform.rotation;
        projectileTransform.position = _weaponTransform.position;
        startPos = _weaponTransform.position;
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
        //We create an object pool before listening. This determines if its a server or a client pool. 
        if (!NetworkManager.IsListening)
        {
            _isServerProjectile = false;
        }
    }

    private void OnDisable()
    {
        //TODO fix
        //I don't think pooling works properly like this..
        //DespawnProjectileServerRPC();
        if (!_isServerProjectile)
        {
            ObjectPooling.Instance.PoolObject(this);
        }
    }

    [ServerRpc]
    private void DespawnProjectileServerRPC()
    {
        networkObject.Despawn();
    }
}