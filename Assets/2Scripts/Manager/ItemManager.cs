using _2Scripts.Enum;
using _2Scripts.ProceduralGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.Manager
{
    public class ItemManager : Singleton<ItemManager>
    {
        [SerializeField] public ItemList itemList;
        [SerializeField] private int itemCount = 10;
        [SerializeField] private List<ItemSpawnPoint> itemSpawnPoints;

        [SerializeField] public ItemList weaponList;
        [SerializeField] public ItemList armorList;
        [SerializeField] public ItemList potionList;
        [SerializeField] public ItemList parchmentList;

        private Dictionary<int, Item> _itemsDictionary;

        private void Start()
        {
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
                        weaponList.Items.Add(item);
                        break;
                    case ArmorItem:
                        armorList.Items.Add(item);
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
            Debug.Log("Start SpawnItem");
            float difficultyMultiplier = DifficultyManager.instance.GetDifficultyMultiplier();
            List<Item> spawnableItems = itemList.Items;

            LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
            List<ItemSpawnPoint> itemSpawnPoints = levelGenerator.GetAllItemSpawnPoints();

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
                        Instantiate(itemToSpawn.ObjectPrefab, spawnPosition, randomRotation);

                        spawnPoint.isOccupied = true;
                        itemsSpawned++;
                    }
                }
            }
        }

        private Item GetRandomItemBasedOnRarity(List<Item> items, float difficultyMultiplier)
        {
            float[] spawnChances = CalculateSpawnChances(difficultyMultiplier);
            float randomWeight = UnityEngine.Random.Range(0f, 100f);

            for (int i = 0; i < items.Count; i++)
            {
                float itemWeight = spawnChances[(int)items[i].Rarity];
                if (randomWeight < itemWeight)
                {
                    return items[i];
                }
                randomWeight -= itemWeight;
            }

            return null;
        }

        private float[] CalculateSpawnChances(float difficultyMultiplier)
        {
            float[] baseChances = { 60f, 24f, 10f, 5f, 1f }; // Base chances for Common, Uncommon, Rare, Epic, Legendary
            float totalWeight = 0f;
            float[] adjustedChances = new float[baseChances.Length];

            for (int i = 0; i < baseChances.Length; i++)
            {
                adjustedChances[i] = baseChances[i] * Mathf.Pow(difficultyMultiplier, i);
                totalWeight += adjustedChances[i];
            }

            for (int i = 0; i < adjustedChances.Length; i++)
            {
                adjustedChances[i] = (adjustedChances[i] / totalWeight) * 100f;
            }

            return adjustedChances;
        }

    }
}