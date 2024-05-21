using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmorType
{
    CHEST,
    PANTS,
    FEET,
    RING,
    NECKLACE
}

[CreateAssetMenu(fileName = "New Armor Item", menuName = "ScriptableObjects/Item/Create New Armor Item")]
public class ArmorItem : EquippableItem
{
    [Header("Armor Specific")]
    public ArmorType ArmorType;
    public int ArmorValue;
    //More maybe? idk seems ok like that.
    public override (bool, EquippableItem) Equip(Inventory inventoryToEquipTo)
    {
        if (!inventoryToEquipTo)
            return (false, null);
        EquippableItem oldItem;
        switch (ArmorType)
        {
            case ArmorType.CHEST:
                oldItem = inventoryToEquipTo.ChestArmor;
                inventoryToEquipTo.ChestArmor = this;
                break;
            case ArmorType.PANTS:
                oldItem = inventoryToEquipTo.LegArmor;
                inventoryToEquipTo.LegArmor = this;
                break;
            case ArmorType.FEET:
                oldItem = inventoryToEquipTo.FeetArmor;
                inventoryToEquipTo.FeetArmor = this;
                break;
            case ArmorType.RING:
                oldItem = inventoryToEquipTo.RingsItem[0];
                inventoryToEquipTo.RingsItem[0] = this;
                break;
            case ArmorType.NECKLACE:
                oldItem = inventoryToEquipTo.NecklaceItem;
                inventoryToEquipTo.NecklaceItem = this;
                break;
            default:
                return (false, null);
        }
        return (true, oldItem);
    }
}
