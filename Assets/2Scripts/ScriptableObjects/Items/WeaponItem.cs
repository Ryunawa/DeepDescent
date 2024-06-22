using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    BOW,
    SWORD,
    AXE,
    DAGGERS,
    MAGIC,
    SHIELD
}

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "ScriptableObjects/Item/Create New Weapon Item")]
public class WeaponItem : EquippableItem
{
    [Header("Weapon Specific")] 
    public GameObject SpellToSpawn;
    public WeaponType WeaponType;
    public int AttackValue;
    public float AttackSpeed;

    public override (bool, List<EquippableItem>) Equip(Inventory inventoryToEquipTo, bool EquipToOffHand = false)
    {
        if (!inventoryToEquipTo)
            return (false, null);
        if (inventoryToEquipTo.stat.CharacterStatPage.EquippableWeaponType.Contains(WeaponType))
            return (false, null);
        List<EquippableItem> oldItems = new List<EquippableItem>();

        switch (WeaponType)
        {
            case WeaponType.SHIELD:
                if (inventoryToEquipTo.OffHandItem)
                    oldItems.Add(inventoryToEquipTo.OffHandItem);
                inventoryToEquipTo.OffHandItem = this;
                break;
            default:
                if (inventoryToEquipTo.MainHandItem)
                    oldItems.Add(inventoryToEquipTo.MainHandItem);
                inventoryToEquipTo.MainHandItem = this;
                break;
        }
        return (true, oldItems);
    }

    public override string GetStats()
    {
        return $"Attack : {AttackValue}\r\nAttack speed : {AttackSpeed}"/* + (IsTwoHanded ? "\r\nTwo handed":"") /*+ (IsDualWieldable?"\r\nCan dual wield":"")*/ ;
    }
}
