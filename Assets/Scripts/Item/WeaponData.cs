using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeaponData : ItemData
{
    public WeaponType WeaponType;
    public AttackType AttackType;

    [Header("Level Required")]
    public int Requirement_STR;
    public int Requirement_DEX;
    public int Requirement_INT;

    [Header("Level DamageFix")]
    public int DamageFix_STR;
    public int DamageFix_DEX;
    public int DamageFix_INT;

    [Header("Visual")]
    public AssetReferenceGameObject WeaponPrefabReference; // gameobject ĳ��ȭ?

    [Header("Random Stat Range")]
    [Tooltip("Range (X ~ Y)")] public Vector2 DamageRange;
    [Tooltip("Range (X ~ Y)")] public Vector2 DefenseRange;

    [Header("Object : Position/ Rotation/ Scale)")]
    public Vector3 HandlePosition;
    public Quaternion HandleRotation;
    public Vector3 HandleScale;

    [Header("Ability")]
    [SerializeReference]
    public List<AbilityLogic> abilityDatas;

    public int AdditionalDamage(int _levelSTR, int _levelDEX, int _levelINT)
    {
        return _levelSTR * DamageFix_STR + _levelDEX * DamageFix_DEX + _levelINT * DamageFix_INT;
    }
}
