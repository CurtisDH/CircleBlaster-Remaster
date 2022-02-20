using System.Collections.Generic;
using System.IO;
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

        public enum ConfigName
        {
            EnemyConfig,
            WaveDataConfig,
            StoreContentConfig
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
            [Tooltip("If a wave is spawned on the same waveID the lowest OrderId will spawn first.")]
            public int orderID;

            [Tooltip("When the wave will spawn")] public int waveIDToSpawnOn;

            //Surely there is a better way to safeguard this so I cant accidently mistype a unique id..
            //Load enemies and turn their ID into enums?? Is that possible?
            [Tooltip("The enemyID that should be spawned")]
            public string uniqueEnemyID; //TODO do i want this here? Maybe I should only reference a unique enemy id

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
            [Tooltip("A string that is used to identify this enemy")]
            public string uniqueID;

            public string displayName;
            [Tooltip("Size of the Enemy")] public float scale;
            public float health;
            public float speed;
            public float damage;

            [Tooltip("Cant remember how this works honestly, gotta come back to this")] //TODO fix
            public List<Color> colours;
        }


        [System.Serializable]
        public struct FullWaveInformation
        {
            public List<Wave> Waves;
        }
        
        private static void DeserializeWaveData()
        {
        }
        
        public static void SerializeData<T>(T data, ConfigName configLocation)
        {
            string location;
            LoadConfigModules(); //TODO temp
            switch (configLocation)
            {
                case ConfigName.EnemyConfig:
                    location = _enemyXmlConfig;
                    break;
                case ConfigName.WaveDataConfig:
                    location = _waveDataXmlConfig;
                    break;
                case ConfigName.StoreContentConfig:
                    location = _storeContentXmlConfig;
                    break;
                default:
                    location = $"{Application.persistentDataPath}/DEFAULTOUTPUTconfig.xml";
                    //TODO create an in game console to log messages to
                    Debug.LogError($"ERROR:: Defaulting config location to:{location}");
                    break;
            }

            TextWriter writer = new StreamWriter(VerifyConfigExists(location));
            Debug.Log(location);
            XmlSerializer x = new(typeof(T));
            x.Serialize(writer, data);
            writer.Close();
        }
    }
}