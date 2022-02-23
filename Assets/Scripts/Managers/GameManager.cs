using System.Collections;
using System.Collections.Generic;
using Enemy;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using Utility;
using Utility.Text;

namespace Managers
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        [SerializeField] private NetworkVariable<bool> isGameOver = new();

        //We don't want a player to spawn during a wave.
        [SerializeField] private NetworkVariable<bool> isWaveActive = new();
        [SerializeField] public NetworkVariable<int> waveRound = new();
        [SerializeField] private NetworkVariable<bool> endGame = new();
        [SerializeField] private List<EnemyBase> activeEnemies = new();

        [SerializeField] private GameObject storeGameObject;
        [SerializeField] private float storeTimerFloat = 10f; //TODO start wave early button (VOTE SYSTEM?)
        private WaitForSeconds _storeTimerWaitForSeconds;

        //todo why do i not have a reference to the alive player components here

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

        public bool IsWaveActive()
        {
            return isWaveActive.Value;
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
                OnWaveComplete();
                //Enable store
                //Enter collision trigger to open store menu automatically
                //Ability to ready up before timer?
                //Start countdown timer till next wave
                //Give wave info??
                //Ideally want to make the world more interactive -- Informative UI is all worldspace
                //If you're out of position then you wont be able to get the info.
                // Wave info spawns with an arrow pointing to it?
                // Store spawns at 0,0,0? || Could spawn randomly and setup a script to display location with arrow.
            }
        }

        private void OnWaveComplete()
        {
            EnableStore(true);

            StartCoroutine(SpawnPendingAndDeadPlayers());
            HealAllPlayers(waveRound.Value);
            StartCoroutine(NextWaveTimer());
        }

        private void HealAllPlayers(int waveRoundValue)
        {
            if (!IsServer) return;

            foreach (var player in PlayerConnectionManager.Instance.ConnectedPlayerComponents)
            {
                player.health.Value += waveRoundValue;

                //Tell the player how much they healed by
                //Maybe a healing particle effect that gets played //TODO
                //Make this look better, looks horrible right now
                StartCoroutine(SlowHealPlayer(player, 0.1f, waveRoundValue));

            }
        }
        //Looks better but still not great. Good enough until the reskin
        IEnumerator SlowHealPlayer(Player player, float delay, float waveRoundValue)
        {
            WaitForSeconds wfs = new WaitForSeconds(delay);
            for (int i = 0; i < waveRoundValue; i++)
            {
                yield return wfs;
                GenerateWorldSpaceText.CreateWorldSpaceTextPopup($"+{1}",
                    player.transform.position, 1f, 2, Color.green, .1f,
                    0, true);
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
            List<TextPopup> textPopups = new();
            foreach (var player in SpawnManager.Instance.GetAllAlivePlayers())
            {
                var text = GenerateWorldSpaceText.CreateWorldSpaceTextPopup($"Next wave in: {storeTimerFloat}",
                    player.transform.position, 0, storeTimerFloat, Color.yellow, .25f);
                text.transform.parent = player.transform;
                textPopups.Add(text);
            }

            var counter = 0;
            while (counter < storeTimerFloat)
            {
                foreach (var txtPopup in textPopups)
                {
                    txtPopup.textMeshComponent.text = $"Next wave in: {storeTimerFloat - counter}";
                }

                yield return _storeTimerWaitForSeconds;
                counter++;
            }

            EnableStore(false);
            if (IsServer)
                WaveManager.Instance.StartNextWaveServerRPC();
        }

        private void UnsubscribeEvents()
        {
            isWaveActive.OnValueChanged -= OnWaveStatusChange;
            EventManager.Instance.OnEnemySpawnEvent -= EnemySpawnEvent;
            endGame.OnValueChanged -= EndGameLogic;
        }
    }
}