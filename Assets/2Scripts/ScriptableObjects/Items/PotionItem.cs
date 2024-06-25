using _2Scripts.Entities;
using _2Scripts.Entities.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PotionType
{
    HEALTH,
}

[CreateAssetMenu(fileName = "New Potion Item", menuName = "ScriptableObjects/Item/Create New Potion Item")]
public class PotionItem : ConsumableItem
{
    [Header("Potion Specific")]
    public PotionType PotionType;
    public float PotionValue;
    public float PotionCooldown;
    public override bool Use(GameObject GameObjectOwner)
    {
        bool returnValue = false;
        switch (PotionType)
        {
            case PotionType.HEALTH:
                if (GameObjectOwner.TryGetComponent(out HealthComponent healthComponent))
                {
                    healthComponent.Heal(PotionValue);
                    returnValue = true;
                }
                break;
            default:
                break;
        }
        return returnValue;
    }

    public override string GetStats()
    {
        string stat = "";
        switch (PotionType)
        {
            case PotionType.HEALTH:
                stat += "Heal :";
                break;
            default:
                break;
        }
        
        return $"{stat} {PotionValue}\r\nCooldown : {PotionCooldown}";
    }
}
