using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;
using static _2Scripts.Helpers.StructureAccessMethods;
using Random = UnityEngine.Random;

namespace _2Scripts.Manager
{
    [Serializable]
    public class LevelData
    {
        public List<GameObject> enemyPrefabsSpawnable;
    }
    
    public class EnemiesSpawnerManager : Singleton<EnemiesSpawnerManager>
    {
        #region Variables

        public bool bSpawnInMultiplayer;
        
        [SerializeField] private List<LevelData> spawnableEnemiesPrefabsByLevel;
        [SerializeField] private int maxEnemiesPerLevel = 5;
        [Range(0.1f, 10)]
        [SerializeField] private float spawnIntervalInSecond = 2f;

        private EnemyTypes _enemiesList;
        private int _currentEnemiesCount;
        private int _currLevel = 1; // Depend on the game manager
        
        #endregion
        
        private void Start()
        {
            _enemiesList = DifficultyManager.instance.GetEnemiesStatsToUse();
        }

        /// <summary>
        /// Return the enemy to spawn depending on his spawn rate
        /// </summary>
        /// <returns></returns>
        private EnemyStats ChooseEnemyToSpawn()
        {
            int index = Math.Min(_currLevel, spawnableEnemiesPrefabsByLevel.Count);
            LevelData currSpawnableEnemiesPrefabs = spawnableEnemiesPrefabsByLevel[index - 1];

            foreach (var spawnableEnemyPrefab in currSpawnableEnemiesPrefabs.enemyPrefabsSpawnable)
            {
                for (int i = 0; i < GetNumberOfElementsInStruct(_enemiesList); i++)
                {
                    if (GetStructElementByIndex<EnemyStats>(_enemiesList, i).enemyPrefab == spawnableEnemyPrefab)
                    {
                        if (Random.value < GetStructElementByIndex<EnemyStats>(_enemiesList, i).spawnRate)
                        {
                            return GetStructElementByIndex<EnemyStats>(_enemiesList, i);
                        }
                    }
                }
            }
            return GetStructElementByIndex<EnemyStats>(_enemiesList, 0);;
        }

        /// <summary>
        /// Return the position of a random room to use to spawn the enemy
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSpawnPosition()
        {
            int randomInt = Random.Range(0, LevelGenerator.instance.dungeon.Length);
            return LevelGenerator.instance.GetPosition(randomInt);
        }

        /// <summary>
        /// Spawn an enemy with an interval between two spawn
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnEnemies()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnIntervalInSecond);

                if (_currentEnemiesCount < maxEnemiesPerLevel)
                {
                    EnemyStats objectToSpawn = ChooseEnemyToSpawn();
                    Vector3 spawningPosition = GetSpawnPosition();

                    if (bSpawnInMultiplayer)
                    {
                         MultiManager.instance.SpawnNetworkObject(objectToSpawn.enemyPrefab.GetComponent<NetworkObject>(),
                             spawningPosition,
                             quaternion.identity);
                    }
                    else
                    {
                        //DEBUG ONLY
                        Instantiate(objectToSpawn.enemyPrefab, new Vector3(spawningPosition.x, 1, spawningPosition.z),
                            quaternion.identity);
                    }

                    _currentEnemiesCount++;
                }
            }
        }
        
        /// <summary>
        /// Decrement the total enemies number 
        /// </summary>
        public void EnemyDestroyed()
        {
            _currentEnemiesCount--;
        }

        
        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_StartSpawn()
        {
            _enemiesList = DifficultyManager.instance.GetEnemiesStatsToUse();
            StartCoroutine(SpawnEnemies());
        }
        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_StopSpawn()
        {
            StopCoroutine(SpawnEnemies());
        }
    }

}

