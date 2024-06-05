using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using Unity.Netcode;
using UnityEngine;

public class Object : NetworkBehaviour, IInteractable
{
    public void Interact(PlayerBehaviour playerBehaviour)
    {
        //Pickup Object
        bool isItemAdded = playerBehaviour.inventory.AddToInventory(ItemDetails.ID, amount);
        if(isItemAdded) DespawnNetworkObjectRpc();
    }
    
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void DespawnNetworkObjectRpc()
    {
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        
        networkObject.Despawn();
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }
    public Item ItemDetails;
    public int amount;
    public GameObject GOText;
}
