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
    [SerializeField] private bool IsInventory;
    [SerializeField] private bool IsDrop;
    [SerializeField] private bool IsOffHand;
    [SerializeField] private int Price;
    [SerializeField] private float sellMultiplicator = 0.9f;

    private int ItemID;
    private int ItemPos;

    private bool _isGrabbed;

    private Transform parentAfterDrag;
    
    int clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;
    
    public void Setup(int itemID, int quantity)
    {
        ItemID = itemID;
        Border.color = InventoryUIManager.Colors[ItemManager.instance.GetItem(itemID).Rarity];
        Image.sprite = ItemManager.instance.GetItem(itemID).InventoryIcon;
        Price = ItemManager.instance.GetItem(itemID).SellValue;

        if (Quantity != null) Quantity.text = ItemManager.instance.GetItem(ItemID).Stackable ? quantity.ToString() : "";
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
        if ( IsDrop) return;
        
        
        _isGrabbed = true;
        ItemPos = transform.GetSiblingIndex();
        parentAfterDrag = transform.parent;
        transform.SetParent(InventoryUIManager.instance.InventoryMove.transform);
        Image.raycastTarget = false;
        Border.raycastTarget = false;
        Border.transform.parent.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsDrop) return;
        
        transform.position = Input.mousePosition;
        InventoryUIManager.instance.ItemDetailUI.ToggleUI(false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsDrop) return;
        
        _isGrabbed = false;
        transform.SetParent(parentAfterDrag);
        transform.SetSiblingIndex(ItemPos);
        Image.raycastTarget = true;
        Border.raycastTarget = true;
        Border.transform.parent.GetComponent<Image>().raycastTarget = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Inventory inventory = InventoryUIManager.instance.Inventory;
        if (eventData.pointerDrag.GetComponent<ItemUI>().ItemID == -1) return;

        bool IsInShop = inventory.isInShop;

        // Drag item on the drop pannel
        if (IsDrop)
        {
            if (eventData.pointerDrag.GetComponent<ItemUI>().IsEquipment) return;

            if (!IsInShop)
            {
                inventory.DropFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
            }
            else
            {
                // gain gold for selling item
                int sellPrice = Mathf.FloorToInt(Price * sellMultiplicator);
                inventory.gold += sellPrice;
                Debug.Log(name + " sold for " + sellPrice + " gold.");
                // remove item
                inventory.RemoveFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
            }

            InventoryUIManager.instance.DrawInventory();
            return;
        }
        
        // Equip item from inventory to equipment
        if (IsEquipment && !eventData.pointerDrag.GetComponent<ItemUI>().IsEquipment)
        {
            inventory.EquipFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos, IsOffHand);
            InventoryUIManager.instance.DrawInventory();
            return;
        }

        // Desequip item from equipment to inventory
        if (IsInventory && !eventData.pointerDrag.GetComponent<ItemUI>().IsInventory && !IsInShop)
        {
            inventory.UnequipItem(new List<(EquippableItem, bool)>
            {
                (ItemManager.instance.GetItem(eventData.pointerDrag.GetComponent<ItemUI>().ItemID) as EquippableItem, eventData.pointerDrag.GetComponent<ItemUI>().IsOffHand)
            });
            InventoryUIManager.instance.DrawInventory();
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;

        ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();

        if (itemUI.ItemID == -1) return;
        
        Item item = ItemManager.instance.GetItem(itemUI.ItemID);
        
        InventoryUIManager.instance.ItemDetailUI.Setup(item);
        InventoryUIManager.instance.ItemDetailUI.ToggleUI(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;
        
        ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();
        
        if (itemUI.ItemID == -1) return;
        
        InventoryUIManager.instance.ItemDetailUI.ToggleUI(false);
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

            Inventory inventory = InventoryUIManager.instance.Inventory;
            ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();
            if (itemUI.ItemID == -1 ) return;
            
            if (IsEquipment)
            {

                bool IsInShop = inventory.isInShop;
                // equip item
                if (!IsInShop)
                {
                    inventory.UnequipItem(new List<(EquippableItem, bool)>
                    {
                        (ItemManager.instance.GetItem(itemUI.ItemID) as EquippableItem, itemUI.IsOffHand)
                    });
                }
                // buy item
                else
                {
                    if (inventory.gold >= Price)
                    {
                        bool isItemAdded = inventory.AddToInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos, 1);
                        if (isItemAdded)
                        {
                            inventory.gold -= Price;
                            Debug.Log(name + " bought for " + Price + " gold.");
                        }
                    }
                    else
                    {
                        Debug.Log("Not enough gold.");
                    }
                }
            }

            if (IsInventory)
            {
                itemUI.ItemPos = itemUI.transform.GetSiblingIndex();
                            
                Item item = ItemManager.instance.GetItem(itemUI.ItemID);

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
            
            InventoryUIManager.instance.DrawInventory();
            InventoryUIManager.instance.ItemDetailUI.ToggleUI(false);

        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
 
    }
}
