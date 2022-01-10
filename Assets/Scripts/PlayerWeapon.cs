using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<Vector3> weaponPosition;

    [SerializeField] private SpriteRenderer outerCircle;
    [SerializeField] private SpriteRenderer innerCircle;

    private void OnEnable()
    {
        weaponPosition.Value = Vector3.zero;
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            var worldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var newPos = new Vector3(worldSpace.x, worldSpace.y, 0);
            var weaponTransform = transform;
            weaponTransform.position = newPos;
            UpdateClientWeaponPositionServerRPC(weaponTransform.position);
        }
        if (IsServer)
        {
            UpdateServer();
        }
    }

    private void UpdateServer()
    {
        transform.position = weaponPosition.Value;
    }


    private void SetOuterCircleColour(Color colour)
    {
        outerCircle.color = colour;
    }

    private void SetInnerCircleColour(Color colour)
    {
        innerCircle.color = colour;
    }

    [ServerRpc]
    private void UpdateClientWeaponPositionServerRPC(Vector3 weaponPos)
    {
        weaponPosition.Value = weaponPos;
    }
}