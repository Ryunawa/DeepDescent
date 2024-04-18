using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.ProceduralGeneration
{
    public class Room : MonoBehaviour
    {
        [SerializeField]private int _roomID;

        public int RoomID => _roomID;

        public FaceState north;
        public FaceState east;
        public FaceState south;
        public FaceState west;
    }

    public enum FaceState
    {
        Closed,
        Open,
        Free
    }
    
}