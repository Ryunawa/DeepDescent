using System.Collections.Generic;
using UnityEngine;

namespace _2Scripts.ProceduralGeneration
{
    public class Room : MonoBehaviour
    {
        private int _roomID;

        private List<int> WestCompatibleRooms = new ();
        private List<int> EastCompatibleRooms = new ();
        private List<int> SouthCompatibleRooms = new ();
        private List<int> NorthCompatibleRooms = new ();

        public int RoomID => _roomID;

        public List<int> WestCompatibleRooms1 => WestCompatibleRooms;

        public List<int> EastCompatibleRooms1 => EastCompatibleRooms;

        public List<int> SouthCompatibleRooms1 => SouthCompatibleRooms;

        public List<int> NorthCompatibleRooms1 => NorthCompatibleRooms;
    }
}