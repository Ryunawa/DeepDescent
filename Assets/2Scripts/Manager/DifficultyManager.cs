using System;
using System.Collections.Generic;
using _2Scripts.Enum;
using _2Scripts.Helpers;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static _2Scripts.Helpers.StructureAccessMethods;

namespace _2Scripts.Manager
{
    public class DifficultyManager : GameManagerSync<DifficultyManager>
    {
        public EventHandler<EnemyStats> OnEnemiesStatsUpdatedEventHandler;

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
        private float _difficultyMultiplier;

        public DifficultyMode pDifficulty_var;

        #endregion

        private void OnDisable()
        {
            GameManager.GetManager<GameFlowManager>().OnNextLevelEvent.RemoveListener(UpdateDifficultyOverTime);
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel || !IsHost) return;
            
            AdjustDifficultyParameters(NetworkManager.ConnectedClients.Count, (DifficultyMode)Int32.Parse(GameManager.GetManager<MultiManager>().Lobby.Data["Difficulty"].Value));
            
            GameManager.GetManager<GameFlowManager>().OnNextLevelEvent.AddListener(UpdateDifficultyOverTime);
        }

        /// <summary>
        /// Must be called by the game manager or whatever starts the game.
        /// Adjust difficulty depending on the parameters.
        /// </summary>
        /// <param name="pDifficulty"> Easy, Normal or Hard</param>
        /// <param name="pNumPlayers">One, two, three or four</param>
        public void AdjustDifficultyParameters(int pNumPlayers, DifficultyMode pDifficulty = DifficultyMode.Normal)
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

            for (int i = 0; i < _enemyTypesStructToUse.statsInfos.Count; i++)
            {
                OnEnemiesStatsUpdatedEventHandler?.Invoke(this, GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i));
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

            for (int i = 0; i < _enemyTypesStructToUse.statsInfos.Count; i++)
            {
                _enemyTypesStructToUse.statsInfos[i] = AdjustEnemyStats(_enemyTypesStructToUse.statsInfos[i], multiplier);
            }
        }

        /// <summary>
        /// Adjust some stats of an enemy type depending on the number of players.
        /// </summary>
        /// <param name="pStatsToAdjust">struct of an enemy type's stats</param>
        /// <param name="pRate"></param>
        /// <returns></returns>
        private EnemyStats AdjustEnemyStats(EnemyStats pStatsToAdjust, float pRate)
        {
            pStatsToAdjust.health *= pRate;
            pStatsToAdjust.spawnRate *= pRate;
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
        private void UpdateDifficultyOverTime(Timer.Timer pTimer)
        {
            // play sound
            GameManager.GetManager<AudioManager>().PlaySfx("TimerUpdate");

            double elapsedTimeMinutes = pTimer.GetStopWatchObject().Elapsed.TotalMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + elapsedTimeMinutes / timeInterval) * baseTimeRate);

            for (int i = 0; i < _enemyTypesStructToUse.statsInfos.Count; i++)
            {
                _enemyTypesStructToUse.statsInfos[i] = AdjustEnemyStats(_enemyTypesStructToUse.statsInfos[i], multiplier);
            }

            for (int i = 0; i < _enemyTypesStructToUse.statsInfos.Count; i++)
            {
                OnEnemiesStatsUpdatedEventHandler?.Invoke(this, GetStructElementByIndex<EnemyStats>(_enemyTypesStructToUse, i));
            }

            _difficultyMultiplier = multiplier;
            Debug.Log("Multiplicateur : " + multiplier);
        }

        /// <summary>
        /// Return the difficulty multiplier apply to the enemy
        /// </summary>
        /// <returns></returns>
        public float GetDifficultyMultiplier()
        {
            return _difficultyMultiplier;
        }

        // /!\ DEBUG ONLY /!\
        [Button]
        public void DEBUG_SetEasyStatsForEnemies()
        {
            AdjustDifficultyParameters(1, DifficultyMode.Easy);
        }

        // /!\ DEBUG ONLY /!\
        [Button]
        public void DEBUG_CalculateEnemyMultiplierFor40Minutes()
        {
            double totalElapsedTimeMinutes = totalTimeInMinutes;
            float multiplier = 1 + (float)(Math.Log(1 + totalElapsedTimeMinutes / timeInterval) * baseTimeRate);

            Debug.Log("Stats multipliées par " + multiplier + " au bout de " + totalElapsedTimeMinutes + " minutes.");
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
