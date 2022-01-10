using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private NetworkVariable<Vector3> weaponPosition;

    [SerializeField] private SpriteRenderer outerCircle;
    [SerializeField] private SpriteRenderer innerCircle;

    private void OnEnable()
    {
        weaponPosition.Value = Vector3.zero;
    }
    

    private void SetOuterCircleColour(Color colour)
    {
        outerCircle.color = colour;
    }

    private void SetInnerCircleColour(Color colour)
    {
        innerCircle.color = colour;
    }
    
}