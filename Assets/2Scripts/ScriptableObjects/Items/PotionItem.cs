using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PotionType
{

}

[CreateAssetMenu(fileName = "New Potion Item", menuName = "ScriptableObjects/Item/Create New Potion Item")]
public class PotionItem : ConsumableItem
{
    [Header("Potion Specific")]
    public PotionType PotionType;
    public float PotionValue;
    public float PotionCooldown;
    public override bool Use()
    {
        //TODO
        throw new System.NotImplementedException();
    }

    public override string GetStats()
    {
        string stat = "";
        switch (PotionType)
        {
            default:
                stat += "Heal :";
                break;
        }
        
        return $"{stat} {PotionValue}\r\nCooldown : {PotionCooldown}";
    }
}
