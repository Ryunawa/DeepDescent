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
    public void SpawnSpellRpc(int id, Vector3 pos, Quaternion rotation ,bool isFromStaff = false, bool isFromCrossbow = false)
    {
        // play sound
        if (isFromCrossbow) GameManager.GetManager<AudioManager>().PlaySfx("ArrowWhoosh", this, 1, 5);
        else  GameManager.GetManager<AudioManager>().PlaySfx("FireBallWhoosh", this, 1, 5);

        Debug.Log("SpellSpawned");
        GameObject spell;
        if (isFromStaff)
        {
            spell = ((WeaponItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
        }
        else if(isFromCrossbow)
        {
            spell = ((WeaponItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
        }
        else 
        {
            spell = ((ParchmentItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn;
            pos = transform.position + Vector3.up;
        }
        
        
        NetworkObject o = Instantiate(spell.GetComponent<NetworkObject>(), pos, rotation);

        if (isFromStaff || isFromCrossbow)
            o.GetComponent<Projectile>().projectileDamage =
                GameManager.playerBehaviour.inventory.MainHandItem.AttackValue;
        
        o.GetComponent<Projectile>().projectileDirection = gameObject.transform.forward.normalized;
        o.Spawn();
    }
}
