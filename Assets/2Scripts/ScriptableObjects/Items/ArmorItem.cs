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
    public override bool Equip()
    {
        //TODO
        throw new System.NotImplementedException();
    }
}
