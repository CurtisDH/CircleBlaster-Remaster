using UnityEngine;
using Utility;

namespace Managers
{
    public sealed class EventManager : MonoSingleton<EventManager>
    {
        public delegate void ProjectileHitEvent(GameObject target, float damage);

        public event ProjectileHitEvent OnProjectileHitEvent;


        public delegate void EnemyHitEvent(GameObject player, float damage);

        public event EnemyHitEvent OnEnemyHitEvent;

        [SerializeField] private bool trackEvents;

        public void InvokeOnProjectileHitEvent(GameObject target, float damage)
        {
            if (trackEvents)
                Debug.Log($"InvokeOnProjectileHitEvent Target:{target}, Damage:{damage}");
            OnProjectileHitEvent?.Invoke(target, damage);
        }

        public void InvokeOnEnemyHitEvent(GameObject target, float damage)
        {
            if (trackEvents)
                Debug.Log($"InvokeOnEnemyHitEvent Target:{target}, Damage:{damage}");
            OnEnemyHitEvent?.Invoke(target, damage);
        }
    }
}