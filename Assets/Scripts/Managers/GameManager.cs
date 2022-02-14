using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.Text;

namespace Managers
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        [SerializeField] private NetworkVariable<bool> isGameOver = new();

        //We don't want a player to spawn during a wave.
        [SerializeField] private NetworkVariable<bool> isWaveActive = new();
        [SerializeField] private NetworkVariable<int> waveRound = new();
        [SerializeField] private NetworkVariable<bool> endGame = new();
        [SerializeField] private List<EnemyBase> activeEnemies = new();

        [SerializeField] private GameObject storeGameObject;
        [SerializeField] private float storeTimerFloat = 20f;
        private WaitForSeconds _storeTimerWaitForSeconds;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _storeTimerWaitForSeconds = new WaitForSeconds(1f);
            isWaveActive.OnValueChanged += OnWaveStatusChange;
            endGame.OnValueChanged += EndGameLogic;
            EventManager.Instance.OnEnemySpawnEvent += EnemySpawnEvent;
        }

        private void EndGameLogic(bool previousvalue, bool newvalue)
        {
            if (newvalue)
            {
                EventManager.Instance.InvokeOnEndGameEvent();
                if (IsServer)
                    waveRound.Value = 0;
            }
            //Kill all enemies
            //Reset wave count
            //Reset all player items & weapons?
            //Respawn all players
        }

        private void EnemySpawnEvent(EnemyBase enemyBaseComponent, bool isActive)
        {
            if (activeEnemies.Contains(enemyBaseComponent) && !isActive)
            {
                activeEnemies.Remove(enemyBaseComponent);
            }

            if (!activeEnemies.Contains(enemyBaseComponent) && isActive)
            {
                activeEnemies.Add(enemyBaseComponent);
            }

            UpdateWaveStatus();
        }

        public void EndGame()
        {
            if (IsServer)
            {
                endGame.Value = true;
            }
        }

        private void UpdateWaveStatus()
        {
            if (activeEnemies.Count <= 0)
            {
                isWaveActive.Value = false;
                return;
            }

            isWaveActive.Value = true;
        }

        private void OnWaveStatusChange(bool previousValue, bool waveIsActive)
        {
            //makes sense in my head right now, but its late so might not do what i expect
            if (previousValue == waveIsActive)
            {
                return;
            }

            if (!waveIsActive)
            {
                if (IsServer)
                {
                    waveRound.Value++;
                }

                EventManager.Instance.InvokeOnWaveComplete(waveRound.Value);
                //Enable store
                EnableStore(true);
                //Enter collision trigger to open store menu automatically
                StartCoroutine(SpawnPendingAndDeadPlayers());
                //Start countdown timer till next wave
                StartCoroutine(NextWaveTimer());
                //Ability to ready up before timer?

                //Give wave info??
                //Ideally want to make the world more interactive -- Informative UI is all worldspace
                //If you're out of position then you wont be able to get the info.
                // Wave info spawns with an arrow pointing to it?
                // Store spawns at 0,0,0? || Could spawn randomly and setup a script to display location with arrow.
            }
        }

        IEnumerator SpawnPendingAndDeadPlayers()
        {
            while (!isWaveActive.Value)
            {
                yield return _storeTimerWaitForSeconds;
                SpawnManager.Instance.SpawnDeadPlayers();
                SpawnManager.Instance.SpawnPendingConnectionPlayers();
            }
        }

        private void EnableStore(bool setStatus)
        {
            storeGameObject.SetActive(setStatus);

            var Colour = UnityEngine.Color.green;
            var storeStatus = "Open";
            if (!setStatus)
            {
                Colour = Color.red;
                storeStatus = "Closed";
            }

            foreach (var player in SpawnManager.Instance.GetAllAlivePlayers())
            {
                GenerateWorldSpaceText.CreateWorldSpaceTextPopup($"Store is now {storeStatus}",
                    player.transform.position, 2, 2, Colour, .25f);
            }
        }

        private IEnumerator NextWaveTimer()
        {
            var counter = 0;
            while (counter < storeTimerFloat)
            {
                yield return _storeTimerWaitForSeconds;
                counter++;
            }

            EnableStore(false);
        }

        private void UnsubscribeEvents()
        {
            isWaveActive.OnValueChanged -= OnWaveStatusChange;
            EventManager.Instance.OnEnemySpawnEvent -= EnemySpawnEvent;
            endGame.OnValueChanged -= EndGameLogic;
        }
    }
}