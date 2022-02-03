using System;
using System.Collections.Generic;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private float health;
        [SerializeField] private float speed;
        [SerializeField] private float damage;
        [SerializeField] private List<Color> colours;

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        //TODO update this so it works client side (Extremely laggy when using network transform)
        [SerializeField] public Transform closestPlayerTransform;

        private void OnEnable()
        {
            foreach (var sr in spriteRenderers)
            {
                if (colours.Count > 0)
                {
                    sr.color = colours[^1];
                    colours.RemoveAt(colours.Count - 1);
                    continue;
                }
            }
        }
        
        

        private void Update()
        {
            if (closestPlayerTransform != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, closestPlayerTransform.position,
                    speed * Time.deltaTime);
            }

        }
    }
}