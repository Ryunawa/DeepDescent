using _2Scripts.Entities.Player;
using Unity.Netcode;
using UnityEngine;

public class Object : NetworkBehaviour, IInteractable
{
    public Item ItemDetails;
    public int amount;
    public GameObject GOText;
    private PlayerBehaviour _playerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = other.GetComponent<PlayerBehaviour>();
            GOText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_playerInRange != null)
            {
                GOText.SetActive(false);
                _playerInRange = null;
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

        networkObject.Despawn();
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }
}
