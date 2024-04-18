using System;
using System.Diagnostics;
using _2Scripts.Enum;
using _2Scripts.Struct;
using UnityEngine;

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
        [SerializeField] private ResourcesDropRate easyDifficultyResourceDropRate;
        [SerializeField] private ResourcesDropRate normalDifficultyResourceDropRate;
        [SerializeField] private ResourcesDropRate hardDifficultyResourceDropRate;

        [Space(15)]
        
        [Header("Difficulty Rate Depending Number Of Players")]
        [SerializeField] private float onePlayerMultiplier;
        [SerializeField] private float twoPlayerMultiplier;
        [SerializeField] private float threePlayerMultiplier;
        [SerializeField] private float fourPlayerMultiplier;

        [Space(15)]
        
        [Header("Difficulty Over Time")] 
        //[SerializeField] private float interval;
        [SerializeField] private float rate;
        
        private EnemyType _enemyTypeStructToUse;
        private ResourcesDropRate _resourcesDropRateStructToUse;
        
        private TimeSpan _lastDifficultyIncreaseTime = TimeSpan.Zero;
        private TimeSpan _difficultyIncreaseInterval = TimeSpan.FromMinutes(5);
        
        #endregion
        
        public void AdjustDifficultyParameters(DifficultyMode pDifficulty, int pNumPlayers)
        {
            switch (pDifficulty)
            {
                case DifficultyMode.Easy:
                    _enemyTypeStructToUse = easyDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = easyDifficultyResourceDropRate;
                    break;
                
                case DifficultyMode.Normal:
                    _enemyTypeStructToUse = normalDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = normalDifficultyResourceDropRate;
                    break;
                
                case DifficultyMode.Hard:
                    _enemyTypeStructToUse = hardDifficultyEnemyStats;
                    _resourcesDropRateStructToUse = hardDifficultyResourceDropRate;
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

            _enemyTypeStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType1, multiplier);
            _enemyTypeStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType2, multiplier);
            _enemyTypeStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType3, multiplier);
        }
        
        /// <summary>
        /// Adjust some stats of a enemy type depending on the number of players
        /// </summary>
        /// <param name="pStatsToAdjust">struct of an enemy type's stats</param>
        /// <param name="rate"></param>
        /// <returns></returns>
        private EnemyStats AdjustEnemyStats(EnemyStats pStatsToAdjust, float rate)
        {
            pStatsToAdjust.spawnRate *= rate;
            pStatsToAdjust.health *= rate;
            pStatsToAdjust.damageDealt *= rate;

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
        public void IncreaseDifficultyOverTime(Stopwatch pTimer)
        {
            TimeSpan elapsedTime = pTimer.Elapsed;
            
            if (elapsedTime - _lastDifficultyIncreaseTime >= _difficultyIncreaseInterval)
            {
                _enemyTypeStructToUse.enemyType1 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType1, rate);
                _enemyTypeStructToUse.enemyType2 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType2, rate);
                _enemyTypeStructToUse.enemyType3 = AdjustEnemyStats(_enemyTypeStructToUse.enemyType3, rate);

                _lastDifficultyIncreaseTime = elapsedTime;
            }
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
        public ResourcesDropRate ResourcesDropRateToUse()
        {
            return _resourcesDropRateStructToUse;
        }
    }
}