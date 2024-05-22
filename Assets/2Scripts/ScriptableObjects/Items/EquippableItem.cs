using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public abstract class EquippableItem : Item
{

    public abstract string GetStats();
    public abstract (bool, List<EquippableItem>) Equip(Inventory inventoryToEquipTo, bool EquipToOffHand = false);
}
