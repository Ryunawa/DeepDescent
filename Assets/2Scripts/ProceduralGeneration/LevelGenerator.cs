using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.ProceduralGeneration;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelGenerator : Singleton<LevelGenerator>
{
    [SerializeField] private int dungeonSize = 5;
    [SerializeField] private List<Room> rooms = new List<Room>();
    [SerializeField] private List<Room> roomsToHave = new List<Room>();
    
    private List<List<Room>> _dungeon = new List<List<Room>>();

    // Start is called before the first frame update
    void Start()
    {
        // Random.InitState((int)DateTime.Now.Ticks);
        Random.InitState(35896);

        for (int i = 0; i < _dungeon.Count; i++)
        {
            //in each cell we fill it with all the possible rooms, to run the wave function collapse
            _dungeon[i] = rooms;
        }
        
        //TODO : have the wave function collapse run, and give me a grid of rooms
        
        //TODO : have build that grid, rooms will NOT be different sizes
        
    }
}
