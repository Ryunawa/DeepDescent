using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item List", menuName = "ScriptableObjects/Item/Create New ItemList")]
public class ItemList : ScriptableObject
{
    [SerializeField, Expandable] private List<Item> items;

    public Item FindItemFromID(int id)
    {
        return items.Find(x => x.ID == id);
    }

    public List<Item> Items => items;
}
