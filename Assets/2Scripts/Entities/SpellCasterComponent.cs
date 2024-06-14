using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellCasterComponent : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void SpawnSpellRpc(int id, string name)
    {
        NetworkObject o = Instantiate(((ParchmentItem)GameManager.GetManager<ItemManager>().GetItem(id)).SpellToSpawn.GetComponent<NetworkObject>(), transform.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
        Debug.Log($"Spawned from object {GameManager.GetManager<ItemManager>().GetItem(id).Name} by {name}");
        o.GetComponent<Projectile>().projectileDirection = gameObject.transform.forward.normalized;
        o.Spawn();
        //o.ChangeOwnership(NetworkManager.ServerClientId);
    }
}
