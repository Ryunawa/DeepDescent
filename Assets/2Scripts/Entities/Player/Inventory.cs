using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct InventoryObject
{
    public int ID;
    public int Amount;

    public InventoryObject(int id, int amount)
    {
        this.ID = id;
        this.Amount = amount;
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
            int indexOfItem = InventoryItems.FindIndex(x => x.ID == itemID);
            if(indexOfItem != -1)
            {
                InventoryItems[indexOfItem] = new InventoryObject(itemID, InventoryItems[indexOfItem].Amount + itemAmount);
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
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            SpawnFromInventory(newInventoryObject);
            if (InventoryItems[itemPos].Amount > 1)
            {
                newInventoryObject.Amount =- 1;
                InventoryItems[itemPos] = newInventoryObject;
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}. Remaning item {InventoryItems[itemPos].Amount}");
            }
            else
            {
                InventoryItems.RemoveAt(itemPos);
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}.No remaining item.");
            }
            return;
        }
        Debug.Log("[Inventory::DropFromInventory()] - Tried to drop item from inventory that was out of bound");
    }

    private void SpawnFromInventory(InventoryObject inventoryObject)
    {
        Instantiate(GlobalItemList.items.Find(x => x.ID == inventoryObject.ID).ObjectPrefab, transform.position, Quaternion.identity);
    }

    public void  UseFromInventory(int itemPos)
    {
        if (InventoryItems.Count < (InventorySpace - 1))
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            ConsumableItem realItem = (ConsumableItem) GlobalItemList.items.Find(x => x.ID == newInventoryObject.ID);
            realItem.Use();
            if (InventoryItems[itemPos].Amount > 1)
            {
                newInventoryObject.Amount = -1;
                InventoryItems[itemPos] = newInventoryObject;
                Debug.Log($"[Inventory::UseFromInventory()] - Used item at pos {itemPos}. Remaning item {InventoryItems[itemPos].Amount}");
            }
            else
            {
                InventoryItems.RemoveAt(itemPos);
                Debug.Log($"[Inventory::UseFromInventory()] - Used item at pos {itemPos}.No remaining item.");
            }
            return;
        }
        Debug.Log("[Inventory::UseFromInventory()] - Tried to use item from inventory that was out of bound");
    }
}
