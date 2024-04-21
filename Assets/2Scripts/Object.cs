using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour, IInteractable
{
    public void Interact(PlayerBehaviour playerBehaviour)
    {
        //Pickup Object
        playerBehaviour.inventory.AddToInventory(ItemDetails.ID, amount);
        Destroy(gameObject);
    }
    public Item ItemDetails;
    public int amount;
}
