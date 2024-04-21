using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item List", menuName = "ScriptableObjects/Item/Create New ItemList")]
public class ItemList : ScriptableObject
{
    public List<Item> items;
}
