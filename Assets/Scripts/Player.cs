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

    [SerializeField] private NetworkVariable<float> upDownPos = new();
    [SerializeField] private NetworkVariable<float> leftRightPos = new();


    [SerializeField] float oldUpDownPos;
    [SerializeField] private float oldLeftRightPos;


    private void OnEnable()
    {
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient && IsOwner)
        {
            float hoz = 0;
            float vert = 0;


            hoz = KeyDown(hoz, ref vert);


            var direction = new Vector2(hoz, vert);
            direction = Vector3.ClampMagnitude(direction, 1f);


            //transform.Translate(direction * Time.deltaTime * moveSpeed);
            if (hoz != oldLeftRightPos || vert != oldUpDownPos)
            {
                var position = transform.position;
                UpdateClientPositionServerRPC(vert, hoz);
                oldLeftRightPos = hoz;
                oldUpDownPos = vert;
            }
        }
    }


    private void UpdateServer()
    {
        var position = transform.position;
        position = new Vector3(position.x + leftRightPos.Value,
            position.y + upDownPos.Value, 0);
        transform.position = position;
    }


    private float KeyDown(float hoz, ref float vert)
    {
        if (Input.GetKey(KeybindManager.Instance.moveLeft))
        {
            hoz -= (moveSpeed / 100);
        }

        if (Input.GetKey(KeybindManager.Instance.moveRight))
        {
            hoz += (moveSpeed / 100);
        }

        if (Input.GetKey(KeybindManager.Instance.moveDown))
        {
            vert -= (moveSpeed / 100);
        }

        if (Input.GetKey(KeybindManager.Instance.moveUp))
        {
            vert += (moveSpeed / 100);
        }

        return hoz;
    }

    [ServerRpc]
    public void UpdateClientPositionServerRPC(float upDown, float leftRight)
    {
        upDownPos.Value = upDown;
        leftRightPos.Value = leftRight;
    }
}