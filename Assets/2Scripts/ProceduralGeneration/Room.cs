using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.ProceduralGeneration
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private RoomType roomType;
        //Ordered : North, East, South, West
        [SerializeField] private FaceState[] originalFaceStatesArray = new FaceState[4];

        [SerializeField] private int _generation;

        [SerializeField] private int numberOfRightRotation = 0;
        public int ID;
        private float _sizeRoom = 0;

        [SerializeField] private GameObject[] doorPrefab;

        public float SizeRoom
        {
            get => _sizeRoom;
            set => _sizeRoom = value;
        }

        public int Generation
        {
            get => _generation;
            set => _generation = value;
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
            Vector3 pos = this.gameObject.transform.position;

            if (HasDoor(Directions.North))
            {
                Vector3 doorNorth = pos + (Vector3.forward * SizeRoom / 2) + (Vector3.left * 0.6f);
                Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], doorNorth, Quaternion.AngleAxis(180, Vector3.up));
            }
            if (HasDoor(Directions.East))
            {
                Vector3 doorEast = pos + (Vector3.right * SizeRoom / 2) + (Vector3.forward * 0.6f);
                Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], doorEast, Quaternion.AngleAxis(-90, Vector3.up));
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