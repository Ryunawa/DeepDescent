using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities;
using _2Scripts.Entities.AI;
using _2Scripts.Helpers;
using _2Scripts.ProceduralGeneration;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;
using Random = UnityEngine.Random;

namespace _2Scripts.Manager
{
    [Serializable]
    public class LevelData
    {
        public List<int> enemyIndex; // List of enemy index for this level
    }

    public class EnemiesSpawnerManager : GameManagerSync<EnemiesSpawnerManager>
    {
        public EventHandler<int> OnEnemiesSpawnedOrKilledEventHandler;

        #region Variables
        
        [Tooltip("All the index of enemies that can be spawn for each level, \n Allows us to set the visibility for the mesh depending of the index")]
        [SerializeField] private List<LevelData> spawnableEnemiesIndexByLevel;
        [Tooltip("Max number of enemies in the level at the same time")]
        [SerializeField] private int maxEnemiesPerLevel = 5;
        [Tooltip("Interval between each spawn")]
        [Range(0.1f, 10)]
        [SerializeField] private float spawnIntervalInSecond;
        [Tooltip("This particle will be spawn with the player to add a nice visual spawn effect")]
        [SerializeField] private GameObject spawnParticle;
        [Tooltip("Parent GameObject to organized a bit the hierarchy")]
        [SerializeField] private GameObject spawnedEnemyFolder;
        [Tooltip("This is the prefab that will be spawn, it contains all the mesh for every enemy.")]
        [SerializeField] private GameObject enemyPrefab; // Add this field if it's missing

        private List<(Room, List<GameObject>)> _roomsTuple;
        private int _currentEnemiesCount;
        
        #endregion

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel) return;
            
            if (spawnedEnemyFolder == null)
            {
                spawnedEnemyFolder = GameObject.FindWithTag("SpawnedEnemies");
                if (spawnedEnemyFolder == null)
                {
                    Debug.LogError("No GameObject with tag 'SpawnedEnemies' found. Please ensure there is a GameObject with this tag in the scene.");
                }
            }

            if (NetworkManager.Singleton.IsServer)
            {
                if (GameManager.instance.levelGenerator.spawnShop)
                    return;
                
                StartCoroutine(SpawnEnemies());
            }
        }

        /// <summary>
        /// Return the enemy mesh info to spawn depending on its spawn rate
        /// </summary>
        /// <returns></returns>
        private EnemyStats ChooseEnemyMeshInfo()
        {
            int index = GameManager.GetManager<GameFlowManager>().CurrLevel % 4;
            LevelData currSpawnableEnemiesPrefabs = spawnableEnemiesIndexByLevel[index];
            EnemyTypes allTypeOfEnemies = GameManager.GetManager<DifficultyManager>().GetEnemiesStatsToUse();
            List<int> enemiesMeshIndex = currSpawnableEnemiesPrefabs.enemyIndex;
            
                // foreach (var enemyStats in allTypeOfEnemies.statsInfos)
                // {
                //     if (Random.value < enemyStats.spawnRate)
                //     {
                //         foreach (var enemyMeshIndex in enemiesMeshIndex)
                //         {
                //             if (enemyMeshIndex == enemyStats.index)
                //                 return enemyStats;
                //         }
                //     }
                // }

                foreach (var enemyIndex in enemiesMeshIndex)
                {
                    foreach (var enemyStats in allTypeOfEnemies.statsInfos)
                    {
                        if(enemyIndex == enemyStats.index)
                            if (Random.value < enemyStats.spawnRate)
                                return enemyStats;
                    }
                }
                return allTypeOfEnemies.statsInfos[0];
        }

        /// <summary>
        /// Return the position of a random room to use to spawn the enemy
        /// </summary>
        /// <returns></returns>
        private (Room, List<GameObject>) GetRandomRoomToSpawnIn()
        {
            (Room, List<GameObject>) randomRoom;
            _roomsTuple = GameManager.instance.levelGenerator.GetAllEnemySpawnPoints();
            int roomIndex;

            do
            {
                int randomInt = Random.Range(0, _roomsTuple.Count);
                randomRoom = _roomsTuple[randomInt];
                roomIndex = GameManager.instance.levelGenerator.GetIndexOfRoom(randomRoom.Item1);
            } while (GameManager.instance.levelGenerator.IsRoomEmpty(roomIndex));

            return randomRoom;
        }

        /// <summary>
        /// Spawn an enemy with an interval between two spawns
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnEnemies()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnIntervalInSecond);

                if (_currentEnemiesCount < maxEnemiesPerLevel)
                {
                    EnemyStats meshInfoToActivate = ChooseEnemyMeshInfo();
                    (Room, List<GameObject>) roomToSpawnIn = GetRandomRoomToSpawnIn();
                    Vector3 spawningPosition = roomToSpawnIn.Item2[Random.Range(0, roomToSpawnIn.Item2.Count)].transform.position;

                    OnEnemiesSpawnedOrKilledEventHandler?.Invoke(roomToSpawnIn.Item1, 1);

                    GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(spawningPosition.x, -1, spawningPosition.z), quaternion.identity);
                    newEnemy.GetComponent<NetworkObject>().Spawn();

                    newEnemy.transform.SetParent(spawnedEnemyFolder.transform);
                    newEnemy.GetComponent<AIController>().enabled = true;

                    // Activate the appropriate mesh
                    for (int i = 0; i < newEnemy.transform.childCount; i++)
                    {
                        Transform child = newEnemy.transform.GetChild(i);
                        child.gameObject.SetActive(i == meshInfoToActivate.index);

                        newEnemy.GetComponent<AIController>().ChangeSkinRpc(meshInfoToActivate.index);
                    }

                    // Ensure the root (last child) is always active
                    GetLastChild(newEnemy.transform).gameObject.SetActive(true);

                    // Set the health component
                    HealthComponent healthComponent = newEnemy.GetComponent<HealthComponent>();
                    newEnemy.GetComponent<EnemyData>().enemyStats = meshInfoToActivate;
                    if (healthComponent != null)
                    {
                        healthComponent.SetMaxHealth(meshInfoToActivate.health);
                        healthComponent.Heal(meshInfoToActivate.health); // Start with full health
                    }
                    else
                    {
                        Debug.LogError("HealthComponent is missing on the spawned enemy.");
                    }

                    Vector3 newEnemyPosition = newEnemy.transform.position;
                    StartCoroutine(StartSpawnAnim(newEnemyPosition, newEnemy));
                    _currentEnemiesCount++;
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ChangeMeshRpc()
        {
            
        }

        public void SpawnBossEnemy(Room roomTp)
        {
            
            EnemyStats meshInfoToActivate = ChooseEnemyMeshInfo();
            // get all spawn points
            List<GameObject> gameObjectList = roomTp.GetAllEnemySpawnPoint();
            // select the spawn point
            (Room, List<GameObject>) roomToSpawnIn = (roomTp, gameObjectList);
            Vector3 spawningPosition = roomToSpawnIn.Item2[Random.Range(0, roomToSpawnIn.Item2.Count)].transform.position;

            // Boss is coming
            GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(spawningPosition.x, -1, spawningPosition.z), quaternion.identity);
            newEnemy.GetComponent<NetworkObject>().Spawn();
            AIController aiController = newEnemy.GetComponent<AIController>();
            newEnemy.transform.SetParent(spawnedEnemyFolder.transform);
            aiController.enabled = true;

            // tell that he is the boss>
            newEnemy.transform.localScale *= 1.5f;
            aiController.isBoss = true;

            // Activate the appropriate mesh
            for (int i = 0; i < newEnemy.transform.childCount; i++)
            {
                Transform child = newEnemy.transform.GetChild(i);
                child.gameObject.SetActive(i == meshInfoToActivate.index);
            }

            // Ensure the root (last child) is always active
            GetLastChild(newEnemy.transform).gameObject.SetActive(true);

            // Set the health component
            HealthComponent healthComponent = newEnemy.GetComponent<HealthComponent>();
            if (healthComponent != null)
            {
                healthComponent.SetMaxHealth(meshInfoToActivate.health * 2);
                healthComponent.Heal(meshInfoToActivate.health); // Start with full health
            }
            else
            {
                Debug.LogError("HealthComponent is missing on the spawned enemy.");
            }

            Vector3 newEnemyPosition = newEnemy.transform.position;
            StartCoroutine(StartSpawnAnim(newEnemyPosition, newEnemy));
            
        }

        private Transform GetLastChild(Transform parentTransform)
        {
            return parentTransform.GetChild(parentTransform.childCount - 1);
        }

        IEnumerator StartSpawnAnim(Vector3 pEnemyPosition, GameObject pEnemyGameObject)
        {
            yield return new WaitForSeconds(0.2f);

            GameObject newParticle = Instantiate(spawnParticle, new Vector3(pEnemyPosition.x, 0.2f, pEnemyPosition.z - 0.5f), quaternion.identity);
            if (newParticle)
            {
                newParticle.GetComponent<NetworkObject>().Spawn();
                newParticle.transform.localScale *= 2.5f;
            }

        }


        /// <summary>
        /// Decrement the total enemies number 
        /// </summary>
        public void EnemyDestroyed(EnemyData pEnemyKilled)
        {
            _currentEnemiesCount--;
            OnEnemiesSpawnedOrKilledEventHandler?.Invoke(pEnemyKilled.roomSpawnedInID, -1);
        }

        /// <summary>
        /// Stop the spawn and clear all the left enemies
        /// </summary>
        public void StopSpawning()
        {
            StopAllCoroutines();
            var objects = spawnedEnemyFolder.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < objects.Length; index++)
            {
                var networkObjectChild = objects[index];
                if (networkObjectChild.gameObject != spawnedEnemyFolder && networkObjectChild.IsSpawned)
                    networkObjectChild.Despawn();
            }

            Debug.Log("All enemies left removed");
        }
    }
}
