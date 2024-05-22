using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    BOW,
    SWORD,
    AXE,
    DAGGERS,
    MAGIC_STAFF
}

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "ScriptableObjects/Item/Create New Weapon Item")]
public class WeaponItem : EquippableItem
{
    [Header("Weapon Specific")]
    public WeaponType WeaponType;
    public int AttackValue;
    public float AttackSpeed;
    public bool IsTwoHanded;
    //public bool IsDualWieldable;

    public override (bool, List<EquippableItem>) Equip(Inventory inventoryToEquipTo, bool EquipToOffHand = false)
    {
        if (!inventoryToEquipTo)
            return (false, null);
        List<EquippableItem> oldItems = new List<EquippableItem>();

        if (!IsTwoHanded)
        {
            if (EquipToOffHand)
            {
                oldItems.Add(inventoryToEquipTo.OffHandItem);
                inventoryToEquipTo.OffHandItem = this;
            }
            else
            {
                oldItems.Add(inventoryToEquipTo.MainHandItem);
                inventoryToEquipTo.MainHandItem = this;
            }
        }
        else
        {
            oldItems.Add(inventoryToEquipTo.MainHandItem);
            oldItems.Add(inventoryToEquipTo.OffHandItem);
            inventoryToEquipTo.MainHandItem = this;
        }

        return (true, oldItems);
    }

    public override string GetStats()
    {
        return $"Attack : {AttackValue}\r\nAttack speed : {AttackSpeed}" + (IsTwoHanded ? "\r\nTwo handed":"") + /*(IsDualWieldable?"\r\nCan dual wield":"")*/ ;
    }
}
