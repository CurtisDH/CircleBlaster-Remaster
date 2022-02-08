using System;
using Enemy;
using Managers;
using Particle_Scripts;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Utility;
using Utility.Text;

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
    [SerializeField] private bool _isServerProjectile = true;

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
        _isServerProjectile = isServerProjectile;
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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;

        if (IsServer)
        {
            var target = col.gameObject;
            hitTarget = target;
            if (!_isServerProjectile)
                projectileDamage = 0;

            EventManager.Instance.InvokeOnProjectileHitEvent(target, projectileDamage);

            if (_isServerProjectile && networkObject.IsSpawned)
            {
                DespawnProjectileServerRPC();
            }
        }

        OnDeath();
    }

    private void OnDeath()
    {
        Debug.Log("hello");
        GenerateWorldSpaceText.CreateWorldSpaceTextPopup("OnDeath", Vector3.zero, 2, 2, Color.green);
        var deathParticle = ObjectPooling.Instance.RequestComponentFromPool<PlayerCollisionProjectileParticle>();
        deathParticle.transform.position = transform.position;
        deathParticle.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        ObjectPooling.Instance.PoolObject(this);
    }

    [ServerRpc]
    private void DespawnProjectileServerRPC()
    {
        //TODO Particle display
        networkObject.Despawn();
    }
}