using System;
using Unity.Netcode;
using UnityEngine;
using Utility;

public class Projectile : NetworkBehaviour
{
    private int _teamID;
    [SerializeField] private float projectileSpeed;

    [SerializeField] public NetworkObject networkObject;

    //This currently only determines whether or not it should be in the client pool. It will influence hit reg later on.
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
    
    public void ProjectileSetup(int teamId, float speed, int pierce)
    {
        _teamID = teamId;
        projectileSpeed = speed + SpeedIncrement;
        _pierceLevel = pierce;
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
        startPos = transform.position;
        //We create an object pool before listening. This determines if its a server or a client pool. 
        if (!NetworkManager.IsListening)
        {
            _isServerProjectile = false;
        }
    }

    private void HitRegistration()
    {
        if (!_isServerProjectile)
        {
            // do hit reg
            DespawnProjectileServerRPC();
            //Despawn the network object particles etc.
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
        //Particle display
        networkObject.Despawn();
    }
}