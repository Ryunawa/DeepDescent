using _2Scripts.Entities.Player;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public GameObject shopUI;
    public PlayerBehaviour playerBehaviour;
    public Inventory inventory;
    public KeyCode openShopKey = KeyCode.E;
    public KeyCode closeShopKey = KeyCode.Escape;
    public KeyCode inventoryKey = KeyCode.Tab;
    private bool isShopOpen = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected. Press " + openShopKey.ToString() + " to open shop.");
            playerBehaviour = other.GetComponent<PlayerBehaviour>();
        }
    }

    void Update()
    {
        if (playerBehaviour != null && Input.GetKeyDown(openShopKey))
        {
            OpenShop();
        }
        else if (isShopOpen && Input.GetKeyDown(closeShopKey))
        {
            CloseShop();
        }
    }

    void OpenShop()
    {
        isShopOpen = true;
        shopUI.SetActive(true);
        playerBehaviour.canMove = false;
    }

    void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        playerBehaviour.canMove = true;
    }

    public void BuyItem(Item item)
    {
        if (playerBehaviour.gold >= item.SellValue)
        {
            bool isItemAdded = playerBehaviour.inventory.AddToInventory(item.ID, 1);
            if (isItemAdded)
            {
                playerBehaviour.gold -= item.SellValue;
                Debug.Log(item.Name + " bought for " + item.SellValue + " gold.");
            }
        }
        else
        {
            Debug.Log("Not enough gold.");
        }
    }

    public void SellItem(Item item)
    {
        bool isItemRemoved = playerBehaviour.inventory.RemoveFromInventory(item.ID, 1);
        if (isItemRemoved)
        {
            int sellPrice = Mathf.FloorToInt(item.SellValue * 0.9f);
            playerBehaviour.gold += sellPrice;
            Debug.Log(item.Name + " sold for " + sellPrice + " gold.");
        }
    }
}
