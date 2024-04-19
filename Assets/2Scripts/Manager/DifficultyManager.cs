using System;
using System.Diagnostics;
using _2Scripts.Enum;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _2Scripts.Manager
{
    public class DifficultyManager: MonoBehaviour
    {
        #region Variables

        [Header("Enemy Stats By Enemy Type")] 
        [SerializeField] private EnemyType easyDifficultyEnemyStats;
        [SerializeField] private EnemyType normalDifficultyEnemyStats;
        [SerializeField] private EnemyType hardDifficultyEnemyStats;
        
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
        
        private EnemyType _enemyTypeStructToUse;
        private ResourceType _resourcesDropRateStructToUse;
        
        private TimeSpan _lastDifficultyIncreaseTime = TimeSpan.Zero;
        private readonly TimeSpan _difficultyIncreaseInterval = TimeSpan.FromMinutes(0.1666666666666667);
        
        #endregion

        // /!\ DEBUG ONLY /!\
        // private void Update()
        // {
        //     if (timer._timer.Elapsed >= _lastDifficultyIncreaseTime + _difficultyIncreaseInterval)
        //     {
        //         DEBUG_UpdateDifficulty();
        //         _lastDifficultyIncreaseTime = timer._timer.Elapsed;
        //     }
        // }

        public void AdjustDifficultyParameters(DifficultyMode pDifficulty, int pNumPlayers)
        {
            switch (pDifficulty)
            {
                case DifficultyMode.Easy:
                    _enemyTypeStructToUse = easyDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = easyDifficultyResourceStats;
                    break;
                
                case DifficultyMode.Normal:
                    _enemyTypeStructToUse = normalDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = normalDifficultyResourceStats;
                    break;
                
                case DifficultyMode.Hard:
                    _enemyTypeStructToUse = hardDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = hardDifficultyResourceStats;
                    break;
            }
            AdjustEnemiesStatsForNumPlayers(pNumPlayers);
        }

        /// <summary>
        /// Adjust all the enemy types stats depending on the number of players 
        /// </summary>
        /// <param name="pNumPlayers">The number of players</param>
        /// <returns></returns>
        private void AdjustEnemiesStatsForNumPlayers(int pNumPlayers)
        {
            float multiplier = GetMultiplierForNumPlayers(pNumPlayers);

            // Might break the others stats we didn't modify (such as prefab ref, damage taken)
            _enemyTypeStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType1, multiplier);
            _enemyTypeStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType2, multiplier);
            _enemyTypeStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType3, multiplier);
        }
        
        /// <summary>
        /// Adjust some stats of a enemy type depending on the number of players
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
        /// Return a multiplier depending on the number of players
        /// </summary>
        /// <param name="pNumPlayers"></param>
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
        /// Increase few stats for the difficulty over the time
        /// </summary>
        /// <param name="pTimer"></param>
        public void UpdateDifficultyOverTime(Stopwatch pTimer)
        {
            double elapsedTimeMinutes = pTimer.Elapsed.TotalMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + elapsedTimeMinutes / timeInterval) * baseTimeRate);
            
            // _enemyTypeStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType1, multiplier);
            // _enemyTypeStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType2, multiplier);
            // _enemyTypeStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType3, multiplier);
            
            Debug.Log("Multiplicateur : " + multiplier);
        }
        
        // /!\ DEBUG ONLY /!\
        private void DEBUG_UpdateDifficulty()
        {
            Debug.Log("===============================================");
            Debug.Log("Temps écoulé : " + timer.GetTimerElapsedTime());
            UpdateDifficultyOverTime(timer._timer);
        }
        
        // /!\ DEBUG ONLY /!\
        [Button]
        public void DEBUG_CalculateEnemyMultiplierFor30Minutes()
        {
            double totalElapsedTimeMinutes = totalTimeInMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + totalElapsedTimeMinutes / timeInterval) * baseTimeRate);

            Debug.Log("Stats multipliées par " + multiplier + " au bout de " + totalTimeInMinutes + " minutes.");
        }


        /// <summary>
        /// Return all the enemy types with their own stats to use for the game
        /// </summary>
        /// <returns></returns>
        public EnemyType EnemiesStatsToUse()
        {
            return _enemyTypeStructToUse;
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