using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    private Dictionary<PlayerBehaviour, GameObject> activeShopUIs = new Dictionary<PlayerBehaviour, GameObject>();

    public KeyCode openShopKey = KeyCode.E;
    public KeyCode closeShopKey = KeyCode.Escape;
    private bool isNearShop = false;
    private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();

    [SerializeField] private GameObject itemUIPrefab; // Prefab for ItemUI

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        ItemManager itemManager = ItemManager.instance;

        if (itemManager == null)
        {
            Debug.LogError("ItemManager instance is not found.");
            return;
        }

        if (itemUIPrefab == null)
        {
            Debug.LogError("ItemUIPrefab is not assigned.");
            return;
        }

        // Fill weapon UI
        CreateAndSetupItemUI(itemManager.weaponList.Items, InventoryUIManager.instance.weaponUIParent);

        // Fill armor UI
        CreateAndSetupItemUI(itemManager.armorList.Items, InventoryUIManager.instance.armorUIParent);

        // Fill potion UI
        CreateAndSetupItemUI(itemManager.potionList.Items, InventoryUIManager.instance.potionUIParent);

        // Fill parchment UI
        CreateAndSetupItemUI(itemManager.parchmentList.Items, InventoryUIManager.instance.parchmentUIParent);
    }

    private void CreateAndSetupItemUI(List<Item> items, Transform parent)
    {
        if (items == null)
        {
            Debug.LogError("Items list is null.");
            return;
        }

        foreach (var item in items)
        {
            GameObject itemUIGameObject = Instantiate(itemUIPrefab, parent);
            ItemUI itemUI = itemUIGameObject.GetComponent<ItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(item.ID, 1);
                itemUI.IsShop = true;
                itemUI.IsInventory = false;
            }
            else
            {
                Debug.LogError("ItemUI component is not found in the instantiated prefab.");
            }
        }
    }

    void Update()
    {
        if (!isNearShop) return;

        foreach (var player in nearbyPlayers)
        {
            if (Input.GetKeyDown(openShopKey))
            {
                OpenShop(player);
            }
            else if (Input.GetKeyDown(closeShopKey) || Input.GetKeyDown(openShopKey))
            {
                CloseShop(player);
            }
        }
    }

    public void OpenShop(PlayerBehaviour player)
    {
        Inventory inventory = InventoryUIManager.instance.Inventory;
        GameObject inventoryUI = InventoryUIManager.instance.inventoryUI;
        GameObject shopUI = InventoryUIManager.instance.shopUI;

        if (activeShopUIs.ContainsKey(player) || !inventory || inventoryUI.activeSelf == true) return;

        InventoryUIManager.instance.DrawInventoryShop();
        inventory.isInShop = true;
        shopUI.SetActive(true);
        activeShopUIs[player] = shopUI;
    }

    public void CloseShop(PlayerBehaviour player)
    {
        Inventory inventory = InventoryUIManager.instance.Inventory;

        if (activeShopUIs.ContainsKey(player) && inventory)
        {
            inventory.isInShop = false;
            InventoryUIManager.instance.shopUI.SetActive(false);
            activeShopUIs.Remove(player);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour playerInRange = other.GetComponentInChildren<PlayerBehaviour>();
            if (playerInRange != null)
            {
                nearbyPlayers.Add(playerInRange); // add player to the collection
                isNearShop = true;
                Debug.Log("Press " + openShopKey.ToString() + " to open shop.");
            }
            else
            {
                Debug.LogError("PlayerBehaviour component not found in children of the collider.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour playerInRange = other.GetComponentInChildren<PlayerBehaviour>();
            if (playerInRange != null && nearbyPlayers.Contains(playerInRange))
            {
                nearbyPlayers.Remove(playerInRange);
                CloseShop(playerInRange);
                if (nearbyPlayers.Count == 0)
                {
                    isNearShop = false;
                }
            }
        }
    }
}
