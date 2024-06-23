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
using UnityEngine.Serialization;
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
        public static EnemiesSpawnerManager instance; // Static instance variable

        public EventHandler<int> OnEnemiesSpawnedOrKilledEventHandler;

        #region Variables

        public bool bSpawnInMultiplayer;

        [SerializeField] private List<(Room, List<GameObject>)> roomsTuple;
        [SerializeField] private List<LevelData> spawnableEnemiesIndexByLevel;
        [SerializeField] private int maxEnemiesPerLevel = 5;
        [SerializeField] private int maxEnemiesPerRoom = 3;
        [Range(0.1f, 10)]
        [SerializeField] private float spawnIntervalInSecond;

        [SerializeField] private GameObject spawnParticle;
        [SerializeField] private GameObject spawnedEnemyFolder;

        [SerializeField] private GameObject enemyPrefab; // Add this field if it's missing

        private EnemyTypes _enemiesList;
        private int _currentEnemiesCount;

        private LevelGenerator _levelGenerator;

        public bool bShouldSpawn;

        #endregion

        protected override void Start()
        {
            base.Start();

            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple instances of EnemiesSpawnerManager detected. Destroying duplicate.");
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
            }

            if (spawnedEnemyFolder == null)
            {
                spawnedEnemyFolder = GameObject.FindWithTag("SpawnedEnemies");
                if (spawnedEnemyFolder == null)
                {
                    Debug.LogError("No GameObject with tag 'SpawnedEnemies' found. Please ensure there is a GameObject with this tag in the scene.");
                }
            }
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel) return;

            if (NetworkManager.Singleton.IsServer)
            {
                if (GameManager.GetManager<GameFlowManager>().CurrLevel == 1)
                    GameManager.GetManager<DifficultyManager>().AdjustDifficultyParameters(GameManager.GetManager<MultiManager>().GetAllPlayerGameObjects().Count);

                if (GameManager.instance.levelGenerator.spawnShop)
                    return;
                _enemiesList = GameManager.GetManager<DifficultyManager>().GetEnemiesStatsToUse();
                StartCoroutine(SpawnEnemies());
            }
        }

        /// <summary>
        /// Return the enemy mesh info to spawn depending on its spawn rate
        /// </summary>
        /// <returns></returns>
        private EnemyStats ChooseEnemyMeshInfo()
        {
            int index = Math.Min(GameManager.GetManager<GameFlowManager>().CurrLevel, spawnableEnemiesIndexByLevel.Count);
            LevelData currSpawnableEnemiesPrefabs = spawnableEnemiesIndexByLevel[index - 1];
            EnemyTypes allTypeOfEnemies = GameManager.GetManager<DifficultyManager>().GetEnemiesStatsToUse();
            List<int> enemiesMeshIndex = currSpawnableEnemiesPrefabs.enemyIndex;
            
                foreach (var enemyStats in allTypeOfEnemies.statsInfos)
                {
                    if (Random.value < enemyStats.spawnRate)
                    {
                        foreach (var enemyMeshIndex in enemiesMeshIndex)
                        {
                            if (enemyMeshIndex == enemyStats.index)
                                return enemyStats;
                        }
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
            roomsTuple = GameManager.instance.levelGenerator.GetAllEnemySpawnPoints();
            int roomIndex;

            do
            {
                int randomInt = Random.Range(0, roomsTuple.Count);
                randomRoom = roomsTuple[randomInt];
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

        public void SpawnBossEnemy(Room roomTp)
        {
            if (_currentEnemiesCount < maxEnemiesPerLevel)
            {
                EnemyStats meshInfoToActivate = ChooseEnemyMeshInfo();
                // get all spawn points
                List<GameObject> gameObjectList = roomTp.GetAllEnemySpawnPoint();
                // select the spawn point
                (Room, List<GameObject>) roomToSpawnIn = (roomTp, gameObjectList);
                Vector3 spawningPosition = roomToSpawnIn.Item2[Random.Range(0, roomToSpawnIn.Item2.Count)].transform.position;

                // Boss is coming
                GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(spawningPosition.x, -1, spawningPosition.z), quaternion.identity);
                AIController aiController = newEnemy.GetComponent<AIController>();

                newEnemy.GetComponent<NetworkObject>().Spawn();
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
        }

        private Transform GetLastChild(Transform parentTransform)
        {
            return parentTransform.GetChild(parentTransform.childCount - 1);
        }

        IEnumerator StartSpawnAnim(Vector3 pEnemyPosition, GameObject pEnemyGameObject)
        {
            yield return new WaitForSeconds(0.2f);
            GameObject newParticle = Instantiate(spawnParticle, new Vector3(pEnemyPosition.x, 0.2f, pEnemyPosition.z - 0.5f), quaternion.identity);
            newParticle.transform.localScale *= 2.5f;

            yield return new WaitForSeconds(pEnemyGameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + 0.25f);
            Destroy(newParticle);
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
            foreach (var networkObjectChild in spawnedEnemyFolder.GetComponentsInChildren<NetworkObject>())
            {
                if (networkObjectChild.gameObject != spawnedEnemyFolder)
                    networkObjectChild.Despawn();
            }
            Debug.Log("All enemies left removed");
        }

        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_StartSpawn()
        {
            GameManager.GetManager<DifficultyManager>().DEBUG_SetEasyStatsForEnemies();
            _enemiesList = GameManager.GetManager<DifficultyManager>().GetEnemiesStatsToUse();
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
