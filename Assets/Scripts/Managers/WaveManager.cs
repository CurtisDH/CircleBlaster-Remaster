using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Managers
{
    public class WaveManager : NetworkSingleton<WaveManager>
    {
        private List<XmlManager.FullWaveInformation> _fullWaveInformation = new();
        private Dictionary<int, List<XmlManager.Wave>> _sortedWaveData = new();

        private void GetFullWaveInformation()
        {
            _fullWaveInformation = XmlManager.DeserializeWaveData();
            foreach (var waveInfo in _fullWaveInformation)
            {
                foreach (var wave in waveInfo.Waves)
                {
                    if (_sortedWaveData.ContainsKey(wave.waveIDToSpawnOn))
                    {
                        _sortedWaveData[wave.waveIDToSpawnOn].Add(wave);
                    }
                    else
                    {
                        var newWaveList = new List<XmlManager.Wave> { wave };
                        _sortedWaveData.Add(wave.waveIDToSpawnOn, newWaveList);
                    }
                }
            }
        }

        [ServerRpc]
        public void StartNextWaveServerRPC()
        {
            var keyToCheckFor = GameManager.Instance.waveRound.Value;
            if (!_sortedWaveData.ContainsKey(GameManager.Instance.waveRound.Value))
            {
                keyToCheckFor = _sortedWaveData.Count - 1;
            }

            var wavesToSpawn = _sortedWaveData[keyToCheckFor].ToList();

            var sortedWaves = wavesToSpawn.OrderBy(wave => wave.orderID).ToList();
            StartCoroutine(StartSpawning(sortedWaves, 0.1f));

            //Generate Spawn the last spawned wave + wave before that then repeat +1 .. +2 ... etc ?
        }

        //TODO potential bug -- If an enemy immediately dies before the next one is spawned
        // the wave will be considered over
        IEnumerator StartSpawning(List<XmlManager.Wave> listOfWaves, float delay)
        {
            var spawnMangerInstance = SpawnManager.Instance;
            foreach (var waveData in listOfWaves)
            {
                var waitForSeconds = new WaitForSeconds(waveData.delayBetweenSpawns);
                for (var i = 0; i < waveData.amountToSpawn; i++)
                {
                    yield return waitForSeconds;
                    spawnMangerInstance.SpawnNetworkObjectFromPrefabObject(
                        spawnMangerInstance.GetObjectFromUniqueID(waveData.uniqueEnemyID),
                        spawnMangerInstance.SetSpawnPosition());
                }
            }
        }

        public void SetupWaveData()
        {
            Debug.Log("test");
            GetFullWaveInformation();
        }

    }
}