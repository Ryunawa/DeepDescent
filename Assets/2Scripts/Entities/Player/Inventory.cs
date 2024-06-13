using _2Scripts.Manager;
using _2Scripts.Save;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using _2Scripts.Interfaces;
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

public class Inventory : GameManagerSync<Inventory>
{
    [Header("Options")]
    [SerializeField] private bool overrideWithSavedInventory;
    [SerializeField] private VisibleItems visibleItems;
    
    [Space,Header("Equipped Items")]
    public ArmorItem ChestArmor;
    public ArmorItem LegArmor;
    public ArmorItem FeetArmor;
    public ArmorItem[] RingsItem = new ArmorItem[2];
    public ArmorItem NecklaceItem;

    public InventoryObject[] QuickSlots = new InventoryObject[3]; 
    
    public int gold;
    public bool isInShop;

    public WeaponItem MainHandItem;
    public bool CanDualWield;
    public WeaponItem OffHandItem;

    [Header("InventoryStuff")]
    [DoNotSerialize] public List<InventoryObject> InventoryItems = new List<InventoryObject>();
    //public ItemList GlobalItemList;
    public int InventorySpace = 6;

    public StatComponent stat;

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        if (gameState == GameState.InLevel)
        {
            if (overrideWithSavedInventory)
            {
                SaveSystem.LoadInventory();
            }
        }
    }

    public bool AddToInventory(int itemID, int itemAmount)
    {
        if (InventoryItems.Count + 1 <= InventorySpace)  // 0 is counted
        {
            var item = GameManager.GetManager<ItemManager>().GetItem(itemID);
            if (item.Stackable)
            {
                int indexOfItem = InventoryItems.FindIndex(x => x.ID == itemID);
                if (indexOfItem != -1)
                {
                    InventoryItems[indexOfItem] = new InventoryObject(itemID, InventoryItems[indexOfItem].Amount + itemAmount);
                    Debug.Log($"[Inventory::AddToInventory()] - Increased amount of stackable item of ID: {itemID} by {itemAmount}");
                }
                else
                {
                    InventoryItems.Add(new InventoryObject(itemID, itemAmount));
                    Debug.Log($"[Inventory::AddToInventory()] - Added new stackable item of ID: {itemID} with an amount of {itemAmount} to inventory");
                }
            }
            else
            {
                InventoryItems.Add(new InventoryObject(itemID, itemAmount));
                Debug.Log($"[Inventory::AddToInventory()] - Added new unstackable item of ID: {itemID} with an amount of {itemAmount} to inventory");
            }

            ActivateItemVisibilityInventory(item);
            SaveSystem.Save();
            GameManager.GetManager<InventoryUIManager>().DrawInventory();
            return true;
        }
        Debug.Log("[Inventory::AddToInventory()]; - Inventory is full");
        SaveSystem.Save();

        return false;
    }


    public void RemoveFromInventory(int itemPos)
    {
        if (InventoryItems.Count <= InventorySpace)
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            if (InventoryItems[itemPos].Amount > 1)
            {
                newInventoryObject.Amount =- 1;
                InventoryItems[itemPos] = newInventoryObject;
                Debug.Log($"[Inventory::DropFromInventory() | SellItem] - Dropped/Sold item at pos {itemPos}. Remaning item {InventoryItems[itemPos].Amount}");
            }
            else
            {
                InventoryItems.RemoveAt(itemPos);
                Debug.Log($"[Inventory::DropFromInventory() | SellItem] - Dropped/Sold item at pos {itemPos}.No remaining item.");
            }
            DeactivateItemVisibilityInventory(GameManager.GetManager<ItemManager>().GetItem(newInventoryObject.ID));
            SaveSystem.Save();
            return;
        }
        Debug.Log($"[Inventory::DropFromInventory() | SellItem] - Tried to drop/sell item from inventory that was out of bound: InventoryItems.Count({InventoryItems.Count + 1}) <= InventorySpace({InventorySpace})");
        
        SaveSystem.Save();
    }

    public void DropFromInventory(int itemPos)
    {
        if (InventoryItems.Count <= InventorySpace)
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            SpawnInventoryItemsRpc(newInventoryObject.ID);
            if (InventoryItems[itemPos].Amount > 1)
            {
                newInventoryObject.Amount = -1;
                InventoryItems[itemPos] = newInventoryObject;
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}. Remaning item {InventoryItems[itemPos].Amount}");
            }
            else
            {
                InventoryItems.RemoveAt(itemPos);
                Debug.Log($"[Inventory::DropFromInventory()] - Dropped item at pos {itemPos}.No remaining item.");
            }
            DeactivateItemVisibilityInventory(GameManager.GetManager<ItemManager>().GetItem(newInventoryObject.ID));
            SaveSystem.Save();
            return;
        }
        Debug.Log($"[Inventory::DropFromInventory()] - Tried to drop item from inventory that was out of bound: InventoryItems.Count({InventoryItems.Count + 1}) <= InventorySpace({InventorySpace})");

        SaveSystem.Save();
    }

    [Rpc(SendTo.Server)]
    public void SpawnInventoryItemsRpc(int id)
    {
        NetworkObject o = Instantiate(GameManager.GetManager<ItemManager>().GetItemNetworkObject(id), transform.position, Quaternion.identity);
        o.Spawn();
    }

    public void UseFromInventory(int itemPos)
    {
        if (InventoryItems.Count <= InventorySpace)
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            ConsumableItem realItem = GameManager.GetManager<ItemManager>().GetItem(newInventoryObject.ID) as ConsumableItem;
            if (realItem)
            {
                realItem.Use(gameObject);
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
                DeactivateItemVisibilityInventory(GameManager.GetManager<ItemManager>().GetItem(newInventoryObject.ID));
                SaveSystem.Save();
                return;
            }
            else
            {
                Debug.Log("[Inventory::UseFromInventory()] - Tried to use an item from inventory that isn't a consumable.");
                SaveSystem.Save();
                return;
            }
        }
        Debug.Log($"[Inventory::UseFromInventory()] - Tried to use item from inventory that was out of bound: InventoryItems.Count({InventoryItems.Count + 1}) <= InventorySpace({InventorySpace})");

        SaveSystem.Save();
    }

    public void EquipFromInventory(int itemPos, bool OffSlot = false)
    {
        if (InventoryItems.Count <= InventorySpace)
        {
            InventoryObject newInventoryObject = InventoryItems[itemPos];
            EquippableItem realItem = GameManager.GetManager<ItemManager>().GetItem(newInventoryObject.ID) as EquippableItem;
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
                        AddFromEquipment(realItem, OffSlot);
                        Debug.Log($"[Inventory::EquipFromInventory()] - Equipped new item at pos {itemPos}.");
                    }
                }
                else
                {
                    Debug.Log($"[Inventory::EquipFromInventory()] - Couldn't equip item at pos {itemPos}. Nothing happened");
                }

                SaveSystem.Save();
                stat.UpdateArmourValue(this);
                return;
            }
            else
            {
                Debug.Log("[Inventory::EquipFromInventory()] - Tried to equip an item from inventory that isn't an equippable item.");
                SaveSystem.Save();
                return;
            }
        }
        Debug.Log($"[Inventory::EquipFromInventory()] - Tried to use item from inventory that was out of bound: InventoryItems.Count({InventoryItems.Count + 1}) <= InventorySpace({InventorySpace})");

        SaveSystem.Save();
    }

    public void UnequipItem(List<(EquippableItem, bool)> itemsToUnequip)
    {
        if (itemsToUnequip.Count > 0)
        {
            for (int i = 0; i < itemsToUnequip.Count; i++)
            {
                InventoryObject oldEquippedItem = new InventoryObject(itemsToUnequip[i].Item1.ID, 1);
                bool isItemAdded = AddToInventory(oldEquippedItem.ID, 1);
                if(isItemAdded)
                {
                    RemoveFromEquipment(oldEquippedItem, itemsToUnequip[i].Item2);
                    HideVisibleItem(itemsToUnequip[i].Item1, itemsToUnequip[i].Item2);
                }
                else Debug.Log($"[Inventory::UnequipItem()] - No available slot in inventory: {itemsToUnequip.Count}");
            }
            Debug.Log($"[Inventory::UnequipItem()] - Unequipped {itemsToUnequip.Count} item(s)");
            SaveSystem.Save();
            stat.UpdateArmourValue(this);
            return;
        }
        Debug.Log($"[Inventory::UnequipItem()] - Couldn't find any object to unequip.");
        SaveSystem.Save();
    }

    [Button]
    public void DropFirstItem()
    {
        DropFromInventory(0);
    }

    public void RemoveFromEquipment(InventoryObject item, bool offHand = false)
    {
        switch (GameManager.GetManager<ItemManager>().GetItem(item.ID))
        {
            case ArmorItem armorItem:

                ArmorType type = armorItem.ArmorType;
                switch (type)
                {
                    case ArmorType.NECKLACE :
                        NecklaceItem = null;
                        break;
                    case ArmorType.CHEST : 
                        ChestArmor = null;
                        break;
                    case ArmorType.PANTS: 
                        LegArmor = null;
                        break;
                    case ArmorType.FEET : 
                        FeetArmor = null;
                        break;
                    case ArmorType.RING :
                        if (offHand)
                        {
                            RingsItem[1] = null;
                        }
                        else
                        {
                            RingsItem[0] = null;
                        }
                        break;
                }
                
                    
                break;
            case WeaponItem:
                if (offHand)
                {
                    OffHandItem = null;
                }
                else
                {
                    MainHandItem = null;
                }
                break;
        }
        stat.UpdateArmourValue(this);
    }


    private void AddFromEquipment(EquippableItem item, bool OffSlot = false)
    {
        switch (item)
        {
            case ArmorItem:
                visibleItems.AddVisibleArmor();
                break;
            case WeaponItem weapon:
                // left hand - shield
                if (OffSlot)
                {
                    visibleItems.EquipLeftHand(weapon.Name);
                }
                // right hand - weapon
                else
                {
                    visibleItems.EquipRightHand(weapon.Name);
                }
                break;
            default:
                break;
        }
        stat.UpdateArmourValue(this);
    }

    private void HideVisibleItem(EquippableItem item, bool OffSlot = false)
    {
        switch (item)
        {
            case ArmorItem:
                visibleItems.RemoveVisibleArmor();
                break;
            case WeaponItem weapon:
                // left hand - shield
                if (OffSlot)
                {
                    visibleItems.UnequipLeftHand();
                }
                // right hand - weapon
                else
                {
                    visibleItems.UnequipRightHand();
                }
                break;
            default:
                break;
        }
    }

    public List<int> GetEquipmentIds()
    {
        List<int> equipment = new List<int>();
        
        equipment.Add(NecklaceItem != null?NecklaceItem.ID:-1);
        equipment.Add(ChestArmor != null?ChestArmor.ID:-1);
        equipment.Add(LegArmor != null?LegArmor.ID:-1);
        equipment.Add(FeetArmor != null?FeetArmor.ID:-1);
        equipment.Add(RingsItem[0] != null?RingsItem[0].ID:-1);
        equipment.Add(RingsItem[1] != null?RingsItem[1].ID:-1);
        equipment.Add(MainHandItem != null?MainHandItem.ID:-1);
        equipment.Add(OffHandItem != null?OffHandItem.ID:-1);

        return equipment;
    }

    public void SetEquipment(List<int> ids)
    {
        bool isOffHand = false;
        for (int i = 0; i < ids.Count; i++)
        {
            EquippableItem item = GameManager.GetManager<ItemManager>().GetItem(ids[i]) as EquippableItem;
            if (i == 5 || i ==7)
            {
                isOffHand = true;
            }

            if (item != null)
            {
                item.Equip(this, isOffHand);
                AddFromEquipment(item, isOffHand);
            }
            isOffHand = false;
        }
    }

    // Activates the visibility of the item based on its type
    private void ActivateItemVisibilityInventory(Item item)
    {
        switch (item)
        {
            case ArmorItem:
                visibleItems.AddVisibleArmor();
                break;
            case WeaponItem weapon:
                // left hand - shield
                if (weapon.WeaponType == WeaponType.SHIELD)
                {
                    visibleItems.AddVisibleShield();
                }
                // right hand - weapon
                else
                {
                    visibleItems.AddVisibleSword();
                }
                break;
            case ParchmentItem:
                visibleItems.AddVisibleSpellBook();
                break;
            case PotionItem:
                visibleItems.AddVisiblePotions();
                break;
            default: // scrap
                visibleItems.AddVisibleScrap();
                break;
        }
    }

    private void DeactivateItemVisibilityInventory(Item item)
    {
        switch (item)
        {
            case ArmorItem _:
                visibleItems.RemoveVisibleArmor();
                break;
            case WeaponItem weapon:
                // left hand - shield
                if (weapon.WeaponType == WeaponType.SHIELD)
                {
                    visibleItems.RemoveVisibleShield();
                }
                // right hand - weapon
                else
                {
                    visibleItems.RemoveVisibleSword();
                }
                break;
            case ParchmentItem _:
                visibleItems.RemoveVisibleSpellBook();
                break;
            case PotionItem _:
                visibleItems.RemoveVisiblePotions();
                break;
            default: // scrap
                visibleItems.RemoveVisibleScrap();
                break;
        }
    }
    
    public void EquipQuickSlot(int index, int itemID)
    {
        QuickSlots[index] = InventoryItems[InventoryItems.FindIndex(x=> x.ID == itemID)];
        Debug.Log($"Item {itemID} equipped in slot number {index}");
    }

    public void UnEquipQuickSlot(int itemID)
    {
        QuickSlots[Array.FindIndex(QuickSlots, x => x.ID == itemID)] = new InventoryObject(-1,0);
        Debug.Log($"Item {itemID} has been un-equipped from quick slots");
    }
}
