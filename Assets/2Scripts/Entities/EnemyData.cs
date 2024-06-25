using _2Scripts.Helpers;
using _2Scripts.Manager;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEngine;

namespace _2Scripts.Entities
{
    
    public class EnemyData : GameManagerSync<EnemyData> 

    {
        public int roomSpawnedInID;
        private EnemyStats _enemyStats;

        public EnemyStats enemyStats
        {
            get => _enemyStats;
            set => _enemyStats = value;
        }


        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            GameManager.GetManager<DifficultyManager>().OnEnemiesStatsUpdatedEventHandler -= UpdateStatsOnNewLevel;
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState == GameState.Generating)
            {
                GameManager.GetManager<DifficultyManager>().OnEnemiesStatsUpdatedEventHandler += UpdateStatsOnNewLevel;
            }
            
        }

        /// <summary>
        /// Allow us to increase enemy stats at each new level
        /// Do not use it to decrease health or armor if the enemy is hit
        /// </summary>
        /// <param name="pReceiver">the enemy prefab to act on</param>
        /// <param name="pNewEnemyStats">the new stats for the prefab</param>
        private void UpdateStatsOnNewLevel(object pReceiver, EnemyStats pNewEnemyStats)
        {
            if (pReceiver.Equals(gameObject))
            {
                _enemyStats = pNewEnemyStats;
            }
        }
        
        //DEBUG ONLY
        [Header("DEBUG ONLY")]
        [SerializeField] private int damageToInflict;
        [SerializeField] private int armorPenetration;
        [ReadOnly] public float damageInflicted;
        
        [Button]
        private void DEBUG_DamageTaken()
        {
            float effectiveArmor = _enemyStats.armor * (1 - armorPenetration / 100f);
            float damageReductionFactor = 1 - effectiveArmor / 100;
            float damage = damageToInflict * damageReductionFactor;
            damageInflicted = damage;
        }
    }
}