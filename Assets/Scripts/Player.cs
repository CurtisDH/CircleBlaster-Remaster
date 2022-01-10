using Managers;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] private SpriteRenderer teamColourSpriteRenderer;
    [SerializeField] private PlayerWeapon weapon;

    [SerializeField] float moveSpeed = 10;

    private void OnEnable()
    {
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            UpdatePlayerWeapon();
            PlayerMovement();
        }
    }

    private void PlayerMovement()
    {
        float hoz = 0;
        float vert = 0;
        hoz = KeyDown(hoz, ref vert);
        vert = ReleaseKey(vert, ref hoz);
        var direction = new Vector2(hoz, vert);
        direction = Vector3.ClampMagnitude(direction, 1f);
        transform.Translate(direction * Time.deltaTime * moveSpeed);
    }

    private void UpdatePlayerWeapon()
    {
        var worldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var newPos = new Vector3(worldSpace.x, worldSpace.y, 0);
        weapon.transform.position = newPos;
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