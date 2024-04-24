using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item/Create New Item(Junk)")]
public class Item : ScriptableObject
{
    [Header("Item Global")]
    public int ID;
    public string Name;
    public string Description;
    public int SellValue;
    public bool QuickUse;
    public bool Stackable;
    public Sprite InventoryIcon;
    public GameObject ObjectPrefab;
}
