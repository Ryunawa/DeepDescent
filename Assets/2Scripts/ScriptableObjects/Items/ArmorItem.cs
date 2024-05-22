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
    public override (bool, List<EquippableItem>) Equip(Inventory inventoryToEquipTo, bool EquipToOffHand = false)
    {
        if (!inventoryToEquipTo)
            return (false, null);
        List<EquippableItem> oldItems = new List<EquippableItem>();
        switch (ArmorType)
        {
            case ArmorType.CHEST:
                oldItems.Add(inventoryToEquipTo.ChestArmor);
                inventoryToEquipTo.LegArmor = this;
                break;
            case ArmorType.PANTS:
                oldItems.Add(inventoryToEquipTo.LegArmor);
                inventoryToEquipTo.LegArmor = this;
                break;
            case ArmorType.FEET:
                oldItems.Add(inventoryToEquipTo.FeetArmor);
                inventoryToEquipTo.FeetArmor = this;
                break;
            case ArmorType.RING:
                if (EquipToOffHand)
                {
                    oldItems.Add(inventoryToEquipTo.RingsItem[1]);
                    inventoryToEquipTo.RingsItem[1] = this;
                }
                else
                {
                    oldItems.Add(inventoryToEquipTo.RingsItem[0]);
                    inventoryToEquipTo.RingsItem[0] = this;
                }
                break;
            case ArmorType.NECKLACE:
                oldItems.Add(inventoryToEquipTo.NecklaceItem);
                inventoryToEquipTo.NecklaceItem = this;
                break;
            default:
                return (false, null);
        }
        return (true, oldItems);
    }

    public override string GetStats()
    {
        //TODO
        return "NEED TO DO";
    }
}
