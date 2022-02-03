using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy
{
    public class EnemyDetectionCollider : MonoBehaviour
    {
        [SerializeField] private List<Transform> nearbyPlayerTransforms;

        [SerializeField] private EnemyBase _enemyBase;

        private bool _updateIsRunning = false;
        
        [SerializeField]
        private float updateTime = 5f;
        private WaitForSeconds _updateTimer;


        private void OnEnable()
        {
            _updateTimer = new WaitForSeconds(updateTime);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("Collision");
            if (col.gameObject.CompareTag("Player"))
            {
                if (nearbyPlayerTransforms.Contains(col.transform))
                {
                    return;
                }

                nearbyPlayerTransforms.Add(col.transform);
                _enemyBase.closestPlayerTransform =
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms);
                if (!_updateIsRunning)
                {
                    StartCoroutine(UpdateToClosestPlayer());
                }
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            Debug.Log("ExitCollision");
            if (col.gameObject.CompareTag("Player"))
            {
                _enemyBase.closestPlayerTransform =
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms);
            }
        }
        
        private IEnumerator UpdateToClosestPlayer()
        {
            _updateIsRunning = true;
            while (_updateIsRunning)
            {
                yield return _updateTimer;
                _enemyBase.closestPlayerTransform =
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms);
            }
        }
        
    }
}