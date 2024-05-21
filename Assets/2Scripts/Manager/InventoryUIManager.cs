using System;
using System.Collections.Generic;
using _2Scripts.Enum;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _2Scripts.Manager
{
    public class InventoryUIManager : Singleton<InventoryUIManager>
    {
        [SerializeField] private ItemUI ItemPrefab;
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private ItemDetailUI itemDetailUI;
        [Header("Slots")]
        [SerializeField] private ItemUI Head;
        [SerializeField] private ItemUI Chest;
        [SerializeField] private ItemUI Legs;
        [SerializeField] private ItemUI Feet;
        [SerializeField] private ItemUI[] Rings = new ItemUI[2];
        [SerializeField] private ItemUI MainHand;
        [SerializeField] private ItemUI OffHand;
        
        public static readonly Dictionary<Rarity, Color> Colors = new Dictionary<Rarity, Color>()
        {
            {Rarity.Common, Color.white},
            {Rarity.Uncommon, new Color(30, 255, 0)},
            {Rarity.Rare, new Color(0, 112, 221)},
            {Rarity.Epic, new Color(163, 53, 238)},
            {Rarity.Legendary, new Color(255, 128, 0)}
        };
        
        private Inventory _inventory;
        private List<ItemUI> ListUI = new List<ItemUI>();
        private bool _isOpened;

        public Inventory Inventory => _inventory;

        public ItemDetailUI ItemDetailUI => itemDetailUI;

        private void Start()
        {
            InputManager.instance.Inputs.Player.Inventory.started += context => ToggleInventory();
            
            _inventory = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().inventory;
            SetupInventory();
            
            gameObject.SetActive(false);
        }


        private void ToggleInventory()
        {
            Debug.Log("Toggle Inv");

            bool value = _isOpened = !_isOpened;
            
            gameObject.SetActive(value);
            Cursor.visible = value;
        }
        
        
        private void SetupInventory()
        {
            for (int i = 0; i < _inventory.InventorySpace; i++)
            {
                ItemUI item = Instantiate(ItemPrefab, inventoryRoot.transform);
                ListUI.Add(item);
            }
            
            DrawInventory();
        }

        [Button]
        public void DrawInventory()
        {
            for (int i = 0; i < _inventory.InventorySpace; i++)
            {
                if (i < _inventory.InventoryItems.Count)
                {
                    ListUI[i].Setup(_inventory.InventoryItems[i].ID, _inventory.InventoryItems[i].Amount);
                }
                else
                {
                    ListUI[i].Clear();
                }
            }

            DrawEquipment(Head, _inventory.NecklaceItem);
            DrawEquipment(Chest, _inventory.ChestArmor);
            DrawEquipment(Legs, _inventory.LegArmor);
            DrawEquipment(Feet, _inventory.FeetArmor);
            DrawEquipment(Rings[0], _inventory.RingsItem[0]);
            DrawEquipment(Rings[1], _inventory.RingsItem[1]);
            DrawEquipment(MainHand, _inventory.MainHandItem);
            DrawEquipment(OffHand, _inventory.OffHandItem);
        }

        private void DrawEquipment(ItemUI itemUI, Item item)
        {
            if (item != null)
                itemUI.Setup(item.ID, -1);
            else
                itemUI.Clear();
        }
    }
}