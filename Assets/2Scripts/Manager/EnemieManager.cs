using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace _2Scripts.Manager
{
    public class EnemyManager : NetworkBehaviour
    {
        [SerializeField] private List<NetworkObject> enemies = new List<NetworkObject>();

        private Dictionary<int, NetworkObject> _enemiesDictionary;


        private void Start()
        {
            if (IsServer)
            {
                _enemiesDictionary = enemies.ToDictionary( y=> enemies.IndexOf(y), x=>x);
            }
        }

        [Rpc(SendTo.Server)]
        public void SpawnEnemyRpc(int id, Vector3 position, Quaternion quaternion)
        {
            NetworkObject o = Instantiate(_enemiesDictionary[id], position, quaternion);
            o.Spawn();
        }
        
    }
}