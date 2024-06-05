using _2Scripts.Manager;
using _2Scripts.Struct;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.Entities
{
    public class EnemyData : MonoBehaviour

    {
        public int roomSpawnedInID;
        public EnemyStats enemyStats;

        private void OnEnable()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdatedEventHandler += UpdateStatsOnNewLevel;
        }

        private void OnDestroy()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdatedEventHandler -= UpdateStatsOnNewLevel;
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
                enemyStats = pNewEnemyStats;
            }
        }

        public void SetRoomSpawnedInID(int pRoomID)
        {
            roomSpawnedInID = pRoomID;
        }

        public void OnGettingHit(float pDamage, float pArmorPenetration = 0)
        {
            float effectiveArmor = enemyStats.armor * (1 - pArmorPenetration / 100);
            float damageReductionFactor = 1 - effectiveArmor / 100;
            float damage = pDamage * damageReductionFactor;
            enemyStats.health -= damage;
        }
        
        //DEBUG ONLY
        [Header("DEBUG ONLY")]
        [SerializeField] private int damageToInflict;
        [SerializeField] private int armorPenetration;
        [ReadOnly] public float damageInflicted;
        
        [Button]
        private void DEBUG_DamageTaken()
        {
            float effectiveArmor = enemyStats.armor * (1 - armorPenetration / 100f);
            float damageReductionFactor = 1 - effectiveArmor / 100;
            float damage = damageToInflict * damageReductionFactor;
            damageInflicted = damage;
        }
    }
}