using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Enum;
using _2Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    static ItemList GlobalItemList;
    
    [SerializeField] private Image Image;
    [SerializeField] private Image Border;
    [SerializeField] private bool IsEquipment;
    [SerializeField] private bool IsDrop;
    private int ItemID;
    private int ItemPos;
    
    private bool _isGrabbed;

    private Transform parentAfterDrag;

    private Dictionary<Rarity, Color> Colors = new Dictionary<Rarity, Color>()
    {
        {Rarity.Common, Color.white},
        {Rarity.Uncommon, new Color(30, 255, 0)},
        {Rarity.Rare, new Color(0, 112, 221)},
        {Rarity.Epic, new Color(163, 53, 238)},
        {Rarity.Legendary, new Color(255, 128, 0)}
    };
    
    public void Setup(int itemID)
    {
        if (GlobalItemList == null)
        {
            GlobalItemList = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().inventory.GlobalItemList;
        }
        
        ItemID = itemID;
        Border.color = Colors[GlobalItemList.FindItemFromID(itemID).Rarity];
        Image.sprite = GlobalItemList.FindItemFromID(itemID).InventoryIcon;
        Image.color = Color.white;
    }

    public void Clear()
    {
        Image.color = Color.clear;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEquipment || IsDrop)
        {
            return;
        }
        
        _isGrabbed = true;
        ItemPos = transform.GetSiblingIndex();
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        Image.raycastTarget = false;
        Border.raycastTarget = false;
        Border.transform.parent.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
            inventory.DropFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
        }
        
        if (IsEquipment)
        {
            inventory.EquipFromInventory(eventData.pointerDrag.GetComponent<ItemUI>().ItemPos);
        }
        
        InventoryUIManager.instance.DrawInventory();
    }
}
