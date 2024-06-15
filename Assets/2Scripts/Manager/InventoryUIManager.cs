using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Enum;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.UI;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

namespace _2Scripts.Manager
{
    public class InventoryUIManager : GameManagerSync<InventoryUIManager>
    {
        [SerializeField] private ItemUI ItemPrefab;
        public GameObject inventoryUI;
        public GameObject shopUI;
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private GameObject inventoryBG;
        public GameObject shopRoot;
        public Transform weaponUIParent;
        public Transform armorUIParent;
        public Transform potionUIParent;
        public Transform parchmentUIParent;
        [SerializeField] private GameObject shopBG;
        [SerializeField] private GameObject inventoryMove;
        [SerializeField] private GameObject shopMove;
        [SerializeField] private ItemDetailUI itemDetailUI;
        [SerializeField] private HUD _hud;
        
        [Space, Header("Slots")]
        [SerializeField] private ItemUI Head;
        [SerializeField] private ItemUI Chest;
        [SerializeField] private ItemUI Legs;
        [SerializeField] private ItemUI Feet;
        [SerializeField] private ItemUI[] Rings = new ItemUI[2];
        [SerializeField] private ItemUI MainHand;
        [SerializeField] private ItemUI OffHand;
        
        [Space, Header("Slots (but the fast ones)")]
        [SerializeField] private ItemUI[] quickSlots = new ItemUI[3];


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

        public bool IsOpened => _isOpened;

        public HUD HUD
        {
            get => _hud;
            set => _hud = value;
        }

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
        
        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel)return;
            
            InputManager.instance.Inputs.Player.Inventory.started += context => ToggleInventory();
            
            _inventory = GameManager.GetPlayerComponent<PlayerBehaviour>().inventory;
            
            for (var index = 0; index < _inventory.QuickSlots.Length; index++)
            {
                _inventory.QuickSlots[index] = new InventoryObject(-1, 0);
            }
                        
            SetupInventory(inventoryRoot, inventoryBG, ListUI);
            SetupInventory(shopRoot, shopBG, ListShop);
                        
            
            inventoryUI.SetActive(false);
        }


        private void ToggleInventory()
        {
            // if shop is not open
            if (shopUI.activeSelf == false)
            {
                bool value = _isOpened = !_isOpened;

                inventoryUI.SetActive(value);
                itemDetailUI.gameObject.SetActive(false);
                Cursor.visible = value;
            }
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

            for (var i = 0; i < _inventory.QuickSlots.Length; i++)
            {
                InventoryObject slot = _inventory.QuickSlots[i];

                if (slot.ID == -1)
                {
                    quickSlots[i].Clear();
                    _hud.ClearQuickSlot(i);
                }
                else
                {
                    quickSlots[i].Setup(_inventory.QuickSlots[i].ID, _inventory.QuickSlots[i].Amount);
                    _hud.SetQuickSlot(GameManager.GetManager<ItemManager>().GetItem(_inventory.QuickSlots[i].ID).InventoryIcon,_inventory.QuickSlots[i].Amount,i);
                }
            }
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