using UnityEngine;

public interface IWeaponEnhancement
{
    public int EnhancedLevel { get; }
    public void EnhanceWeapon();
}