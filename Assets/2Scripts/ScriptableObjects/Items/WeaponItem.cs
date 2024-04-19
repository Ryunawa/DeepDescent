using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    BOW,
    SWORD,
    AXE,
    DAGGERS,
    MAGIC_STAFF
}

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "ScriptableObjects/Item/Create New Weapon Item")]
public class WeaponItem : EquippableItem
{
    [Header("Weapon Specific")]
    public WeaponType WeaponType;
    public int AttackValue;
    public float AttackSpeed;
    public bool TwoHanded;
    public bool IsDualWieldable;

    public override bool Equip()
    {
        //TODO
        throw new System.NotImplementedException();
    }
}
