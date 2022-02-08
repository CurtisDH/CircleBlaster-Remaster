using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using PlayerScripts;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy
{
    public class EnemyDetectionCollider : MonoBehaviour
    {
        [SerializeField] private List<Transform> nearbyPlayerTransforms;

        [SerializeField] private EnemyBase _enemyBase;

        private bool _updateIsRunning = false;

        [SerializeField] private float updateTime = 5f;
        private WaitForSeconds _updateTimer;


        private void OnEnable()
        {
            _updateTimer = new WaitForSeconds(updateTime);
            EventManager.Instance.OnPlayerDeath += FindNewTarget;
        }

        private void FindNewTarget(Player playercomponent)
        {
            if (nearbyPlayerTransforms.Contains(playercomponent.transform))
                nearbyPlayerTransforms.Remove(playercomponent.transform);

            _enemyBase.SetClosestPlayerTransform(
                SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms));
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                if (nearbyPlayerTransforms.Contains(col.transform))
                {
                    return;
                }

                nearbyPlayerTransforms.Add(col.transform);
                _enemyBase.SetClosestPlayerTransform(
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms));
                if (!_updateIsRunning)
                {
                    StartCoroutine(UpdateToClosestPlayer());
                }
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                _enemyBase.SetClosestPlayerTransform(
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms));
            }
        }

        private IEnumerator UpdateToClosestPlayer()
        {
            _updateIsRunning = true;
            while (_updateIsRunning)
            {
                yield return _updateTimer;
                _enemyBase.SetClosestPlayerTransform(
                    SpawnManager.Instance.GetClosestPlayer(transform.position, nearbyPlayerTransforms));
            }
        }
    }
}