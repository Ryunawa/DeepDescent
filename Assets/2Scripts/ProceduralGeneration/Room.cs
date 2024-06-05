using _2Scripts.Manager;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _2Scripts.ProceduralGeneration
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private RoomType roomType;
        //Ordered : North, East, South, West
        [SerializeField] private FaceState[] originalFaceStatesArray = new FaceState[4];

        [SerializeField] private int generation;

        [SerializeField] private int numberOfRightRotation = 0;
        public int ID;
        private float _sizeRoom = 0;

        private RoomProps _roomProps;
        
        [SerializeField] private GameObject[] doorPrefab;

        public int enemiesCount { get; private set; }

        public RoomProps RoomProps
        {
            get => _roomProps;
            set => _roomProps = value;
        }


        public float SizeRoom
        {
            get => _sizeRoom;
            set => _sizeRoom = value;
        }

        public int Generation
        {
            get => generation;
            set => generation = value;
        }

        private void OnEnable()
        {
            EnemiesSpawnerManager.instance.OnEnemiesSpawnedOrKilledEventHandler += UpdateSpawnedNumber;
        }

        private void UpdateSpawnedNumber(object receiver, int value)
        {
            if (receiver.Equals(this))
            {
                enemiesCount += value;
            }
        }
        
        public RoomType GetRoomType()
        {
            return roomType;
        }

        public void SetNumberOfRotation(int newNumberOfRightRotation)
        {
            numberOfRightRotation = newNumberOfRightRotation;
            this.gameObject.transform.rotation = Quaternion.AngleAxis(90f * newNumberOfRightRotation, Vector3.up);

            createDoors();
        }

        public FaceState[] GetOriginalFaceStatesArray()
        { 
            return originalFaceStatesArray;
        }

        public FaceState[] GetRotatedFaceStates(int numberOfRightRotations)
        {
            FaceState[] newFaceStates = new FaceState[4];

            for (int i = 0; i < originalFaceStatesArray.Length; i++)
            {
                newFaceStates[(i + numberOfRightRotations) % originalFaceStatesArray.Length] = originalFaceStatesArray[i];
            }

            return newFaceStates;
        }

        public bool HasDoor(Directions direction)
        {
            switch (direction)
            {
                case Directions.North:
                    return GetRotatedFaceStates(numberOfRightRotation)[(int)direction] == FaceState.Open;
                case Directions.East:
                    return GetRotatedFaceStates(numberOfRightRotation)[(int)direction] == FaceState.Open;
                case Directions.South:
                    return GetRotatedFaceStates(numberOfRightRotation)[(int)direction] == FaceState.Open;
                case Directions.West:
                    return GetRotatedFaceStates(numberOfRightRotation)[(int)direction] == FaceState.Open;
                default:
                    return false;
            }
        }

        public void createDoors()
        {
            GameObject doorsParent = GameObject.Find("Doors");
            Vector3 pos = this.gameObject.transform.position;

            if (HasDoor(Directions.North))
            {
                Vector3 doorNorth = pos + (Vector3.forward * SizeRoom / 2) + (Vector3.left * 0.6f);
                GameObject instantiatedNorthDoor = Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], doorNorth, Quaternion.AngleAxis(180, Vector3.up));
                instantiatedNorthDoor.transform.SetParent(doorsParent.transform);
            }
            if (HasDoor(Directions.East))
            {
                Vector3 doorEast = pos + (Vector3.right * SizeRoom / 2) + (Vector3.forward * 0.6f);
                GameObject instantiatedEastDoor = Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], doorEast, Quaternion.AngleAxis(-90, Vector3.up));
                instantiatedEastDoor.transform.SetParent(doorsParent.transform);
            }
        }

    }


public enum FaceState
{
    Closed, // no door (just a wall)
    Open, // has door
    Free // other
}

public enum RoomType
{
    Zero,
    One,
    Two,
    TwoOpposite,
    Three,
    Four
}

}