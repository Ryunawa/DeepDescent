using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumableItem : Item
{
    public abstract bool Use(GameObject GameObjectOwner);
    public abstract string GetStats();
    
}
