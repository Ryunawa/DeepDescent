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
    
    private Room[] _dungeon = new Room[]{};

    private static int _staticDungeonSize;
    // Start is called before the first frame update
    void Start()
    {
        _staticDungeonSize = dungeonSize;
        // Random.InitState((int)DateTime.Now.Ticks);

        _dungeon = new Room[_staticDungeonSize*_staticDungeonSize];
        
        Random.InitState(35896);

        for (int i = 0; i < _dungeon.Length; i++)
        {
             
        }
    }

    private void GetNeighbouringRooms(int roomIndex)
    {
        Dictionary<Directions, Room> rooms = new Dictionary<Directions, Room>()
        {
            { Directions.North , new Room()},
            { Directions.East , new Room()},
            { Directions.South , new Room()},
            { Directions.West , new Room()}
        };


        rooms[Directions.East] = _dungeon[roomIndex + 1];
        rooms[Directions.West] = _dungeon[roomIndex - 1];
        rooms[Directions.North] = roomIndex + _staticDungeonSize > _dungeon.Length ? _dungeon[(roomIndex + _staticDungeonSize)] : null;
        rooms[Directions.South] = roomIndex + _staticDungeonSize < 0 ? _dungeon[(roomIndex - _staticDungeonSize)] : null;
    }
    
    private void DoesNeighbouringRoomNeedDoor()
    {
        
    }
    
    
    
}

public enum Directions
{
    North,
    East,
    South,
    West
}
