using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCharacter : MonoBehaviour
{

    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        int selectedCharacter = PlayerPrefs.GetInt("selectedCharacter");
        GameObject prefab = characterPrefabs[selectedCharacter];
        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }
}
