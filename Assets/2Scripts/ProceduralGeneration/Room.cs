using System.Collections.Generic;
using UnityEngine;

namespace _2Scripts.ProceduralGeneration
{
    public class Room : MonoBehaviour
    {
        [SerializeField]private int _roomID;

        [SerializeField]private List<int> WestCompatibleRooms = new ();
        [SerializeField]private List<int> EastCompatibleRooms = new ();
        [SerializeField]private List<int> SouthCompatibleRooms = new ();
        [SerializeField]private List<int> NorthCompatibleRooms = new ();

        public int RoomID => _roomID;

        public List<int> WestCompatibleRooms1 => WestCompatibleRooms;

        public List<int> EastCompatibleRooms1 => EastCompatibleRooms;

        public List<int> SouthCompatibleRooms1 => SouthCompatibleRooms;

        public List<int> NorthCompatibleRooms1 => NorthCompatibleRooms;
    }
}