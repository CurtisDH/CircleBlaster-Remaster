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

        private void OnEnable()
        {
            EventManager.Instance.OnServerStart += SetupWaveData;
        }

        private void OnDisable()
        {
            EventManager.Instance.OnServerStart -= SetupWaveData;
        }

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

        IEnumerator StartSpawning(List<XmlManager.Wave> listOfWaves, float delay)
        {
            var waitForSeconds = new WaitForSeconds(delay);
            var spawnMangerInstance = SpawnManager.Instance;
            foreach (var waveData in listOfWaves)
            {
                for (var i = 0; i < waveData.amountToSpawn; i++)
                {
                    yield return waitForSeconds;
                    spawnMangerInstance.SpawnNetworkObjectFromPrefabObject(
                        spawnMangerInstance.GetObjectFromUniqueID(waveData.uniqueEnemyID),
                        spawnMangerInstance.SetSpawnPosition());
                }
            }
        }

        private void SetupWaveData()
        {
            Debug.Log("test");
            GetFullWaveInformation();
        }
    }
}