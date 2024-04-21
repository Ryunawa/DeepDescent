using _2Scripts.Enum;
using UnityEngine;

namespace _2Scripts.Struct
{
    [System.Serializable]
    public struct ItemStats
    {
        public GameObject prefab;
        public ItemCategory category;
        public Rarity rarity;
        public float dropRate;
    }
}