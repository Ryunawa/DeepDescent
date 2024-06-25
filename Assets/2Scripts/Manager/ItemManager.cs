using _2Scripts.Enum;
using _2Scripts.ProceduralGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.Manager
{
    public class ItemManager : GameManagerSync<ItemManager>
    {
        [SerializeField] public ItemList itemList;
        [SerializeField] private int itemCount = 20;
        [SerializeField] private List<ItemSpawnPoint> itemSpawnPoints;

        [SerializeField] public ItemList weaponList;
        [SerializeField] public ItemList armorList;
        [SerializeField] public ItemList potionList;
        [SerializeField] public ItemList parchmentList;

        [SerializeField] public Gradient commonGradient;
        [SerializeField] public Gradient uncommonGradient;
        [SerializeField] public Gradient rareGradient;
        [SerializeField] public Gradient epicGradient;
        [SerializeField] public Gradient legendaryGradient;

        private Dictionary<int, Item> _itemsDictionary;

        protected override void Start()
        {
            base.Start();
            _itemsDictionary = itemList.Items.ToDictionary(x => x.ID, x=>x);
            InitializeItemLists();
        }

        public void StartSpawningItems()
        {
            SpawnItems(itemCount);
        }

        private void InitializeItemLists()
        {
            if (weaponList == null || armorList == null || potionList == null || parchmentList == null)
            {
                Debug.LogError("One or more ItemLists are not assigned in the Inspector.");
                return;
            }

            weaponList.Items.Clear();
            armorList.Items.Clear();
            potionList.Items.Clear();
            parchmentList.Items.Clear();

            foreach (var item in itemList.Items)
            {
                switch (item)
                {
                    case WeaponItem:
                        if(item.Rarity != Rarity.Legendary) weaponList.Items.Add(item);
                        break;
                    case ArmorItem:
                        if (item.Rarity != Rarity.Legendary) armorList.Items.Add(item);
                        break;
                    case PotionItem:
                        potionList.Items.Add(item);
                        break;
                    case ParchmentItem:
                        parchmentList.Items.Add(item);
                        break;
                }
            }
        }

        public Item GetItem(int id)
        {
            return itemList.FindItemFromID(id);
        }

        public GameObject GetItemPrefab(int id)
        {
            return itemList.FindItemFromID(id).ObjectPrefab;
        }

        public NetworkObject GetItemNetworkObject(int id)
        {
            itemList.FindItemFromID(id).ObjectPrefab.TryGetComponent(out NetworkObject networkObject);
            return networkObject;
        }

        private void SpawnItems(int numberOfItems)
        {
            float difficultyMultiplier = GameManager.GetManager<DifficultyManager>().GetDifficultyMultiplier();
            List<Item> spawnableItems = itemList.Items;

            LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
            itemSpawnPoints = levelGenerator.GetAllShuffledItemSpawnPoints();

            int itemsSpawned = 0;
            foreach (var spawnPoint in itemSpawnPoints)
            {
                if (itemsSpawned >= numberOfItems)
                {
                    break;
                }

                if (!spawnPoint.isOccupied)
                {
                    Item itemToSpawn = GetRandomItemBasedOnRarity(spawnableItems, difficultyMultiplier);
                    if (itemToSpawn != null)
                    {
                        Vector3 spawnPosition = spawnPoint.transform.position;
                        Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(-180f, 180f), 0);

                        GameObject instantiatedItem = Instantiate(itemToSpawn.ObjectPrefab, spawnPosition, randomRotation);
                        instantiatedItem.GetComponent<NetworkObject>().Spawn();

                        spawnPoint.isOccupied = true;
                        itemsSpawned++;
                    }
                }
            }
        }

        private Item GetRandomItemBasedOnRarity(List<Item> items, float difficultyMultiplier)
        {
            List<Item> itemsTemp = ShuffleItems(items);
            float[] spawnChances = CalculateSpawnChances(difficultyMultiplier);

            float totalWeight = spawnChances.Sum();
            float randomWeight = UnityEngine.Random.Range(0f, totalWeight);

            for (int i = 0; i < items.Count; i++)
            {
                float itemWeight = spawnChances[(int)items[i].Rarity];
                if (randomWeight < itemWeight)
                {
                    return items[i];
                }
                randomWeight -= itemWeight;
            }

            return items[UnityEngine.Random.Range(0, items.Count)];
        }

        public List<Item> ShuffleItems(List<Item> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                Item temp = items[i];
                items[i] = items[randomIndex];
                items[randomIndex] = temp;
            }
            return items;
        }

        private float[] CalculateSpawnChances(float difficultyMultiplier)
        {
            float[] baseChances = { 60f, 24f, 10f, 5f, 1f };
            float totalWeight = 0f;
            float[] adjustedChances = new float[baseChances.Length];

            for (int i = 0; i < baseChances.Length; i++)
            {
                adjustedChances[i] = baseChances[i] * Mathf.Pow(difficultyMultiplier, i);
                totalWeight += adjustedChances[i];
            }

            return adjustedChances;
        }

        public Gradient GetGradientFromRarity(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonGradient,
                Rarity.Uncommon => uncommonGradient,
                Rarity.Rare => rareGradient,
                Rarity.Epic => epicGradient,
                Rarity.Legendary => legendaryGradient,
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
            };
        }
    }
}