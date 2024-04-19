using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct InventoryObject
{
    public int id;
    public int amount;

    public InventoryObject(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}

public class Inventory : MonoBehaviour
{
    [Header("Equipped Items")]
    public ArmorItem ChestArmor;
    public ArmorItem LegArmor;
    public ArmorItem FeetItem;
    public ArmorItem[] RingsItem = new ArmorItem[2];
    public ArmorItem NecklaceItem;

    public WeaponItem MainHandItem;
    public WeaponItem OffHandItem;

    [Header("InventoryStuff")]
    [DoNotSerialize] public List<InventoryObject> InventoryItems = new List<InventoryObject>();
    public ItemList GlobalItemList;
    public int InventorySpace = 6;
    public void AddToInventory(int itemID, int itemAmount)
    {
        if (InventoryItems.Count < InventorySpace)
        {
            int indexOfItem = InventoryItems.FindIndex(x => x.id == itemID);
            if(indexOfItem != -1)
            {
                InventoryItems[indexOfItem] = new InventoryObject(itemID, InventoryItems[indexOfItem].amount + itemAmount);
                Debug.Log($"[Inventory::AddToInventory()] - Increased amount of item of ID: {itemID} by {itemAmount}");
            }
            else
            {
                InventoryItems.Add(new InventoryObject(itemID, itemAmount));
                Debug.Log($"[Inventory::AddToInventory()] - Added new item of ID: {itemID} with an amount of {itemAmount} to inventory");
            }
            return;
        }
        Debug.Log("[Inventory::AddToInventory()]; - Inventory is full");
    }

    public void DropFromInventory(int itemPos)
    {
        if (InventoryItems.Count < (InventorySpace - 1))
        {
            if (InventoryItems[itemPos].amount > 1)
            {
                //TODO do something, spawn item near
                InventoryObject newInventoryObject = InventoryItems[itemPos];
                newInventoryObject.amount =- 1;
                InventoryItems[itemPos] = newInventoryObject;
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}. Remaning item {InventoryItems[itemPos].amount}");
            }
            else
            {
                //TODO do something, spawn item near
                InventoryItems.RemoveAt(itemPos);
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}.No remaining item.");
            }
            return;
        }
        Debug.Log("[Inventory::DropFromInventory()] - Tried to drop item from inventory that was out of bound");
    }

    public void  UseFromInventory(int itemPos)
    {

    }
}
