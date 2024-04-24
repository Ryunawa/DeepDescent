using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public abstract class EquippableItem : Item
{
    public abstract (bool, EquippableItem) Equip(Inventory inventoryToEquipTo);
}
