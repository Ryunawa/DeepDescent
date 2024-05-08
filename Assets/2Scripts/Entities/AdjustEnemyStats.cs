using _2Scripts.Manager;
using _2Scripts.Struct;
using UnityEngine;

namespace _2Scripts.Entities
{
    public class AdjustEnemyStats : MonoBehaviour

    {
        public EnemyStats enemyStats;

        private void OnEnable()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdated += UpdateStats;
        }

        private void OnDestroy()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdated -= UpdateStats;
        }

        private void UpdateStats(object receiver, EnemyStats newEnemyStats)
        {
            if (receiver.Equals(gameObject))
            {
                enemyStats = newEnemyStats;
            }
        }

    }
}