using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _2Scripts.Enum;
using _2Scripts.Manager;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image Image;
    [SerializeField] private Image Border;
    [SerializeField] private TextMeshProUGUI Quantity;
    [SerializeField] private bool IsEquipment;
    [SerializeField] private bool isInventory;
    [SerializeField] private bool IsDrop;
    [SerializeField] private bool IsOffHand;
    [SerializeField] private bool isShop;
    [SerializeField] private bool isInventoryShop;
    [SerializeField] private bool isQuickSlot;
    [SerializeField] private int Price;
    [SerializeField] private float sellMultiplicator = 0.9f;

    private int ItemID;
    private int ItemPos;

    private bool _isGrabbed;

    private Transform parentAfterDrag;
    
    int clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    public bool IsShop
    {
        get => isShop;
        set => isShop = value;
    }

    public bool IsInventory
    {
        get => isInventory;
        set => isInventory = value;
    }

    public bool IsInventoryShop
    {
        get => isInventoryShop;
        set => isInventoryShop = value;
    }

    public void Setup(int itemID, int quantity)
    {
        ItemID = itemID;
        Border.color = InventoryUIManager.Colors[GameManager.GetManager<ItemManager>().GetItem(itemID).Rarity];
        Image.sprite = GameManager.GetManager<ItemManager>().GetItem(itemID).InventoryIcon;
        Price = GameManager.GetManager<ItemManager>().GetItem(itemID).SellValue;

        if (Quantity != null) Quantity.text = GameManager.GetManager<ItemManager>().GetItem(ItemID).Stackable ? quantity.ToString() : "";
        Image.color = Color.white;
    }

    public void Clear()
    {
        ItemID = -1;
        if (Quantity != null) Quantity.text = "";
        Image.color = Color.clear;
        Border.color = Color.white;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemUI draggedItemUI = eventData.pointerDrag.GetComponent<ItemUI>();
        if (IsDrop || !draggedItemUI.IsInventory && !draggedItemUI.IsShop && !draggedItemUI.IsEquipment || draggedItemUI.Price <= 0) return;

        _isGrabbed = true;
        ItemPos = transform.GetSiblingIndex();
        parentAfterDrag = transform.parent;

        if (draggedItemUI.isInventoryShop || draggedItemUI.IsShop)
        {
            transform.SetParent(GameManager.GetManager<InventoryUIManager>().ShopMove.transform);
        }
        else
        {
            transform.SetParent(GameManager.GetManager<InventoryUIManager>().InventoryMove.transform);
        }
        Image.raycastTarget = false;
        Border.raycastTarget = false;
        Border.transform.parent.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ItemUI draggedItemUI = eventData.pointerDrag.GetComponent<ItemUI>();
        if (IsDrop || !draggedItemUI.IsInventory && !draggedItemUI.IsShop && !draggedItemUI.IsEquipment || draggedItemUI.Price <= 0) return;

        transform.position = Input.mousePosition;
        GameManager.GetManager<InventoryUIManager>().ItemDetailUI.ToggleUI(false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemUI draggedItemUI = eventData.pointerDrag.GetComponent<ItemUI>();
        if (IsDrop || !draggedItemUI.IsInventory && !draggedItemUI.IsShop && !draggedItemUI.IsEquipment || draggedItemUI.Price <= 0) return;

        _isGrabbed = false;
        transform.SetParent(parentAfterDrag);
        transform.SetSiblingIndex(ItemPos);
        Image.raycastTarget = true;
        Border.raycastTarget = true;
        Border.transform.parent.GetComponent<Image>().raycastTarget = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Inventory inventory = GameManager.GetManager<InventoryUIManager>().Inventory;
        ItemUI draggedItemUI = eventData.pointerDrag.GetComponent<ItemUI>();

        if (draggedItemUI.ItemID == -1) return;

        // Drag item on the drop panel
        if (IsDrop)
        {
            if (draggedItemUI.IsEquipment) return;

            if (!draggedItemUI.isShop && !draggedItemUI.IsInventoryShop)
            {
                inventory.DropFromInventory(draggedItemUI.ItemPos);
            }
            // Check if dropped in sell from inventory
            else if (!draggedItemUI.isShop && draggedItemUI.IsInventoryShop)
            {
                // gain gold for selling item
                int sellPrice = Mathf.FloorToInt(draggedItemUI.Price * draggedItemUI.sellMultiplicator);
                inventory.gold += sellPrice;

                Debug.Log(draggedItemUI.name + " sold for " + sellPrice + " gold.");

                inventory.RemoveFromInventory(draggedItemUI.ItemPos);
                GameManager.GetManager<InventoryUIManager>().DrawInventoryShop();
            }

            GameManager.GetManager<InventoryUIManager>().DrawInventory();

            return;
        }

        //drag and drop to equip potion in quick slot
        if (isQuickSlot && GameManager.GetManager<ItemManager>().GetItem(draggedItemUI.ItemID).GetType().BaseType == typeof(ConsumableItem))
        {
            int index = transform.GetSiblingIndex();
            
            inventory.EquipQuickSlot(index, draggedItemUI.ItemID);
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
        }

        if (draggedItemUI.isQuickSlot && isInventory)
        {
            inventory.UnEquipQuickSlot(draggedItemUI.ItemID);
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
        }
        
        // Equip item from inventory to equipment
        if (IsEquipment && !draggedItemUI.IsEquipment && !draggedItemUI.isShop)
        {
            inventory.EquipFromInventory(draggedItemUI.ItemPos, IsOffHand);
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
            return;
        }

        // Unequip item from equipment to inventory
        if (IsInventory && !draggedItemUI.IsInventory && !draggedItemUI.isShop)
        {
            inventory.UnequipItem(new List<(EquippableItem, bool)> {
                (GameManager.GetManager<ItemManager>().GetItem(draggedItemUI.ItemID) as EquippableItem, draggedItemUI.IsOffHand)
            });
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
        }

        // Buying item from shop
        if (IsInventory && !draggedItemUI.IsInventory && draggedItemUI.isShop)
        {
            if (inventory.gold >= draggedItemUI.Price)
            {
                bool isItemAdded = inventory.AddToInventory(draggedItemUI.ItemID, 1);
                if (isItemAdded)
                {
                    inventory.gold -= draggedItemUI.Price;

                    Debug.Log(draggedItemUI.name + " bought for " + draggedItemUI.Price + " gold.");

                    GameManager.GetManager<InventoryUIManager>().DrawInventoryShop();
                }
                else
                {
                    Debug.LogWarning("Can't add Item.");
                }
            }
            else
            {
                Debug.LogWarning("Not enough gold.");
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;

        ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();

        if (itemUI.ItemID == -1) return;
        
        Item item = GameManager.GetManager<ItemManager>().GetItem(itemUI.ItemID);
        
        GameManager.GetManager<InventoryUIManager>().ItemDetailUI.Setup(item);
        GameManager.GetManager<InventoryUIManager>().ItemDetailUI.ToggleUI(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;
        
        ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();
        
        if (itemUI.ItemID == -1) return;
        
        GameManager.GetManager<InventoryUIManager>().ItemDetailUI.ToggleUI(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;
        
        clicked++;
        if (clicked == 1) clicktime = Time.time;
 
        if ((clicked > 1 && Time.time - clicktime < clickdelay) || eventData.button == PointerEventData.InputButton.Right)
        {
            clicked = 0;
            clicktime = 0;

            Inventory inventory = GameManager.GetManager<InventoryUIManager>().Inventory;
            ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();
            if (itemUI.ItemID == -1 ) return;
            
            if (IsEquipment)
            {
                // equip item
                if (!isShop)
                {
                    inventory.UnequipItem(new List<(EquippableItem, bool)>
                    {
                        (GameManager.GetManager<ItemManager>().GetItem(itemUI.ItemID) as EquippableItem, itemUI.IsOffHand)
                    });
                }
            }

            if (IsInventory)
            {
                itemUI.ItemPos = itemUI.transform.GetSiblingIndex();
                            
                Item item = GameManager.GetManager<ItemManager>().GetItem(itemUI.ItemID);

                switch (item)
                {
                    case EquippableItem:
                        inventory.EquipFromInventory(itemUI.ItemPos, IsOffHand);
                        break;
                    case ConsumableItem:
                        inventory.UseFromInventory(itemUI.ItemPos);
                        break;
                    default:
                        break;
                }
            }

            //un-equip quick slot
            if (isQuickSlot)
            {
                inventory.UnEquipQuickSlot(ItemID);
            }
            
            
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
            GameManager.GetManager<InventoryUIManager>().ItemDetailUI.ToggleUI(false);

        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
 
    }
}
