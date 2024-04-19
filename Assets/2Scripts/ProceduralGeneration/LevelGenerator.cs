using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.ProceduralGeneration;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelGenerator : Singleton<LevelGenerator>
{
    // Prefabs
    public GameObject roomOnePrefab;
    public GameObject roomTwoPrefab;
    public GameObject roomTwoOppositePrefab;
    public GameObject roomThreePrefab;
    public GameObject roomFourPrefab;

    [SerializeField] private int dungeonSize = 5;
    [SerializeField] private List<Room> rooms = new List<Room>();
    [SerializeField] private List<Room> roomsToHave = new List<Room>();
    
    private Room[] _dungeon = new Room[]{};

    private static int _staticDungeonSize;
    public static float _roomSize;
    // Start is called before the first frame update
    void Start()
    {
        _staticDungeonSize = dungeonSize;
        // Random.InitState((int)DateTime.Now.Ticks);

        _dungeon = new Room[_staticDungeonSize*_staticDungeonSize];
        
        Random.InitState(35896);


        //Generation n = 1 : center
        int centerIndex = (_staticDungeonSize /2) * (_staticDungeonSize +1);
        InstantiateRoom(RoomType.Four, GetPosition(centerIndex), Quaternion.identity);

        // TODO
        // for size of the dungeon
        // check if within bounds, if yes -> generate
        // Generation n+1
        CreateAdjacentRooms(centerIndex);
    }

    private Dictionary<Directions, Room> GetNeighbouringRooms(int roomIndex)
    {
        Dictionary<Directions, Room> rooms = new Dictionary<Directions, Room>()
    {
        { Directions.North , null},
        { Directions.East , null},
        { Directions.South , null},
        { Directions.West , null}
    };

        // east
        if (roomIndex + 1 < _dungeon.Length)
        {
            rooms[Directions.East] = _dungeon[roomIndex + 1];
        }

        // west
        if (roomIndex - 1 >= 0)
        {
            rooms[Directions.West] = _dungeon[roomIndex - 1];
        }

        // north
        if (roomIndex + _staticDungeonSize < _dungeon.Length)
        {
            rooms[Directions.North] = _dungeon[roomIndex + _staticDungeonSize];
        }

        // south
        if (roomIndex - _staticDungeonSize >= 0)
        {
            rooms[Directions.South] = _dungeon[roomIndex - _staticDungeonSize];
        }

        return rooms;
    }

    private bool DoesNeighbouringRoomNeedDoor(Directions direction, Dictionary<Directions, Room> neighbouringRooms)
    {
        Room neighbourRoom = neighbouringRooms[direction];

        if (neighbourRoom != null)
        {
            // get opposite direction
            Directions oppositeDirection = GetOppositeDirection(direction);

            // room has a door?
            bool hasDoorOpposite = neighbourRoom.HasDoor(oppositeDirection);

            // if has door : door, if not no door
            return hasDoorOpposite;
        }
        else
        {
            // is in array?
            bool isWithinBounds = IsWithinBounds(direction);

            if (isWithinBounds)
            {
                // room does not exist and is not out of bounds -> rand door
                return Random.Range(0, 2) == 1;
            }
            else
            {
                // room does not exist and is out of bounds -> no door
                return false;
            }
        }
    }

    // check if its still inside of the array
    private bool IsWithinBounds(Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return _staticDungeonSize >= 0;
            case Directions.East:
                return _staticDungeonSize <= (_dungeon.Length - 1);
            case Directions.South:
                return _staticDungeonSize <= (_dungeon.Length - 1);
            case Directions.West:
                return _staticDungeonSize >= 0;
            default:
                return false;
        }
    }

    public bool[] GetAllDoorsNeeded(int roomIndex)
    {
        bool[] doorNeeded = new bool[4];
        Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

        foreach (KeyValuePair<Directions, Room> kvp in neighbouringRooms)
        {
            Directions direction = kvp.Key;
            Room room = kvp.Value;

            bool needsDoor = DoesNeighbouringRoomNeedDoor(direction, neighbouringRooms);

            switch(direction)
            {
                case Directions.North :
                    doorNeeded[0] = needsDoor;
                    break;
                case Directions.East:
                    doorNeeded[1] = needsDoor;
                    break;
                case Directions.South:
                    doorNeeded[2] = needsDoor;
                    break;
                case Directions.West:
                    doorNeeded[3] = needsDoor;
                    break;
            }
        }
        return doorNeeded;
    }


    // create rooms in the four direction next to the given index
    public void CreateAdjacentRooms(int roomIndex)
    {
        Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

        foreach(KeyValuePair<Directions, Room> kvp in neighbouringRooms)
        {
            Directions direction = kvp.Key;
            Room room = kvp.Value;

            if (room == null)
            {
                Vector3 position = GetPositionNeighbour(roomIndex, direction);
                int neighbourIndex = GetIndexNeighbour(roomIndex, direction);
                bool[] doorNeeded = GetAllDoorsNeeded(neighbourIndex);
                RoomType roomType = ChooseRoomType(doorNeeded);

                Quaternion rotation = Quaternion.identity; // TODO adjust this later, maybe further in the code
                InstantiateRoom(roomType, position, rotation);
            }
        }
    }

    // select a room type to construct --> will depends on the door next to it
    private RoomType ChooseRoomType(bool[] doorsNeeded)
    {
        // how many doors needed?
        int neededDoorsCount = 0;
        foreach (bool doorNeeded in doorsNeeded)
        {
            if (doorNeeded)
            {
                neededDoorsCount++;
            }
        }

        // select the good prefab of doors
        switch(neededDoorsCount)
        {
            case 1:
                return RoomType.One;
            case 2:
                if (doorsNeeded[(int)Directions.North] && doorsNeeded[(int)Directions.South] || doorsNeeded[(int)Directions.East] && doorsNeeded[(int)Directions.West])
                {
                    return RoomType.TwoOpposite;
                }
                else
                {
                    return RoomType.Two;
                }
            case 3:
                return RoomType.Three;
            case 4:
                return RoomType.Four;
            default:
                Debug.LogError("Invalid door count : " + neededDoorsCount);
                return RoomType.One;
        }
    }

    // create a room
    public GameObject InstantiateRoom(RoomType roomType, Vector3 position, Quaternion rotation)
    {
        GameObject roomPrefab = null;

        switch (roomType)
        {
            case RoomType.One:
                roomPrefab = roomOnePrefab;
                break;
            case RoomType.Two:
                roomPrefab = roomTwoPrefab;
                break;
            case RoomType.TwoOpposite:
                roomPrefab = roomTwoOppositePrefab;
                break;
            case RoomType.Three:
                roomPrefab = roomThreePrefab;
                break;
            case RoomType.Four:
                roomPrefab = roomFourPrefab;
                break;
            default:
                Debug.LogError("Unknown room type: " + roomType);
                break;
        }

        // instantiation of the room
        if (roomPrefab != null)
        {
            GameObject roomInstance = Instantiate(roomPrefab, position, rotation);
            return roomInstance;
        }
        else
        {
            return null;
        }
    }

    // get position of the room at roomIndex
    private Vector3 GetPosition(int roomIndex)
    {
        int row = roomIndex / _staticDungeonSize;
        int col = roomIndex % _staticDungeonSize;

        float x = col * _roomSize;
        float z = row * _roomSize;

        return new Vector3(x, 0, z);
    }

    // get the position of the adjacent room in the given direction
    private Vector3 GetPositionNeighbour(int roomIndex, Directions direction)
    {
        Vector3 position = GetPosition(roomIndex);
        switch (direction)
        {
            case Directions.North:
                position.z += _roomSize;
                break;
            case Directions.South:
                position.z -= _roomSize;
                break;
            case Directions.East:
                position.x -= _roomSize;
                break;
            case Directions.West:
                position.x += _roomSize;
                break;
        }

        return position;
    }

    private int GetIndexNeighbour(int roomIndex, Directions direction)
    {
        int neighbourIndex = -1;

        switch (direction)
        {
            case Directions.North:
                neighbourIndex = roomIndex + _staticDungeonSize;
                break;
            case Directions.East:
                neighbourIndex = roomIndex + 1;
                break;
            case Directions.South:
                neighbourIndex = roomIndex - _staticDungeonSize;
                break;
            case Directions.West:
                neighbourIndex = roomIndex - 1;
                break;
        }

        if (neighbourIndex >= 0 && neighbourIndex < _dungeon.Length)
        {
            return neighbourIndex;
        }
        else
        {
            return -1;
        }
    }


    // get opposite cardinal direction
    private Directions GetOppositeDirection(Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return Directions.South;
            case Directions.East:
                return Directions.West;
            case Directions.South:
                return Directions.North;
            case Directions.West:
                return Directions.East;
            default:
                return Directions.North;
        }
    }
}

public enum Directions
{
    North,
    East,
    South,
    West
}
