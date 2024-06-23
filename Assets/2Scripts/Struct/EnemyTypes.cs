using System;
using System.Collections.Generic;

namespace _2Scripts.Struct
{
    [Serializable]
    public struct EnemyTypes
    {
        public List<EnemyStats> statsInfos; // List of mesh info containing indices and health values
    }
}