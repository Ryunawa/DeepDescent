using System.Collections;
using System.Collections.Generic;
using _2Scripts.Struct;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;
using static _2Scripts.Helpers.StructureAccessMethods;
using Random = UnityEngine.Random;

namespace _2Scripts.Manager
{
    public class EnemiesSpawnerManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private List<GameObject> spawnableEnemiesPrefabs;
        [SerializeField] private int maxEnemiesPerLevel = 5;
        [Range(0.1f, 10)]
        [SerializeField] private float spawnIntervalInSecond = 2f;


        private EnemyTypes _enemiesList;
        private int _currentEnemiesCount;
        
        #endregion
        
        private void Start()
        {
            _enemiesList = DifficultyManager.instance.GetEnemiesStatsToUse();
            StartCoroutine(SpawnEnemies());
        }

        /// <summary>
        /// Return the enemy to spawn depending on his spawn rate
        /// </summary>
        /// <returns></returns>
        private EnemyStats ChooseEnemyToSpawn()
        {
            foreach (var spawnableEnemyPrefab in spawnableEnemiesPrefabs)
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
            return default;
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
                    
                    MultiManager.instance.SpawnNetworkObject(objectToSpawn.enemyPrefab.GetComponent<NetworkObject>(),
                        spawningPosition,
                        quaternion.identity);
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
    }

}

