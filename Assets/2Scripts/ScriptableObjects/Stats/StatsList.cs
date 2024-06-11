using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats Page List", menuName = "ScriptableObjects/Stats/Create New Stat Page List")]
public class StatsList : ScriptableObject
{
    public List<Stats> Stats;
}
