using Unity.Netcode;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class SpawnerManager : NetworkSingleton<SpawnerManager>
    {
        
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void SpawnNetworkObjectRpc(NetworkObject objectToSpawn, Vector3 position, Quaternion rotation)
        {
            objectToSpawn.InstantiateAndSpawn(NetworkManager.Singleton, NetworkManager.Singleton.LocalClientId,
                position: position, rotation: rotation);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void DespawnNetworkObjectRpc(NetworkObject objectToDespawn)
        {
            objectToDespawn.Despawn();
            if (objectToDespawn.gameObject)
            {
                Destroy(objectToDespawn.gameObject);
            }
        }
        
        
    }
}