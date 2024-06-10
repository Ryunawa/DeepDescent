using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StatComponent : MonoBehaviour
{
    public float BaseArmour = 0.0f;
    public float EquippedArmour = 0.0f;

    public float DamageInflictedModifier = 1.0f;
    public float DamageReceivedModifier = 1.0f;
    public float CalcDamageReceived(float damage)
    {
        return damage * (100/(100 + (BaseArmour +  EquippedArmour))) * DamageReceivedModifier;
    }

    public float CalcDamageInflicted(float damage)
    {
        return damage * DamageInflictedModifier;
    }

    public void UpdateArmourValue(Inventory inventory)
    {
        if (inventory != null) 
        {
            EquippedArmour = 0.0f;
            EquippedArmour += inventory.OffHandItem ? inventory.OffHandItem.AttackValue : 0;
            EquippedArmour += inventory.ChestArmor ? inventory.ChestArmor.ArmorValue : 0;
            EquippedArmour += inventory.FeetArmor ? inventory.FeetArmor.ArmorValue : 0;
            EquippedArmour += inventory.LegArmor ? inventory.LegArmor.ArmorValue : 0;
            
        }
    }
}
