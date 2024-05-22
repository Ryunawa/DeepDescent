using _2Scripts.Manager;
using _2Scripts.Struct;
using UnityEngine;

namespace _2Scripts.Entities
{
    public class EnemyData : MonoBehaviour

    {
        public int roomSpawnedInID;
        public EnemyStats enemyStats;

        private void OnEnable()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdatedEventHandler += UpdateStats;
        }

        private void OnDestroy()
        {
            DifficultyManager.instance.OnEnemiesStatsUpdatedEventHandler -= UpdateStats;
        }

        private void UpdateStats(object receiver, EnemyStats newEnemyStats)
        {
            if (receiver.Equals(gameObject))
            {
                enemyStats = newEnemyStats;
            }
        }

        public void SetRoomSpawnedInID(int pRoomID)
        {
            roomSpawnedInID = pRoomID;
        }

    }
}