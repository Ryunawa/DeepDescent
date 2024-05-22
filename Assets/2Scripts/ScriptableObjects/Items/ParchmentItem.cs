using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Parchment Item", menuName = "ScriptableObjects/Item/Create New Parchment Item")]
public class ParchmentItem : ConsumableItem
{
    [Header("Parchment Specific")]
    public GameObject SpellToSpawn;
    public float ParchmentCooldown;
    public override bool Use()
    {
        throw new System.NotImplementedException();
    }

    public override string GetStats()
    {
        return $"Cooldown : {ParchmentCooldown}";
    }
}
