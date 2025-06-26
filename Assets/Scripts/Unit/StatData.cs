using System;
using UnityEngine;

[Serializable]
public class StatData : ScriptableObject
{
    public UnitType UnitType;
    public int Health;
    public int Damage;
    public int Defense;

    public int ID;
}