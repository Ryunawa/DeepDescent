using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Parchment Item", menuName = "ScriptableObjects/Item/Create New Parchment Item")]
public class ParchmentItem : ConsumableItem
{
    [Header("Parchment Specific")]
    public GameObject SpellToSpawn;
    public float ParchmentCooldown;
    public override bool Use(GameObject GameObjectOwner)
    {
        Debug.Log("Trying to use parchment");
        if (GameObjectOwner)
        {
            if (GameObjectOwner.TryGetComponent(out SpellCasterComponent spellcasterComp))
            {
                spellcasterComp.SpawnSpellRpc(ID, Vector3.zero);
                return true;
            }
        }
        return false;
    }

    public override string GetStats()
    {
        return $"Cooldown : {ParchmentCooldown}";
    }
}
