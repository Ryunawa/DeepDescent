using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _2Scripts.Enum;
using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    static ItemList GlobalItemList;
    
    [SerializeField] private Image Image;
    [SerializeField] private Image Border;
    [SerializeField] private TextMeshProUGUI Quantity;
    [SerializeField] private bool IsEquipment;
    [SerializeField] private bool IsDrop;
    [SerializeField] private bool IsOffHand;
    private int ItemID;
    private int ItemPos;
    
    private bool _isGrabbed;

    private Transform parentAfterDrag;
    
    int clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;
    
    public void Setup(int itemID, int quantity)
    {
        if (GlobalItemList == null)
        {
            GlobalItemList = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().inventory.GlobalItemList;
        }
        
        ItemID = itemID;
        Border.color = InventoryUIManager.Colors[GlobalItemList.FindItemFromID(itemID).Rarity];
        Image.sprite = GlobalItemList.FindItemFromID(itemID).InventoryIcon;
        
        if (Quantity != null) Quantity.text = GlobalItemList.FindItemFromID(ItemID).Stackable ? quantity.ToString() : "";
        Image.color = Color.white;
    }

    public void Clear()
    {
        ItemID = -1;
        if (Quantity != null) Quantity.text = "";
        Image.color = Color.clear;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEquipment || IsDrop) return;
        
        
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
        if (IsEquipment || IsDrop) return;
        
        transform.position = Input.mousePosition;
        InventoryUIManager.instance.ItemDetailUI.ToggleUI(false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsEquipment || IsDrop) return;
        
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

        if (IsDrop)
        {
            Debug.Log("Drop");
            inventory.DropFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
        }
        
        if (IsEquipment)
        {
            inventory.EquipFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
        }
        
        InventoryUIManager.instance.DrawInventory();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isGrabbed || IsDrop) return;
        
        Debug.Log("Hovering " + eventData.hovered.Count);
        foreach (var VARIABLE in eventData.hovered)
        {
            Debug.Log(VARIABLE.gameObject.name);
        }

        ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();

        if (itemUI.ItemID == -1) return;
     
        if (GlobalItemList == null) GlobalItemList = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().inventory.GlobalItemList;
        
        Item item = GlobalItemList.FindItemFromID(itemUI.ItemID);
        
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
        if (_isGrabbed || IsEquipment || IsDrop) return;
        
        clicked++;
        if (clicked == 1) clicktime = Time.time;
 
        if ((clicked > 1 && Time.time - clicktime < clickdelay) || eventData.button == PointerEventData.InputButton.Right)
        {
            clicked = 0;
            clicktime = 0;

            Inventory inventory = InventoryUIManager.instance.Inventory;
            
            ItemUI itemUI = eventData.pointerEnter.gameObject.GetComponentInParent<ItemUI>();
            
            itemUI.ItemPos = itemUI.transform.GetSiblingIndex();
            
            Item item = GlobalItemList.FindItemFromID(itemUI.ItemID);
            
            switch (item)
            {
                case EquippableItem:
                    inventory.EquipFromInventory(itemUI.ItemPos);
                    break;
                case ConsumableItem:
                    inventory.UseFromInventory(itemUI.ItemPos);
                    break;
                default:
                    break;
            }
            
            InventoryUIManager.instance.DrawInventory();
            InventoryUIManager.instance.ItemDetailUI.ToggleUI(false);

        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
 
    }
}
