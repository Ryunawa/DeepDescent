using System;
using System.Diagnostics;
using _2Scripts.Enum;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static _2Scripts.Helpers.StructureAccessMethods;

namespace _2Scripts.Manager
{
    public class DifficultyManager: Singleton<DifficultyManager>
    {
        public EventHandler<EnemyStats> OnEnemiesStatsUpdated;
        
        #region Variables

        [Header("Enemy Stats By Enemy Type")] 
        [SerializeField] private EnemyTypes easyDifficultyEnemyStats;
        [SerializeField] private EnemyTypes normalDifficultyEnemyStats;
        [SerializeField] private EnemyTypes hardDifficultyEnemyStats;
        
        [Space(15)] 
        
        [Header("Resources")]
        [SerializeField] private ResourceType easyDifficultyResourceStats;
        [SerializeField] private ResourceType normalDifficultyResourceStats;
        [SerializeField] private ResourceType hardDifficultyResourceStats;

        [Space(15)]
        
        [Header("Difficulty Rate Depending Number Of Players")]
        [SerializeField] private float onePlayerMultiplier;
        [SerializeField] private float twoPlayerMultiplier;
        [SerializeField] private float threePlayerMultiplier;
        [SerializeField] private float fourPlayerMultiplier;

        [Space(15)]
        
        [Header("Difficulty Over Time")] 
        // With these values, the enemies stats are multiply by 3
        [SerializeField] private int timeInterval = 5;
        [SerializeField] private float baseTimeRate = 0.9f;
        [SerializeField] private int totalTimeInMinutes = 40;

        [SerializeField] private Timer.Timer timer;
        
        private EnemyTypes _enemyTypesStructToUse;
        private ResourceType _resourcesDropRateStructToUse;
        
        #endregion
        
        /// <summary>
        /// Must be called by the game manager or whatever start the game.
        /// Adjust difficulty depending on the parameters.
        /// </summary>
        /// <param name="pDifficulty"> Easy, Normal or Hard</param>
        /// <param name="pNumPlayers">One, two, three or four</param>
        public void AdjustDifficultyParameters(DifficultyMode pDifficulty, int pNumPlayers)
        {
            switch (pDifficulty)
            {
                case DifficultyMode.Easy:
                    _enemyTypesStructToUse = easyDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = easyDifficultyResourceStats;
                    break;
                
                case DifficultyMode.Normal:
                    _enemyTypesStructToUse = normalDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = normalDifficultyResourceStats;
                    break;
                
                case DifficultyMode.Hard:
                    _enemyTypesStructToUse = hardDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = hardDifficultyResourceStats;
                    break;
            }
            AdjustEnemiesStatsForNumPlayers(pNumPlayers);
            
            for (int i = 0; i <= GetNumberOfElementsInStruct(_enemyTypesStructToUse); i++)
            {
                OnEnemiesStatsUpdated?.Invoke(GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i).enemyPrefab, 
                    GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i));
            }
        }

        /// <summary>
        /// Adjust all the enemy types stats depending on the number of players.
        /// </summary>
        /// <param name="pNumPlayers">The number of players</param>
        /// <returns></returns>
        private void AdjustEnemiesStatsForNumPlayers(int pNumPlayers)
        {
            float multiplier = GetMultiplierForNumPlayers(pNumPlayers);

            // WARNING: Might break the others stats we didn't modify (such as prefab ref, damage taken)
            _enemyTypesStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType1, multiplier);
            _enemyTypesStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType2, multiplier);
            _enemyTypesStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType3, multiplier);
            _enemyTypesStructToUse.enemyType4 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType3, multiplier);
        }
        
        /// <summary>
        /// Adjust some stats of a enemy type depending on the number of players.
        /// </summary>
        /// <param name="pStatsToAdjust">struct of an enemy type's stats</param>
        /// <param name="pRate"></param>
        /// <returns></returns>
        private EnemyStats AdjustEnemyStats(EnemyStats pStatsToAdjust, float pRate)
        {
            pStatsToAdjust.spawnRate *= pRate;
            pStatsToAdjust.health *= pRate;
            pStatsToAdjust.damageDealt *= pRate;

            return pStatsToAdjust;
        }
        
        /// <summary>
        /// Return a multiplier depending on the number of players.
        /// </summary>
        /// <param name="pNumPlayers">Number of player</param>
        /// <returns></returns>
        private float GetMultiplierForNumPlayers(int pNumPlayers)
        {
            switch (pNumPlayers)
            {
                case 1:
                    return onePlayerMultiplier;

                case 2:
                    return twoPlayerMultiplier;

                case 3:
                    return threePlayerMultiplier;

                case 4:
                    return fourPlayerMultiplier;

                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Must be called by the game manager.
        /// Increase few stats for the difficulty over the time.
        /// </summary>
        /// <param name="pTimer"></param>
        public void UpdateDifficultyOverTime(Stopwatch pTimer)
        {
            double elapsedTimeMinutes = pTimer.Elapsed.TotalMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + elapsedTimeMinutes / timeInterval) * baseTimeRate);
            
            _enemyTypesStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType1, multiplier);
            _enemyTypesStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType2, multiplier);
            _enemyTypesStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypesStructToUse.enemyType3, multiplier);

            for (int i = 0; i <= GetNumberOfElementsInStruct(_enemyTypesStructToUse); i++)
            {
                OnEnemiesStatsUpdated?.Invoke(GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i).enemyPrefab, 
                                        GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i));
            }
            
            Debug.Log("Multiplicateur : " + multiplier);
        }

        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_SetEasyStatsForEnemies()
        {
            AdjustDifficultyParameters(DifficultyMode.Easy, 1);
        }
        
        // /!\ DEBUG ONLY /!\
        [Button]
        private void DEBUG_UpdateDifficulty()
        {
            Debug.Log("===============================================");
            Debug.Log("Temps écoulé : " + timer.GetTimerElapsedTime());
            UpdateDifficultyOverTime(timer._timer);
        }
        
        // /!\ DEBUG ONLY /!\
        [Button]
        public void DEBUG_CalculateEnemyMultiplierFor40Minutes()
        {
            double totalElapsedTimeMinutes = totalTimeInMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + totalElapsedTimeMinutes / timeInterval) * baseTimeRate);

            Debug.Log("Stats multipliées par " + multiplier + " au bout de " + totalTimeInMinutes + " minutes.");
        }
        
        /// <summary>
        /// Return all the enemy types with their own stats to use for the game
        /// </summary>
        /// <returns></returns>
        public EnemyTypes GetEnemiesStatsToUse()
        {
            return _enemyTypesStructToUse;
        }
        
        /// <summary>
        /// Return all the resources that can be dropped in the game
        /// </summary>
        /// <returns></returns>
        public ResourceType ResourcesDropRateToUse()
        {
            return _resourcesDropRateStructToUse;
        }
    }
}