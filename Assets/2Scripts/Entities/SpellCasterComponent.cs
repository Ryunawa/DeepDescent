using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SpellCasterComponent : NetworkBehaviour
{
    [SerializeField] private List<GameManager> projectile = new ();

    public Vector3 positionToCastFrom;
    
    [Rpc(SendTo.Server)]
    public void SpawnSpellRpc(int id, bool isFromStaff = false, bool isFromCrossbow = false)
    {
        Debug.Log("SpellSpawned");
        Vector3 pos;
        GameObject spell;
        if (isFromStaff)
        {
            spell = ((WeaponItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
            pos = GameManager.playerBehaviour.HandPosition.position;

        }
        else if(isFromCrossbow)
        {
            spell = ((WeaponItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
            pos = positionToCastFrom;
        }
        else 
        {
            spell = ((ParchmentItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
            pos = transform.position + Vector3.up;
        }
        
        
        NetworkObject o = Instantiate(spell.GetComponent<NetworkObject>(), pos, Quaternion.identity);

        if (isFromStaff || isFromCrossbow)
            o.GetComponent<Projectile>().projectileDamage =
                GameManager.playerBehaviour.inventory.MainHandItem.AttackValue;
        
        o.GetComponent<Projectile>().projectileDirection = gameObject.transform.forward.normalized;
        o.Spawn();
    }
}
