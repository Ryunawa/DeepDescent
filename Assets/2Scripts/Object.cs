using System.Collections.Generic;
using _2Scripts.Entities.Player;
using Unity.Netcode;
using UnityEngine;

namespace _2Scripts
{
    public class Object : NetworkBehaviour, IInteractable
    {
        public Item ItemDetails;
        public int amount;
        public GameObject GOText;
        private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
                if (playerInRange != null)
                {
                    nearbyPlayers.Add(playerInRange); // remove player from the collection
                    GOText.SetActive(true);
                    GetComponent<Outline>().enabled = true;
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
                    nearbyPlayers.Remove(playerInRange); // add player to the collection
                    if (nearbyPlayers.Count == 0)
                    {
                        GOText.SetActive(false);
                        GetComponent<Outline>().enabled = false;
                    }
                }
            }
        }

        private void Update()
        {
            foreach (var player in nearbyPlayers)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact(player);
                    break;
                }
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
}
