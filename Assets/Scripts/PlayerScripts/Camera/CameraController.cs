using System;
using UnityEngine;

namespace PlayerScripts.Camera
{
    public class CameraController : MonoBehaviour
    {
        //Follow the player
        //Smooth the follow || have an option for it
        //Ability to select another player when requested
        //Raycast to select a GameObject to follow? -- I.E could follow an enemy or projectile.
        // But we then need more information e.g is the player dead? 
        //Event driven?

        [SerializeField] private Transform targetTransform;
        private bool _playerIsDead;
        [SerializeField]
        private float smoothSpeed = 10;
        [SerializeField] private float cameraZOffset = -10f;

        private void Update()
        {
            if (targetTransform is null)
            {
                return;
            }

            var targetTransformPos = targetTransform.position;
            var targetPosition = new Vector3(targetTransformPos.x, targetTransformPos.y, cameraZOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed*Time.deltaTime);
        }

        public void TogglePlayerDeath(bool isDead)
        {
            _playerIsDead = isDead;
        }

        public void SetTargetTransform(Transform target)
        {
            targetTransform = target;
            transform.parent = null;
        }
    }
}