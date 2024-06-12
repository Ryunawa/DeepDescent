using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProps : MonoBehaviour
{

    [SerializeField] private List<GameObject> spawnPoints = new List<GameObject>();

    [SerializeField] private List<ItemSpawnPoint> itemSpawnPoints = new List<ItemSpawnPoint>();


    public List<GameObject> SpawnPoints => spawnPoints;
    public List<ItemSpawnPoint> ItemSpawnPoints => itemSpawnPoints;
}
