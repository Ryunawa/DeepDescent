using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellCasterComponent : NetworkBehaviour
{

    GameObject _spellToCast;
    public void CastSpell(GameObject Prefab)
    {
        _spellToCast = Prefab;
        SpawnSpellRpc();
    }

    [Rpc(SendTo.Server)]
    public void SpawnSpellRpc()
    {
        NetworkObject o = Instantiate(_spellToCast.GetComponent<NetworkObject>(), transform.position + new Vector3(0.0f, 1.0f,0.0f), Quaternion.identity);
        o.Spawn();
        o.GetComponent<Projectile>().projectileDirection = gameObject.transform.forward.normalized;
    }
}
