using Unity.Netcode;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class SpawnerManager : NetworkSingleton<SpawnerManager>
    {
        
        [Rpc(SendTo.Server)]
        public void SpawnInventoryItemsRpc(int id)
        {
            NetworkObject o = Instantiate(ItemManager.instance.GetItemNetworkObject(id), transform.position, Quaternion.identity);
            o.Spawn();
        }
    }
}