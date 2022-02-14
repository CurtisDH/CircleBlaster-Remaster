using System;
using Enemy;
using PlayerScripts;
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

        public delegate void EnemySpawnEvent(EnemyBase enemyBaseComponent, bool isActive);

        public event EnemySpawnEvent OnEnemySpawnEvent;


        public delegate void ServerStartEvent();

        public event ServerStartEvent OnServerStart;

        public delegate void PlayerDeathEvent(Player playerComponent);

        public event PlayerDeathEvent OnPlayerDeath;

        public delegate void PlayerSpawnEvent(Player playerComponent);

        public event PlayerSpawnEvent OnPlayerSpawn;

        public delegate void WaveCompleteEvent(int waveCount);

        public event WaveCompleteEvent OnWaveComplete;


        public event Action OnEndGameEvent;


        [SerializeField] private bool trackEvents;

        public void InvokeOnProjectileHitEvent(GameObject target, float damage)
        {
            TrackEvent($"InvokeOnProjectileHitEvent Target:{target}, Damage:{damage}");
            OnProjectileHitEvent?.Invoke(target, damage);
        }

        public void InvokeOnEnemyHitEvent(GameObject target, float damage)
        {
            TrackEvent($"InvokeOnEnemyHitEvent Target:{target}, Damage:{damage}");
            OnEnemyHitEvent?.Invoke(target, damage);
        }

        public void InvokeEnemySpawnEvent(EnemyBase enemyBaseComponent, bool isActive)
        {
            TrackEvent($"InvokeEnemySpawnEvent: enemyBaseComponent:{enemyBaseComponent}, isActive:{isActive}");


            OnEnemySpawnEvent?.Invoke(enemyBaseComponent, isActive);
        }

        public void InvokeOnServerStart()
        {
            TrackEvent("InvokeOnServerStart");
            OnServerStart?.Invoke();
        }

        public void InvokeOnPlayerDeath(Player playerComponent)
        {
            TrackEvent($"InvokeOnPlayerDeath:{playerComponent}");
            OnPlayerDeath?.Invoke(playerComponent);
        }

        public void InvokeOnWaveComplete(int WaveCount)
        {
            TrackEvent($"InvokeOnWaveComplete: WaveCount{WaveCount}");
            OnWaveComplete?.Invoke(WaveCount);
        }

        public void InvokeOnPlayerSpawn(Player playercomponent)
        {
            TrackEvent($"InvokeOnPlayerSpawn: PlayerComp:{playercomponent}");
            OnPlayerSpawn?.Invoke(playercomponent);
        }

        private void TrackEvent(string message)
        {
            if (trackEvents)
            {
                Debug.Log(message);
            }
        }

        public void InvokeOnEndGameEvent()
        {
            TrackEvent("InvokeEndGameEvent");
            OnEndGameEvent?.Invoke();
        }
    }
}