using UnityEngine;

namespace _2Scripts.Struct
{
    [System.Serializable]
    public struct EnemyStats
    {
        [Tooltip("Id to identifie the mesh to use for the enemy")] 
        public int index;
        [Tooltip("Rate of spawn")] 
        public float spawnRate;
        [Tooltip("The health of an enemy")]
        public float health;
        [Tooltip("The damage dealt by the enemy")]
        public float damageDealt;
        [Tooltip("Percentage of damage reduced by the enemy' armor")]
        public float armor;
    }
}