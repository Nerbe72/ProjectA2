using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponInstanceData : MonoBehaviour
{
    public int WeaponId;

    public int WeaponDamage;
    public int WeaponDefense;

    public int UpgradedLevel;
}

[Serializable]
public class WeaponInstanceWrapper
{
    public List<WeaponInstanceData> instances;

}
