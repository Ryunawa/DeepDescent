using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace _2Scripts.ProceduralGeneration
{
    public class LevelGenerator : Singleton<LevelGenerator>
    {
        // Prefabs
        [Header("Rooms")]
        public GameObject roomZeroPrefab;
        public GameObject roomOnePrefab;
        public GameObject roomTwoPrefab;
        public GameObject roomTwoOppositePrefab;
        public GameObject roomThreePrefab;
        public GameObject roomFourPrefab;

        public Room[] dungeon = new Room[]{};
    
        [Header("Props")]
        [SerializeField] private GameObject[] _props;

        [Header("Setting")]
        [SerializeField] private float _roomSize;
        [SerializeField] private int dungeonSize = 5;
        [SerializeField] private bool IsOneRoomType;
    
        public UnityEvent dungeonGeneratedEvent = new UnityEvent();

        private static int _staticDungeonSize;
        private int _roomNumber = 1;

        private bool _lateUpdateOnlyOnce = false;
    
        // folder
        private GameObject generatedDungeonParent;
        private GameObject roomsParent;
        private GameObject doorsParent;
        private GameObject propsParent;

        // Start is called before the first frame update
        async void Start()
        {
            // create folders
            generatedDungeonParent = new GameObject("GeneratedDungeon");
            roomsParent = new GameObject("Rooms");
            doorsParent = new GameObject("Doors");
            propsParent = new GameObject("Props");

            roomsParent.transform.SetParent(generatedDungeonParent.transform);
            doorsParent.transform.SetParent(generatedDungeonParent.transform);
            propsParent.transform.SetParent(generatedDungeonParent.transform);


            _staticDungeonSize = dungeonSize;
            // Random.InitState((int)DateTime.Now.Ticks);

            dungeon = new Room[_staticDungeonSize*_staticDungeonSize];
        
            Random.InitState(35896);

            //Generation n = 1 : center
            int centerIndex = (_staticDungeonSize /2) * (_staticDungeonSize +1);
            Room startRoom = InstantiateRoom(RoomType.Four, GetPosition(centerIndex), Quaternion.identity).GetComponent<Room>();
            startRoom.transform.SetParent(roomsParent.transform);
            startRoom.SizeRoom = _roomSize;
            startRoom.createDoors();
            startRoom.Generation = 1;

            // create props
            GameObject instantiatedProps = InstantiateProps(RoomType.Four, GetPosition(centerIndex));

            // place it in their folder
            if (instantiatedProps)
            {
                instantiatedProps.transform.SetParent(propsParent.transform);
                startRoom.RoomProps = instantiatedProps.GetComponentInChildren<RoomProps>();
            }


            dungeon[centerIndex] = startRoom;

            _roomNumber = 0;
        
            await DoGen(1);
            Debug.Log("GenerationFinished");
            DynamicNavMesh.UpdateNavMesh();

            _lateUpdateOnlyOnce = true;
        
            MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().TeleportPlayer(GetPosition(centerIndex) + Vector3.up * 5);
        }

        private void LateUpdate()
        {
            if (_lateUpdateOnlyOnce)
            {
                _lateUpdateOnlyOnce = !_lateUpdateOnlyOnce;
                DoWhenGenEnd();
            }
        }
    
        private void DoWhenGenEnd()
        {
            dungeonGeneratedEvent.Invoke();
                
            SceneManager.instance.DeactivateLoadingScreen();
        }
    

        private async Task DoGen(int startDepth)
        {
            if (startDepth == _staticDungeonSize)
            {
                return;
            }
        
            Room[] roomsToGenNeighbours = GetRoomFromGeneration(startDepth);
        
            startDepth++;
        
        
            foreach (Room room in roomsToGenNeighbours)
            { 
                _roomNumber++;
                // Skip if the room is already generated in the current depth
                if (room != null && room.Generation >= startDepth)
                {
                    continue;
                }
            
                await CreateAdjacentRooms(GetIndexOfRoom(room), startDepth, _roomNumber);
            }
        
            await DoGen(startDepth);
        
        
        }

        public int GetIndexOfRoom(Room room)
        {
            for (int i = 0; i < dungeon.Length; i++)
            {
                if (dungeon[i] == room)
                {
                    return i;
                }
            }

            return -1;
        }

        private Room[] GetRoomFromGeneration(int genNumber)
        {
            List<Room> roomsOfgivenGen = new List<Room>(); 

            foreach (Room Room in dungeon)
            {
                if (Room != null && Room.Generation == genNumber)
                {
                    roomsOfgivenGen.Add(Room);
                }
            }

            return roomsOfgivenGen.ToArray();
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
            if ((roomIndex + 1) % dungeonSize != 0)
            {
                rooms[Directions.East] = dungeon[roomIndex + 1];
            }

            // west
            if (roomIndex % dungeonSize != 0)
            {
                rooms[Directions.West] = dungeon[roomIndex - 1];
            }

            // north
            if (roomIndex + _staticDungeonSize < dungeon.Length)
            {
                rooms[Directions.North] = dungeon[roomIndex + _staticDungeonSize];
            }

            // south
            if (roomIndex - _staticDungeonSize >= 0)
            {
                rooms[Directions.South] = dungeon[roomIndex - _staticDungeonSize];
            }

            return rooms;
        }

        private bool DoesNeighbouringRoomNeedDoor(Directions direction, Dictionary<Directions, Room> neighbouringRooms, int actualRoomIndex)
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
                int isWithinBounds = GetIndexNeighbour(actualRoomIndex, direction);


                if (isWithinBounds >= 0)
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

        public bool[] GetAllDoorsNeeded(int roomIndex)
        {
            bool[] doorNeeded = new bool[4];
            Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

            foreach (KeyValuePair<Directions, Room> kvp in neighbouringRooms)
            {
                Directions direction = kvp.Key;

                bool needsDoor = DoesNeighbouringRoomNeedDoor(direction, neighbouringRooms, roomIndex);

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
        public async Task CreateAdjacentRooms(int roomIndex, int genNumber, int roomNum)
        {
            Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

            foreach (KeyValuePair<Directions, Room> kvp in neighbouringRooms)
            {
                Directions direction = kvp.Key;
                Room room = kvp.Value;

                await Task.Delay(100);

                if (room == null && IsRoomInArray(roomIndex, direction))
                {
                    Vector3 position = GetPositionNeighbour(roomIndex, direction);
                    int neighbourIndex = GetIndexNeighbour(roomIndex, direction);
                    bool[] doorNeeded = GetAllDoorsNeeded(neighbourIndex);
                    RoomType roomType = ChooseRoomType(doorNeeded);
                    Quaternion rotation = Quaternion.identity;

                    // create room
                    Room instantiatedRoom = InstantiateRoom(roomType, position, rotation).GetComponent<Room>();

                    if(instantiatedRoom)
                    {
                        // set room
                        instantiatedRoom.SizeRoom = _roomSize;
                        instantiatedRoom.Generation = genNumber;
                        instantiatedRoom.ID = roomNum;

                        // rotate room
                        int rotationNeeded = GetRotationsNeeded(instantiatedRoom, doorNeeded);
                        instantiatedRoom.SetNumberOfRotation(rotationNeeded);

                        // create props
                        GameObject instantiatedProps = InstantiateProps(roomType, position);

                        // place it in their folder
                        instantiatedRoom.transform.SetParent(roomsParent.transform);
                        if (instantiatedProps)
                        {
                            instantiatedRoom.RoomProps = instantiatedProps.GetComponent<RoomProps>();
                            instantiatedProps.transform.SetParent(propsParent.transform);
                        }

                        dungeon[neighbourIndex] = instantiatedRoom;
                    }
                }
            }
        }


        private int GetRotationsNeeded(Room room, bool[] doorNeeded)
        {
            int rotationsNeeded = 0;

            bool[] neededCopy = new bool[doorNeeded.Length];
            doorNeeded.CopyTo(neededCopy, 0);

            FaceState[] roomDoors = room.GetOriginalFaceStatesArray();

            while (!ArraysAreEqual(neededCopy, roomDoors))
            {
                rotationsNeeded++;
                roomDoors = room.GetRotatedFaceStates(rotationsNeeded);

                // avoid infinite loop
                if (rotationsNeeded >= 4)
                {
                    Debug.LogError("Error: Unable to align doors after 4 rotations.");
                    break;
                }
            }
            return rotationsNeeded;
        }

        private bool ArraysAreEqual(bool[] array1, FaceState[] array2)
        {
            // same lenght?
            if (array1.Length != array2.Length)
            {
                return false;
            }

            // same array?
            for (int i = 0; i < array1.Length; i++)
            {
                bool boolValue = array1[i];
                FaceState faceStateValue = array2[i];

                // Convertir from boolean to faceState
                FaceState expectedFaceState = boolValue ? FaceState.Open : FaceState.Closed;

                if (expectedFaceState != faceStateValue)
                {
                    return false;
                }
            }
            return true;
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

            if (IsOneRoomType)
            {
                return RoomType.Four;
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
                    return RoomType.Zero;
            }
        }

        // create a room
        public GameObject InstantiateRoom(RoomType roomType, Vector3 position, Quaternion rotation)
        {
            GameObject roomPrefab = roomType switch
            {
                RoomType.One => roomOnePrefab,
                RoomType.Two => roomTwoPrefab,
                RoomType.TwoOpposite => roomTwoOppositePrefab,
                RoomType.Three => roomThreePrefab,
                RoomType.Four => roomFourPrefab,
                _ => roomZeroPrefab
            };
    
            GameObject roomInstance = Instantiate(roomPrefab, position, rotation);
            return roomInstance;
        }

        // create a props
        public GameObject InstantiateProps(RoomType roomType, Vector3 position)
        {
            // TODO later: take the size of the room type to adjust the props
            if (roomType == RoomType.Zero)
            {
                return null;
            }

            GameObject propsPrefab = _props[Random.Range(0, _props.Length)]; 

            // instantiation of the props
            if (propsPrefab != null)
            {
                // calcul random rotation
                int numberOfRotation = Random.Range(0, 4);
                float rotatedSide = 90 * numberOfRotation;
                Quaternion rotation = Quaternion.Euler(0, rotatedSide, 0);

                // rotate
                GameObject roomInstance = Instantiate(propsPrefab, position, rotation);
                return roomInstance;
            }
            else
            {
                Debug.LogError("Props list is null");
                return null;
            }
        }


        // get position of the room at roomIndex
        public Vector3 GetPosition(int roomIndex)
        {
            int row = Mathf.FloorToInt(roomIndex / _staticDungeonSize);
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
                    position.x += _roomSize;
                    break;
                case Directions.West:
                    position.x -= _roomSize;
                    break;
            }

            return position;
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

        private int GetIndexNeighbour(int roomIndex, Directions direction)
        {
            int neighbourIndex = -1;

            switch (direction)
            {
                case Directions.North:
                    neighbourIndex = roomIndex + _staticDungeonSize;
                    break;
                case Directions.East:
                    if ((roomIndex + 1) % dungeonSize != 0)
                    {
                        neighbourIndex = roomIndex + 1;
                    }
                    else neighbourIndex = -1;
                    break;
                case Directions.South:
                    neighbourIndex = roomIndex - _staticDungeonSize;
                    break;
                case Directions.West:

                    if (roomIndex % dungeonSize != 0)
                    {
                        neighbourIndex = roomIndex - 1;
                    }
                    else neighbourIndex = -1;
                    break;
            }

            if (neighbourIndex >= 0 && neighbourIndex < dungeon.Length)
            {
                return neighbourIndex;
            }
            else
            {
                return -1;
            }
        }

        private bool IsRoomInArray(int roomIndex, Directions direction)
        {
            int newIndex = GetIndexNeighbour(roomIndex, direction);
            return newIndex != -1;
        }

        public bool IsRoomEmpty(int roomIndex)
        {
            return dungeon[roomIndex].GetRoomType() == RoomType.Zero;
        }

        public List<(Room,List<GameObject>)> GetEnemySpawnPoints(GameObject pPlayer)
        {
            Vector3 playerPos = pPlayer.transform.position;

            Room playerRoom = dungeon[0];
        
            foreach (Room room in dungeon)
            {
                if (Vector3.Distance(room.transform.position, playerPos) < Vector3.Distance(playerRoom.transform.position, playerPos))
                {
                    playerRoom = room;
                }
            }


            Dictionary<Directions, Room> rooms = GetNeighbouringRooms(GetIndexOfRoom(playerRoom));

            List<(Room,List<GameObject>)> spawnPointsOfSurroundingRooms = new List<(Room, List<GameObject>)>();

            Room[] roomsList = rooms.Values.ToArray();
        
            foreach (var t in roomsList)
            {
                if (t != null)
                {
                    spawnPointsOfSurroundingRooms.Add((t,t.RoomProps.SpawnPoints));
                }
            }

            return spawnPointsOfSurroundingRooms;
        }

        public List<(Room, List<GameObject>)> GetAllEnemySpawnPoints()
        {
            List<(Room, List<GameObject>)> roomAndSpawnPoints = new List<(Room, List<GameObject>)>();

            foreach (GameObject player in MultiManager.instance.GetAllPlayerGameObjects())
            {
                roomAndSpawnPoints.AddRange(GetEnemySpawnPoints(player.GetComponentInChildren<PlayerBehaviour>().gameObject));
            }

            return roomAndSpawnPoints;
        }
    }

    public enum Directions
    {
        North,
        East,
        South,
        West
    }
}
