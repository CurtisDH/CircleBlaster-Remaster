using Managers;
using Unity.Netcode;
using UnityEngine;
using Utility;

public class Player : NetworkBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] private SpriteRenderer teamColourSpriteRenderer;
    [SerializeField] private PlayerWeapon weapon;

    [SerializeField] float moveSpeed = 10;

    private void OnEnable()
    {
        //TODO Allow customisation within a UI menu.
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
        weapon.SetOuterCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 1));
        weapon.SetInnerCircleColour(PlayerManager.Instance.GetColourFromTeamID(teamID + 2));
    }

    private void Update()
    {
        if (IsClient && IsOwner)
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
            projectile.gameObject.SetActive(true);
            projectile.ProjectileSetup(teamID,moveSpeed,1,weapon.transform);
            //Server
            //RequestProjectileSpawnServerRPC();
        }
    }
    
    [ServerRpc]
    private void RequestProjectileSpawnServerRPC()
    {
        var projectile = ObjectPooling.Instance.RequestComponentFromPool<Projectile>();
        projectile.ProjectileSetup(teamID, moveSpeed, 1, weapon.transform);
        projectile.gameObject.SetActive(true);
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