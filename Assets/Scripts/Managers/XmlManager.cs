using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

namespace Managers
{
    public static class XmlManager
    {
        // Ideally enemy prefabs will also be serialized and be able to be modified through xml
        // we then need to send this information to a client on connection.
        // this will allow for the client side pooling to pool the newly created object from the xml file


        /*
         * XML structure
         * WaveInformation - Contains all the information for upcoming waves. This can be modified by the server
         * and then clients will have it sent to them upon connection
         *
         * EnemyTypes - Contains all the information about the enemies in the game. This will allow for fields such as
         * Weapon type, Damage, Speed, Colours, whatever else we decide to add to the enemies.
         * This will then need to be added into the game starting and once again, sent to the clients
         *
         * Store - Everything related to store, how long it should stay open for after each wave
         * What items can be found, their rarity and what wave the item should appear on
         * 
         */


        // Needs to be called in awake or whenever we decide to check all the config files
        private static string _enemyXmlDirectory;
        private static string _waveDataXmlDirectory;
        private static string _storeContentXmlDirectory;

        private static string _enemyXmlConfig;
        private static string _waveDataXmlConfig;
        private static string _storeContentXmlConfig;

        private static void LoadConfigModules()
        {
            _enemyXmlDirectory = $"{Application.persistentDataPath}/Modules/Enemies";
            _waveDataXmlDirectory = $"{Application.persistentDataPath}/Modules/WaveData";
            _storeContentXmlDirectory = $"{Application.persistentDataPath}/Modules/Store";
            //TODO load the whole directory , i.e. we would have 10 separate enemy.xml to allow to easy modification
            //Alternatively we could have one massive xml holding the data and still load the whole folder
            // This would also allow for custom enemies to be added without modifying the original content.
            _enemyXmlConfig = $"{_enemyXmlDirectory}/enemyTypes.xml";

            _waveDataXmlConfig = $"{_waveDataXmlDirectory}/waveData.xml";
            _storeContentXmlConfig = $"{_storeContentXmlDirectory}/storeContent.xml";

            string[] directories = { _enemyXmlDirectory, _waveDataXmlDirectory, _storeContentXmlDirectory };
            CheckIfDirectoriesExist(directories);
        }


        private static void CheckIfDirectoriesExist(string[] dirs)
        {
            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    continue;
                }

                Directory.CreateDirectory(dir);
            }
        }

        private static string VerifyConfigExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return filePath;
            }

            var f = File.Create(filePath);
            f.Close();
            return filePath;
        }

        [System.Serializable]
        public struct Wave //TODO wave is containing an entire enemy type currently. Should probably only be the ID
        {
            // If a wave is spawned on the same waveID the lowest orderid will spawn first.
            public int orderID;
            public int waveIDToSpawnOn;
            public Enemy enemyToSpawn; //TODO do i want this here? Maybe I should only reference a unique enemy id

            public int amountToSpawn;
            //option to delay spawn
        }

        public struct Store
        {
            //This will contain all the items that can be found in the store, what wave they can be found on
            //the rarity, the effect it has on the player/game
            //Allow for player weapon to be affected - Num of projectiles, direction,firing speed, projectile speed etc.
            // TODO Look into this concept more i.e how games allow easy modification 
            //List<Item> Items = new();
        }

        [System.Serializable]
        public struct Enemy
        {
            public string uniqueID;
            public string displayName;
            public float health;
            public float speed;
            public float damage;
            public List<Color> colours;
        }


        [System.Serializable]
        public struct FullWaveInformation
        {
            public List<Wave> Waves;
        }

        public static void SerializeWaveData(FullWaveInformation waveInformation)
        {
            LoadConfigModules(); //TODO temp
            TextWriter writer = new StreamWriter(VerifyConfigExists(_waveDataXmlConfig));
            Debug.Log(Application.persistentDataPath + "/ConfigTest/config.xml");
            XmlSerializer x = new(typeof(FullWaveInformation));
            x.Serialize(writer, waveInformation);
            writer.Close();
        }

        private static void DeserializeWaveData()
        {
        }
    }
}