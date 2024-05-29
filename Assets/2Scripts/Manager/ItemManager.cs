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
        
        private Dictionary<int, Item> _itemsDictionary;

        private void Start()
        {
            _itemsDictionary = itemList.Items.ToDictionary(x => x.ID, x=>x);
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