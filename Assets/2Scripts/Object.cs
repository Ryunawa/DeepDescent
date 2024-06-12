using _2Scripts.Entities.Player;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Object : NetworkBehaviour, IInteractable
{
    public Item ItemDetails;
    public int amount;
    public GameObject GOText;
    private PlayerBehaviour _playerInRange;
    private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
            if (playerInRange != null)
            {
                nearbyPlayers.Add(playerInRange); // add player to the collection
                GOText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
            if (playerInRange != null && nearbyPlayers.Contains(playerInRange))
            {
                nearbyPlayers.Remove(playerInRange); // remove player of the collection
                if (nearbyPlayers.Count == 0)
                {
                    GOText.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        if (_playerInRange != null && Input.GetKeyDown(KeyCode.E))
        {
            Interact(_playerInRange);
        }
    }

    public void Interact(PlayerBehaviour playerBehaviour)
    {
        // Pickup Object
        bool isItemAdded = playerBehaviour.inventory.AddToInventory(ItemDetails.ID, amount);
        if (isItemAdded) DespawnNetworkObjectRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void DespawnNetworkObjectRpc()
    {
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

        networkObject.Despawn(true);
    }
}
