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
    }
}