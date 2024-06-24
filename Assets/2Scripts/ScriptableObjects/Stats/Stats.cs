using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats Page", menuName = "ScriptableObjects/Stats/Create New Character Stat Page")]
public class Stats : ScriptableObject
{
    public float MaxLife = 0.0f;
    public float BaseArmour = 0.0f;
    public float DamageReceivedModifier = 1.0f;
    public float DamageInflictedModifier = 1.0f;
    public List<WeaponType> EquippableWeaponType = new List<WeaponType>();
    public List<Item> StartingItemInInventory = new List<Item>();
}
