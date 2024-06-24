using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using _2Scripts.Manager;
using NaughtyAttributes;
using UnityEngine;

public class AlignGameObjects : MonoBehaviour
{
    [SerializeField] private float space = 1;
    [SerializeField] private float nbColumns = 5;
    
    [SerializeField] private ItemList ItemList;

    [SerializeField] private List<GameObject> gameObjectsList;


    [Button]
    public void Align()
    {
        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);
        
        if (ItemList)
        {
            for (var index = 0; index < ItemList.Items.Count; index++)
            {
                var item = ItemList.Items[index];
                
                Vector3 position = new Vector3(index % nbColumns * space,0, (int)(index / nbColumns) * space);
                
                Instantiate(item.ObjectPrefab, transform.position + position, Quaternion.identity, transform);
            }
        }
        else
        {
            for (var index = 0; index < gameObjectsList.Count; index++)
            {
                Vector3 position = new Vector3(index * space % nbColumns,0, Mathf.RoundToInt(index / nbColumns) * space);
                
                Instantiate(gameObjectsList[index], transform.position + position, Quaternion.identity, transform);
            }
        }
        
        
    }

    [Button]
    public void DeleteChildren()
    {
        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }
    
    [Button]
    public void ShowDuplicateIDItem()
    {
        int highestValue = ItemList.Items.Max(i => i.ID);

        for (int i = 0; i < highestValue; i++)
        {
            List<Item> items = ItemList.Items.Where(x => x.ID == i).ToList();

            if (items.Count > 1 )
            {
                foreach (Item item in items)
                {
                    Debug.Log($"Duplicate ID : {item.ID}, Item name : {item.Name}, position in List {ItemList.Items.IndexOf(item)}");
                }
            }
            
        }
    }

    [Button]
    public void OrderByIDItems()
    {
        ItemList.Items.OrderBy(x => x.ID);
    }
}
