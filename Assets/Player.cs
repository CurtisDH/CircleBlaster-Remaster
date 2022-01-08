using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] private SpriteRenderer teamColourSpriteRenderer;
    private void OnEnable()
    {
        teamColourSpriteRenderer.color = PlayerManager.Instance.GetColourFromTeamID(teamID);
    }
    
}
