using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Object : NetworkBehaviour, IInteractable
{
    
    public void Interact(PlayerBehaviour playerBehaviour)
    {
        //Pickup Object
        playerBehaviour.inventory.AddToInventory(ItemDetails.ID, amount);
        MultiManager.instance.DespawnNetworkObjectServerRPC(GetComponent<NetworkObject>());
    }
    public Item ItemDetails;
    public int amount;
}
