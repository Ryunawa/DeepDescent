using _2Scripts.Entities;
using _2Scripts.Entities.Player;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StatComponent : MonoBehaviour
{
    public StatsList StatsList;
    public Stats CharacterStatPage;

    private float _equippedArmour = 0.0f;
    public void SetStats(int index)
    {
        CharacterStatPage = StatsList.Stats[index];
    }

    public float CalcDamageReceived(float damage)
    {
        return damage * (100/(100 + (CharacterStatPage.BaseArmour +  _equippedArmour))) * CharacterStatPage.DamageReceivedModifier;
    }

    public float CalcDamageInflicted(float damage)
    {
        return damage * CharacterStatPage.DamageInflictedModifier;
    }

    public void UpdateArmourValue(Inventory inventory)
    {
        if (inventory != null) 
        {
            _equippedArmour = 0.0f;
            _equippedArmour += inventory.OffHandItem ? inventory.OffHandItem.AttackValue : 0;
            _equippedArmour += inventory.ChestArmor ? inventory.ChestArmor.ArmorValue : 0;
            _equippedArmour += inventory.FeetArmor ? inventory.FeetArmor.ArmorValue : 0;
            _equippedArmour += inventory.LegArmor ? inventory.LegArmor.ArmorValue : 0;
        }
    }

    [Button]
    public void InflictDamage()
    {
        GetComponent<HealthComponent>().TakeDamage(10.0f);
    }
}
