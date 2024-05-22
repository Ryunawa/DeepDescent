using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
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
    public ArmorItem FeetArmor;
    public ArmorItem[] RingsItem = new ArmorItem[2];
    public ArmorItem NecklaceItem;

    public WeaponItem MainHandItem;
    public bool CanDualWield;
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
        MultiManager.instance.SpawnNetworkObjectServerRPC(GlobalItemList.FindItemFromID(inventoryObject.ID).ObjectPrefab.GetComponent<NetworkObject>(), transform.position, Quaternion.identity);
    }

    public void  UseFromInventory(int itemPos)
    {
        if (InventoryItems.Count < (InventorySpace - 1))
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            ConsumableItem realItem = (ConsumableItem) GlobalItemList.FindItemFromID(newInventoryObject.ID);
            if (realItem)
            {
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
            else
            {
                Debug.Log("[Inventory::UseFromInventory()] - Tried to use an item from inventory that isn't a consumable.");
                return;
            }
        }
        Debug.Log("[Inventory::UseFromInventory()] - Tried to use item from inventory that was out of bound");
    }

    public void EquipFromInventory(int itemPos, bool OffSlot = false)
    {
        if (InventoryItems.Count < (InventorySpace - 1))
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            EquippableItem realItem = (EquippableItem)GlobalItemList.FindItemFromID(newInventoryObject.ID);
            if (realItem)
            {
                (bool, List<EquippableItem>) result = realItem.Equip(this, OffSlot);
                if (result.Item1)
                {
                    if (result.Item2.Count > 0)
                    {
                        for (int i = 0; i < result.Item2.Count; i++)
                        {
                            InventoryObject oldEquippedItem = new InventoryObject(result.Item2[i].ID, 1);
                            if (i == 0)
                                InventoryItems[itemPos] = oldEquippedItem;
                            else
                                AddToInventory(oldEquippedItem.ID, 1);
                        }
                        Debug.Log($"[Inventory::EquipFromInventory()] - Equipped new item at pos {itemPos} and put old item in it's place");
                    }
                    else
                    {
                        InventoryItems.Remove(newInventoryObject);
                        Debug.Log($"[Inventory::EquipFromInventory()] - Equipped new item at pos {itemPos}.");
                    }
                }
                else
                    Debug.Log($"[Inventory::EquipFromInventory()] - Couldn't equip item at pos {itemPos}. Nothing happened");
                return;
            }
            else
            {
                Debug.Log("[Inventory::EquipFromInventory()] - Tried to use an item from inventory that isn't a consumable.");
                return;
            }
        }
        Debug.Log("[Inventory::EquipFromInventory()] - Tried to use item from inventory that was out of bound");
    }

    public void UnequipItem(List<EquippableItem> itemsToUnequip)
    {
        if (itemsToUnequip.Count > 0)
        {
            for (int i = 0; i < itemsToUnequip.Count; i++)
            {
                InventoryObject oldEquippedItem = new InventoryObject(itemsToUnequip[i].ID, 1);
                AddToInventory(oldEquippedItem.ID, 1);
            }
            Debug.Log($"[Inventory::UnequipItem()] - Unequipped {itemsToUnequip.Count} item(s)");
            return;
        }
        Debug.Log($"[Inventory::UnequipItem()] - Couldn't find any object to unequip.");
            return;
    }

    [Button]
    public void DropFirstItem()
    {
        DropFromInventory(0);
    }
}
