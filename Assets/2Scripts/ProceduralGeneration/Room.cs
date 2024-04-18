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

        private int numberOfRightRotation = 0;
        
        [Button]
        public void DebugFunc()
        {
            
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