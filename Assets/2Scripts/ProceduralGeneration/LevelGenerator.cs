using System.Collections;
using _2Scripts.Entities.Player;
using _2Scripts.Helpers;
using _2Scripts.Manager;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _2Scripts.ProceduralGeneration
{
    public class LevelGenerator : GameManagerSync<LevelGenerator>
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
        [SerializeField, Range(0, 100)] private float doorProbabilityIfEmpty = 50f;

        [Header("Props")]
        [SerializeField] private GameObject[] _props;
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private GameObject shopProps;

        [Header("Setting")]
        public bool spawnShop;
        [SerializeField] private float _roomSize;
        [SerializeField] private int dungeonSize = 5;
        [SerializeField] private bool IsOneRoomType;
    
        [FormerlySerializedAs("_generatedDungeonParent")]
        [Space(1f)]
        [Header("Folders")]
        [SerializeField] private GameObject generatedDungeonParent;
        [SerializeField] private GameObject roomsParent;

        public GameObject roomsParent1 => roomsParent;
        public GameObject doorsParent1 => doorsParent;
        public GameObject propsParent1 => propsParent;
        public GameObject generatedDungeonParent1 => generatedDungeonParent;

        [SerializeField] private GameObject doorsParent;
        [SerializeField] private GameObject propsParent;
        
        public UnityEvent dungeonGeneratedEvent = new ();

        private static int _staticDungeonSize;
        private int _roomNumber = 1;

        private MultiManager _multiManager;
        private NetworkObject portalNO;

        public NetworkObject Portal
        {
            get => portalNO;
            set => portalNO = value;
        }

        private void Awake()
        {
            GameManager.instance.levelGenerator = this;
        }

        protected override void Start()
        {
            base.Start();


            StartCoroutine(WaitForClients());

        }

        private IEnumerator WaitForClients()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == GameManager.GetManager<MultiManager>().Lobby.Players.Count);

            GameManager.instance.ChangeGameState(GameState.Generating);
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Generating:
                    {
                        _multiManager = GameManager.GetManager<MultiManager>();
                            
                        if(_multiManager.IsLobbyHost())
                            StartGeneration();
                        break;
                    }
                case GameState.InLevel:
                {
                    // Play Music
                    GameManager.GetManager<AudioManager>().PlayMusic("InsideTheDungeonMusic", 0.08f);
                    dungeonGeneratedEvent.Invoke();
                    if (!spawnShop) PlacePortal();
                    GameManager.GetManager<SceneManager>().DeactivateLoadingScreen();
                    GameManager.GetManager<InventoryUIManager>().gameObject.SetActive(true);
                        
                    break;
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ChangeStateClientRpc()
        {
            if (GameManager.GameState != GameState.InLevel)
            {
                Debug.Log("CALLED CHANGE STATE FROM RPC");
                // SubToGameManagerEvent();
                            
                GameManager.instance.ChangeGameState(GameState.InLevel);
            }
        }

        public async void StartGeneration()
        {
            if (spawnShop)
            {
                Debug.LogWarning("this is a shop");
                // Play Music
                GameManager.GetManager<AudioManager>().PlayMusic("SafeAreaMusic", 0.1f);
                GenerateShopRoom();
                
                
                ChangeStateClientRpc();
                
                return;
            }

            _staticDungeonSize = dungeonSize;
            // Random.InitState((int)DateTime.Now.Ticks);

            dungeon = new Room[_staticDungeonSize*_staticDungeonSize];

            //Generation n = 1 : center
            int centerIndex = (_staticDungeonSize /2) * (_staticDungeonSize +1);
            Room startRoom = InstantiateRoom(RoomType.Four, GetPosition(centerIndex), Quaternion.identity).GetComponent<Room>();
            startRoom.transform.SetParent(roomsParent.transform);
            startRoom.SizeRoom = _roomSize;
            startRoom.CreateDoors();
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
            
            DynamicNavMesh.UpdateNavMesh();
                        
            GameManager.GetManager<ItemManager>().StartSpawningItems();
            
            ChangeStateClientRpc();
                        
            TeleportHostAndClientRpc(GetPosition(centerIndex));
        }

        private void GenerateShopRoom()
        {
            _staticDungeonSize = 1;
            // Random.InitState((int)DateTime.Now.Ticks);

            dungeon = new Room[_staticDungeonSize * _staticDungeonSize];

            //Generation n = 1 : center
            int centerIndex = (_staticDungeonSize / 2) * (_staticDungeonSize + 1);
            Room shopRoom = InstantiateRoom(RoomType.One, GetPosition(centerIndex), Quaternion.identity).GetComponent<Room>();
            shopRoom.transform.SetParent(roomsParent.transform);
            shopRoom.SizeRoom = _roomSize;
            // shopRoom.CreateDoors();
            shopRoom.Generation = 1;

            // create props
            GameObject instantiatedProps = InstantiateProps(RoomType.One, GetPosition(centerIndex));

            // place it in their folder
            if (instantiatedProps)
            {
                instantiatedProps.transform.SetParent(propsParent.transform);
                shopRoom.RoomProps = instantiatedProps.GetComponentInChildren<RoomProps>();
            }

            dungeon[centerIndex] = shopRoom;

            _roomNumber = 0;

            Debug.Log("GenerationFinished");

            TeleportHostAndClientRpc(GetPosition(centerIndex));
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void TeleportHostAndClientRpc(Vector3 pPosition)
        {
            _multiManager.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().TeleportPlayer( pPosition + Vector3.up * 5);
        }
        
        private async Task DoGen(int startDepth)
        {
            if (startDepth == _staticDungeonSize)
            {
                Debug.Log("DoGenFinished");
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
            
                CreateAdjacentRooms(GetIndexOfRoom(room), startDepth, _roomNumber);
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
            List<Room> roomsOfGivenGen = new List<Room>(); 

            foreach (Room room in dungeon)
            {
                if (room != null && room.Generation == genNumber)
                {
                    roomsOfGivenGen.Add(room);
                }
            }

            return roomsOfGivenGen.ToArray();
        }

        private Dictionary<Directions, Room> GetNeighbouringRooms(int roomIndex)
        {
            Dictionary<Directions, Room> rooms = new Dictionary<Directions, Room>()
            {
                { Directions.North, null },
                { Directions.East, null },
                { Directions.South, null },
                { Directions.West, null }
            };

            // east
            if ((roomIndex + 1) % dungeonSize != 0 && roomIndex + 1 < dungeon.Length)
            {
                rooms[Directions.East] = dungeon[roomIndex + 1];
            }

            // west
            if (roomIndex % dungeonSize != 0 && roomIndex - 1 >= 0)
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


        private bool DoesNeighbouringRoomNeedDoor(Directions direction, Room neighbourRoom, int actualRoomIndex)
        {
            if (neighbourRoom != null)
            {
                // Get opposite direction
                Directions oppositeDirection = GetOppositeDirection(direction);

                // Room has a door?
                bool hasDoorOpposite = neighbourRoom.HasDoor(oppositeDirection);

                // If it has door : door, if not no door
                return hasDoorOpposite;
            }
            else
            {
                // Is in array?
                int isWithinBounds = GetIndexNeighbour(actualRoomIndex, direction);

                if (isWithinBounds >= 0)
                {
                    // Check if neighboring room will have doors
                    if (WillRoomHaveDoors(actualRoomIndex))
                    {
                        // Room does not exist and is not out of bounds -> rand door
                        float randomValue = Random.Range(0f, 100f);
                        return randomValue < doorProbabilityIfEmpty;
                    }
                    else
                    {
                        // Room does not have any planned doors or does not exist
                        return false;
                    }
                }
                else
                {
                    // Room does not exist and is out of bounds -> no door
                    return false;
                }
            }
        }

        private bool WillRoomHaveDoors(int roomIndex)
        {
            // Get neighbouring rooms of the given room
            Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

            foreach (var kvp in neighbouringRooms)
            {
                Directions direction = kvp.Key;
                Room neighbourRoom = kvp.Value;

                if (neighbourRoom != null)
                {
                    // Check if the neighbouring room has doors towards the current room
                    Directions oppositeDirection = GetOppositeDirection(direction);
                    if (neighbourRoom.HasDoor(oppositeDirection))
                    {
                        return true;
                    }
                }
                else
                {
                    // Check if the neighbour room index is within bounds
                    int neighbourRoomIndex = GetIndexNeighbour(roomIndex, direction);
                    if (neighbourRoomIndex >= 0)
                    {
                        // If the neighbour room does not exist yet but is within bounds, check further
                        Room actualRoom = dungeon[roomIndex];
                        int actualRoomGeneration = actualRoom != null ? actualRoom.Generation : 0;
                        int neighbourRoomGeneration = 0; // default for non-existing rooms

                        if (actualRoomGeneration > neighbourRoomGeneration)
                        {
                            // If the actual room generation is greater than the neighbour room generation,
                            // we can consider that the neighbour room will be created and can potentially have doors
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public bool[] GetAllDoorsNeeded(int roomIndex)
        {

            bool[] doorNeeded = new bool[4];
            Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex);

            foreach (KeyValuePair<Directions, Room> kvp in neighbouringRooms)
            {
                Directions direction = kvp.Key;
                Room neighbourRoom = kvp.Value;

                bool needsDoor = DoesNeighbouringRoomNeedDoor(direction, neighbourRoom, roomIndex);

                switch (direction)
                {
                    case Directions.North:
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

        public void CreateAdjacentRooms(int roomIndex, int genNumber, int roomNum)
        {
            Dictionary<Directions, Room> neighbouringRooms = GetNeighbouringRooms(roomIndex); // get all rooms next to ours

            foreach (KeyValuePair<Directions, Room> kvp in neighbouringRooms)
            {
                Directions direction = kvp.Key;
                Room room = kvp.Value;

                if (room == null && IsRoomInArray(roomIndex, direction))
                {
                    Vector3 position = GetPositionNeighbour(roomIndex, direction);
                    int neighbourIndex = GetIndexNeighbour(roomIndex, direction); 
                    bool[] doorNeeded = GetAllDoorsNeeded(neighbourIndex);
                    RoomType roomType = ChooseRoomType(doorNeeded);
                    Quaternion rotation = Quaternion.identity;

                    // Create room
                    Room instantiatedRoom = InstantiateRoom(roomType, position, rotation).GetComponent<Room>();

                    if (instantiatedRoom)
                    {
                        // Set room properties
                        instantiatedRoom.SizeRoom = _roomSize;
                        instantiatedRoom.Generation = genNumber;
                        instantiatedRoom.IdParentRoom = roomNum;
                        instantiatedRoom.MyId = neighbourIndex;


                        // Rotate room
                        int rotationNeeded = GetRotationsNeeded(instantiatedRoom, doorNeeded);
                        instantiatedRoom.SetNumberOfRotation(rotationNeeded);

                        // Create props
                        GameObject instantiatedProps = InstantiateProps(roomType, position);

                        // Place it in their folder
                        instantiatedRoom.transform.SetParent(roomsParent.transform);
                        if (instantiatedProps)
                        {
                            instantiatedRoom.RoomProps = instantiatedProps.GetComponent<RoomProps>();
                            instantiatedProps.transform.SetParent(propsParent.transform);
                        }

                        dungeon[neighbourIndex] = instantiatedRoom;
                        Debug.Log($"Room created at index {neighbourIndex} with generation {genNumber}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to instantiate room at {position} with type {roomType}");
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
            // same length?
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



        // select a room type to construct --> will depend on the door next to it
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
            roomInstance.GetComponent<NetworkObject>().Spawn();
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

            GameObject propsPrefab;

            if (spawnShop) propsPrefab = shopProps;
            else propsPrefab = _props[Random.Range(0, _props.Length)];

            // instantiation of the props
            if (propsPrefab != null)
            {
                Quaternion rotation = Quaternion.identity;

                // calcul random rotation
                if (!spawnShop)
                {
                    int numberOfRotation = Random.Range(0, 4);
                    float rotatedSide = 90 * numberOfRotation;
                    rotation = Quaternion.Euler(0, rotatedSide, 0);
                }

                // rotate
                GameObject roomInstance = Instantiate(propsPrefab, position, rotation);
                roomInstance.GetComponent<NetworkObject>().Spawn();
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
                if (room != null && (playerRoom == null || Vector3.Distance(room.transform.position, playerPos) < Vector3.Distance(playerRoom.transform.position, playerPos)))
                {
                    playerRoom = room;
                }
            }

            Dictionary<Directions, Room> rooms = GetNeighbouringRooms(GetIndexOfRoom(playerRoom));

            List<(Room,List<GameObject>)> spawnPointsOfSurroundingRooms = new List<(Room, List<GameObject>)>();

            foreach (var room in rooms.Values)
            {
                if (room != null && room.RoomProps != null && room.RoomProps.SpawnPoints != null && room.GetRoomType() != RoomType.Zero)
                {
                    spawnPointsOfSurroundingRooms.Add((room, room.RoomProps.SpawnPoints));
                }
            }

            return spawnPointsOfSurroundingRooms;
        }

        public List<(Room, List<GameObject>)> GetAllEnemySpawnPoints()
        {
            List<(Room, List<GameObject>)> roomAndSpawnPoints = new List<(Room, List<GameObject>)>();

            foreach (GameObject player in _multiManager.GetAllPlayerGameObjects())
            {
                roomAndSpawnPoints.AddRange(GetEnemySpawnPoints(player.GetComponentInChildren<PlayerBehaviour>().gameObject));
            }

            return roomAndSpawnPoints;
        }

        private void PlacePortal()
        {
            for (int gen = _staticDungeonSize; gen > 0; gen--)
            {
                Room[] rooms = GetRoomFromGeneration(gen);
                List<Room> validRooms = new List<Room>();

                // filter all RoomType.Zero
                foreach (Room room in rooms)
                {
                    if (room.GetRoomType() != RoomType.Zero)
                    {
                        validRooms.Add(room);
                    }
                }

                // if a valid room has been found -> put the portal
                if (validRooms.Count > 0)
                {
                    Room targetRoom = validRooms[Random.Range(0, validRooms.Count)];
                    Vector3 portalPosition = targetRoom.transform.position;
                    GameObject portal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);
                    portalNO = portal.GetComponent<NetworkObject>();
                    portalNO.Spawn();
                    portal.GetComponent<BossPillarInteraction>().roomTp = targetRoom;
                    return;
                }
            }
        }


        public List<ItemSpawnPoint> GetAllShuffledItemSpawnPoints()
{
            List<ItemSpawnPoint> allSpawnPoints = new List<ItemSpawnPoint>();
            foreach (var room in dungeon)
            {
                if (room != null)
                {
                    allSpawnPoints.AddRange(room.GetAllItemSpawnPoint());
                }
            }
            List<ItemSpawnPoint> shuffledSpawnPoints = ShuffleItemSpawnPoints(allSpawnPoints);
            return shuffledSpawnPoints;
        }


        public List<ItemSpawnPoint> ShuffleItemSpawnPoints(List<ItemSpawnPoint> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                ItemSpawnPoint temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }

            return list;
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
