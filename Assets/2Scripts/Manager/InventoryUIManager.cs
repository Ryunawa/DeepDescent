using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Enum;
using NaughtyAttributes;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class InventoryUIManager : Singleton<InventoryUIManager>
    {
        [SerializeField] private ItemUI ItemPrefab;
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private GameObject inventoryBG;
        [SerializeField] private GameObject inventoryMove;
        [SerializeField] private ItemDetailUI itemDetailUI;
        [Header("Slots")]
        [SerializeField] private ItemUI Head;
        [SerializeField] private ItemUI Chest;
        [SerializeField] private ItemUI Legs;
        [SerializeField] private ItemUI Feet;
        [SerializeField] private ItemUI[] Rings = new ItemUI[2];
        [SerializeField] private ItemUI MainHand;
        [SerializeField] private ItemUI OffHand;
        
        public static readonly Dictionary<Rarity, Color32> Colors = new Dictionary<Rarity, Color32>()
        {
            {Rarity.Common, new Color32(255,255,255,255)},
            {Rarity.Uncommon, new Color32(30, 255, 0,255)},
            {Rarity.Rare, new Color32(0, 112, 221,255)},
            {Rarity.Epic, new Color32(163, 53, 238,255)},
            {Rarity.Legendary, new Color32(255, 128, 0,255)}
        };
        
        private Inventory _inventory;
        private List<ItemUI> ListUI = new List<ItemUI>();
        private bool _isOpened;

        public Inventory Inventory => _inventory;

        public ItemDetailUI ItemDetailUI => itemDetailUI;

        public GameObject InventoryMove
        {
            get => inventoryMove;
            set => inventoryMove = value;
        }

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
                Instantiate(ItemPrefab, inventoryBG.transform);
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