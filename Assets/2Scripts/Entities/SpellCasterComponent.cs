using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellCasterComponent : NetworkBehaviour
{

    GameObject _spellToCast;
    private void CastSpell(GameObject Prefab)
    {
        _spellToCast = Prefab;
        SpawnSpell();
    }

    [Rpc(SendTo.Server)]
    public void SpawnSpell()
    {
        NetworkObject o = Instantiate(_spellToCast.GetComponent<NetworkObject>(), transform.position, Quaternion.identity);
        o.Spawn();
    }
}
