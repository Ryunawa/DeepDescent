using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Entities;
using _2Scripts.Entities.AI;
using _2Scripts.ProceduralGeneration;
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
        public EventHandler<int> OnEnemiesSpawnedOrKilledEventHandler;

        #region Variables

        public bool bSpawnInMultiplayer;
        
        [SerializeField] private List<LevelData> spawnableEnemiesPrefabsByLevel;
        [SerializeField] private int maxEnemiesPerLevel = 5;
        [SerializeField] private int maxEnemiesPerRoom = 3;
        [Range(0.1f, 10)]
        [SerializeField] private float spawnIntervalInSecond;

        [SerializeField] private GameObject spawnParticle;
        [SerializeField] private GameObject spawnedEnemyFolder;

        private EnemyTypes _enemiesList;
        private int _currentEnemiesCount;
        private int _currLevel = 1;

        public int currLevel => _currLevel;

        private LevelGenerator _levelGenerator;
        
        #endregion


        async void Start()
        {
            while (!MultiManager.instance.levelGenerator)
            {
                await Task.Delay(200);
            }
            MultiManager.instance.levelGenerator.dungeonGeneratedEvent.AddListener(() =>
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    DifficultyManager.instance.AdjustDifficultyParameters(MultiManager.instance.GetAllPlayerGameObjects().Count);
                    _enemiesList = DifficultyManager.instance.GetEnemiesStatsToUse();
                    StartCoroutine(SpawnEnemies());
                }
            });
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
            return (GetStructElementByIndex<EnemyStats>(_enemiesList, 0));
        }

        /// <summary>
        /// Return the position of a random room to use to spawn the enemy
        /// </summary>
        /// <returns></returns>
        private (Room,List<GameObject>) GetRandomRoomToSpawnIn()
        {
            (Room,List<GameObject>) randomRoom;
            List<(Room, List<GameObject>)> roomsTuple = MultiManager.instance.levelGenerator.GetAllEnemySpawnPoints();
            int roomIndex;
            
            do
            {
                int randomInt = Random.Range(0, roomsTuple.Count);
                randomRoom = roomsTuple[randomInt];
                roomIndex = MultiManager.instance.levelGenerator.GetIndexOfRoom(randomRoom.Item1);
            } while (MultiManager.instance.levelGenerator.IsRoomEmpty(roomIndex));
            
            return randomRoom;
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
                    (Room,List<GameObject>) roomToSpawnIn = GetRandomRoomToSpawnIn();
                    Vector3 spawningPosition = roomToSpawnIn.Item2[Random.Range(0, roomToSpawnIn.Item2.Count)].transform.position;

                    OnEnemiesSpawnedOrKilledEventHandler?.Invoke(roomToSpawnIn.Item1, 1);
                    
                    
                    GameObject newEnemy = Instantiate(objectToSpawn.enemyPrefab, new Vector3(spawningPosition.x, -1, spawningPosition.z),
                        quaternion.identity);
                    newEnemy.GetComponent<NetworkObject>().Spawn();
                    newEnemy.transform.SetParent(spawnedEnemyFolder.transform);
                    
                    EnemyData newEnemyData = newEnemy.GetComponent<EnemyData>();
                    newEnemyData.enemyStats = objectToSpawn;
                    newEnemyData.roomSpawnedInID = roomToSpawnIn.Item1.ID;
                    
                    newEnemy.GetComponent<AIController>().enabled = false;
                    Vector3 newEnemyPosition = newEnemy.transform.position;
                    StartCoroutine(StartSpawnAnim(newEnemyPosition, newEnemy));
                    _currentEnemiesCount++;
                }
            }
        }

        IEnumerator StartSpawnAnim(Vector3 pEnemyPosition, GameObject pEnemyGameObject)
        {
            yield return new WaitForSeconds(0.2f);
            GameObject newParticle = Instantiate(spawnParticle, new Vector3(pEnemyPosition.x, 0.2f, pEnemyPosition.z - 0.5f),
                quaternion.identity);
                        
            newParticle.transform.localScale *= 2.5f;
                        
            yield return new WaitForSeconds(pEnemyGameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + 0.25f);
                        
            Destroy(newParticle);
            pEnemyGameObject.GetComponent<AIController>().enabled = true;
        }
        
        /// <summary>
        /// Decrement the total enemies number 
        /// </summary>
        public void EnemyDestroyed(EnemyData penemyKilled)
        {
            _currentEnemiesCount--;
            OnEnemiesSpawnedOrKilledEventHandler?.Invoke(penemyKilled.roomSpawnedInID, -1);
        }

        /// <summary>
        /// Stop the spawn and clear all the left enemies
        /// </summary>
        public void StopSpawning()
        {
            StopAllCoroutines();
            foreach (var networkObjectChild in spawnedEnemyFolder.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != spawnedEnemyFolder)
                    networkObjectChild.Despawn();
            }
            Debug.Log("All enemies left removed");
            _currLevel++;
        }
        
        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_StartSpawn()
        {
            DifficultyManager.instance.DEBUG_SetEasyStatsForEnemies();
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

