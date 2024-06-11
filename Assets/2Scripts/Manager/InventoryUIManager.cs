using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Enum;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

namespace _2Scripts.Manager
{
    public class InventoryUIManager : Singleton<InventoryUIManager>
    {
        [SerializeField] private ItemUI ItemPrefab;
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private GameObject inventoryBG;
        [SerializeField] private GameObject shopRoot;
        [SerializeField] private GameObject shopBG;
        [SerializeField] private GameObject inventoryMove;
        [SerializeField] private GameObject shopMove;
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
        public List<ItemUI> ListShop = new List<ItemUI>();
        private bool _isOpened;

        public Inventory Inventory => _inventory;

        public ItemDetailUI ItemDetailUI => itemDetailUI;

        public GameObject InventoryMove
        {
            get => inventoryMove;
            set => inventoryMove = value;
        }

        public GameObject ShopMove
        {
            get => shopMove;
            set => shopMove = value;
        }

        private void Start()
        {
            InputManager.instance.Inputs.Player.Inventory.started += context => ToggleInventory();
            
            _inventory = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().inventory;
            SetupInventory(inventoryRoot, inventoryBG, ListUI);
            SetupInventory(shopRoot, shopBG, ListShop);

            inventoryUI.SetActive(false);
        }



        private void ToggleInventory()
        {
            Debug.Log("Toggle Inv");

            bool value = _isOpened = !_isOpened;

            inventoryUI.SetActive(value);
            Cursor.visible = value;
        }
        
        
        private void SetupInventory(GameObject inventoryRoot, GameObject inventoryBG, List<ItemUI> List)
        {
            for (int i = 0; i < _inventory.InventorySpace; i++)
            {
                ItemUI item = Instantiate(ItemPrefab, inventoryRoot.transform);
                ItemUI itemBG = Instantiate(ItemPrefab, inventoryBG.transform);
                if (List == ListShop) item.IsInventoryShop = true;
                itemBG.IsInventory = false;
                List.Add(item);
            }

            if (List == ListShop) DrawInventoryShop();
            else DrawInventory();
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

        public void DrawInventoryShop()
        {
            for (int i = 0; i < _inventory.InventorySpace; i++)
            {
                if (i < _inventory.InventoryItems.Count)
                {
                    ListShop[i].Setup(_inventory.InventoryItems[i].ID, _inventory.InventoryItems[i].Amount);
                }
                else
                {
                    ListShop[i].Clear();
                }
            }
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