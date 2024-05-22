using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = UnityEngine.Windows.Input;

public class ItemDetailUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDesc;
    [SerializeField] private TextMeshProUGUI itemRarity;
    [SerializeField] private TextMeshProUGUI itemStats;


    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector2 pos = Mouse.current.position.value;

        float pivotX = pos.x / Screen.width;
        float pivotY = pos.y / Screen.height;

        _rectTransform.pivot = new Vector2(pivotX, pivotY);
        
        transform.position = pos;
    }


    public void Setup(Item item)
    {
        itemName.text = item.Name;
        itemDesc.text = item.Description;

        itemRarity.text = item.Rarity.ToString();
        itemRarity.color = InventoryUIManager.Colors[item.Rarity];

        if (item.GetType().BaseType == typeof(EquippableItem) || item.GetType().BaseType == typeof(ConsumableItem))
        {
            itemStats.text = item.GetType().BaseType == typeof(EquippableItem) ? ((EquippableItem)item).GetStats() : ((ConsumableItem)item).GetStats();
        }
        else
        {
            itemStats.text = "";
        }
        
    }

    public void ToggleUI(bool state)
    {
        gameObject.SetActive(state);
    }

}
