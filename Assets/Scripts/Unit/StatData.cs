using System;
using UnityEngine;

using GameStuff;

[Serializable]
public class StatData : ScriptableObject
{
    public UnitType UnitType;
    public int Health;
    public int Damage;
    public int Defense;

    public int ID;
    
    [Header("Sound Settings")]
    public AudioClip HurtSound;
    public AudioClip DeathSound;
}