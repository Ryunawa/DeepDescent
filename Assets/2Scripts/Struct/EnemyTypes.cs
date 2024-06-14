using System;
using System.Collections.Generic;

namespace _2Scripts.Struct
{
    [Serializable]
    public struct EnemyTypes
    {
        public List<MeshInfo> meshInfos; // List of mesh info containing indices and health values
    }

    [Serializable]
    public struct MeshInfo
    {
        public int index; // The index of the mesh
        public float health; // The health of the enemy
    }
}