using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public GameObject shopUIPrefab;
    private Dictionary<PlayerBehaviour, GameObject> activeShopUIs = new Dictionary<PlayerBehaviour, GameObject>();

    public KeyCode openShopKey = KeyCode.E;
    public KeyCode closeShopKey = KeyCode.Escape;
    private bool isNearShop = false;
    private PlayerBehaviour currentPlayer;

    [SerializeField] private List<ItemUI> WeaponUI;
    [SerializeField] private List<ItemUI> ArmorUI;
    [SerializeField] private List<ItemUI> PotionUI;
    [SerializeField] private List<ItemUI> ParchmentUI;

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        ItemManager itemManager = ItemManager.instance;

        // Fill weapon UI
        SetupItemUI(itemManager.weaponList.Items, WeaponUI);

        // Fill armor UI
        SetupItemUI(itemManager.armorList.Items, ArmorUI);

        // Fill potion UI
        SetupItemUI(itemManager.potionList.Items, PotionUI);

        // Fill parchment UI
        SetupItemUI(itemManager.parchmentList.Items, ParchmentUI);
    }

    private void SetupItemUI(List<Item> items, List<ItemUI> uiList)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < items.Count)
            {
                uiList[i].Setup(items[i].ID, 1);
            }
            else
            {
                uiList[i].Clear();
            }
        }
    }

    void Update()
    {
        if (!isNearShop || currentPlayer == null) return;

        if (Input.GetKeyDown(openShopKey))
        {
            OpenShop(currentPlayer);
        }
        else if (Input.GetKeyDown(closeShopKey))
        {
            CloseShop(currentPlayer);
        }
    }

    public void OpenShop(PlayerBehaviour player)
    {
        Inventory inventory = InventoryUIManager.instance.Inventory;

        if (activeShopUIs.ContainsKey(player) || !inventory) return;

        inventory.isInShop = true;
        GameObject shopUI = Instantiate(shopUIPrefab);
        shopUI.SetActive(true);
        activeShopUIs[player] = shopUI;
    }

    public void CloseShop(PlayerBehaviour player)
    {
        Inventory inventory = InventoryUIManager.instance.Inventory;

        if (activeShopUIs.ContainsKey(player) && inventory)
        {
            inventory.isInShop = false;
            Destroy(activeShopUIs[player]);
            activeShopUIs.Remove(player);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shop") && currentPlayer == null)
        {
            isNearShop = true;
            currentPlayer = other.GetComponent<PlayerBehaviour>();
            Debug.Log("Press " + openShopKey.ToString() + " to open shop.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shop") && currentPlayer != null)
        {
            isNearShop = false;
            currentPlayer = null;
        }
    }
}
