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

        private int numberOfRightRotation = 0;
        public int ID;

        public int Generation
        {
            get => _generation;
            set => _generation = value;
        }

        public RoomType GetRoomType()
        {
            return roomType;
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
                    return originalFaceStatesArray[(int)direction] == FaceState.Open;
                case Directions.East:
                    return originalFaceStatesArray[(int)direction] == FaceState.Open;
                case Directions.South:
                    return originalFaceStatesArray[(int)direction] == FaceState.Open;
                case Directions.West:
                    return originalFaceStatesArray[(int)direction] == FaceState.Open;
                default:
                    return false;
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
    One,
    Two,
    TwoOpposite,
    Three,
    Four
}

}