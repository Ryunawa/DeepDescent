using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using System;
using System.Collections.Generic;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using UnityEngine;

public class ShopSystem : GameManagerSync<ShopSystem>
{
    private Dictionary<PlayerBehaviour, GameObject> activeShopUIs = new Dictionary<PlayerBehaviour, GameObject>();

    public KeyCode openShopKey = KeyCode.E;
    public KeyCode closeShopKey = KeyCode.Escape;
    private bool isNearShop = false;
    private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();

    [SerializeField] private GameObject itemUIPrefab; // Prefab for ItemUI

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        if (gameState != GameState.InLevel) return;
        
        InitializeShop();
        
        
    }

    private void InitializeShop()
    {
        ItemManager itemManager = GameManager.GetManager<ItemManager>();

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
        CreateAndSetupItemUI(itemManager.weaponList.Items, GameManager.GetManager<InventoryUIManager>().weaponUIParent);

        // Fill armor UI
        CreateAndSetupItemUI(itemManager.armorList.Items, GameManager.GetManager<InventoryUIManager>().armorUIParent);

        // Fill potion UI
        CreateAndSetupItemUI(itemManager.potionList.Items, GameManager.GetManager<InventoryUIManager>().potionUIParent);

        // Fill parchment UI
        CreateAndSetupItemUI(itemManager.parchmentList.Items, GameManager.GetManager<InventoryUIManager>().parchmentUIParent);
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
        Inventory inventory = GameManager.GetManager<InventoryUIManager>().Inventory;
        GameObject inventoryUI = GameManager.GetManager<InventoryUIManager>().inventoryUI;
        GameObject shopUI = GameManager.GetManager<InventoryUIManager>().shopUI;

        if (activeShopUIs.ContainsKey(player) || !inventory || inventoryUI.activeSelf == true) return;

        GameManager.GetManager<InventoryUIManager>().DrawInventoryShop();
        inventory.isInShop = true;
        shopUI.SetActive(true);
        activeShopUIs[player] = shopUI;
    }

    public void CloseShop(PlayerBehaviour player)
    {
        Inventory inventory = GameManager.GetManager<InventoryUIManager>().Inventory;

        if (activeShopUIs.ContainsKey(player) && inventory)
        {
            inventory.isInShop = false;
            GameManager.GetManager<InventoryUIManager>().shopUI.SetActive(false);
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
