using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] private SpriteRenderer teamColourSpriteRenderer;

    [SerializeField] float moveSpeed = 10;

    private NetworkVariable<Vector3> _position = new();

    private void OnEnable()
    {
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            float hoz = 0;
            float vert = 0;


            hoz = KeyDown(hoz, ref vert);
            vert = ReleaseKey(vert, ref hoz);

            var direction = new Vector2(hoz, vert);
            direction = Vector3.ClampMagnitude(direction, 1f);

            
            transform.Translate(direction * Time.deltaTime * moveSpeed);
            SubmitPositionRequestServerRPC(transform.position);
        }
    }

    [ServerRpc]
    private void SubmitPositionRequestServerRPC(Vector3 position, ServerRpcParams rpcParams = default)
    {
        _position.Value = position;
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