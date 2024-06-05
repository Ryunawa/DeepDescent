using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StatComponent : MonoBehaviour
{
    public float BaseArmour = 0.0f;
    public float EquippedArmour = 0.0f;

    public float DamageModifier = 1.0f;
    public float CalcMitigatedDamageReceiving(float damage)
    {
        return damage * (100/(100 + (BaseArmour +  EquippedArmour)));
    }

    public float CalcDamageInflicting(float damage)
    {
        return damage * DamageModifier;
    }

    public void UpdateArmourValue(Inventory inventory)
    {
        if (inventory != null) 
        {
            EquippedArmour = inventory.OffHandItem.AttackValue + inventory.ChestArmor.ArmorValue + inventory.FeetArmor.ArmorValue + inventory.LegArmor.ArmorValue;
        }
    }
}
