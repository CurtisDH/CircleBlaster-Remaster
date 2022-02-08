using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer outerCircle;
    [SerializeField] private SpriteRenderer innerCircle;
    [SerializeField] private Transform player;
    [SerializeField] private float clampRadius;


    public void UpdatePosition()
    {
        SetRotation();
        
        var playerPos = GetDirection(out var direction);
        transform.position = playerPos + direction;
    }

    private Vector3 GetDirection(out Vector3 direction)
    {
        var mouseWorldPos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        var mousePos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        var radius = clampRadius;


        var playerPos = player.position;
        direction = mousePos - playerPos;

        direction = Vector3.ClampMagnitude(direction, radius);
        return playerPos;
    }

    private void SetRotation()
    {
        var difference = player.transform.position - transform.position;
        var rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    public void SetOuterCircleColour(Color colour)
    {
        outerCircle.color = colour;
    }

    public void SetInnerCircleColour(Color colour)
    {
        innerCircle.color = colour;
    }
}