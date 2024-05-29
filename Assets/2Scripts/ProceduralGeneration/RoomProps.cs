using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProps : MonoBehaviour
{

    [SerializeField] private List<GameObject> spawnPoints = new List<GameObject>();


    public List<GameObject> SpawnPoints => spawnPoints;
}
